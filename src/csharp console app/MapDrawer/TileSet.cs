using MapReader;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MapDrawer;

public class TileSet : IDisposable
{
    /// <summary>
    /// 图块图像列表
    /// </summary>
    private List<Image<Rgba32>> Tiles { get; }
    private Image<Rgba32> Transparent { get; } = new(16, 16, new Rgba32(0, 0, 0, 0));
    private string TileID { get; }

    /// <summary>
    /// 从文件路径加载图块集
    /// </summary>
    /// <param name="tileID">图块集 ID</param>
    public TileSet(string tileID)
    {
        Tiles = new List<Image<Rgba32>>();
        TileID = tileID;
        var path = Path.Combine("assets", string.Concat(tileID, ".png"));
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"未找到 asset 文件夹下的 {tileID}，请按照 README 的指示进行操作。");
        }
        var image = Image.Load<Rgba32>(path);

        // 计算图块集的行列数
        var columns = image.Width / 16;
        var rows = image.Height / 16;

        // 分割图块集
        for (var y = 0; y < rows; y++)
        for (var x = 0; x< columns; x++)
        {
            var sourceRectangle = new Rectangle(x * 16, y * 16, 16, 16);
            var tile = image.Clone(ctx => ctx.Crop(sourceRectangle));
            Tiles.Add(tile);
        }
    }

    /// <summary>
    /// 根据二维索引数组拼接完整的图片
    /// </summary>
    /// <param name="maps">包含二维图块索引数组的地图集</param>
    /// <returns>拼接后的完整图片</returns>
    public void CreateMap(List<Map> maps)
    {
        foreach (var map in maps)
        {
            // 获取地图尺寸
            var width = map.Width;
            var height = map.Height;

            // 创建目标图片
            var result = new Image<Rgba32>(width * 16, height * 16);

            // 定义要绘制的图层顺序
            var layers = new[] { map.BackLayerData, map.BuildingsLayerData, map.FrontLayerData };

            // 按顺序绘制每个图层
            foreach (var layerData in layers)
            {
                for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var index = layerData[y, x];
                    if (index >= Tiles.Count)
                        continue;

                    // 获取对应图块并将图块绘制到目标位置
                    var tile = GetTile(index);
                    var point = new Point(x * 16, y * 16);
                    result.Mutate(ctx => ctx.DrawImage(
                        tile, point, 1f));
                }
            }

            Directory.CreateDirectory(Path.Combine("output", TileID));
            result.Save(Path.Combine("output", TileID, $"{map.Index}.png"));
            Program.IncrementCompletedActions();
        }
    }

    /// <summary>
    /// 获取指定索引的图块
    /// </summary>
    /// <param name="index">图块索引</param>
    /// <returns>图块图像</returns>
    private Image<Rgba32> GetTile(int index)
    {
        if (index >= Tiles.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return index == 0 ? Transparent : Tiles[index - 1];
    }

    /// <summary>
    /// 释放所有图块资源
    /// </summary>
    public void Dispose()
    {
        foreach (var tile in Tiles)
            tile.Dispose();
        Tiles.Clear();
        GC.SuppressFinalize(this);
    }
}