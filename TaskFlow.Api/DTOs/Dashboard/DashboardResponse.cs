namespace TaskFlow.Api.DTOs.Dashboard;

public record ProjectSummary(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    int BoardCount,
    int TaskCount,
    int MemberCount
);

public record DashboardStatsResponse(
    int ProjectCount,
    int BoardCount,
    int TaskCount,
    List<ProjectSummary> RecentProjects,
    List<ProjectSummary> Projects
);