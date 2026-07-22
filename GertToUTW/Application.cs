/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       15.07.2026

    @author
        Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @brief
        Main entry point for the GertToUTW application, parsing input log files and generating corresponding XML output files.

    @details
        - Defined by two file paths: the input file (.log) and the output directory for generated XML files.
        - Encapsulates the core functionality of the GertToUTW application.
        - Handles validation of input parameters and execution of the log-to-XML conversion process.
        - Contains no shared mutable state.

    @defgroup REF_GertToUTWEngine_GertToUTW_Application Application
    @{
    @}
*/

namespace GertToUTW;

/** @ingroup REF_GertToUTWEngine_GertToUTW_Application
    @class Application
    @brief
        Main entry point for the GertToUTW application, parsing input log files and generating corresponding XML output files.

    @details
        - Defined by two file paths: the input file (.log) and the output directory for generated XML files.
        - Encapsulates the core functionality of the GertToUTW application.
        - Validates input parameters and orchestrates the execution of the log-to-XML conversion process.

    @see GertLogParser
    @see UtwXmlGenerator
    @see TestRun
*/
public class Application
    {
    /** @brief Gets the file path to the input log document. */
    public string Input_log_path
        {
        get;
        }

    /** @brief Gets the target directory path for generated output XML files. */
    public string Output_xml_dir
        {
        get;
        }

    /** @brief
        Initializes a new instance of the Application class with specified input and output paths.

    @param[in] input_log_path
        Path to the input log file (.log).

    @param[in] output_xml_dir
        Path to the output directory where generated XML files will be stored.

    @exception ArgumentException
        Thrown when `input_log_path` or `output_xml_dir` is null/empty or does not end with `.log`.

    @exception FileNotFoundException
        Thrown when `input_log_path` does not exist on disk.
    */
    public Application( string input_log_path, string output_xml_dir )
        {
        ArgumentException.ThrowIfNullOrEmpty(input_log_path);
        ArgumentException.ThrowIfNullOrEmpty(output_xml_dir);

        if( !File.Exists(input_log_path) )
            {
            throw new FileNotFoundException("The specified input log file was not found.", input_log_path);
            }
        if( !string.Equals(Path.GetExtension(input_log_path), ".log", StringComparison.OrdinalIgnoreCase) )
            {
            throw new ArgumentException("Input file must have a .log extension.", nameof(input_log_path));
            }


        Input_log_path = input_log_path;
        Output_xml_dir = output_xml_dir;
        }

    /** @brief
        Executes the log parsing and XML generation process.

    @details
        - Parses input log file sessions using @ref GertLogParser.
        - Ensures the output directory exists.
        - Generates uniquely named XML files in the target directory via @ref UtwXmlGenerator.

    @return
        Returns a list of file paths corresponding to all successfully generated XML files.
    */
    public List<string> Execute()
        {
        List<TestRun> file_test_runs = GertLogParser.ParseGertLog(Input_log_path);
        int count = file_test_runs.Count;
        string file_name_without_ext = Path.GetFileNameWithoutExtension(Input_log_path);
        List<string> generated_xml_files = [];

        if( !Directory.Exists(Output_xml_dir) )
            {
            _ = Directory.CreateDirectory(Output_xml_dir);
            }

        for( int i = 0; i < count; i++ )
            {
            TestRun test_run = file_test_runs[i];
            string unique_xml_path = Path.Combine(Output_xml_dir, $"{file_name_without_ext}_{i}.xml");
            generated_xml_files.Add(unique_xml_path);
            UtwXmlGenerator.GenerateUtwXml(test_run, unique_xml_path);
            }

        return generated_xml_files;
        }
    }