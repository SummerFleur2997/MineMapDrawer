using System.Xml;
using JetBrains.Annotations;

namespace MapReader;

/// <summary>
/// 地图类，用于表示和管理游戏地图数据。
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class Map
{
    /// <summary>
    /// 地图索引。
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 地图宽度。
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// 地图高度
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Back 图层数据
    /// </summary>
    public int[,] BackLayerData { get; set; }

    /// <summary>
    /// Buildings 图层数据
    /// </summary>
    public int[,] BuildingsLayerData { get; set; }

    /// <summary>
    /// Front 图层数据
    /// </summary>
    public int[,] FrontLayerData { get; set; }

    /// <summary>
    /// 通过直接传入数据创建地图。
    /// </summary>
    /// <param name="index">地图索引</param>
    /// <param name="width">地图宽度</param>
    /// <param name="height">地图高度</param>
    /// <param name="back">Back 图层数据</param>
    /// <param name="building">Buildings 图层数据</param>
    /// <param name="front">Front 图层数据</param>
    public Map(int index, int width, int height, int[,] back, int[,] building, int[,] front)
    {
        Index = index;
        Width = width;
        Height = height;
        BackLayerData = back;
        BuildingsLayerData = building;
        FrontLayerData = front;
    }

    /// <summary>
    /// 从文件路径加载地图。
    /// </summary>
    /// <param name="filePath">地图文件路径。</param>
    public Map(string filePath)
    {
        var xml = new XmlDocument();
        xml.Load(filePath);

        // 获取layer的宽度和高度
        var map = xml.SelectSingleNode("//map");
        if (map is null)
            throw new FileLoadException("This file seems not to be a valid map file.");
        Width = int.Parse(map.Attributes?["width"]?.Value ?? "0");
        Height = int.Parse(map.Attributes?["height"]?.Value ?? "0");
        if (Width == 0 || Height == 0)
            throw new ArgumentException("Invalid width and height attributes.");

        // 查找指定名称的layer节点
        var layers = xml.SelectNodes("//layer");
        if (layers is null)
            throw new FileLoadException("Cannot find any available layers!");

        BackLayerData = GetLayer("Back");
        BuildingsLayerData = GetLayer("Buildings");
        FrontLayerData = GetLayer("Front");
        Index = int.Parse(new string(filePath.Where(char.IsDigit).ToArray()));

        return;

        // 获取指定名称的图层数据
        int[,] GetLayer(string layerName)
        {
            var layerNode = layers
                .OfType<XmlNode>()
                .FirstOrDefault(n => n.Attributes?["name"]?.Value == layerName);
            if (layerNode is null)
                throw new ArgumentException($"Layer with name '{layerName}' not found in the XML file.");

            // 获取data节点的内容
            var dataNode = layerNode.SelectSingleNode("data");
            if (dataNode is null)
                throw new ArgumentException($"Data node not found in layer '{layerName}'.");

            // 解析CSV格式的数据
            var data = dataNode.InnerText.Trim();
            var rows = data
                .Split("\r\n")
                .Select(s => s.TrimEnd(',').Split(","))
                .ToArray();

            // 创建二维数组并填充数据
            var result = new int[Height, Width];
            for (var i = 0; i < Height; i++)
            for (var j = 0; j < Width; j++)
                result[i, j] = int.Parse(rows[i][j]);

            return result;
        }
    }

    /// <summary>
    /// 从二进制流解析地图。
    /// </summary>
    /// <param name="stream">包含地图数据的二进制流。</param>
    public Map(Stream stream)
    {
        var reader = new BinaryReader(stream);
        Index = reader.ReadByte();
        Width = reader.ReadByte();
        Height = reader.ReadByte();
        reader.ReadByte();

        BackLayerData = new int[Height, Width];
        BuildingsLayerData = new int[Height, Width];
        FrontLayerData = new int[Height, Width];

        foreach (var layer in new List<int[,]> { BackLayerData, BuildingsLayerData, FrontLayerData })
            for (var i = 0; i < Height; i++)
            for (var j = 0; j < Width; j++)
                layer[i, j] = reader.ReadUInt16();
    }

    /// <summary>
    /// 将地图数据编码为二进制数组。
    /// </summary>
    /// <returns>包含编码后地图数据的二进制数组。</returns>
    internal byte[] Encode()
    {
        var binary = new List<byte> { (byte)Index, (byte)Width, (byte)Height, 0x00 };
        foreach (var layer in new List<int[,]> { BackLayerData, BuildingsLayerData, FrontLayerData })
            for (ushort i = 0; i < Height; i++)
            for (ushort j = 0; j < Width; j++)
                binary.AddRange(((ushort)layer[i, j]).ToLittleEndianBytes());

        return binary.ToArray();
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class MapDataHeader(ushort index, ushort length, uint offset)
{
    public const int BINARY_LENGTH = 8;
    public ushort Index { get; set; } = index;
    public ushort Length { get; set; } = length;
    public uint Offset { get; set; } = offset;

    public byte[] Encode()
    {
        var binary = new List<byte>();

        binary.AddRange(Index.ToLittleEndianBytes());
        binary.AddRange(Length.ToLittleEndianBytes());
        binary.AddRange(Offset.ToLittleEndianBytes());

        return binary.ToArray();
    }

    public void Align(int value) => Offset = (uint)(Offset + value);
}