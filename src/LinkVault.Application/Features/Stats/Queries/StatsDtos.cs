namespace LinkVault.Application.Features.Stats.Queries;

public record StatsOverviewDto(int TotalLinks, int FavoriteLinks, int TotalTags, int TotalCollections);
public record PopularTagDto(Guid Id, string Name, int LinkCount);
