using System.Security.Cryptography;
using System.Text;

namespace ServiceC.Services;

public class ShortCodeService(IConfiguration configuration)
{
    private readonly string prefix = configuration["ShortCodeConfig:Prefix"] ?? string.Empty;

    public string Generate(string input)
    {
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        var shortCode = Convert.ToBase64String(hashBytes);

        return $"{prefix}-{shortCode}";
    }
}
