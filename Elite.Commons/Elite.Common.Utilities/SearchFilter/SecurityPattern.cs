using Elite.Common.Utilities.CommonType;

public class SecurityPattern
{
    public string Pattern { get; set; }
    public string Description { get; set; }
    public ThreatType Type { get; set; }

    public SecurityPattern(string pattern, string description, ThreatType type)
    {
        Pattern = pattern;
        Description = description;
        Type = type;
    }
}