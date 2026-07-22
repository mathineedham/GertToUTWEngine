/** @file

    @copyright  &copy; 2026, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       10.07.2026

    @author
        Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @brief
        Provides methods to generate UTW-compliant XML files from testrun data.

    @details
        - Responsible for generating UTW-compliant XML files from parsed test run data.
        - Transforms a `TestRun` instance into an XML document adhering to UTW schema specifications.
        - Provides timestamp formatting and node construction routines.
        - Contains no shared mutable state.

    @defgroup REF_GertToUTWEngine_GertToUTW_UtwXmlGenerator UtwXmlGenerator
    @{
    @}
*/

using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace GertToUTW;

/** @ingroup REF_GertToUTWEngine_GertToUTW_UtwXmlGenerator
    @class UtwXmlGenerator
    @brief
        UTW compliant XML files generator.

    @details
        - Creates UTW-compliant XML files from parsed test run data.
        - Formats execution timelines, builds structured XML element trees, and saves documents to disk.
        - Contains no shared mutable state.

    @see TestRun
    @see TestItem
    @see SerialNumberAttributes
*/
public static partial class UtwXmlGenerator
    {
    /** @brief
        Converts a DateTime instance to an ISO-8601 formatted timestamp string with offset.

    @param[in] dt_obj
        The source @ref DateTime value to format.

    @return
        Returns the formatted timestamp string.
    */
    internal static string format_time( DateTime dt_obj )
        {
        return dt_obj.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
        }

    /** @brief
        Creates a UTW-compliant XML file from a TestRun instance and saves it to the specified output path.

    @details
        - Validates that output filepath is non-empty.
        - Serializes the generated @ref XDocument to disk with UTF-8 encoding.

    @param[in] test_run_instance
        The @ref TestRun instance containing test run context data.

    @param[in] output_filepath
        The target system file path where the generated XML will be written.

    @exception ArgumentNullException
        Thrown when `test_run_instance` is `null`.

    @exception ArgumentException
        Thrown when `output_filepath` is `null` or empty.
    */
    public static void GenerateUtwXml( TestRun test_run_instance, string output_filepath )
        {
        ArgumentNullException.ThrowIfNull(test_run_instance);
        ArgumentException.ThrowIfNullOrEmpty(output_filepath);

        XDocument document = build_utw_xml_document(test_run_instance);
        using StreamWriter writer = new(output_filepath, false, Encoding.UTF8);
        document.Save(writer, SaveOptions.None);
        }

    /** @brief
        Creates a UTW-compliant XML document from a TestRun instance.

    @details
        - Constructs header elements including material details, serial numbers, hardware info, and results.
        - Appends child `SerialNumberAttributes` and `TestItem` nodes to the root element.

    @param[in] test_run_instance
        The @ref TestRun instance containing test run data.

    @return
        Returns an @ref XDocument representing the UTW-compliant XML structure.

    @exception ArgumentNullException
        Thrown when `test_run_instance` is `null`.
    */
    internal static XDocument build_utw_xml_document( TestRun test_run_instance )
        {
        ArgumentNullException.ThrowIfNull(test_run_instance);

        // Root element with header information
        XElement root = new("TestRun",
            new XElement("TestRun_Key", "01"),
            new XElement("MaterialNumber", test_run_instance.MaterialNumber),
            new XElement("MaterialText", test_run_instance.MaterialText),
            new XElement("MaterialRevision", test_run_instance.MaterialRevision),
            new XElement("Lot", test_run_instance.Lot),
            new XElement("Routestep", test_run_instance.Routestep),
            new XElement("Comment", test_run_instance.Comment),
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
            new XElement("DUTPosition", test_run_instance.DUTPosition?.ToString("D2",CultureInfo.InvariantCulture) ?? "1"),
            new XElement("OperatorName", test_run_instance.OperatorName),
            new XElement("SoftwareVersion", test_run_instance.SoftwareVersion ?? "0.0.0"),
            new XElement("ComputerName", test_run_instance.ComputerName),
            new XElement("OperatingSystem", test_run_instance.OperatingSystem ?? "OS"),
            new XElement("OperatingMode", test_run_instance.OperatingMode ?? "OPERATING"),
            new XElement("SequencerId", test_run_instance.SequencerId),
            new XElement("Result", test_run_instance.Result.Value),
            new XElement("StartTime", format_time(test_run_instance.StartTime)),
            new XElement("EndTime", format_time(test_run_instance.EndTime))
        );

        // Add all Test items
        foreach( TestItem item in test_run_instance.TestItem )
            {
            
            root.Add(create_test_item_node(item));
            }

        // Create the XDocument and return it
        return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        }

    internal static XElement create_test_item_node( TestItem item )
        {
        XElement item_node = new("TestItem",
                new XElement("TestItem_Key", "01"),
                new XElement("Idx", item.Idx?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                new XElement("Result", item.Result?.Value),
                new XElement("Name", item.Name)
            );

        item_node.AddIfNotEmpty("Description", item.Description);
        item_node.AddIfNotEmpty("Stdout", item.Stdout);
        item_node.AddIfNotEmpty("Stderr", item.Stderr);

        return item_node;
        }
    }