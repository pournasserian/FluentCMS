﻿namespace FluentCMS.Repositories.EFCore;

public class PluginRepository(FluentCmsDbContext dbContext, IApiExecutionContext apiExecutionContext) : SiteAssociatedRepository<Plugin>(dbContext, apiExecutionContext), IPluginRepository
{
    public async Task<IEnumerable<Plugin>> GetByPageId(Guid pageId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Plugins.Where(x => x.PageId == pageId).ToListAsync(cancellationToken);
    }

    public async Task<Plugin?> UpdateOrder(Guid pluginId, string section, int order, CancellationToken cancellationToken = default)
    {
        var plugin = await GetById(pluginId, cancellationToken);

        if (plugin != null)
        {
            plugin.Order = order;
            plugin.Section = section;

            return await Update(plugin, cancellationToken);
        }
        return default;
    }

    public async Task<Plugin?> UpdateCols(Guid pluginId, int cols, int colsMd, int colsLg, CancellationToken cancellationToken = default)
    {
        var plugin = await GetById(pluginId, cancellationToken);

        if (plugin != null)
        {
            plugin.Cols = cols;
            plugin.ColsMd = colsMd;
            plugin.ColsLg = colsLg;
            return await Update(plugin, cancellationToken);
        }
        return default;
    }
}
