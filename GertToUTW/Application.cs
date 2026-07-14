
namespace GertToUTW;

public class Application
    {
    public Application( string input_log_path, string output_xml_path )
        {
        Input_log_path = input_log_path;
        Output_xml_path = output_xml_path;
        }
    public string Input_log_path
        {
        get;
        }
    public string Output_xml_path
        {
        get;
        }

    public void Execute()
        {
        List<TestRun> file_test_runs = GertLogParser.ParseGertLog(Input_log_path);
        int count = file_test_runs.Count;
        Console.WriteLine($"Successfully parsed log! Found {count} test runs.");
        for( int i = 0; i < count; i++ )
            {
            TestRun test_run = file_test_runs[i];
            string directory = Path.GetDirectoryName(Output_xml_path) ?? "";
            string file_name_without_ext = Path.GetFileNameWithoutExtension(Output_xml_path);
            string unique_xml_path = Path.Combine(directory, $"{file_name_without_ext}_{i}.xml");
            UtwXmlGenerator.GenerateUtwXml(test_run, unique_xml_path);
            }  
        Console.WriteLine("Success");
        }
    }
