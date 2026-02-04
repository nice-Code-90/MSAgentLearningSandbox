using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.Text;

namespace AdvancedRAGTechniques.Utils;

public static class Middleware
{
    public static async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        StringBuilder sb = new($"- Tool Call: '{context.Function.Name}'");
        if (context.Arguments.Count > 0)
            sb.Append($" ({string.Join(", ", context.Arguments.Select(x => $"{x.Key}={x.Value}"))})");
        
        Shared.Utils.WriteLineDarkGray(sb.ToString());
        return await next(context, cancellationToken);
    }
}