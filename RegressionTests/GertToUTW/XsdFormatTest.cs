using System.Xml;
using System.Xml.Schema;

namespace RegressionTests.GertToUTW;

[TestClass]
public class XsdFormatTest
    {
    // The base directory where the test files are located
    private static readonly string theBaseFilesDir = AppDomain.CurrentDomain.BaseDirectory;
    // The path to the XSD file used for validation
    private static readonly string theXsdFilePath = Path.Combine(theBaseFilesDir, "GertToUTW\\XmlTestFiles\\Schema\\GertToUTW.xsd");

    [TestMethod]
    [DataRow("")]
    public void Invalid_Xml( string xml_file )
        {
        // to do
        }

    /** @brief Validates the XML file against the XSD schema and asserts that there are no validation errors. */
    [TestMethod]
    [DataRow("input.log")]
    public void Valid_Xml( string xml_file )
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
    }
