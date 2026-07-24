/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       08.07.2026

    @author
        Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @brief
        Represents a single test item step within an executed test run.

    @details
        - Encapsulates key identifiers, layout indices, names, descriptions, and standard execution logs.
        - Enforces strict boundary constraints restricting sequence indices to valid 16-bit signed integer bounds.
        - Integrates result state validation through the normalized Result model.
        - Provides regular expression match parsing to initialize structured test item instances from raw log data.
        - Contains no shared mutable state.

    @defgroup REF_GertToUTWEngine_GertToUTW_TestItem TestItem
    @{
    @}
*/
// Ignore Spelling: Stdout Stderr
using System.Globalization;
using System.Text.RegularExpressions;

namespace GertToUTW;

/** @ingroup REF_GertToUTWEngine_GertToUTW_TestItem
    @class TestItem
    @brief
        Represents a single test step in a test run.

    @details
        - Handles property initialization, sequence indexing, and structural validation constraints.
        - Restricts layout sequence indices to valid 16-bit signed integer ranges (`0` to `32767`).
        - Normalizes raw test outcome logs into structured result state representations.
        - Contains no shared mutable state.

    @see Result
*/
public class TestItem
    {
    /** @brief Private static lower boundary limit for the sequence index. */
    internal static readonly int theMinIndexLimit;

    /** @brief Private static upper boundary limit for the sequence index corresponding to xsd:short limits. */
    internal static readonly int theMaxIndexLimit = 32767;

    /** @property TestItem_Key
        @brief
            Gets or sets the unique database or configuration key identifier for the test item.

        @details
            - Serves as the primary reference key for database operations or configuration mapping.
            - Defaults to `1`.

        @return
            Returns the key identifier of the test item.
    */
    public int TestItem_Key { get; set; } = 1;

    /** @property Name
        @brief
            Gets or sets the name of the test item.

        @details
            - Stores the descriptive name assigned to the test step.
            - Defaults to an empty string.

        @return
            Returns the name of the test item.
    */
    public string Name { get; set; } = string.Empty;

    /** @property Description
        @brief
            Gets or sets the optional description text of the test item.

        @details
            - Stores secondary detail lines or command metadata extracted from log outputs.
            - May be `null` if no description is present.

        @return
            Returns the description string, or `null` if unassigned.
    */
    public string? Description
        {
        get; set;
        }

    /** @property Stdout
        @brief
            Gets or sets the standard output log produced by the test item execution.

        @details
            - Stores execution standard output text when the result is not marked as FAILED.
            - May be `null` if no standard output was captured.

        @return
            Returns the standard output string, or `null` if unavailable.
    */
    public string? Stdout
        {
        get; set;
        }

    /** @property Stderr
        @brief
            Gets or sets the standard error log produced by the test item execution.

        @details
            - Stores execution error output text when the test outcome is FAILED.
            - May be `null` if no error output was captured.

        @return
            Returns the standard error string, or `null` if unavailable.
    */
    public string? Stderr
        {
        get; set;
        }

    /** @property Idx
        @brief
            Gets or sets the index number of the test item in the test run.

        @details
            - Tracks the relative layout position within the test suite execution sequence.
            - Restricts values to fit within valid `xsd:short` boundary limits (`0` to `32767`).

        @return
            Returns the sequence index value, or `null` if unassigned.

        @exception ArgumentOutOfRangeException
            Thrown when `value` is negative or exceeds `32767`.
    */
    public int? Idx
        {
        get;
        set
            {
            if( value.HasValue )
                {
                if( value.Value < theMinIndexLimit || value.Value > theMaxIndexLimit )
                    {
                    throw new ArgumentOutOfRangeException(nameof(value), value.Value, "The index must be between 0 and 32767.");
                    }
                }

            field = value;
            }
        }

    /** @property Result
        @brief
            Gets or sets the structural execution status outcome of the test item.

        @details
            - Guarantees valid mapping against target result state enumerations.
            - Encapsulates state normalization using the @ref Result context class.

        @return
            Returns the @ref Result instance associated with this test item.
    */
    public Result Result { get; set; } = new();

    /** @brief
        Initializes a new instance of the @ref TestItem class with default values.

    @details
        - Sets default key identifier to `1`.
        - Initializes default empty string for `Name` and a new default @ref Result instance.
    */
    public TestItem()
        {
        }

    /** @brief
        Initializes a new instance of the @ref TestItem class by parsing regular expression match groups.

    @details
        - Extracts index, name, description, output details, and outcome state from the regex match.
        - Routes middle execution output into `Stderr` if the result is `"FAIL"`, or into `Stdout` otherwise.
        - Parses index sequence values using culture-invariant integer conversion.

    @param[in] match
        Provides the regular expression match object containing target log groups.

    @exception ArgumentNullException
        Thrown when `match` is `null`.

    @exception FormatException
        Thrown when index group `1` cannot be parsed as an integer.

    @exception OverflowException
        Thrown when index group `1` represents a value outside integer ranges.

    @see Result
    */
    public TestItem( Match match )
        {
        ArgumentNullException.ThrowIfNull(match);
        if( !match.Success )
            {
            throw new ArgumentException("The provided match was not successful.", nameof(match));
            }
        if( match.Groups.Count < 6 )
            {
            throw new ArgumentException("The provided match does not contain the required number of groups.", nameof(match));
            }

        string result_raw = match.Groups[5].Value.Trim();
        string middle_string = match.Groups[4].Value.Trim();

        string? stdout_val = null;
        string? stderr_val = null;

        if( !string.IsNullOrEmpty(middle_string) )
            {
            if( string.Equals(result_raw, "FAIL", StringComparison.Ordinal) )
                {
                stderr_val = middle_string;
                }
            else
                {
                stdout_val = middle_string;
                }
            }

        TestItem_Key = 1;
        Idx = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        Name = match.Groups[2].Value.Trim();
        Description = match.Groups[3].Value.Trim();
        Result = new Result(result_raw);
        Stdout = stdout_val;
        Stderr = stderr_val;
        }
    }
