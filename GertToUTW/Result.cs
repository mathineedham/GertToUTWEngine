/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       09.07.2026

    @author
        Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @brief
        Defines the Result context class ensuring strict target data compliance.

    @details
        - Tracks final test result tokens and ensures compliance with global XML target schemas.
        - Provides case-insensitive validation against documented target schema result strings.
        - Translates shorthand result abbreviations into canonical uppercase outcome strings.
        - Contains no shared mutable state.

    @defgroup REF_GertToUTWEngine_GertToUTW_Result Result
    @{
    @}
*/

namespace GertToUTW;

/** @ingroup REF_GertToUTWEngine_GertToUTW_Result
    @class Result
    @brief
        Encapsulates a normalized categorical result outcome token tracking global XML target schemas.

    @details
        - Enforces strict validation against permitted result states: PASSED, FAILED, SKIPPED, INCOMPLETE, ERROR.
        - Normalizes input status variations into standard uppercase state outcome tokens.
        - Ensures safe fallback handling for null inputs.
        - Contains no shared mutable state.

    @see Result.Value
*/
public class Result
    {
    /** @brief Private immutable set mapping directly against specific XSD enumerated configurations. */
    private static readonly HashSet<string> theValidResults =
    [
        "PASSED",
        "FAILED",
        "SKIPPED",
        "INCOMPLETE",
        "ERROR"
    ];

    /** @brief Internal static key translation reference map for input abbreviations. */
    internal static readonly Dictionary<string, string> theResultRules = new(StringComparer.Ordinal)
    {
        { "PASS", "PASSED" },
        { "FAIL", "FAILED" },
        { "SKIP", "SKIPPED" },
        { "STOPPED", "INCOMPLETE" }
    };

    /** @property Value
        @brief
            Gets or sets the output outcome of testing.

        @details
            - Bounded state tokens map directly against encapsulated validation configurations.
            - Converts null inputs to an empty string.
            - Performs case-insensitive matching against the allowed result options.

        @return
            Returns the matching categorical result outcome string state token.

        @exception ArgumentException
            Thrown when `value` is not recognized as a valid result option.
    */
    public string Value
        {
        get;
        set
            {
            if( value == null )
                {
                field = string.Empty;
                return;
                }

            string upper_value = value.ToUpperInvariant();
            if( theValidResults.Contains(upper_value) )
                {
                field = upper_value;
                }
            else
                {
                throw new ArgumentException("Invalid Result '" + value + "'. Allowed options: PASSED, FAILED, SKIPPED, INCOMPLETE, ERROR.", nameof(value));
                }
            }
        } = string.Empty;

    /** @brief
        Normalizes incoming status variations into standard validation strings.

    @details
        - Translates defined shorthand abbreviations into standard state result tokens.
        - Passes unmapped raw result strings through unchanged.

    @param[in] raw_result
        Provides the raw status entry string to map.

    @return
        Returns the corresponding uppercase state outcome token, or the original value if unmapped.
    */
    internal static string map_result( string raw_result )
        {
        if( raw_result != null && theResultRules.TryGetValue(raw_result, out string? transformed) )
            {
            return transformed;
            }

        return raw_result!;
        }

    /** @brief
        Initializes a new instance of the @ref Result class with a specified initial value.

    @details
        - Maps incoming shorthand input variations using internal translation rules.
        - Validates and assigns the normalized result value.

    @param[in] value
        Provides the raw or shorthand result string.

    @exception ArgumentException
        Thrown when the mapped `value` does not match an allowed result token.
    */
    public Result( string value )
        {
        Value = map_result(value);
        }

    /** @brief
        Initializes a new instance of the @ref Result class with default empty state.

    @details
        - Sets the initial `Value` property to an empty string.
    */
    public Result()
        {
        }
    }