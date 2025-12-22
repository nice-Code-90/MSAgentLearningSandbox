using Microsoft.Extensions.AI;
using Shared;

namespace Shared.Extensions;

public static class UsageDetailsExtensions
{
    private const string ReasonTokenCountKey = "OutputTokenDetails.ReasoningTokenCount";

    public static long? GetOutputTokensUsedForReasoning(this UsageDetails? usageDetails)
    {
        if (usageDetails?.AdditionalCounts != null &&
            usageDetails.AdditionalCounts.TryGetValue(ReasonTokenCountKey, out object? value))
        {
            return Convert.ToInt64(value);
        }
        return 0;
    }

    public static void OutputAsInformation(this UsageDetails? usageDetails, string? rawResponse = null)
    {
        if (usageDetails == null) return;

        long reasoningCount = usageDetails.GetOutputTokensUsedForReasoning() ?? 0;

        if (reasoningCount == 0 && !string.IsNullOrEmpty(rawResponse))
        {
            int start = rawResponse.IndexOf("<think>");
            if (start != -1)
            {
                int end = rawResponse.IndexOf("</think>");
                string thinkBlock;

                if (end > start)
                {

                    thinkBlock = rawResponse.Substring(start, end - start);
                }
                else
                {

                    thinkBlock = rawResponse.Substring(start);
                }

                reasoningCount = thinkBlock.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            }
        }

        Utils.WriteLineDarkGray($"- Input Tokens: {usageDetails.InputTokenCount}");
        Utils.WriteLineDarkGray($"- Output Tokens: {usageDetails.OutputTokenCount} " +
                                $"({reasoningCount} was {(usageDetails.GetOutputTokensUsedForReasoning() > 0 ? "used" : "estimated")} for reasoning)");
    }
}