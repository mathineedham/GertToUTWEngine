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
using System.Text.RegularExpressions;
using System.Xml.Linq;

[assembly: InternalsVisibleTo("RegressionTest")]
namespace GertToUTW;

/** @ingroup    REF_GertToUTWEngine_GertToUTW_UtwXmlGenerator

    @class      UtwXmlGenerator

    @brief      UTW compliant XML files generator .

    @details    Creates
*/
public static partial class UtwXmlGenerator
    {
    [GeneratedRegex(@"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]")]
    internal static partial Regex invalid_xml_chars_regex(); 
    internal static string sanitize_for_xml( string? input )
        {
        if( string.IsNullOrEmpty(input) )
            {
            return string.Empty;
            }

        // Strips out 0x1B and other breaking control characters
        return invalid_xml_chars_regex().Replace(input, string.Empty);
        }
    public static void GenerateUtwXml( TestRun test_run_instance, string output_filepath )
        {
        ArgumentNullException.ThrowIfNull(output_filepath);

        XDocument document = build_utw_xml_document(test_run_instance);
        using StreamWriter writer = new(output_filepath, false, Encoding.UTF8);
        document.Save(writer, SaveOptions.None);
        }
    internal static XDocument build_utw_xml_document( TestRun test_run_instance )
        {
        ArgumentNullException.ThrowIfNull(test_run_instance);

        // Root element with header information
        XElement root = new("TestRun",
            new XElement("TestRun_Key", "01"),
            new XElement("MaterialNumber", test_run_instance.MaterialNumber),
            new XElement("MaterialText", test_run_instance.MaterialText),
            new XElement("MaterialRevision", test_run_instance.MaterialRevision),
            new XElement("Lot", test_run_instance.MaterialNumber), // MATERIAL NUMBER AS LOT NUMBER
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
            new XElement("Result", test_run_instance.Result?.Value), // Note: Make sure to pull the inner text value if Result is an object!
            new XElement("StartTime", format_iso_time(test_run_instance.StartTime)),
            new XElement("EndTime", format_iso_time(test_run_instance.EndTime))
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

            if( !string.IsNullOrEmpty(item.Description) )
                {
                item_node.Add(new XElement("Description", sanitize_for_xml(item.Description)));
                }

            if( !string.IsNullOrEmpty(item.Stdout) )
                {
                item_node.Add(new XElement("Stdout", new XCData(sanitize_for_xml(item.Stdout))));
                }

            if( !string.IsNullOrEmpty(item.Stderr) )
                {
                // Cleaned Up: Swapped to sanitize_for_xml here too to match your Stdout logic!
                item_node.Add(new XElement("Stderr", new XCData(sanitize_for_xml(item.Stderr))));
                }

            item_node.Add(new XElement("StartTime", format_iso_time(test_run_instance.StartTime)));
            root.Add(item_node);
            }

        // Create the XDocument and return it
        return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        }
   
    internal static string format_iso_time( DateTime dt_obj )
        {
        return dt_obj.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "+02:00";
        }
    }