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
public class ApplicationTest
    {
    private static readonly string theBaseFilesDir = AppDomain.CurrentDomain.BaseDirectory;

    /** @brief  Validates that the constrcutor throws an ArgumentException when provided with an invalid file paths */
    [TestMethod]
    [DataRow("", "")]
    [DataRow("input.log", "input.log")] //output must be .xml
    [DataRow("input.xml", "output.xml")] //input must be .log
    public void Application_Invalid( string input, string output )
        {
        _ = Assert.ThrowsExactly<ArgumentException>(() => new Application(input, output));
        }

    /** @brief  Validates Application constructor with valid input and output file paths even if non existent */
    [TestMethod]
    [DataRow("nonexistent_input.log", "output.xml")]
    [DataRow("GertToUTW\\LogTestFiles\\Valid\\valid_singlerun.log",
             "GertToUTW\\XmlTestFiles\\Generated\\valid_singlerun.xml")]
    public void Application_Valid( string input, string output )
        {
        Application app = new(input, output);
        Assert.AreEqual(input, app.Input_log_path);
        Assert.AreEqual(output, app.Output_xml_path);
        }

    /** @brief  Validates that appliucation correctly generated an xml file as expected */
    [TestMethod]
    [DataRow("GertToUTW\\LogTestFiles\\Valid\\valid_singlerun.log",
             "GertToUTW\\XmlTestFiles\\Generated\\valid_singlerun.xml",
             "GertToUTW\\XmlTestFiles\\Expected\\valid_singlerun.xml")]
    public void Application_Valid_ExistingFiles(string input_relative_path, string output_relative_path, string expected_relative_path)
        {
        // make them absolute paths
        string absolute_out = Path.Combine(theBaseFilesDir, output_relative_path);
        string absolute_in = Path.Combine(theBaseFilesDir, input_relative_path);
        string absolute_expected = Path.Combine(theBaseFilesDir, expected_relative_path);

        // Create Application and execute
        Application app = new(absolute_in, absolute_out);
        _ = app.Execute();


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
