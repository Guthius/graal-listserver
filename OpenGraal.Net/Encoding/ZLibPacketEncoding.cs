using ICSharpCode.SharpZipLib.Zip.Compression;

namespace OpenGraal.Net.Encoding;

public sealed class ZLibPacketEncoding : IPacketEncoding
{
    private readonly Inflater _inflater = new();
    private readonly Deflater _deflater = new(Deflater.BEST_COMPRESSION, false);

    public int Decode(byte[] bytes, int offset, int count, byte[] dest, out int destLen)
    {
        destLen = 0;

        var start = offset;
        var end = offset + count;

        while (true)
        {
            var bytesLeft = end - offset;
            if (bytesLeft < 2)
            {
                break;
            }

            var len = (bytes[offset] << 8) | bytes[offset + 1];
            if (len + 2 > bytesLeft)
            {
                break;
            }

            _inflater.Reset();
            _inflater.SetInput(bytes, offset + 2, len);

            destLen += _inflater.Inflate(dest, destLen, dest.Length - destLen);

            offset += 2 + len;
        }

        return offset - start;
    }

    public void Encode(byte[] bytes, int offset, int count, byte[] dest, out int destLen)
    {
        if (count == 0)
        {
            destLen = 0;

            return;
        }

        _deflater.Reset();
        _deflater.SetInput(bytes, offset, count);
        _deflater.Finish();

        destLen = _deflater.Deflate(dest, 2, dest.Length - 2);

        dest[0] = (byte) ((destLen >> 8) & 0xFF);
        dest[1] = (byte) (destLen & 0xFF);

        destLen += 2;
    }
}