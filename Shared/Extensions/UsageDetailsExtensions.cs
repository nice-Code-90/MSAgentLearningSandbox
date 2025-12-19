using Microsoft.Extensions.AI;
using Shared;

namespace Shared.Extensions;

public static class UsageDetailsExtensions
{
    private const string ReasonTokenCountKey = "OutputTokenDetails.ReasoningTokenCount";

    // Hagyományos Extension Method szintaxis
    public static long? GetOutputTokensUsedForReasoning(this UsageDetails? usageDetails)
    {
        if (usageDetails?.AdditionalCounts != null &&
            usageDetails.AdditionalCounts.TryGetValue(ReasonTokenCountKey, out object? value))
        {
            // Biztonságos típusátalakítás, mert az AdditionalCounts object-et tárol
            return Convert.ToInt64(value);
        }

        return 0; // Null helyett jobb a 0, ha nincs reasoning
    }

    public static void OutputAsInformation(this UsageDetails? usageDetails)
    {
        if (usageDetails == null)
        {
            return;
        }

        Utils.WriteLineDarkGray($"- Input Tokens: {usageDetails.InputTokenCount}");
        Utils.WriteLineDarkGray($"- Output Tokens: {usageDetails.OutputTokenCount} " +
                                $"({usageDetails.GetOutputTokensUsedForReasoning()} was used for reasoning)");
    }
}