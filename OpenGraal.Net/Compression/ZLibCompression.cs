using ICSharpCode.SharpZipLib.Zip.Compression;

namespace OpenGraal.Net.Compression;

public sealed class ZLibCompression
{
    private readonly Inflater _inflater = new();
    private readonly Deflater _deflater = new(Deflater.BEST_COMPRESSION, false);
    
    public int Compress(byte[] src, int srcOffset, int srcLen, byte[] dest, int destOffset, int destLen)
    {
        _deflater.Reset();
        _deflater.SetInput(src, srcOffset, srcLen);
        _deflater.Finish();
        
        return _deflater.Deflate(dest, destOffset, destLen);
    }
    
    public int Decompress(byte[] src, int srcOffset, int srcLen, byte[] dest, int destOffset, int destLen)
    {
        _inflater.Reset();
        _inflater.SetInput(src, srcOffset, srcLen);

        return _inflater.Inflate(dest, destOffset, destLen);
    }
}