namespace GertToUTW;

internal static class Program
    {
    private static void Main()
        {
        string input_log_path = @"C:\Users\needhamm\Documents\Getting started\logs\GERT\1022000000.log";
        string output_xml_path = @"C:\Users\needhamm\Documents\Getting started\pythonparser\1022000000.xml";

        try
            {
            Application App = new(input_log_path, output_xml_path);
            App.Execute();
            }
        catch( Exception ex )
            {
            Console.WriteLine( ex.ToString() );
            }
        Console.WriteLine("\n Press Enter to close this window...");
        Console.ReadLine();
        }
    }