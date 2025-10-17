namespace Crew.Contracts.Users;

public sealed record AddGuestbookEntryRequest(
    string Content,
    int? Rating);
