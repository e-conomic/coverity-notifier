public class CoverityMessage
{
    public string message { get; set; }
}

public class CoverityIssues
{
    public CoverityViewv1 viewContentsV1 { get; set; }
}

public class CoverityViewv1
{
    public int offset { get; set; }
    public int totalRows { get; set; }
    public CoverityViewColumns[] columns { get; set; }
    public CoverityIssue[] rows { get; set; }
}

public class CoverityViewColumns
{
    public string name { get; set; }
    public string label { get; set; }
}

public class CoverityIssue
{
    public string cid { get; set; }
    public string displayType { get; set; }
    public string status { get; set; }
    public string firstDetected { get; set; }
    public string owner { get; set; }
    public string classification { get; set; }
    public string severity { get; set; }
    public string action { get; set; }
    public string displayComponent { get; set; }
    public string displayCategory { get; set; }
    public string displayFile { get; set; }
    public string displayFunction { get; set; }
}