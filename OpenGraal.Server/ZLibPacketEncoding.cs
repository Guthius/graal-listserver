using ICSharpCode.SharpZipLib.Zip.Compression;
using OpenGraal.Net;

namespace OpenGraal.Server;

public class ZLibPacketEncoding : IPacketEncoding
{
    private readonly Deflater _deflater = new(Deflater.BEST_COMPRESSION, false);
    
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

        dest[0] = (byte)((destLen >> 8) & 0xFF);
        dest[1] = (byte)(destLen & 0xFF);

        destLen += 2;
    }
}