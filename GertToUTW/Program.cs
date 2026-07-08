namespace GertToUTW;

internal static class Program
    {
    private static void Main()
        {
        // 1. Define your test paths
        string input_log_path = @"C:\Users\needhamm\Documents\Getting started\logs\GERT\1022000000.log";
        string output_xml_path = @"C:\Users\needhamm\Documents\Getting started\pythonparser\1022000000.xml";

        try
            {
            // 2. Check if the input file actually exists before starting
            if( !File.Exists(input_log_path) )
                {
                Console.WriteLine($"Error: Input log file not found at: {input_log_path}");
                return;
                }

            Console.WriteLine("Starting GERT Log Parsing Pipeline...");

            // 3. Run the Parser
            List<TestRun> file_test_runs = GertLogParser.ParseGertLog(input_log_path);
            int count = file_test_runs.Count;
            Console.WriteLine($"Successfully parsed log! Found {count} test runs.");

            // 4. Run the XML Generator
            for( int i = 0; i < count; i++ )
                {
                TestRun test_run = file_test_runs[i];

                // This breaks the path into its directory/filename and cleanly inserts "_0", "_1", etc.
                string directory = Path.GetDirectoryName(output_xml_path) ?? "";
                string file_name_without_ext = Path.GetFileNameWithoutExtension(output_xml_path);
                string unique_xml_path = Path.Combine(directory, $"{file_name_without_ext}_{i}.xml");

                Console.WriteLine($"Generating UTW Compliance XML ({i + 1}/{count}) -> {Path.GetFileName(unique_xml_path)}");
                UtwXmlGenerator.GenerateUtwXml(test_run, unique_xml_path);
                }


            Console.WriteLine($"Success");
            }
        catch( Exception ex )
            {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Pipeline failed with an error: {ex.Message}");
            Console.ResetColor();
            }
        Console.WriteLine("\n Press Enter to close this window...");
        Console.ReadLine();
        }
    }