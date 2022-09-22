using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace Learn.Antlr4.CSharp;

public class RoslynRepositoryGenerator
{
    public static string GenerateRepository(QueryModel model)
    {
        var name = FormatName(model.Name);
        var className = $"{name}Repository";
        CompilationUnitSyntax cu = SyntaxFactory.CompilationUnit()
        .AddUsings(SyntaxFactory.UsingDirective
            (SyntaxFactory.IdentifierName("System")))
        .AddUsings(SyntaxFactory.UsingDirective
            (SyntaxFactory.IdentifierName("System.Collections.Generic")))
        .AddUsings(SyntaxFactory.UsingDirective
            (SyntaxFactory.IdentifierName("System.Linq")))
        .AddUsings(SyntaxFactory.UsingDirective
            (SyntaxFactory.IdentifierName("System.Text")))
        .AddUsings(SyntaxFactory.UsingDirective
            (SyntaxFactory.IdentifierName("Dapper")))
        .AddUsings(SyntaxFactory.UsingDirective
            (SyntaxFactory.IdentifierName("System.Threading.Tasks")));

        var localNamespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("Antlr.Generated.Example"));
        var localClass = SyntaxFactory.ClassDeclaration(className);
        localClass = localClass.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        var method = GenerateMethod(model);

        localClass = localClass.AddMembers(method);
        localNamespace = localNamespace.AddMembers(localClass);
        cu = cu.AddMembers(localNamespace);

        var cw = new AdhocWorkspace();
        var options = cw.Options;
        cw.Options.WithChangedOption(CSharpFormattingOptions.IndentBraces, true);
        var formattedNode = Formatter.Format(cu, cw, options);

        var sw = new StringWriter();
        formattedNode.WriteTo(sw);

        return sw.GetStringBuilder().ToString();
    }

    private static MethodDeclarationSyntax GenerateMethod(QueryModel model)
    {
        var returnType = model.Return ?? "void";
        if (model.Name == null)
            throw new NullReferenceException();
        if (model.SQLQuery == null)
            throw new NullReferenceException();
        var method = SyntaxFactory.MethodDeclaration(
                returnType: SyntaxFactory.ParseTypeName(returnType),
                identifier: model.Name
            );
        method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));


        var ssl = SyntaxFactory.SeparatedList<ParameterSyntax>();
        foreach (var paramKV in model.Params)
        {
            ParameterSyntax ps = SyntaxFactory.Parameter(
                new SyntaxList<AttributeListSyntax>(),
                new SyntaxTokenList(),
                SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(paramKV.Value)),
                SyntaxFactory.Identifier(paramKV.Key), null);
            ssl = ssl.Add(ps);
        }
        method = method.AddParameterListParameters(ssl.ToArray());

        var commandDeclarator = SyntaxFactory.VariableDeclarator("command")
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(model.SQLQuery)
                                    )));

        var singletonList =
            SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName(
                        SyntaxFactory.Identifier(
                            SyntaxFactory.TriviaList(),
                            SyntaxKind.VarKeyword,
                            "var",
                            "var",
                            SyntaxFactory.TriviaList()
                        )))
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(commandDeclarator)
                        ));

        var anonymousMember = model
                            .Params
                            .Select(x => SyntaxFactory.AnonymousObjectMemberDeclarator(SyntaxFactory.IdentifierName(x.Key)))
                            .ToList();
        var anonymousMemberList = SyntaxFactory.SeparatedList<AnonymousObjectMemberDeclaratorSyntax>(anonymousMember);

        var usingStatement = SyntaxFactory.UsingStatement(
             SyntaxFactory.Block(
                 SyntaxFactory.SingletonList<StatementSyntax>(
                     SyntaxFactory.ReturnStatement(
                         SyntaxFactory.InvocationExpression(
                             SyntaxFactory.MemberAccessExpression(
                                 SyntaxKind.SimpleMemberAccessExpression,
                                 SyntaxFactory.IdentifierName("connection"),
                                    SyntaxFactory.GenericName(
                                     SyntaxFactory.Identifier("QueryFirstOrDefault"))
                                 .WithTypeArgumentList(
                                     SyntaxFactory.TypeArgumentList(
                                         SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                             SyntaxFactory.IdentifierName("Base"))))))
                         .WithArgumentList(
                             SyntaxFactory.ArgumentList(
                                 SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                     new SyntaxNodeOrToken[]{
                                         SyntaxFactory.Argument(
                                             SyntaxFactory.IdentifierName("command")),
                                         SyntaxFactory.Token(SyntaxKind.CommaToken),
                                         SyntaxFactory.Argument(
                                             SyntaxFactory.AnonymousObjectCreationExpression(anonymousMemberList))
                                        })))))))
         .WithDeclaration(
             SyntaxFactory.VariableDeclaration(
                 SyntaxFactory.IdentifierName(
                     SyntaxFactory.Identifier(
                         SyntaxFactory.TriviaList(),
                         SyntaxKind.VarKeyword,
                         "var",
                         "var",
                         SyntaxFactory.TriviaList())))
             .WithVariables(
                 SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                     SyntaxFactory.VariableDeclarator(
                         SyntaxFactory.Identifier("connection"))
                     .WithInitializer(
                         SyntaxFactory.EqualsValueClause(
                             SyntaxFactory.InvocationExpression(
                                 SyntaxFactory.IdentifierName("GetDefaultConnection")))))));

        method = method.AddBodyStatements(singletonList);
        method = method.AddBodyStatements(usingStatement);
        return method;
    }

    public static string FormatName(string? name)
    {
        if (name == null)
            return "";
        return name
            .Replace("get", "")
            .Replace("update", "")
            .Replace("insert", "")
            .Replace("delete", "");
    }
}