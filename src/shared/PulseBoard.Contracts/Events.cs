namespace PulseBoard.Contracts.Events;

public record PullRequestEvent(
    string OrgName,       // GitHub org login, e.g. "dyad-tech" (empty for personal repos)
    string RepoName,
    string Action,        // opened, closed, merged
    string Author,
    int Number,
    DateTimeOffset OccurredAt
);

public record CommitEvent(
    string OrgName,
    string RepoName,
    string Author,
    string Message,
    DateTimeOffset OccurredAt
);

public record IssueEvent(
    string OrgName,
    string RepoName,
    string Action,        // opened, closed, reopened
    string Author,
    int Number,
    DateTimeOffset OccurredAt
);
