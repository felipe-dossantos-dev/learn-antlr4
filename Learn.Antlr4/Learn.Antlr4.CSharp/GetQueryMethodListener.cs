using Antlr4.Grammars.CSharp;
using Antlr4.Runtime.Misc;

namespace Learn.Antlr4.CSharp;

public partial class GetQueryMethodListener : CSharpParserBaseListener
{
    public IList<QueryModel> Queries { get; set; } = new List<QueryModel>();
    public QueryModel? ActualModel = null;
    public GetQueryMethodListener()
    {
    }
    public override void EnterString_literal([NotNull] CSharpParser.String_literalContext context)
    {
        var text = context.GetText().ToLower();
        if (text.Contains("select")
            || text.Contains("update")
            || text.Contains("delete")
            || text.Contains("insert"))
        {
            ActualModel = new QueryModel 
            {
                SQLQuery = context.GetText()
            };
        }
    }

    public override void ExitMethod_declaration([NotNull] CSharpParser.Method_declarationContext context)
    {
        if (ActualModel != null)
        {
            var methodName = context.method_member_name().GetText();
            ActualModel.Name = methodName;

            var formalParameterList = context.formal_parameter_list();
            if (formalParameterList != null)
            {
                var parameter_array = formalParameterList.parameter_array();
                if (parameter_array != null)
                {
                    var paramType = parameter_array.array_type().GetText();
                    var paramName = parameter_array.identifier().GetText();
                    ActualModel.Params.TryAdd(paramName, paramType);
                }
                var fixed_parameters = formalParameterList.fixed_parameters();
                if (fixed_parameters != null)
                {
                    foreach(var fixedParam in fixed_parameters.fixed_parameter())
                    {
                        var arg_declaration = fixedParam.arg_declaration();
                        var paramType = arg_declaration.type_().GetText();
                        var paramName = arg_declaration.identifier().GetText();
                        ActualModel.Params.TryAdd(paramName, paramType);
                    }
                }
            }
            Queries.Add(ActualModel);
        }
    }

    public override void ExitTyped_member_declaration([NotNull] CSharpParser.Typed_member_declarationContext context)
    {
        if (ActualModel != null)
        {
            var returnType = context.type_().GetText();
            ActualModel.Return = returnType;
        }
    }

    public override void ExitCommon_member_declaration([NotNull] CSharpParser.Common_member_declarationContext context)
    {
        Reset();
    }

    private void Reset()
    {
        ActualModel = null;
    }
}