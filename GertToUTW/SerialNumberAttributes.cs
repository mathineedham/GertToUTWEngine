/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       09.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @defgroup   REF_GertToUTWEngine_GertToUTW_SerialNumberAttributes   SerialNumberAttributes 
    @{
    @ingroup    PROJ_GertToUTWEngine_GertToUTW

    @brief      Defines data structures representing serial number attributes and a test run .

    @details    This file includes metadata models used to serialize and manage properties 
                associated with individual test sequence executions, hardware metadata, 
                and regulatory data boundary compliance tracking.
    @}
*/
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RegressionTest")]
namespace GertToUTW;

/** @ingroup    REF_GertToUTWEngine_GertToUTW_SerialNumberAttributes

    @class      SerialNumberAttributes

    @brief      Represents key-value attribute extensions for a unique serial number.

    @details    Provides custom dynamic properties without modifying the fixed schematic layout.
*/
public class SerialNumberAttributes
    {
    /** @brief      configuration key 

        @return     The key of the serial number attributes.
    */
    public int SerialNumberAttributes_Key { get; set; } = 1;
    /** @brief      The descriptor name of the metadata attribute field.

        @return     The name of the attribute.
    */
    public string Name { get; set; } = string.Empty;
    /** @brief      The text value matching the descriptive attribute property.

        @return     The value of the attribute.
    */
    public string Value { get; set; } = string.Empty;
    /** @brief      An optional user or system log comment regarding this attribute record.

        @return     The comment block text string, or null if unassigned.
    */
    public string? Comment
        {
        get; set;
        }
    }