namespace MapReader;

public static class ResourceHelper
{
    public static readonly List<Map> AllMaps;

    static ResourceHelper()
    {
        AllMaps = Directory
            .GetFiles("maps", "*.xml")
            .OrderBy(s => s.HumanSort())
            .Select(path => new Map(path))
            .ToList();
    }

    /// <summary>
    /// 编码数据为自定义的二进制格式。
    /// </summary>
    public static void ConvertToBinary()
    {
        var allMaps = Directory
            .GetFiles("maps", "*.xml")
            .OrderBy(s => s.HumanSort())
            .Select(path => new Map(path))
            .ToList();

        // 存储边界信息的数组，格式为：索引、长度、偏移量。
        var record = new List<MapDataHeader>();

        // 存储地图二进制信息的列表。
        var body = new List<byte>();

        // 读取所有地图文件并编码。
        foreach (var map in allMaps)
        {
            var binary = map.Encode();
            record.Add(new MapDataHeader((ushort)map.Index, (ushort)binary.Length, (uint)body.Count));
            body.AddRange(binary);
        }

        // 更新头部表格偏移量信息。
        var head = new List<byte> { 0x4D, 0x41, 0x50, 0x53, 0x3B, 0x00, 0x00, 0x00 }; // MAPS 59
        foreach (var header in record)
            header.Align(record.Count * MapDataHeader.BINARY_LENGTH + 8);

        var table = record
            .Select(h => h.Encode())
            .SelectMany(h => h);

        var buffer = head.Concat(table).Concat(body).ToArray();

        // 将二进制数据压缩并编码为 base 64，然后分别写入文件。
        var compressedData = buffer.Compress();
        var base64String = Convert.ToBase64String(compressedData);

        File.WriteAllBytes(".maps", compressedData);
        File.WriteAllText("maps_base64.txt", base64String);
    }

#if DEBUG

    /// <summary>
    /// 从二进制文件解码地图数据。
    /// </summary>
    /// <returns>解码后的地图列表。</returns>
    public static List<Map> DecodeFromBinary()
    {
        // 读取并解压文件数据
        var buffer = File.ReadAllBytes(".maps").Decompress();
    
        // 验证文件头
        if (buffer.Length < 8 || 
            buffer[0] != 0x4D || buffer[1] != 0x41 || 
            buffer[2] != 0x50 || buffer[3] != 0x53)
            throw new InvalidDataException("Invalid map file format.");

        // 读取头部表格
        var headers = new List<MapDataHeader>();
        for (var i = 0; i < 59; i++)
        {
            var offset = 8 + i * MapDataHeader.BINARY_LENGTH;
            var index = BitConverter.ToUInt16(buffer, offset);
            var length = BitConverter.ToUInt16(buffer, offset + 2);
            var offsetValue = BitConverter.ToUInt32(buffer, offset + 4);
            headers.Add(new MapDataHeader(index, length, offsetValue));
        }

        // 解码地图数据
        var maps = new List<Map>();
        foreach (var header in headers)
        {
            using var stream = new MemoryStream(buffer, (int)header.Offset, header.Length);
            maps.Add(new Map(stream));
        }

        return maps.OrderBy(m => m.Index).ToList();
    }

#endif
}