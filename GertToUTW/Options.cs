/** @file

    @copyright  &copy; 2026, TRIA Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       2026-07-30

    @author
        Developer (developer@tria-technologies.de)

    @brief
        Defines the command-line arguments model for the GertToUTW execution tool.

    @details
        - Represents positional arguments and optional options parsed by CommandLineParser.
        - Defines required input log file path and output directory path properties.
        - Provides optional lot identifier metadata for downstream execution.
        - Contains no shared mutable state.

    @defgroup REF_GertToUTW_GertToUTW_Options Options
    @{
    @}
*/

using CommandLine;

namespace GertToUTW;

/** @ingroup REF_GertToUTW_GertToUTW_Options
    @class Options
    @brief
        Represents the strongly-typed options bound from command-line arguments.

    @details
        - Acts as the immutable metadata descriptor for CLI invocation context.
        - Maps positional values and optional flags into validated properties.
        - Holds initialized default values to satisfy Nullable contract requirements.
*/
public class Options
    {
    /** @brief
            Gets or sets the target input log file path.

        @details
            - Corresponds to positional CLI argument index 0.
            - Required for tool execution.

        @return
            Returns the path string pointing to the input log file.
    */
    [Value(0, Required = true, HelpText = "Input log file path.")]
    public string InputPath { get; set; } = string.Empty;

    /** @brief
            Gets or sets the target output directory path.

        @details
            - Corresponds to positional CLI argument index 1.
            - Required for tool execution.

        @return
            Returns the directory path string where output artifacts are written.
    */
    [Value(1, Required = true, HelpText = "Output directory path.")]
    public string OutputDirectory { get; set; } = string.Empty;

    /** @brief
            Gets or sets the optional lot number identifier.

        @details
            - Specified via short flag `-l` or long flag `--lot`.
            - Defaults to `string.Empty` when omitted from CLI execution.

        @return
            Returns the provided lot number string, or empty string if not given.
    */
    [Option('l', "lot", Required = false, HelpText = "Given lot number")]
    public string LotNumber { get; set; } = string.Empty;
    }
