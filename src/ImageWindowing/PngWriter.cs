using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace ImageWindowing;

/// <summary>Writes 8-bit grayscale PNG files without any third-party imaging library.</summary>
public static class PngWriter
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

    public static void Write(Stream stream, ReadOnlySpan<byte> pixels, int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width and height must be positive.");
        if (pixels.Length != width * height)
            throw new ArgumentException($"Expected {width * height} pixels but got {pixels.Length}.", nameof(pixels));

        stream.Write(Signature);

        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(0), width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);
        header[8] = 8; // bit depth
        header[9] = 0; // color type: grayscale
        WriteChunk(stream, "IHDR", header);

        // Each scanline starts with a filter-type byte (0 = none).
        var raw = new byte[(width + 1) * height];
        for (var row = 0; row < height; row++)
            pixels.Slice(row * width, width).CopyTo(raw.AsSpan(row * (width + 1) + 1));

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(raw);
        WriteChunk(stream, "IDAT", compressed.ToArray());

        WriteChunk(stream, "IEND", []);
    }

    public static void Write(string path, ReadOnlySpan<byte> pixels, int width, int height)
    {
        using var file = File.Create(path);
        Write(file, pixels, width, height);
    }

    private static void WriteChunk(Stream stream, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buffer, data.Length);
        stream.Write(buffer);

        var typeBytes = Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes);
        stream.Write(data);

        var crc = Crc32.Compute(typeBytes, data);
        BinaryPrimitives.WriteUInt32BigEndian(buffer, crc);
        stream.Write(buffer);
    }
}
