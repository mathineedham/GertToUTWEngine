/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       08.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @defgroup   REF_GertToUTWEngine_GertToUTW_TestItem   TestItem 
    @{
    @ingroup    PROJ_GertToUTWEngine_GertToUTW

    @brief      Represents a test item

    @details    The `TestItem` class is used to handle and validate the attributes associated with an executed test. 
                It allows for managing key identifiers, checking boundary constraints for individual indexes, 
                and validating matching XSD result states.
    @}
*/
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RegressionTests")]
namespace GertToUTW;

/** @ingroup    PROJ_GertToUTWEngine_GertToUTW_TestItem

    @class     TestItem

    @brief      Represents a single test step in a test run.

    @details    This class handles the initialization, properties, and structural validation constraints of a test item.
                It enforces strict range rules for sequence indices and strict string matching boundaries for 
                the XML Schema definition result enumerations.
*/
public class TestItem
{
    internal static readonly int theMinIndexLimit;
    internal static readonly int theMaxIndexLimit = 32767;
    /** @brief      The unique database or configuration key identifier for the test item.

        @return     The key of the test item.
    */
    public int TestItem_Key { get; set; } =1;

    /** @brief      The name of the test item.

        @details    The `Name` property represents the name assigned to the test. It defaults to an empty string.

        @return     The name of the test item.
    */
    public string Name { get; set; } = string.Empty;

    /** @brief      The optional description text of the test item.
      
        @details    In the case of transforming test logs from GERT to UTW, the description holds the second line of a test step log, which often contains more information on the command

        @return     The description string, or null if no description is provided.
    */
    public string? Description { get; set; }

    /** @brief      The standard output log produced by the test item execution.

        @details    If the result of a test item is not FAILED all output from the test item is stored in Stdout

        @return     The standard output stream contents, or null if unavailable.
    */
    public string? Stdout { get; set; }

    /** @brief      The standard error log produced by the test item execution.
     
        @details    If the result of a test item is FAILED all output from the test item is stored in Stderr

        @return     The standard error stream contents, or null if unavailable.
    */
    public string? Stderr { get; set; }

    /** @brief      The index number of the tet item in the test run

        @details    The `Idx` property tracks the relative layout position. It enforces a structural constraint 
                    restricting values to fit strictly within valid xsd:short bounds, so that it will fulfill the xml structure.

        @throw      ArgumentOutOfRangeException if the value is negative or exceeds 32767.

        @return     The numerical sequence short index value, or null if unassigned.
    */
    public int? Idx
        {
        get;
        set
            {
            if( value < theMinIndexLimit || ( value > theMaxIndexLimit ))
                {
                throw new ArgumentOutOfRangeException(nameof(value));
                }
            field = value;
            }
        }

    /** @brief      The structural execution status outcome of the test item.

        @details    The `Result` property guarantees a strict matching against the XSD result enumeration strings. 
                    Assigned strings are transformed and stored normalized into upper-case invariants.

        @throw      ArgumentException if the assigned string format fails to match PASSED, FAILED, SKIPPED, INCOMPLETE, or ERROR.

        @return     The uppercase normalized result state string.
    */
    public Result Result { get; set; } = new();
    }