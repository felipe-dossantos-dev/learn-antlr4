using Antlr4.Grammars.TSql;

namespace Learn.Antlr4.TSql;

public class GetTablesNameListener : TSqlParserBaseListener
{
    public List<string> TablesName { get; set; } = new List<string>();

    public override void ExitCreate_table(TSqlParser.Create_tableContext context)
    {
        var tableName = context.table_name().GetText();
        TablesName.Add(tableName);
    }
}