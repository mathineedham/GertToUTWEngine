/** @file       UtwXmlGenerator.cs

    @copyright  &copy; 2026, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       13.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)
 
    @defgroup   REF_GertToUTWEngine_GertToUTW_XElementExtension  XElementExtension
    @{
    @ingroup    REF_GertToUTWEngine_GertToUTW

    @brief      Provides a method for an XElement object to add a child element only if the value is not empty or null.

    @details    The `XElementExtension` class provides an extension method for the `XElement` class that allows adding
                a child element only if the provided value is not empty or null.
    @}
*/
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
[assembly: InternalsVisibleTo("RegressionTests")]
namespace GertToUTW;

/** @ingroup    REF_GertToUTWEngine_GertToUTW_XElementExtension

    @class      XElementExtension

    @brief      Methods for adding child elements to an XElement object conditionally.

    @details    Allows adding a child element to an `XElement` only if the provided value is not empty or null, and sanitizes the value for XML compatibility.
*/
public static partial class XElementExtension
    {
    /**@brief All characters that may be harmful to an xml format
     * i.e all control char except for tab, line feed and carriage return
     */
    [GeneratedRegex(@"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]")]
    internal static partial Regex invalid_xml_chars_regex();
    /** @brief Strips a string from any characters that may interfer with the xml formal*/
    internal static string sanitize_for_xml( string? input )
        {
        if( string.IsNullOrEmpty(input) )
            {
            return string.Empty;
            }

        // Strips out 0x1B and other breaking control characters
        return invalid_xml_chars_regex().Replace(input, string.Empty);
        }

    /** @brief Adds a child element to an XElement only if the provided value is not empty or null.
        @param[in]  parent          The parent XElement to which the child element will be added.
        @param[in]  element_name    The name of the child element to be added.
        @param[in]  value           The value to be added as the content of the child element. If null or empty, no element is added.
    */
    public static void AddIfNotEmpty( this XElement parent, string element_name, string? value)
        {
        if( parent == null )
            {
            throw new ArgumentNullException(nameof(parent), "Cannot add a child element to a null XElement.");
            }
        if( string.IsNullOrEmpty(value) )
            {
            return;
            }

        string sanitized_value = sanitize_for_xml(value);

        parent.Add(new XElement(element_name, new XCData(sanitized_value)));
        }
    }