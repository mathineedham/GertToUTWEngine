/** @file       GertLogParserTests.cs

    @copyright  &copy; 2026, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       13.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)
 
    @defgroup   REF_GertToUTWEngine_RegressionTest_GertToUTW_XElementExtensionTest XElementExtensionTest
    @{
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW

    @brief      Regression tests for the `XElementExtension` class, focusing on XML sanitization and conditional element addition.

    @details    Validates the behavior of the `XElementExtension` class, including sanitization of strings for XML compatibility
                and conditional addition of child elements to an `XElement`.
    @}
*/

using System.Xml.Linq;

using GertToUTW;
namespace RegressionTests.GertToUTW;

/** @class      SanitizeForXmlTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_XElementExtensionTest

    @brief      Unit tests for the 'sanitize_for_xml' method of the `XElementExtension` class.

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
    [DataRow("dafdsfY>", "dafdsfY>")]
    [DataRow(null, "")]
    [DataRow("", "")]
    public void TestSanitizeForXml_RemovesInvalidCharacters( string input, string expected )
        {
        string result = XElementExtension.sanitize_for_xml(input);
        Assert.AreEqual(expected, result);
        }
    }

/** @class      XElementExtensionsTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_XElementExtensionTest

    @brief      Unit tests for the 'AddIfNotEmpty' method of the `XElementExtension` class.

    @details    Verifies that the 'AddIfNotEmpty' method behaves correctly when adding child elements to an XElement based on the provided value.
*/
[TestClass]
public class XElementExtensionsTests
    {
    /** @brief Tests that the 'AddIfNotEmpty' method throws an ArgumentNullException when the parent XElement is null. */
    [TestMethod]
    public void AddIfNotEmpty_ParentNull()
        {
        XElement? null_parent = null;
        _ = Assert.ThrowsExactly<ArgumentNullException>(() =>null_parent!.AddIfNotEmpty("TestElement", "Valid Value"));
        }

    /** @brief Tests that the 'AddIfNotEmpty' method does not add a child element when the value is null or empty. */
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void AddIfNotEmpty_ValueEmptyorNull( string? testing_value )
        {
        XElement parent = new("Parent");
        parent.AddIfNotEmpty("TestElement", testing_value);
        Assert.IsFalse(parent.HasElements, "Parent should not have any child elements when value is null or empty.");
        }

    /** @brief Tests that the 'AddIfNotEmpty' method correctly adds a child element with an XCData node when the value is valid. */
    [TestMethod]
    public void AddIfNotEmpty_ValidValue()
        {
        XElement parent = new("Parent");
        string element_name = "TestElement";
        string value = "Hello World <>&"; // Including characters that normally need XML escaping

        parent.AddIfNotEmpty(element_name, value);
        XElement? child = parent.Element(element_name);
        Assert.IsNotNull(child, "Child element should have been added.");
        _ = Assert.IsInstanceOfType<XCData>(child.FirstNode, "The element's content should be wrapped in an XCData node.");
        Assert.AreEqual(value, child.Value);
        }
    }