namespace ModelWorld.Models;

public sealed record EnterpriseChatUsageProfile(
    string Name,
    int EmployeeCount,
    decimal DailyActiveUserRate,
    int ChatsPerActiveUserPerWorkday,
    int WorkdaysPerMonth,
    int AverageInputTokensPerChat,
    int AverageOutputTokensPerChat)
{
    public static EnterpriseChatUsageProfile MediumCorporate { get; } = new(
        Name: "Medium corporate chat app",
        EmployeeCount: 1_500,
        DailyActiveUserRate: 0.35m,
        ChatsPerActiveUserPerWorkday: 12,
        WorkdaysPerMonth: 22,
        AverageInputTokensPerChat: 1_200,
        AverageOutputTokensPerChat: 500);

    public int DailyActiveUsers =>
        (int)Math.Round(EmployeeCount * DailyActiveUserRate, MidpointRounding.AwayFromZero);

    public int MonthlyChatCount =>
        DailyActiveUsers * ChatsPerActiveUserPerWorkday * WorkdaysPerMonth;

    public decimal MonthlyInputTokens =>
        MonthlyChatCount * AverageInputTokensPerChat;

    public decimal MonthlyOutputTokens =>
        MonthlyChatCount * AverageOutputTokensPerChat;
}