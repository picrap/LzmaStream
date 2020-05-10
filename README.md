# LzmaStream
Trying to get LZMA mainstream

As of today, an experiment to get a simple `LzmaStream`, similar to `GZipStream`

## How to use it

To decompress data:
```csharp
// packedStream is the LZMA packed source stream
using (var lzmaStream = new LzmaStream(packedStream, CompressionMode.Decompress))
{
    lzmaStream.CopyTo(unpackTarget);
    // or
    lzmaStream.Read(unpackData, 0, unpackData.Length);
}
```
