/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       23.07.2026

    @author
        Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @brief
        Coordinates validation of Gert log input and generation of the corresponding UTW XML output files.

    @details
        - Stores the validated input log path and output directory path as immutable instance state.
        - Requires the input file name to use the `.log` extension and validates that the file exists at construction time.
        - Creates the output directory when required and generates deterministic, culture-invariant indexed XML file names.
        - Performs file-system I/O without coordinating concurrent executions that target the same output paths.
        - Contains no shared mutable state.

    @defgroup REF_GertToUTWEngine_GertToUTW_Application Application
    @{
    @}
*/

using System.Globalization;

namespace GertToUTW;

/** @ingroup REF_GertToUTWEngine_GertToUTW_Application
    @class Application
    @brief
        Coordinates parsing of one Gert log file and generation of its UTW XML output files.

    @details
        - Stores validated input and output paths through get-only properties.
        - Rejects null, empty, whitespace-only, missing, and non-`.log` input paths with deterministic exception classification.
        - Creates the configured output directory before generating XML files.
        - Uses culture-invariant indexes to keep generated file names stable across runtime cultures.
        - Contains no mutable instance state, but concurrent executions targeting the same output paths are not coordinated.

    @see GertLogParser
    @see UtwXmlGenerator
    @see TestRun
*/
public class Application
    {
    /** @property Input_log_path
        @brief
            Gets the validated path to the input Gert log file.

        @details
            - Returns the path supplied during construction.
            - The file is confirmed to exist when the instance is created.
            - The file may still be moved, replaced, or deleted before execution.

        @return
            Returns the validated input log file path.
    */
    public string Input_log_path
        {
        get;
        }

    /** @property Output_xml_dir
        @brief
            Gets the target directory path for generated UTW XML files.

        @details
            - Returns the path supplied during construction.
            - The directory is created by @ref Execute when it does not already exist.
            - The path is not normalized or restricted to a specific base directory.

        @return
            Returns the configured output directory path.
    */
    public string Output_xml_dir
        {
        get;
        }

    /** @brief
        Initializes a new instance of the @ref Application class with validated input and output paths.

        @details
            - Rejects null, empty, and whitespace-only path arguments.
            - Validates the `.log` extension before checking file-system existence to keep exception classification deterministic.
            - Uses an ordinal, case-insensitive extension comparison.
            - Stores the supplied paths without culture-sensitive conversion or normalization.
            - Does not allocate shared mutable state.

        @param[in] input_log_path
            Provides the path to the input Gert log file.

        @param[in] output_xml_dir
            Provides the output directory path used for generated UTW XML files.

        @exception ArgumentNullException
            Thrown when `input_log_path` or `output_xml_dir` is `null`.

        @exception ArgumentException
            Thrown when either path is empty or whitespace-only, or when `input_log_path` does not use the `.log` extension.

        @exception FileNotFoundException
            Thrown when `input_log_path` does not identify an existing file.
    */
    public Application( string input_log_path, string output_xml_dir )
        {
        ArgumentException.ThrowIfNullOrWhiteSpace(input_log_path);
        ArgumentException.ThrowIfNullOrWhiteSpace(output_xml_dir);

        if( !string.Equals(Path.GetExtension(input_log_path), ".log", StringComparison.OrdinalIgnoreCase) )
            {
            throw new ArgumentException("Input file must have a .log extension.", nameof(input_log_path));
            }

        if( !File.Exists(input_log_path) )
            {
            throw new FileNotFoundException("The specified input log file was not found.", input_log_path);
            }

        Input_log_path = input_log_path;
        Output_xml_dir = output_xml_dir;
        }

    /** @brief
        Parses the configured Gert log file and generates the corresponding UTW XML files.

        @details
            - Parses input log sessions through @ref GertLogParser.
            - Creates the configured output directory idempotently before writing files.
            - Generates deterministic file names from the input file stem and a zero-based, culture-invariant index.
            - Adds each output path to the result only after @ref UtwXmlGenerator completes successfully.
            - Does not synchronize concurrent executions that target the same output paths.

        @return
            Returns the generated XML file paths in the same order as the parsed test runs.

        @exception FileNotFoundException
            Thrown when the input file is no longer available when parsing begins.

        @exception UnauthorizedAccessException
            Thrown when the process is not permitted to read the input file or create or write the output files.

        @exception IOException
            Thrown when a file-system operation fails while reading the input, creating the output directory, or writing an output file.
    */
    public List<string> Execute()
        {
        List<TestRun> file_test_runs = GertLogParser.ParseGertLog(Input_log_path);
        int count = file_test_runs.Count;
        string file_name_without_ext = Path.GetFileNameWithoutExtension(Input_log_path);
        List<string> generated_xml_files = [];

        _ = Directory.CreateDirectory(Output_xml_dir);

        for( int i = 0; i < count; i++ )
            {
            TestRun test_run = file_test_runs[i];
            string file_index = i.ToString(CultureInfo.InvariantCulture);
            string unique_xml_path = Path.Combine(Output_xml_dir, $"{file_name_without_ext}_{file_index}.xml");
            UtwXmlGenerator.GenerateUtwXml(test_run, unique_xml_path);
            generated_xml_files.Add(unique_xml_path);
            }

        return generated_xml_files;
        }
    }
