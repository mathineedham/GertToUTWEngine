
using System.Runtime.CompilerServices;

/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       09.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @defgroup   REF_GertToUTWEngine_GertToUTW_Result  Result
    @{
    @ingroup    PROJ_GertToUTWEngine_GertToUTW

    @brief      Defines the Result context class ensuring strict target data compliance.

    @details    This file keeps track of final results and makes sure they match XML formatting rules before they are saved.
    @}
*/
[assembly: InternalsVisibleTo("RegressionTests")]
namespace GertToUTW;


/** @ingroup    REF_GertToUTWEngine_GertToUTW_Result

    @class      Result

    @brief      Encapsulates a normalized categorical result outcome token tracking global XML target schemas.*/
public class Result
    {
    /** @brief Private immutable state mapping directly against specific XSD enumerated configurations. */
    private static readonly HashSet<string> theValidResults =
    [
        "PASSED",
        "FAILED",
        "SKIPPED",
        "INCOMPLETE",
        "ERROR"
    ];
    /** @brief      Internal static key translation reference map for input abbreviations. */
    internal static readonly Dictionary<string, string> theResultRules = new(StringComparer.Ordinal)
    {
        { "PASS", "PASSED" },
        { "FAIL", "FAILED" },
        { "SKIP", "SKIPPED" },
        {"STOPPED", "INCOMPLETE"  }
    };

    /** @brief      The output outcome of testing.

        @details    Explicitly bounded states map directly against encapsulated validation configurations
                    namely: FAILED, PASSED, SKIPPED, INCOMPLETE, or ERROR state strings.

        @return     The matching categorical result outcome string state token.
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
                throw new ArgumentException($"Invalid Result '{value}'. Allowed options: {string.Join(", ", theValidResults)}.", nameof(value));
                }
            }
        } = string.Empty;
    /** @brief      Normalizes incoming status variations into standard validation strings.

    @param[in]  raw_result   The target unstructured log string status entry.

    @return     The corresponding upper-case tracking state outcome token.
    */
    internal static string map_result( string raw_result )
        {
        return theResultRules.TryGetValue(raw_result, out string? transformed) ? transformed : raw_result;
        }

    public Result(string value )
        {
        Value = map_result(value);
        }

    public Result()
        {
        }
    }