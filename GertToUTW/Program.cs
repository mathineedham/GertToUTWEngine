namespace GertToUTW;

internal static class Program
    {
    private static void Main()
        {
        string input_log_path = @"C:\Users\needhamm\Documents\GertToUTWEngine\GertToUTW\valid_singlerun.log";
        string output_xml_path = @"C:\Users\needhamm\Documents\GertToUTWEngine\GertToUTW\valid_singlerun.xml";

        try
            {
            Application app = new(input_log_path, output_xml_path);
            _ = app.Execute();
            }
        catch(Exception ex )
            {
            Console.WriteLine(ex.Message );
            }
        Console.WriteLine("\n Press Enter to close this window...");
        Console.ReadLine();
        }
    }