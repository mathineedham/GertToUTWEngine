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

using GertToUTW;
namespace RegressionTests.GertToUTW;

/** @class      ApplicationTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_ApplicationTest

    @brief      Unit tests for the `Application` class.

    @details    Validates the behavior of the `Application` class, including constructor validation
                for input and output file paths, and expected exception handling in error scenarios.
*/
[TestClass]
public partial class ApplicationTest
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

        // Ensure the schema file exists prior to test execution
        Assert.IsTrue(File.Exists(theXsdFilePath),theXsdFilePath);
        }


    /** @brief  Validates that the constrcutor throws an ArgumentException when provided with an invalid file paths */
    [TestMethod]
    [DataRow("", "")]
    [DataRow("GertToUTW\\XmlTestFiles\\Structure\\machine-readable-logs.xsd", "output")] //input must be .log
    public void Application_Constructor_InvalidArguments_ThrowsArgumentException( string input, string output )
        {
        string absolute_input_file = string.IsNullOrEmpty(input) ? input : Path.Combine(theBaseFilesDir, input);
        string absolute_output_dir = string.IsNullOrEmpty(output) ? output : Path.Combine(theBaseFilesDir, output);

        _ = Assert.ThrowsExactly<ArgumentException>(() => new Application(absolute_input_file, absolute_output_dir));

        //input must exist
        string absolute_input_file2 = Path.Combine(theBaseFilesDir, "GertToUTW\\XmlTestFiles\\LogTestFiles\\nonexistent.log");
        _ = Assert.ThrowsExactly<FileNotFoundException>(() => new Application(absolute_input_file2, "output"));
        }

    [TestMethod]
    public void Application_create_directory_if_not_exists()
        {
        string absolute_input_file = Path.Combine(theBaseFilesDir, "GertToUTW\\LogTestFiles\\Valid\\valid_singlerun.log");
        string temp_dir = Path.Combine(theBaseFilesDir, Path.GetRandomFileName());
        Application app = new(absolute_input_file, temp_dir);
        _ = app.Execute();
        Assert.IsTrue(Directory.Exists(temp_dir), temp_dir);
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
        string xml_file_path = Path.Combine(theBaseFilesDir,xml_file);

        XmlReaderSettings xml_settings = new();
        _ = xml_settings.Schemas.Add("", theXsdFilePath);
        xml_settings.ValidationType = ValidationType.Schema;

        XmlDocument xml_doc = new();

        using XmlReader reader = XmlReader.Create(xml_file_path, xml_settings);
        xml_doc.Load(reader);

        xml_doc.Validate(( sender, e ) =>Assert.Fail(e.Message));

        }

    /** @brief  Validates that appliucation correctly generated an xml file as expected */
    [TestMethod]
    [DataRow("GertToUTW\\XmlTestFiles\\Generated\\valid_singlerun_0.xml",
             "GertToUTW\\XmlTestFiles\\Expected\\valid_singlerun.xml")]
    [DataRow("GertToUTW\\XmlTestFiles\\Generated\\valid_doublerun_0.xml",
             "GertToUTW\\XmlTestFiles\\Expected\\valid_doublerun_fail.xml")]
    [DataRow("GertToUTW\\XmlTestFiles\\Generated\\valid_doublerun_1.xml",
             "GertToUTW\\XmlTestFiles\\Expected\\valid_doublerun_sucess.xml")]
    public void Application_Valid_ExistingFiles( string output_relative_path, string expected_relative_path )
        {
        string absolute_out = Path.Combine(theBaseFilesDir, output_relative_path);
        string absolute_expected = Path.Combine(theBaseFilesDir, expected_relative_path);

        // Load both files into XElements
        XElement generated = XElement.Load(absolute_out);
        XElement expected = XElement.Load(absolute_expected);

        var gen_nodes = generated.DescendantsAndSelf().Select(e => new { e.Name, Value = e.Value.Replace("\r\n", "\n").Replace("\r", "\n").Trim() }).ToList();
        var exp_nodes = expected.DescendantsAndSelf().Select(e => new { e.Name, Value = e.Value.Replace("\r\n", "\n").Replace("\r", "\n").Trim() }).ToList();

        // Verify they have the same number of nodes
        Assert.HasCount(exp_nodes.Count, gen_nodes, "The structure or node count does not match.");

        // Loop through and compare only the names and clean data values
        for( int i = 0; i < gen_nodes.Count; i++ )
            {
            Assert.IsTrue(gen_nodes[i].Name == exp_nodes[i].Name);
            Assert.AreEqual(exp_nodes[i].Value, gen_nodes[i].Value);
            }
        }
    }
