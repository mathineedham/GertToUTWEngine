/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       08.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @defgroup   REF_GertToUTWEngine_GertToUTW_TestRun   TestRun 
    @{
    @ingroup    PROJ_GertToUTWEngine_GertToUTW

    @brief      Defines data structures representing serial number attributes and a test run .

    @details    This file includes metadata models used to serialize and manage properties 
                associated with individual test sequence executions, hardware metadata, 
                and regulatory data boundary compliance tracking.
    @}
*/
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("RegressionTests")]
namespace GertToUTW;

/** @ingroup    REF_GertToUTWEngine_GertToUTW_TestRun

    @class      TestRun

    @brief      Main context class capturing structural tracking metrics for a completed test sequence.

    @details    This class encapsulates data spanning environmental properties, time logs, target hardware 
                serial designations, operator footprints, structural collections of executed sub-items, and strict validation 
                bounds for string formatting patterns matching expected global XML target schemas.
*/
public partial class TestRun
    {

    [GeneratedRegex(@"^\d*[1-9]+\d*$")]
    private static partial Regex MatNb();
    [GeneratedRegex(@"^\d{1,18}$")]
    private static partial Regex MatNb2();
    [GeneratedRegex(@"^([A-Z0-9]{4})*$")]
    private static partial Regex MatRev();
    /** @brief Pattern to find and extract both the RouteStep and PHandle from LinkPHandle's SET outputs */
    [GeneratedRegex(@"SET\s+\[LinkPHandle\][\s\S]*?SET\s+\[(?<routestep>[^\]]+)\][\s\S]*?SET\s+\[[^\]]+\][\s\S]*?SET\s+\[(?<phandle>[^\]]+)\]", RegexOptions.Singleline)]
    internal static partial Regex LinkPHandleRegex();

    private static readonly HashSet<string> theValidOperatingModes =
    [
        "OPERATING",
        "ENGINEERING",
        "REPAIR",
        "DEVELOPMENT",
        "RMA"
    ];


    /** @brief      key identifier 

        @return     The key value.
    */
    public int TestRun_Key { get; set; } = 1;

    /** @brief      The structural material index number identification pattern.

        @details    Enforces patterns for XML schema definitions. 
                    either `\d*[1-9]+\d*` or `\d{1,18}` 

        @return     The string value of the material number, or null if undefined.
    */
    public string? MaterialNumber
        {
        get;
        set
            {
            if( value == null )
                {
                field = null;
                return;
                }
            if( value != string.Empty &&
                !MatNb().IsMatch(value) &&
                !MatNb2().IsMatch(value) )
                {
                throw new ArgumentException(null, nameof(value));
                }

            field = value;
            try_auto_generate_lot();
            }
        } = string.Empty;

    /** @brief      Plain-text naming description of the material unit under test.

        @return     The descriptive string text for the material.
    */
    public string MaterialText { get; set; } = string.Empty;

    /** @brief      The production revision of the product.

        @details    Formatted identifiers are strictly configured to structure patterns like `([A-Z0-9]{4})*`.

        @return     The item revision string.
    */
    public string MaterialRevision
        {
        get;
        set
            {
            if( value != string.Empty &&
                !MatRev().IsMatch(value)  )
                {
                throw new ArgumentException(null, nameof(value));
                }

            field = value;
            try_auto_generate_lot();
            }
        } = string.Empty;

    /** @brief      The unique factory physical serial number value of the product.

        @return     The hardware identity serial number string.
    */
    public string SerialNumber { get; set; } = string.Empty;

    /** @brief      The login credential name tracking the human operator conducting the run.

        @return     The operator configuration identity string.
    */
    public string OperatorName { get; set; } = string.Empty;

    /** @brief      The structural machine network node or workstation name executing the sequence.

        @return     The physical client workstation name string.
    */
    public string ComputerName { get; set; } = string.Empty;

    /** @brief      The engine identifier (UTW/GERT) executing the test sequence.
     * 
        @return     The sequencer program ID token.
    */
    public string SequencerId { get; set; } = string.Empty;

    /** @brief      The strict structural output outcome result tracking tag.

        @details    Explicitly bounded states map directly against specific enumerated configurations 
                    namely: FAILED, PASSED, SKIPPED, INCOMPLETE, or ERROR state strings.

        @return     The matching categorical result outcome string state token.
    */
    public Result Result { get; set; } = new();

    /** @brief      The absolute localized starting timestamp tracking the test execution initialization.

        @details    Formatted to comply sequentially with serialization constraints governing `xsd:dateTime`.

        @return     The starting timeline event timestamp.
    */
    public DateTime StartTime
        {
        get; set;
        }

    /** @brief      The absolute localized end timestamp tracking the completed test execution run conclusion.

        @details    Formatted to comply sequentially with serialization constraints governing `xsd:dateTime`.

        @return     The ending timeline event timestamp.
    */
    public DateTime EndTime
        {
        get; set;
        }

    /** @brief      The structured relational list container holding all test steps/items.

        @return     An iterable list of underlying child `TestItem` validation models.
    */
    public List<TestItem> TestItem { get; set; } = [];

    /** @brief      The internal test station tracking identity node.

        @return     The testing physical facility name string context, or null if generic.
    */
    public string? Station
        {
        get; set;
        }

    /** @brief      The exact operational test process step context string identifier.

        @return     The process route stage name descriptor tracking string, or null if unspecified.
    */
    public string? Routestep
        {
        get; set;
        }

    /** @brief      The factory batch production lot allocation context mask.

        @details    Formatted values must fit structural validation tracking layouts matching `([0-9]{6,7})*`.

        @return     The production lot id string.
    */
    public string Lot
        {
        get;
        private set;
        } = "000000";

    /** @brief      An optional open-ended string comment detailing general aspects of this test execution run.

        @return     The comment description text, or null if unassigned.
    */
    public string? Comment { get; set; }

    /** @brief      The custom key-value property tracking collection mapping unique attributes for item serial configurations.

        @return     An iterable array tracking specific custom variable entries.
    */
    public List<SerialNumberAttributes> SerialNumberAttributes { get; set; } = [];

    /** @brief      The individual physical slot or device under test layout station index assignment value.

        @details    Structural parameters must fall into bounds that are convertible to a valid standard `xsd:short` format integer.

        @return     The numerical physical slot layout index location, or null if generic.
    */
    public int? DUTPosition { get; set; } = 1;

    /** @brief      The application software version tag context running the sequence.

        @details    Follows structured format validation boundaries matching standard sequence masks: 
                    `[0-9]{1,2}.[0-9]{1,2}(.[0-9]{1,2})*(.[0-9]{1,4})*`.

        @return     The system application software version string identifier, or null if generic.
    */
    public string? SoftwareVersion { get; set; } = "0.0.0";

    /** @brief      The background runtime operating system environment infrastructure details tag context.

        @return     The platform environment text identification string, or null if generalized.
    */
    public string? OperatingSystem { get; set; } = "OS";

    /** @brief      The execution environment state configuration mode context string token.

        @details    The value assigned must fit explicitly matching options: `OPERATING`, `ENGINEERING`, 
                    `REPAIR`, `DEVELOPMENT`, or `RMA`.

        @return     The structural operational execution state type tracking token string, or null if generic.
    */
    public string? OperatingMode
        {
        get;
        set
            {
            if( value == null )
                {
                field = null;
                return;
                }

            string upper_value = value.ToUpperInvariant();
            if( theValidOperatingModes.Contains(upper_value) )
                {
                field = upper_value;
                }
            else
                {
                throw new ArgumentException(null, nameof(value));
                }
            }
        } = "OPERATING";

    /** @brief Automatically generates a lot number if the current lot is empty and the material revision is set. */
    private void try_auto_generate_lot()
        {
        try
            {
            // lot number = material number + revision number where revision number is converted from hex to decimal
            Lot = generate_lot_number(MaterialNumber, MaterialRevision);
            }
        catch( FormatException )
            {}
        catch( ArgumentException )
            {}
        catch( OverflowException )
            {}
        }

    /**@brief      Generates 6 digit lot number from material number and revision number.
     * @param[in]  string     The material number.
     * @param[in]  string     The revision number, in hexadecimal format.
     * @return     string     The generated lot number.
     */
    internal static string generate_lot_number( string? material_number, string? revision_number )
        {
        if( string.IsNullOrEmpty(material_number) || string.IsNullOrEmpty(revision_number) )
            {
            throw new ArgumentException(material_number, revision_number);
            }
        int mn = int.Parse(material_number, CultureInfo.InvariantCulture);
        int rn = Convert.ToInt32(revision_number, 16);
        string value = ((mn + rn) % 1000000).ToString("D6", CultureInfo.InvariantCulture);
        return value;

        }

    /** @brief Function that finds the test item with "Write QS-Ticket" in its name, and returns the routerstep
     *  @param[in]  steps  The list of test steps to search through.
     *  @return    routestep or our test run or Empty string if not found.
     */
    public TestRun Find_link_phandle_step()
        {
        TestItem? linkstep = null;
        foreach( TestItem step in TestItem )
            {
            if( step.Name.Contains("Link PHandle", StringComparison.OrdinalIgnoreCase) )
                {
                linkstep = step;
                break;
                }
            }
        if( linkstep != null )
            {
            string? output = linkstep.Stdout;
            string res = linkstep.Result.Value;
            if( output != null)
                {
                Match match = LinkPHandleRegex().Match(output);
                if( match.Success )
                    {
                    Routestep = match.Groups["routestep"].Value;
                    if( res == "PASSED")
                        {
                        SerialNumberAttributes.Add(new SerialNumberAttributes { SerialNumberAttributes_Key = 1, Name = "PHandle", Value = match.Groups["phandle"].Value });
                        }
                    }
                }
            }
        return this;
        }
    }