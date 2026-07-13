/** @file       GertLogParserTests.cs

    @copyright  &copy; 2026, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       10.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)
 
    @defgroup   REF_GertToUTWEngine_RegressionTest_GertToUTW_UtwXmlGeneratorTest UtwXmlGeneratorTest
    @{
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW

    @brief      Provides utility mechanisms to read, interpret, and parse execution log files.

    @details    The `GertLogParser` class splits raw file contents into individual historical test data chunks, 
                extracting metadata attributes, tracking evaluation criteria, and translating execution timelines 
                into serializable structural objects.
    @}
*/

using System.Text;
using System.Xml.Linq;

using GertToUTW;
namespace RegressionTests.GertToUTW;

/** @class      SanitizeForXmlTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_UtwXmlGeneratorTest

    @brief      Unit tests for the 'sanitize_for_xml' method of the `UtwXmlGenerator` class.

    @details    Verifies that all non-printable and control characters are correctly removed from strings.
*/
[TestClass]
public sealed class SanitizeForXmlTests
    {
    /** @brief Tests that the 'sanitize_for_xml' method correctly removes invalid XML characters from a string. */
    [TestMethod]
    [DataRow("Hello\x01World", "HelloWorld")]
    [DataRow("ValidString\n", "ValidString\n")]
    [DataRow("StringWith\x1F ControlChar", "StringWith ControlChar")]
    [DataRow("StringWith\x7F DeleteChar", "StringWith DeleteChar")]
    [DataRow("StringWith\x0BVerticalTab", "StringWithVerticalTab")]
    [DataRow("StringWith\x0C FormFeed", "StringWith FormFeed")]
    [DataRow("StringWith\x0EShiftOut", "StringWithShiftOut")]
    [DataRow("StringWith\x0FShiftIn", "StringWithShiftIn")]
    [DataRow("dafdsfY>","dafdsfY>")]
    public void TestSanitizeForXml_RemovesInvalidCharacters(string input, string expected)
        {
        string result = UtwXmlGenerator.sanitize_for_xml(input);
        Assert.AreEqual(expected, result);
        }
    }

/** @class     FormatTimeTests

    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_UtwXmlGeneratorTest

    @brief      Unit tests for the 'format_time' method of the `UtwXmlGenerator` class.

    @details    Simple unit tests to verify that the `format_time` method correctly formats DateTime
                objects into the expected string representation.
*/
[TestClass]
public sealed class FormatTimeTests
    {
    /** @brief Tests that the 'format_time' method correctly formats DateTime objects into the expected string representation. */
    [TestMethod]
    [DataRow("2026-07-10 08:30:15.123", "2026-07-10T08:30:15.123+02:00")]
    [DataRow("2026-07-10 22:15:45.999", "2026-07-10T22:15:45.999+02:00")]
    [DataRow("2026-01-02 03:04:05.006", "2026-01-02T03:04:05.006+02:00")]
    public void FormatTimeTest( string input, string expected )
        {
        DateTime test_date = DateTime.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
        string actual_result = UtwXmlGenerator.format_time(test_date);
        Assert.AreEqual(expected, actual_result);
        }
    }

/** @class      UtwXmlGeneratorTests

    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_UtwXmlGeneratorTest

    @brief      Unit tests for the 'build_utw_xml_document' method of the `UtwXmlGenerator` class.

    @details    Tests the generation of UTW-compliant XML documents from `TestRun` instances,
                ensuring that the output adheres to the expected structure and content.
*/
[TestClass]
public class UtwXmlGeneratorTests
    {
    /** @brief Tests that the 'GenerateUtwXml' method correctly throws an ArgumentNullException when provided with a null TestRun instance. */
    [TestMethod]
    public void BuildUTWXMLDOC_Error()
        {
        _ = Assert.ThrowsExactly<ArgumentNullException>(() => UtwXmlGenerator.build_utw_xml_document(null!));
        }

    /** @brief Allows us to test using DataRow method */
    private static TestRun create_test_run_fixture(
        string mat_num, string mat_text, string mat_rev, string serial_num, string op_name, string comp_name, string result_val )
        {
        return new TestRun
            {
            TestRun_Key = 1,
            ComputerName = comp_name,
            OperatorName =op_name,
            MaterialNumber =mat_num,
            MaterialText = mat_text,
            MaterialRevision = mat_rev,
            SerialNumber = serial_num,
            Result = new Result { Value = result_val },
            SequencerId = "GERT",
            StartTime =new DateTime(2026, 7, 10, 12, 00, 00),
            EndTime = new DateTime(2026, 07, 10, 12, 05, 00),
            SerialNumberAttributes =[],
            TestItem = []
            };
        }

    /** @brief Tests that the 'GenerateUtwXml' method correctly generates a UTW XML document from a valid TestRun instance. */
    [TestMethod]
    //  MaterialNumber | MaterialText | MaterialRevision | SerialNumber | OperatorName | ComputerName | ExpectedResult
    [DataRow("12345678", "Display Mod", "B001", "SN-001", "Jane Doe", "STATION-01", "PASSED")]
    [DataRow("12345678", "Core Engine", "B001", "SN-002", "John Smith", "STATION-02", "FAILED")]
    [DataRow("12345678", "Power Board", "B001", "SN-099-XYZ", "System_Auto", "STATION-99", "ERROR")]
    public void BuildUtwXmlDocument_HeaderDataRows_MapsAllFieldsCorrectly(
        string mat_num, string mat_text, string mat_rev, string serial_num, string op_name, string comp_name, string result_val )
        {
        TestRun test_run_fixture = create_test_run_fixture(mat_num, mat_text, mat_rev, serial_num, op_name, comp_name, result_val);

        XDocument doc = UtwXmlGenerator.build_utw_xml_document(test_run_fixture);
        XElement root = doc.Root;

        Assert.IsNotNull(root, "XML Document generation failed to produce a valid root element node.");

        Assert.AreEqual(mat_num, root.Element("MaterialNumber")?.Value);
        Assert.AreEqual(mat_num, root.Element("Lot")?.Value, "The 'Lot' element must always mirror the primary MaterialNumber.");
        Assert.AreEqual(mat_text, root.Element("MaterialText")?.Value);
        Assert.AreEqual(mat_rev, root.Element("MaterialRevision")?.Value);
        Assert.AreEqual(serial_num, root.Element("SerialNumber")?.Value);
        Assert.AreEqual(op_name, root.Element("OperatorName")?.Value);
        Assert.AreEqual(comp_name, root.Element("ComputerName")?.Value);
        Assert.AreEqual(result_val, root.Element("Result")?.Value);

        Assert.AreEqual("01", root.Element("TestRun_Key")?.Value);
        Assert.AreEqual("GERT", root.Element("SequencerId")?.Value);
        Assert.AreEqual("2026-07-10T12:00:00.000+02:00", root.Element("StartTime")?.Value);
        Assert.AreEqual("2026-07-10T12:05:00.000+02:00", root.Element("EndTime")?.Value);
        Assert.AreEqual("0.0.0", root.Element("SoftwareVersion")?.Value);
        Assert.AreEqual("OS", root.Element("OperatingSystem")?.Value);
        Assert.AreEqual("OPERATING", root.Element("OperatingMode")?.Value);
        }
    }

/** @class      UtwXmlGeneratorFileIoTests

    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_UtwXmlGeneratorTest

    @brief      Unit tests for the 'GenerateUtwXml' method of the `UtwXmlGenerator` class.

    @details    Test the file I/O operations of the `GenerateUtwXml` method, ensuring that the generated XML
                files are correctly written to disk and can be read back without data loss or corruption.
*/
[TestClass]
public class UtwXmlGeneratorFileIoTests
    {
    private string m_temp_file_path;

    [TestInitialize]
    public void Setup()
        {
        m_temp_file_path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xml");
        }

    [TestCleanup]
    public void Teardown()
        {
        if( File.Exists(m_temp_file_path) )
            {
            File.Delete(m_temp_file_path);
            }
        }

    /** @brief Tests that the 'GenerateUtwXml' throws an error if the path is null or empty */
    [TestMethod]
    public void GenerateXml_Error()
        {
        TestRun valid_test_run = new()
            { StartTime = DateTime.Now, EndTime = DateTime.Now };
        _ = Assert.ThrowsExactly<ArgumentException>(() => UtwXmlGenerator.GenerateUtwXml(valid_test_run, ""));
       
        }

    /** @brief Tests that the 'GenerateUtwXml' method correctly writes a UTW XML document to disk and can be read back without data loss or corruption. */
    [TestMethod]
    public void GenerateXml_Valid()
        {
        TestRun minimal_run = new()
            {
            StartTime = new DateTime(2026, 07, 10),
            EndTime = new DateTime(2026, 07, 10),
            SerialNumberAttributes = [],
            TestItem = []
            };

        UtwXmlGenerator.GenerateUtwXml(minimal_run, m_temp_file_path);

        Assert.IsTrue(File.Exists(m_temp_file_path), "The function completed execution but failed to create a physical file on disk.");
        string written_text = File.ReadAllText(m_temp_file_path, Encoding.UTF8);
        StringAssert.Contains(written_text, "<TestRun>", "The output file content stream is missing the fundamental XML document root wrapper element.");
        StringAssert.Contains(written_text, "encoding=\"utf-8\"", "The document declaration tag was not written out using standard explicit UTF-8 encoding strings.");
        }
    }