/** @file

    @copyright  &copy; 2026, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       22.07.2026

    @author
        Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @brief
        Provides extension methods for conditionally appending sanitized child elements to XElement instances.

    @details
        - Provides helper methods for LINQ to XML `XElement` manipulation.
        - Uses allocation-free `ReadOnlySpan<char>` scanning to sanitize input values by stripping non-printable XML control characters.
        - Wraps non-empty sanitized values in XCData sections.
        - Suppresses element creation when values are `null` or empty strings.
        - Contains no shared mutable state.

    @defgroup REF_GertToUTWEngine_GertToUTW_XElementExtension XElementExtension
    @{
    @}
*/

using System.Xml.Linq;

namespace GertToUTW;

/** @ingroup REF_GertToUTWEngine_GertToUTW_XElementExtension
    @class XElementExtension
    @brief
        Provides extension methods for adding child elements to an XElement object conditionally.

    @details
        - Allows appending child elements to an `XElement` instance only when content is present.
        - Sanitizes content strings against illegal XML control characters using `ReadOnlySpan<char>` span-based scanning.
        - Avoids heap allocations when input strings contain no invalid control characters.
        - Contains no shared mutable state.

    @see XElementExtension.AddIfNotEmpty
*/
public static class XElementExtension
    {
    /** @brief
        Determines whether a character is an invalid XML control character.

    @details
        - Evaluates whether the character falls within non-printable ASCII ranges `0x00`-`0x08`, `0x0B`-`0x0C`, `0x0E`-`0x1F`, or `0x7F`.
        - Allows standard XML formatting whitespace characters: tab (`0x09`), line feed (`0x0A`), and carriage return (`0x0D`).

    @param[in] character
        Provides the character to evaluate.

    @return
        Returns `true` if the character is an invalid XML control character; otherwise, returns `false`.
    */
    private static bool is_invalid_xml_char( char character )
        {
        if( character < 0x20 )
            {
            if( character == 0x09 || character == 0x0A || character == 0x0D )
                {
                return false;
                }

            return true;
            }

        if( character == 0x7F )
            {
            return true;
            }

        return false;
        }

    /** @brief
        Strips invalid XML control characters from an input string using ReadOnlySpan scanning.

    @details
        - Inspects input characters in-place via `ReadOnlySpan<char>`.
        - Returns the original string reference without allocation when no invalid characters are present.
        - Builds a new sanitized string via `string.Create` only when invalid control characters are detected.
        - Returns an empty string if `input` is `null` or empty.

    @param[in] input
        Provides the text string to sanitize for XML compatibility.

    @return
        Returns the sanitized string stripped of invalid control characters.
    */
    internal static string sanitize_for_xml( string? input )
        {
        if( string.IsNullOrEmpty(input) )
            {
            return string.Empty;
            }

        ReadOnlySpan<char> source = input.AsSpan();
        int invalid_count = 0;

        for( int index = 0; index < source.Length; index++ )
            {
            if( is_invalid_xml_char(source[index]) )
                {
                invalid_count++;
                }
            }

        if( invalid_count == 0 )
            {
            return input;
            }

        int target_length = source.Length - invalid_count;
        if( target_length == 0 )
            {
            return string.Empty;
            }

        return string.Create(target_length, input, ( destination, str ) =>
        {
            ReadOnlySpan<char> src = str.AsSpan();
            int dest_index = 0;

            for( int src_index = 0; src_index < src.Length; src_index++ )
                {
                char ch = src[src_index];
                if( !is_invalid_xml_char(ch) )
                    {
                    destination[dest_index] = ch;
                    dest_index++;
                    }
                }
        });
        }

    /** @brief
        Adds a child element with XCData content to an XElement parent only if the provided value is not empty or null.

    @details
        - Validates that both `parent` and `element_name` are non-null.
        - Sanitizes input strings using @ref sanitize_for_xml before constructing the element.
        - Wraps non-empty sanitized values inside an `XCData` section.
        - Skips element addition if `value` is `null` or empty.

    @param[in] parent
        Provides the target parent `XElement` instance.

    @param[in] element_name
        Provides the XML tag name for the child element.

    @param[in] value
        Provides the string content for the child element.

    @exception ArgumentNullException
        Thrown when `parent` or `element_name` is `null`.

    @see sanitize_for_xml
    */
    public static void AddIfNotEmpty( this XElement parent, string element_name, string? value )
        {
        ArgumentNullException.ThrowIfNull(parent, nameof(parent));
        ArgumentException.ThrowIfNullOrWhiteSpace(element_name, nameof(element_name));

        if( string.IsNullOrEmpty(value) )
            {
            return;
            }

        string sanitized_value = sanitize_for_xml(value);

        parent.Add(new XElement(element_name, new XCData(sanitized_value)));
        }
    }