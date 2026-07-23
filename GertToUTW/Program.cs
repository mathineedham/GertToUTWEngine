/** @file

    @copyright  &copy; 2026, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       22.07.2026

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
internal static class Program
    {
    /** @brief
        Main executable entry point for the console runner application.
    */
    private static void Main()
        {
        string input_log_path = @"C:\Users\needhamm\Documents\GertToUTWEngine\GertToUTW\valid_singlerun.log";
        string output_xml_path = @"C:\Users\needhamm\Documents\GertToUTWEngine\GertToUTW\valid_singlerun.xml";

        try
            {
            Application app = new(input_log_path, output_xml_path);
            _ = app.Execute();
            }
        catch( Exception ex )
            {
            Console.WriteLine(ex.Message);
            }

        Console.WriteLine("\n Press Enter to close this window...");
        _ = Console.ReadLine();
        }
    }
