using HashPeek;

namespace HashPeek.Tests;

public class HashCalculatorTests
{
    [Theory]
    [InlineData("abc",
        "a9993e364706816aba3e25717850c26c9cd0d89d",
        "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
        "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f",
        "900150983cd24fb0d6963f7d28e17f72",
        "352441c2",
        "44bc2cf5ad770999",
        "78af5f94892f3950",
        "6437b3ac38465133ffb63b75273a8db548c558465d79db03fd359c6cd5bd9d85")]
    [InlineData("",
        "da39a3ee5e6b4b0d3255bfef95601890afd80709",
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
        "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e",
        "d41d8cd98f00b204e9800998ecf8427e",
        "00000000",
        "ef46db3751d8e999",
        "2d06800538d394c2",
        "af1349b9f5f9a1a6a0404dea36dcc9499bcb25c9adc112b7cc9a93cae41f3262")]
    public void Compute_MatchesKnownVectors(string content, string sha1, string sha256, string sha512,
        string md5, string crc32, string xxHash64, string xxHash3, string blake3)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, content);

            var results = HashCalculator.Compute(path);
            var expected = new Dictionary<string, string>
            {
                ["SHA-1"] = sha1,
                ["SHA-256"] = sha256,
                ["SHA-512"] = sha512,
                ["MD5"] = md5,
                ["CRC32"] = crc32,
                ["xxHash64"] = xxHash64,
                ["XXH3_64"] = xxHash3,
                ["BLAKE3"] = blake3
            };

            Assert.Equal(expected.Count, results.Count);
            foreach (var result in results)
            {
                Assert.Equal(expected[result.Name], result.Value);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }
}
