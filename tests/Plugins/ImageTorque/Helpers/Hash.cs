using System.Security.Cryptography;
using System.Text;

namespace AyBorg.Plugins.ImageTorque.Tests;

internal static class Hash
{
    public static string Create(ReadOnlySpan<byte> imageData)
    {
        byte[] hashBytes = SHA256.HashData(imageData);
        var sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("x2"));
        }
        return sb.ToString();
    }
}
