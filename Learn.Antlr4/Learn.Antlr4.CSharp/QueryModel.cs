namespace Learn.Antlr4.CSharp;

public class QueryModel
{
    public string? Name { get; set; }
    public string? SQLQuery { get; set; }
    public string? Return { get; set; }
    public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
}