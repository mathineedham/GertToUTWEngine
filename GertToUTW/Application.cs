
using System.Reflection.Metadata.Ecma335;

/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       15.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @defgroup   REF_GertToUTWEngine_GertToUTW_Application   Application 
    @{
    @ingroup    PROJ_GertToUTWEngine_GertToUTW

    @brief      Main entry point for the GertToUTW application, parsing input log files and generating corresponding XML output files.
    
    @details    This file defines the Application class dfined by two file paths,the input file (.log) and the ouput file (.xml).
                and creates the main entry point for the GertToUTW application, py parsing the input log file and generating corresponding XML output files.
    @}
*/
namespace GertToUTW;

/** @ingroup    REF_GertToUTWEngine_GertToUTW_Application

    @class      Application

    @brief      Main entry point for the GertToUTW application, parsing input log files and generating corresponding XML output files.

    @details    Defined by two file paths, the input file (.log) and the output file (.xml), this class encapsulates the core functionality
                of the GertToUTW application, including validation of input parameters and execution of the log-to-XML conversion process.
*/
public class Application
    {
    /** @brief      Constructor for the Application class, initializing with specified input and output file paths.
        @param[in]  input_log_path   Path to the input log file (.log).
        @param[in]  output_xml_path  Path to the output XML directory 
        @throws     ArgumentException if the input file is not .log or does not exist
    */
    public Application( string input_log_path, string output_xml_dir )
        {
        //check its a .log file and it exists
        if(!File.Exists(input_log_path) || Path.GetExtension(input_log_path) != ".log" )
            {
            throw new ArgumentException("Input file must have a .log extension.", nameof(input_log_path));
            }
        Input_log_path = input_log_path;

        Output_xml_dir = output_xml_dir;
        }
    public string Input_log_path
        {
        get;
        }
    public string Output_xml_dir
        {
        get;
        }

    /** @brief      Execute our app program
     *  @return     List of the paths of all files that have been generated*/
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
