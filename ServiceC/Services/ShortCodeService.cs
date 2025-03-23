using System.Security.Cryptography;
using System.Text;

namespace ServiceC.Services;

public class ShortCodeService
{
    public string Generate(string input)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
