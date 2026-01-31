using MapReader;

namespace MapDrawer;

public static class Program
{
    private const int TotalActions = 1180;

    public static int CompletedActions
    {
        get => _completedActions;
        set
        {
            lock (Lock)
            {
                _completedActions = value;
            }
        }
    }

    private static int _completedActions;
    private static int _threadAmount;

    private static readonly object Lock = new();

    public static void Main()
    {
        _threadAmount = Environment.ProcessorCount;

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

        CompletedActions = 0;
        Console.WriteLine($"使用多线程创建任务成功，当前线程数：{_threadAmount}");

        var progressTask = Task.Run(DisplayProgress);
        ParallelRun(tileSets, t => t.CreateMap(ResourceHelper.AllMaps));
        progressTask.Wait();
        Console.Write("完成！按任意键退出");
        Console.ReadKey();
    }

    /// <summary>
    /// 显示进度条
    /// </summary>
    private static void DisplayProgress()
    {
        var lastPercent = 0;

        while (CompletedActions < TotalActions)
        {
            lock (Lock)
            {
                var percent = (int)(CompletedActions * 100.0 / TotalActions);

                if (percent > lastPercent)
                {
                    Console.Write("\r");
                    var filledBlocks = percent / 2; 
                    Console.Write($"进度: [{new string('█', filledBlocks)}{new string(' ', 50 - filledBlocks)}] {percent}%");
                    lastPercent = percent;
                }
            }

            Thread.Sleep(200);
        }

        Console.Write("\r");
        Console.Write($"进度: [{new string('█', 50)}] 100%\n");
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
            new ParallelOptions { MaxDegreeOfParallelism = _threadAmount },
            action
        );
    }
}