using MapReader;

namespace MapDrawer;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            var tileSets = new List<TileSet>();
            var styles = new[]
            {
                TileResources.Style.Common,
                TileResources.Style.Frost,
                TileResources.Style.Lava,
                TileResources.Style.Desert
            };

            foreach (var style in styles)
            {
                tileSets.Add(new TileSet(TileResources.GetResource(style)));
                tileSets.Add(new TileSet(TileResources.GetResource(style, dark: true)));
                tileSets.Add(new TileSet(TileResources.GetResource(style, dangerous: true)));
                tileSets.Add(new TileSet(TileResources.GetResource(style, dark: true, dangerous: true)));
            }

            tileSets.Add(new TileSet(TileResources.GetResource(TileResources.Style.Slime)));
            tileSets.Add(new TileSet(TileResources.GetResource(TileResources.Style.Slime, dangerous: true)));
            tileSets.Add(new TileSet(TileResources.GetResource(TileResources.Style.Dinosaur)));
            tileSets.Add(new TileSet(TileResources.GetResource(TileResources.Style.Quarry)));

            ParallelRun(tileSets, t => t.CreateMap(ResourceHelper.AllMaps));
            return;
        }

        if (args.Length == 1 && args[0] == "export")
        {
            ResourceHelper.ConvertToBinary();
        }
    }

        
    /// <summary>
    /// 以并行方式对集合中的每个元素执行指定操作，最大线程数基于处理器线程数。
    /// 若任意元素操作的退出值为 99 或 -1，则停止整个操作并记录错误。
    /// </summary>
    /// <param name="items">需要并行处理的元素集合</param>
    /// <param name="action">元素的操作函数</param>
    private static void ParallelRun(IEnumerable<TileSet> items, Action<TileSet> action)
    {
        Parallel.ForEach(
            items,
            new ParallelOptions { MaxDegreeOfParallelism = 20 },
            action
        );
    }
}