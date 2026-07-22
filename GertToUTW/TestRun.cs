/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       08.07.2026

    @author
        Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @brief
        Defines the context structure and property accessors representing a complete test run.

    @details
        - Encapsulates data spanning environmental properties, timestamps, target material metadata, and execution state.
        - Provides validation logic for material numbers, material revisions, and operational mode constraints.
        - Includes automatic production lot generation based on material and revision inputs.
        - Supports routing step and process handle extraction from test output logs.
        - Contains no shared mutable state.

    @defgroup REF_GertToUTWEngine_GertToUTW_TestRun TestRun
    @{
    @}
*/

using System.Globalization;
using System.Text.RegularExpressions;

namespace GertToUTW;

/** @ingroup REF_GertToUTWEngine_GertToUTW_TestRun
    @class TestRun
    @brief
        Main context class capturing structural tracking metrics for a completed test sequence.

    @details
        - Encapsulates test run metrics, serial numbers, hardware metadata, and operator attributes.
        - Validates input formats against XML schema constraints for material numbers and revisions.
        - Automates lot identification number calculation.
        - Contains no shared mutable state.

    @see TestItem
    @see SerialNumberAttributes
    @see Result
*/
public partial class TestRun
    {
    /** @brief Compiled regex validating non-zero positive integer sequence strings. */
    [GeneratedRegex(@"^\d*[1-9]+\d*$")]
    private static partial Regex MatNb();

    /** @brief Compiled regex validating numeric sequence strings up to 18 digits. */
    [GeneratedRegex(@"^\d{1,18}$")]
    private static partial Regex MatNb2();

    /** @brief Compiled regex validating 4-character uppercase alphanumeric revision identifiers. */
    [GeneratedRegex(@"^([A-Z0-9]{4})*$")]
    private static partial Regex MatRev();

    /** @brief Pattern to find and extract both the RouteStep and PHandle from LinkPHandle's SET outputs. */
    [GeneratedRegex(@"SET\s+\[LinkPHandle\][\s\S]*?SET\s+\[(?<routestep>[^\]]+)\][\s\S]*?SET\s+\[[^\]]+\][\s\S]*?SET\s+\[(?<phandle>[^\]]+)\]", RegexOptions.Singleline)]
    internal static partial Regex LinkPHandleRegex();

    /** @brief Set of valid operational mode tokens. */
    private static readonly HashSet<string> theValidOperatingModes =
    [
        "OPERATING",
        "ENGINEERING",
        "REPAIR",
        "DEVELOPMENT",
        "RMA"
    ];

    /** @property TestRun_Key
        @brief
            Gets or sets the unique key identifier for the test run context.

        @details
            - Defaults to `1`.

        @return
            Returns the key integer value.
    */
    public int TestRun_Key { get; set; } = 1;

    /** @property MaterialNumber
        @brief
            Gets or sets the structural material index number identification pattern.

        @details
            - Enforces pattern matching against `\d*[1-9]+\d*` or `\d{1,18}`.
            - Automatically triggers lot number recalculation when updated.

        @return
            Returns the material number string, or `null` if undefined.

        @exception ArgumentException
            Thrown when `value` does not conform to valid numeric material patterns.
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

            if( value != string.Empty && !MatNb().IsMatch(value) && !MatNb2().IsMatch(value) )
                {
                throw new ArgumentException("Material number must be non-empty and match numeric pattern constraints.", nameof(value));
                }

            field = value;
            try_auto_generate_lot();
            }
        } = string.Empty;

    /** @property MaterialText
        @brief
            Gets or sets the plain-text descriptive name of the material unit under test.

        @return
            Returns the material description string.
    */
    public string MaterialText { get; set; } = string.Empty;

    /** @property MaterialRevision
        @brief
            Gets or sets the production revision of the product.

        @details
            - Formatted identifiers are restricted to 4-character alphanumeric patterns matching `([A-Z0-9]{4})*`.
            - Automatically triggers lot number recalculation when updated.

        @return
            Returns the item revision string.

        @exception ArgumentException
            Thrown when `value` does not match valid 4-character alphanumeric patterns.
    */
    public string MaterialRevision
        {
        get;
        set
            {
            if( value != string.Empty && !MatRev().IsMatch(value) )
                {
                throw new ArgumentException("Material revision must match valid 4-character uppercase alphanumeric patterns.", nameof(value));
                }

            field = value;
            try_auto_generate_lot();
            }
        } = string.Empty;

    /** @property SerialNumber
        @brief
            Gets or sets the unique factory physical serial number value of the product.

        @return
            Returns the serial number string.
    */
    public string SerialNumber { get; set; } = string.Empty;

    /** @property OperatorName
        @brief
            Gets or sets the login credential name tracking the operator conducting the run.

        @return
            Returns the operator identity string.
    */
    public string OperatorName { get; set; } = string.Empty;

    /** @property ComputerName
        @brief
            Gets or sets the workstation host name executing the test sequence.

        @return
            Returns the client computer name string.
    */
    public string ComputerName { get; set; } = string.Empty;

    /** @property SequencerId
        @brief
            Gets or sets the engine identifier executing the test sequence.

        @return
            Returns the program ID token.
    */
    public string SequencerId { get; set; } = string.Empty;

    /** @property Result
        @brief
            Gets or sets the strict structural output outcome result tracking object.

        @return
            Returns the @ref Result context instance.
    */
    public Result Result { get; set; } = new();

    /** @property StartTime
        @brief
            Gets or sets the starting timestamp tracking test execution initialization.

        @return
            Returns the starting local @ref DateTime.
    */
    public DateTime StartTime
        {
        get; set;
        }

    /** @property EndTime
        @brief
            Gets or sets the ending timestamp tracking test sequence completion.

        @return
            Returns the ending local @ref DateTime.
    */
    public DateTime EndTime
        {
        get; set;
        }

    /** @property TestItem
        @brief
            Gets or sets the list container holding all child test steps.

        @return
            Returns a list of underlying @ref TestItem models.
    */
    public List<TestItem> TestItem { get; set; } = [];

    /** @property Station
        @brief
            Gets or sets the internal test station tracking identity node.

        @return
            Returns the facility station identifier, or `null` if unassigned.
    */
    public string? Station
        {
        get; set;
        }

    /** @property Routestep
        @brief
            Gets or sets the operational test process route step context string.

        @return
            Returns the process route stage descriptor, or `null` if unspecified.
    */
    public string? Routestep
        {
        get; set;
        }

    /** @property Lot
        @brief
            Gets the factory batch production lot allocation string.

        @details
            - Defaults to `"000000"`.
            - Managed internally via @ref generate_lot_number.

        @return
            Returns the 6-digit production lot identifier.
    */
    public string Lot { get; private set; } = "000000";

    /** @property Comment
        @brief
            Gets or sets an optional comment detailing general execution notes.

        @return
            Returns the comment string, or `null` if unassigned.
    */
    public string? Comment
        {
        get; set;
        }

    /** @property SerialNumberAttributes
        @brief
            Gets or sets custom property attributes associated with item serial configurations.

        @return
            Returns a list of @ref SerialNumberAttributes instances.
    */
    public List<SerialNumberAttributes> SerialNumberAttributes { get; set; } = [];

    /** @property DUTPosition
        @brief
            Gets or sets the physical slot index assignment value for the device under test.

        @return
            Returns the slot index location, or `null` if generic.
    */
    public int? DUTPosition { get; set; } = 1;

    /** @property SoftwareVersion
        @brief
            Gets or sets the application software version running the sequence.

        @return
            Returns the software version string identifier, or `null` if unspecified.
    */
    public string? SoftwareVersion { get; set; } = "0.0.0";

    /** @property OperatingSystem
        @brief
            Gets or sets the background runtime operating system environment context string.

        @return
            Returns the operating system string, or `null` if unspecified.
    */
    public string? OperatingSystem { get; set; } = "OS";

    /** @property OperatingMode
        @brief
            Gets or sets the execution mode configuration context string.

        @details
            - Validates input against accepted tokens: `OPERATING`, `ENGINEERING`, `REPAIR`, `DEVELOPMENT`, or `RMA`.
            - Converts input strings to uppercase invariant form.

        @return
            Returns the normalized operational mode string, or `null` if unassigned.

        @exception ArgumentException
            Thrown when `value` is not one of the allowed operational mode strings.
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
                throw new ArgumentException("OperatingMode must be one of: OPERATING, ENGINEERING, REPAIR, DEVELOPMENT, RMA.", nameof(value));
                }
            }
        } = "OPERATING";

    /** @property TestItems
        @brief
            Gets or sets the internal enumeration of test items.

        @return
            Returns an enumerable sequence of @ref TestItem instances.
    */
    public IEnumerable<TestItem> TestItems
        {
        get;
        internal set;
        } = [];

    /** @brief
        Automatically attempts to generate a lot number if required material attributes are set.

    @details
        - Swallows format and numerical overflow exceptions if material or revision inputs cannot be parsed.
    */
    private void try_auto_generate_lot()
        {
        try
            {
            Lot = generate_lot_number(MaterialNumber, MaterialRevision);
            }
        catch( FormatException )
            {
            }
        catch( ArgumentException )
            {
            }
        catch( OverflowException )
            {
            }
        }

    /** @brief
        Generates a 6-digit lot number from material number and hexadecimal revision strings.

    @details
        - Parses `material_number` using culture-invariant integer parsing.
        - Parses `revision_number` as a base-16 hexadecimal integer.
        - Combines parsed inputs modulo `1000000` formatted as a 6-digit zero-padded string.

    @param[in] material_number
        Provides the raw material number string.

    @param[in] revision_number
        Provides the hexadecimal revision string.

    @return
        Returns the generated 6-digit zero-padded lot number string.

    @exception ArgumentException
        Thrown when `material_number` or `revision_number` is `null` or empty.

    @exception FormatException
        Thrown when `material_number` or `revision_number` cannot be parsed into numeric form.

    @exception OverflowException
        Thrown when parsed numeric values exceed maximum bounds.
    */
    internal static string generate_lot_number( string? material_number, string? revision_number )
        {
        if( string.IsNullOrEmpty(material_number) || string.IsNullOrEmpty(revision_number) )
            {
            throw new ArgumentException("Both material number and revision number must be provided to generate a lot number.");
            }

        long mn = long.Parse(material_number, CultureInfo.InvariantCulture);
        int rn = Convert.ToInt32(revision_number, 16);
        string value = ((mn + rn) % 1000000).ToString("D6", CultureInfo.InvariantCulture);
        return value;
        }

    /** @brief
        Searches test step outputs for Link PHandle metadata and extracts route step and PHandle attributes.

    @details
        - Scans child @ref TestItem entries for names containing `"Link PHandle"`.
        - Uses regular expression parsing on stdout text to extract `Routestep` and `PHandle` entries.
        - Appends extracted `PHandle` entries to @ref SerialNumberAttributes when the step result is `"PASSED"`.

    @return
        Returns the current @ref TestRun instance.

    @see LinkPHandleRegex
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
            if( output != null )
                {
                Match match = LinkPHandleRegex().Match(output);
                if( match.Success )
                    {
                    Routestep = match.Groups["routestep"].Value;
                    if( string.Equals(res, "PASSED", StringComparison.Ordinal) )
                        {
                        SerialNumberAttributes.Add(new SerialNumberAttributes
                            {
                            SerialNumberAttributes_Key = 1,
                            Name = "PHandle",
                            Value = match.Groups["phandle"].Value
                            });
                        }
                    }
                }
            }

        return this;
        }
    }