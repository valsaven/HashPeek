using HashPeek;

namespace HashPeek.Tests;

public class HashCalculatorTests
{
    [Theory]
    [InlineData("abc",
        "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
        "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f",
        "900150983cd24fb0d6963f7d28e17f72",
        "352441c2",
        "44bc2cf5ad770999")]
    [InlineData("",
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
        "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e",
        "d41d8cd98f00b204e9800998ecf8427e",
        "00000000",
        "ef46db3751d8e999")]
    public void Compute_MatchesKnownVectors(string content, string sha256, string sha512, string md5, string crc32, string xxHash64)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, content);

            var results = HashCalculator.Compute(path);
            var expected = new Dictionary<string, string>
            {
                ["SHA-256"] = sha256,
                ["SHA-512"] = sha512,
                ["MD5"] = md5,
                ["CRC32"] = crc32,
                ["xxHash64"] = xxHash64
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
