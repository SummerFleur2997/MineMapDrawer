using System.IO.Compression;

namespace MapReader;

public static class Utilities
{
    public static byte[] ToLittleEndianBytes(this ushort value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] ToLittleEndianBytes(this uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    public static int HumanSort(this string source)
    {
        var digit = source.Where(char.IsDigit).ToArray();
        return int.Parse(new string(digit));
    }

    /// <summary>
    /// 使用 gzip 算法压缩字节数组。
    /// </summary>
    public static byte[] Compress(this byte[] data)
    {
        var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionMode.Compress))
            gzip.Write(data, 0, data.Length);

        return output.ToArray();
    }

    /// <summary>
    /// 使用 gzip 算法解压字节数组。
    /// </summary>
    public static byte[] Decompress(this byte[] data)
    {
        var gzip = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
        var output = new MemoryStream();
        gzip.CopyTo(output);
        return output.ToArray();
    }
}