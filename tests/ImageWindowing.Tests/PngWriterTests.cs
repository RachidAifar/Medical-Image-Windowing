using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using ImageWindowing;

namespace ImageWindowing.Tests;

public class PngWriterTests
{
    [Fact]
    public void Write_ProducesValidHeaderAndDecodablePixels()
    {
        byte[] pixels = [0, 64, 128, 255, 10, 20];
        using var stream = new MemoryStream();

        PngWriter.Write(stream, pixels, width: 3, height: 2);
        var png = stream.ToArray();

        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, png[..8]);
        Assert.Equal("IHDR", Encoding.ASCII.GetString(png, 12, 4));
        Assert.Equal(3, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16)));
        Assert.Equal(2, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20)));
        Assert.Equal(pixels, DecodeScanlines(png, width: 3));
    }

    [Fact]
    public void Write_RejectsPixelCountMismatch()
    {
        using var stream = new MemoryStream();

        Assert.Throws<ArgumentException>(() => PngWriter.Write(stream, new byte[5], 3, 2));
    }

    private static byte[] DecodeScanlines(byte[] png, int width)
    {
        var offset = 8;
        while (offset < png.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(offset));
            var type = Encoding.ASCII.GetString(png, offset + 4, 4);
            if (type == "IDAT")
            {
                using var zlib = new ZLibStream(new MemoryStream(png, offset + 8, length), CompressionMode.Decompress);
                using var raw = new MemoryStream();
                zlib.CopyTo(raw);

                // Drop the filter byte at the start of every scanline.
                return raw.ToArray().Chunk(width + 1).SelectMany(line => line.Skip(1)).ToArray();
            }
            offset += 12 + length;
        }
        throw new InvalidDataException("No IDAT chunk found.");
    }
}
