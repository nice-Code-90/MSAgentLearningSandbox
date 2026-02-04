using Microsoft.Extensions.AI;
using Shared;

namespace Shared.Extensions;

public static class UsageDetailsExtensions
{
    private static readonly string[] ReasoningKeys =
    [
        "OutputTokenDetails.ReasoningTokenCount",
        "ReasoningTokenCount",
        "reasoning_tokens"
    ];

    public static long? GetOutputTokensUsedForReasoning(this UsageDetails? usageDetails)
    {
        if (usageDetails?.AdditionalCounts == null) return 0;

        foreach (var key in ReasoningKeys)
        {
            if (usageDetails.AdditionalCounts.TryGetValue(key, out object? value))
                return Convert.ToInt64(value);
        }
        return 0;
    }

    public static void OutputAsInformation(this UsageDetails? usageDetails, string? messageText = null)
    {
        if (usageDetails == null) return;

        long reasoningCount = usageDetails.GetOutputTokensUsedForReasoning() ?? 0;
        string sourceInfo = "provided by API";

        if (reasoningCount == 0 && !string.IsNullOrEmpty(messageText))
        {
            int start = messageText.IndexOf("<think>");
            int end = messageText.IndexOf("</think>");

            if (start != -1 && end > start)
            {
                string thinkBlock = messageText.Substring(start + 7, end - (start + 7));
                int wordCount = thinkBlock.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
                reasoningCount = (long)(wordCount * 1.33);
                sourceInfo = "estimated (word count * 1.33)";
            }
        }

        Utils.WriteLineDarkGray($"- Input Tokens: {usageDetails.InputTokenCount}");
        Utils.WriteLineDarkGray($"- Output Tokens: {usageDetails.OutputTokenCount} " +
                                $"({reasoningCount} was {sourceInfo} for reasoning)");
    }
}