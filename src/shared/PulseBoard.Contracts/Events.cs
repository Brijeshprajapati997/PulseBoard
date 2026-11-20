namespace PulseBoard.Contracts.Events;

public record PullRequestEvent(
    string RepoName,
    string Action,
    string Author,
    int Number,
    DateTimeOffset OccurredAt
);

public record CommitEvent(
    string RepoName,
    string Author,
    string Message,
    DateTimeOffset OccurredAt
);

public record IssueEvent(
    string RepoName,
    string Action,
    string Author,
    int Number,
    DateTimeOffset OccurredAt
);
