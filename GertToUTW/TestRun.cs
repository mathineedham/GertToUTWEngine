namespace GertToUTW;



public class SerialNumberAttributes
    {
    public int SerialNumberAttributes_Key { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Comment { get; set; } 
    }

public class TestRun
    {
    public int TestRun_Key { get; set; } = 1;
    public string? MaterialNumber { get; set; } = string.Empty; //"\d*[1-9]+\d*" OR "\d{1,18}"
    public string MaterialText { get; set; } = string.Empty;
    public string MaterialRevision { get; set; } = string.Empty; //"([A-Z0-9]{4})*"
    public string SerialNumber { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
    public string SequencerId { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty; // FAILED; PASSED; SKIPPED; INCOMPLETE;ERROR
    public DateTime StartTime
        {
        get; set;
        } //xsd::datetime
    public DateTime EndTime
        {
        get; set;
        }//xsd::datetime
    public List<TestItem> TestItem { get; set; } = [];
    public string? Station
        {
        get; set;
        }
    public string? Routestep
        {
        get; set;
        }
    public string Lot { get; set; } = "000000"; //"([0-9]{6,7})*"
    public string? Comment { get; set; }
    public List<SerialNumberAttributes> SerialNumberAttributes { get; set; } = [];
    public int? DUTPosition { get; set; } = 1; //must be convertable to xsd::short
    public string? SoftwareVersion { get; set; } = "0.0.0"; //"[0-9]{1,2}.[0-9]{1,2}(.[0-9]{1,2})*(.[0-9]{1,4})*"
    public string? OperatingSystem { get; set; } = "OS";
    public string? OperatingMode { get; set; } = "OPERATING"; // MUST BE "OPERATING","ENGINEERING","REPAIR",DEVELOPMENT, RMA
    }