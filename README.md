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

## Credits

This library uses unmodified source code from https://www.7-zip.org/sdk.html ([from another repository](https://github.com/picrap/lzma-sdk))
