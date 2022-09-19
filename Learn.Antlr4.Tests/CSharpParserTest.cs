using System.Text.Json;
using Antlr4.Grammars.CSharp;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Learn.Antlr4.CSharp;
using Xunit.Abstractions;

namespace Learn.Antlr4.Tests;

public class CSharpParserTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CSharpParserTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private CSharpParser Parse(string code)
    {
        var lexer = new CSharpLexer(new AntlrInputStream(code));
        var codeTokenStream = new CommonTokenStream(lexer);
        var parser = new CSharpParser(codeTokenStream);
        if (parser == null)
        {
            throw new NullReferenceException();
        }
        return parser;
    }

    private void PrintResult(object obj)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(obj, options);
        _testOutputHelper.WriteLine(jsonString);
    }

    [Fact]
    public void GetLegacyQueryMethod()
    {
        // arrange
        var code = @"
        public class Test {
            private int getParametro(string param)
            {
                var command = ""SELECT parametro FROM parametros_gerais WHERE UPPER(nome_parametro) = @param;"";

                DalIndividual.command.Parameters.Clear();
                var parameters = DalIndividual.command.Parameters;
                parameters.Add(""@param"", MySqlDbType.String);
                parameters[""@param""].Value = param.ToUpper();
                return Convert.ToInt32(DalIndividual.Select(command).Rows[0][""nome_parametro""].ToString());
            }

            private void updateModelo(Modelo modelo)
            {
                var command = @""UPDATE modelo SET valor = @valor, data = @data WHERE id = @id;"";
                DalIndividual.command.Parameters.Clear();
                var parameters = DalIndividual.command.Parameters;
                parameters.AddWithValue(""@valor"", modelo.valor);
                parameters.AddWithValue(""@data"", modelo.data);
                parameters.AddWithValue(""@id"", modelo.id);
                ExecuteCommand(command);
            }
        }
        ";
        var parser = Parse(code);
        var tree = parser.compilation_unit();
        var listener = new GetQueryMethodListener();

        // act
        ParseTreeWalker.Default.Walk(listener, tree);
        PrintResult(listener.Queries);

        // assert
        Assert.Equal(2, listener.Queries.Count);
        var expected = new QueryModel
        {
            Name = "getParametro",
            Params = new Dictionary<string, string>
            {
                ["param"] = "string"
            },
            Return = "int",
            SQLQuery = "SELECT parametro FROM parametros_gerais WHERE UPPER(nome_parametro) = @param;"
        };
        var actual = listener.Queries[0];
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Params, actual.Params);
        Assert.Equal(expected.Return, actual.Return);
        Assert.Equal(expected.SQLQuery, actual.SQLQuery);


        expected = new QueryModel
        {
            Name = "updateModelo",
            Params = new Dictionary<string, string>
            {
                ["modelo"] = "Modelo"
            },
            Return = null,
            SQLQuery = "UPDATE modelo SET valor = @valor, data = @data WHERE id = @id;"
        };
        actual = listener.Queries[1];
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Params, actual.Params);
        Assert.Null(actual.Return);
        Assert.Equal(expected.SQLQuery, actual.SQLQuery);
    }

    [Fact]
    public void GenerateNewQueryMethod()
    {
        // arrange
        // act
        var result = RoslynRepositoryGenerator.GenerateRepository(new QueryModel
        {
            Name = "updateModelo",
            Params = new Dictionary<string, string>
            {
                ["modelo"] = "Modelo"
            },
            Return = null,
            SQLQuery = "UPDATE modelo SET valor = @valor, data = @data WHERE id = @id;"
        });
        _testOutputHelper.WriteLine(result);
        // assert
        Assert.NotNull(result);
    }
}