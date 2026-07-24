/** @file

    @copyright  &copy; 2026, TRIA Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       24.07.2026

    @author
        Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @brief
        Provides the executable command-line entry point for the GertToUTW application.

    @details
        - Instantiates the `Application` orchestration engine with target file paths.
        - Triggers log file processing and XML generation workflows.
        - Captures unhandled runtime exceptions during conversion.

    @defgroup REF_GertToUTWEngine_GertToUTW_Program Program
    @{
    @}
*/

namespace GertToUTW;

/** @ingroup REF_GertToUTWEngine_GertToUTW_Program
    @class Program
    @brief
        Static entry point class for the GertToUTW console runner.

    @details
        - Contains the main application entry point method.
        - Handles top-level process execution and exception safety.

    @see Application
*/
internal class Program
    {
    /** @brief
            Main entry point for the GertToUTW console application.

        @details
            - Parses command line arguments to identify input and output target paths.
            - Instantiates and executes the conversion workflow engine.
            - Catches top-level runtime exceptions and outputs error diagnostics.

        @param[in] args
            Provides the command-line arguments passed to the executable.
    */
    private static void Main( string[] args )
        {
        if( !try_parse_arguments(args, out string input_path, out string output_directory) )
            {
            return;
            }

        try
            {
            Application app = new(input_path, output_directory);
            _ = app.Execute();
            Console.WriteLine("Conversion completed successfully.");
            }
        catch( Exception ex )
            {
            Console.WriteLine($"Error: {ex.Message}");
            }
        }

    /** @brief
            Attempts to parse command-line arguments into input and output paths.

        @details
            - Validates argument count and checks for standard help switches.
            - Displays CLI help text or invalid argument notices when parsing fails.
            - Assigns out parameters upon successful validation.

        @param[in] args
            Provides the raw command-line arguments passed to the application.

        @param[out] input_file
            Receives the input file path when arguments are valid.

        @param[out] output_dir
            Receives the target output directory path when arguments are valid.

        @return
            Returns `true` when arguments were successfully parsed; otherwise, returns `false`.
    */
    private static bool try_parse_arguments( string[] args, out string input_file, out string output_dir )
        {
        input_file = string.Empty;
        output_dir = string.Empty;

        if( args.Length == 1 && (args[0] is "-h" or "--help" or "/?" or "-help") )
            {
            Console.WriteLine("Help:");
            Console.WriteLine("  Usage: App.exe [input_log_path] [output_xml_path]");
            Console.WriteLine("  Example: App.exe C:\\path\\in.log C:\\path\\out.xml");
            return false;
            }

        if( args.Length == 2 )
            {
            input_file = args[0];
            output_dir = args[1];
            return true;
            }

        Console.WriteLine("Invalid arguments provided!");
        Console.WriteLine("Usage: App.exe <input_log_path> <output_xml_path> OR App.exe --help");
        return false;
        }
    }
