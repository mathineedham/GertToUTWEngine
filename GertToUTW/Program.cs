

using CommandLine;

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
    private static void Main( string[] args )
        {
        _ = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(run_application)
            .WithNotParsed(errors =>
            {
            });
        }

    private static void run_application( Options opts )
        {
        try
            {
            Application app = new(opts.InputPath, opts.OutputDirectory, opts.LotNumber);
            _ = app.Execute();
            Console.WriteLine("Conversion completed successfully.");
            }
        catch( Exception ex )
            {
            Console.Error.WriteLine($"Error: {ex.Message}");
            }

        Console.WriteLine("\n Press Enter to close this window...");
        _ = Console.ReadLine();
        }
    }
