# LzmaStream
Trying to get LZMA mainstream  :stuck_out_tongue_winking_eye:

Available as a [![NuGet package](http://img.shields.io/nuget/v/LzmaStream.svg?style=flat-square)](https://www.nuget.org/packages/LzmaStream) package.

## What is it?

A `Stream` that works exactly as you would expect: read from it and unpack or write to it and pack (the same way you would do with `GZipStream`)

## How to use it

To decompress data:
```csharp
// packedStream is the LZMA packed source stream
using (var lzmaStream = new LzmaStream(packedStream, CompressionMode.Decompress))
{
    lzmaStream.CopyTo(unpackTarget);
    // or
    // lzmaStream.Read(unpackData, 0, unpackData.Length);
}
```

… And to compress it:
```csharp
// rawStream is a stream containing data to be packed
using (var lzmaStream = new LzmaStream(rawStream, CompressionMode.Compress))
// or
// using (var lzmaStream = new LzmaStream(rawStream, LzmaCompressionParameters.Defaut /* or .Optimal or .Fast or custom… */))
{
    lzmaStream.CopyTo(packTarget);
    // or
    // lzmaStream.Write(rawData, 0, rawData.Length);
}
```


## Credits

This library uses unmodified source code from https://www.7-zip.org/sdk.html ([from another repository](https://github.com/picrap/lzma-sdk))
