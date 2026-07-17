/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       15.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @defgroup   REF_GertToUTWEngine_RegressionTest_GertToUTW_ApplicationTest   ApplicationTest
    @{
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW

    @brief      Regression tests for the `Application` class 

    @details    Validates the behavior of the `Application` class, including constructor validation
                for input and output file paths, and expected exception handling in error scenarios.
    @}
*/
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

using GertToUTW;
namespace RegressionTests.GertToUTW;

/** @class      ApplicationTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_ApplicationTest

    @brief      Unit tests for the `Application` class.

    @details    Validates the behavior of the `Application` class, including constructor validation
                for input and output file paths, and expected exception handling in error scenarios.
*/
[TestClass]
public class ApplicationTest
    {
    private static readonly string theBaseFilesDir = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string theXsdFilePath = Path.Combine(theBaseFilesDir, "GertToUTW\\XmlTestFiles\\Structure\\machine-readable-logs.xsd");
  
    /** @brief  Generates all the .xml files whose structure we will be testing against the XSD schema */
    [ClassInitialize]
    public static void Init( TestContext test_context )
        {
        string absolute_input_file1 = Path.Combine(theBaseFilesDir, "GertToUTW\\LogTestFiles\\Valid\\valid_singlerun.log");
        string absolute_output_dir1 = Path.Combine(theBaseFilesDir, "GertToUTW\\XmlTestFiles\\Generated");
        Application app_valid_singlerun = new(absolute_input_file1, absolute_output_dir1);
        _ = app_valid_singlerun.Execute(); // contains "Expected\\valid_singlerun_0.xml"
        string absolute_input_file2 = Path.Combine(theBaseFilesDir, "GertToUTW\\LogTestFiles\\Valid\\valid_doublerun.log");
        string absolute_output_dir2 = Path.Combine(theBaseFilesDir, "GertToUTW\\XmlTestFiles\\Generated");
        Application app_valid_doublerun = new(absolute_input_file2, absolute_output_dir2);
        _ = app_valid_doublerun.Execute(); // contains "Expected\\valid_doublerun_0.xml" and "Expected\\valid_doublerun_1.xml"
        }


    /** @brief  Validates that the constrcutor throws an ArgumentException when provided with an invalid file paths */
    [TestMethod]
    [DataRow("", "")]
    [DataRow("GertToUTW\\XmlTestFiles\\Structure\\machine-readable-logs.xsd", "output")] //input must be .log
    [DataRow("GertToUTW\\XmlTestFiles\\LogTestFiles\\nonexistent.log", "output")] //input must exists
    public void Application_Invalid( string input, string output )
        {
        string absolute_input_file = Path.Combine(theBaseFilesDir, input);
        string absolute_output_dir = Path.Combine(theBaseFilesDir, output);

        _ = Assert.ThrowsExactly<ArgumentException>(() => new Application(absolute_input_file, absolute_output_dir));
        }

    /** @brief  Validates Application constructor with valid input and output file paths even if non existent */
    [TestMethod]
    [DataRow("GertToUTW\\LogTestFiles\\Valid\\valid_doublerun.log",
             "GertToUTW\\XmlTestFiles\\Generated")]
    public void Application_Valid( string input, string output )
        {
        Application app = new(input, output);
        Assert.AreEqual(input, app.Input_log_path);
        Assert.AreEqual(output, app.Output_xml_dir);
        }

    /** @brief Validates the XML file against the XSD schema and asserts that there are no validation errors. */
    [TestMethod]
    [DataRow("GertToUTW\\XmlTestFiles\\Generated\\valid_singlerun_0.xml")]
    [DataRow("GertToUTW\\XmlTestFiles\\Generated\\valid_doublerun_0.xml")]
    [DataRow("GertToUTW\\XmlTestFiles\\Generated\\valid_doublerun_1.xml")]
    public void Valid_Xsd( string xml_file )
        {
        string absolute_xml = Path.Combine(theBaseFilesDir, xml_file);
        List<string> validation_errors = [];
        XmlReaderSettings settings = new()
            {
            ValidationType = ValidationType.Schema
            };
        settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
        settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

        settings.ValidationEventHandler += ( sender, args ) =>
        {
            string severity = args.Severity == XmlSeverityType.Error ? "Error" : "Warning";
            validation_errors.Add($"[{severity}] Line {args.Exception.LineNumber}, Col {args.Exception.LinePosition}: {args.Message}");
        };

        _ = settings.Schemas.Add(targetNamespace: null, schemaUri: theXsdFilePath);

        using( XmlReader reader = XmlReader.Create(absolute_xml, settings) )
            {
            while( reader.Read() )
                {
                }
            }
        if( validation_errors.Count > 0 )
            {
            string combined_errors = string.Join("\n", validation_errors);
            Assert.Fail($"XML Validation Failed with {validation_errors.Count} error(s):\n{combined_errors}");
            }
        }

    /** @brief  Validates that appliucation correctly generated an xml file as expected */
    [TestMethod]
    [DataRow("GertToUTW\\LogTestFiles\\Valid\\valid_singlerun.log",
             "GertToUTW\\XmlTestFiles\\Generated\\valid_singlerun_0.xml",
             "GertToUTW\\XmlTestFiles\\Expected\\valid_singlerun.xml")]
    public void Application_Valid_ExistingFiles( string input_relative_path, string output_relative_path, string expected_relative_path )
        {
        // make them absolute paths
        string absolute_out = Path.Combine(theBaseFilesDir, output_relative_path);
        string absolute_in = Path.Combine(theBaseFilesDir, input_relative_path);
        string absolute_expected = Path.Combine(theBaseFilesDir, expected_relative_path);

        // Read the generated output and expected output
        string generated_content = File.ReadAllText(absolute_out);
        XDocument generated_output = XDocument.Parse(generated_content, LoadOptions.PreserveWhitespace);
        string expected_content = File.ReadAllText(absolute_expected);
        XDocument expected_output = XDocument.Parse(expected_content, LoadOptions.PreserveWhitespace);

        Assert.IsTrue(XNode.DeepEquals(generated_output, expected_output), "Generated XML does not match expected XML.");
        // normalize the XML for comparison
        generated_output.Descendants().Where(e => string.IsNullOrWhiteSpace(e.Value)).ToList().ForEach(e => e.SetValue(string.Empty));
        expected_output.Descendants().Where(e => string.IsNullOrWhiteSpace(e.Value)).ToList().ForEach(e => e.SetValue(string.Empty));
        Assert.IsTrue(XNode.DeepEquals(generated_output, expected_output), "Generated XML does not match expected XML after normalization.");
        }
    }
