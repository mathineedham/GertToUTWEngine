/** @file       UtwXmlGenerator.cs

    @copyright  &copy; 2026, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       10.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)
 
    @defgroup   REF_GertToUTWEngine_GertToUTW_UtwXmlGenerator  UtwXmlGenerator
    @{
    @ingroup    REF_GertToUTWEngine_GertToUTW

    @brief      Provides methods to generate UTW-compliant XML files from testrun data.

    @details    The `UtwXmlGenerator` class is responsible for generating UTW-compliant XML files
                from the parsed test run data. It takes a `TestRun` instance and an output file path,
                and creates an XML document that adheres to the UTW schema.
    @}
*/
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

[assembly: InternalsVisibleTo("RegressionTests")]
namespace GertToUTW;

/** @ingroup    REF_GertToUTWEngine_GertToUTW_UtwXmlGenerator

    @class      UtwXmlGenerator

    @brief      UTW compliant XML files generator .

    @details    Creates UTW-compliant XML files from parsed test run data. It provides methods to format time, build XML documents, and save them to specified file paths.
*/
public static partial class UtwXmlGenerator
    {
    /** @brief Converts our DateTime object to the current string format.*/
    internal static string format_time( DateTime dt_obj )
        {
        return dt_obj.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "+02:00";
        }

    /** @brief      Creates a UTW-compliant XML file from a TestRun instance and saves it to the specified output path.

        @param[in]  test_run_instance  The TestRun instance containing the test run data.
        @param[in]  output_file_path    The path where the generated XML file will be saved.
    */
    public static void GenerateUtwXml( TestRun test_run_instance, string output_filepath )
        {
        ArgumentException.ThrowIfNullOrEmpty(output_filepath);

        XDocument document = build_utw_xml_document(test_run_instance);
        using StreamWriter writer = new(output_filepath, false, Encoding.UTF8);
        document.Save(writer, SaveOptions.None);
        }

    /**@brief      Generates lot number from material number and revision number.
     * @param[in]  string     The material number.
     * @param[in]  string     The revision number, in hexadecimal format.
     * @return     string     The generated lot number.
     */
    internal static string generate_lot_number( string? material_number, string? revision_number )
        {
        if( material_number == null || revision_number == null )
            {
            throw new ArgumentNullException(material_number,revision_number);
            }
        int mn = int.Parse(material_number, CultureInfo.InvariantCulture);
        int rn = Convert.ToInt32(revision_number,16);
        return (mn + rn).ToString(CultureInfo.InvariantCulture);

        }

    /** @brief      Creates a UTW-compliant XML document from a TestRun instance.

        @param[in]  test_run_instance  The TestRun instance containing the test run data.

        @return     An XDocument representing the UTW-compliant XML document.
    */
    internal static XDocument build_utw_xml_document( TestRun test_run_instance )
        {
        ArgumentNullException.ThrowIfNull(test_run_instance);
        // lot number = material number + revision number where revision number is converted from hex to decimal
        string lot_number = generate_lot_number(test_run_instance.MaterialNumber, test_run_instance.MaterialRevision);
        // Root element with header information
        XElement root = new("TestRun",
            new XElement("TestRun_Key", "01"),
            new XElement("MaterialNumber", test_run_instance.MaterialNumber),
            new XElement("MaterialText", test_run_instance.MaterialText),
            new XElement("MaterialRevision", test_run_instance.MaterialRevision),
            new XElement("Lot",lot_number), 
            new XElement("SerialNumber", test_run_instance.SerialNumber)
        );
        // Add any SerialNumber Attributes
        foreach( SerialNumberAttributes sna in test_run_instance.SerialNumberAttributes )
            {
            root.Add(new XElement("SerialNumberAttributes",
                new XAttribute("SerialNumberAttributes_Key", "1"),
                new XAttribute("Name", sna.Name),
                new XAttribute("Value", sna.Value)
            ));
            }

        // Add other header information
        root.Add(
            new XElement("DUTPosition", "01"),
            new XElement("OperatorName", test_run_instance.OperatorName),
            new XElement("SoftwareVersion", "0.0.0"),
            new XElement("ComputerName", test_run_instance.ComputerName),
            new XElement("OperatingSystem", "OS"),
            new XElement("OperatingMode", "OPERATING"),
            new XElement("SequencerId", test_run_instance.SequencerId),
            new XElement("Result", test_run_instance.Result.Value), // Note: Make sure to pull the inner text value if Result is an object!
            new XElement("StartTime", format_time(test_run_instance.StartTime)),
            new XElement("EndTime", format_time(test_run_instance.EndTime))
        );

        //Add all Test items
        foreach( TestItem item in test_run_instance.TestItem )
            {
            XElement item_node = new("TestItem",
                new XElement("TestItem_Key", "01"),
                new XElement("Idx", item.Idx?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                new XElement("Result", item.Result?.Value), // Pull inner value if this is an object
                new XElement("Name", item.Name)
            );

            item_node.AddIfNotEmpty("Description", item.Description);
            item_node.AddIfNotEmpty("Stdout", item.Stdout);
            item_node.AddIfNotEmpty("Stderr", item.Stderr);

            item_node.Add(new XElement("StartTime", format_time(test_run_instance.StartTime)));
            root.Add(item_node);
            }

        // Create the XDocument and return it
        return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        }
    }