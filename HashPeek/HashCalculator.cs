using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Security.Cryptography;
using Blake2Fast;

namespace HashPeek;

public sealed class HashResult
{
    public HashResult(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public string Value { get; }
}

public static class HashCalculator
{
    private const int BufferSize = 1024 * 1024;

    public static IReadOnlyList<HashResult> Compute(string filePath, IProgress<int>? progress = null)
    {
        using var sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var sha512 = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
        using var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        var crc32 = new Crc32();
        var xxHash64 = new XxHash64();
        var xxHash3 = new XxHash3();
        var blake3 = Blake3.CreateIncrementalHasher();

        var totalBytes = new FileInfo(filePath).Length;
        var buffer = new byte[BufferSize];
        long bytesRead = 0;

        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BufferSize, FileOptions.SequentialScan))
        {
            int count;
            while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var chunk = new ReadOnlySpan<byte>(buffer, 0, count);
                sha1.AppendData(buffer, 0, count);
                sha256.AppendData(buffer, 0, count);
                sha512.AppendData(buffer, 0, count);
                md5.AppendData(buffer, 0, count);
                crc32.Append(chunk);
                xxHash64.Append(chunk);
                xxHash3.Append(chunk);
                blake3.Update(chunk);

                bytesRead += count;
                if (totalBytes > 0)
                {
                    progress?.Report((int)(bytesRead * 100.0 / totalBytes));
                }
            }
        }

        progress?.Report(100);

        return new[]
        {
            new HashResult("SHA-1", ToHex(sha1.GetHashAndReset())),
            new HashResult("SHA-256", ToHex(sha256.GetHashAndReset())),
            new HashResult("SHA-512", ToHex(sha512.GetHashAndReset())),
            new HashResult("MD5", ToHex(md5.GetHashAndReset())),
            new HashResult("CRC32", ToHexBigEndian(crc32.GetCurrentHash())),
            new HashResult("xxHash64", ToHex(xxHash64.GetCurrentHash())),
            new HashResult("XXH3_64", ToHex(xxHash3.GetCurrentHash())),
            new HashResult("BLAKE3", ToHex(blake3.Finish()))
        };
    }

    // Crc32 returns little-endian bytes; common crc32 tools print big-endian.
    private static string ToHexBigEndian(byte[] bytes)
    {
        Array.Reverse(bytes);
        return ToHex(bytes);
    }

    private static string ToHex(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
    }
}
