using System.Security.Cryptography;
using System.Text;
using AyBorg.Web.Shared.Models;
using ImageTorque;
using Microsoft.AspNetCore.Components.Forms;

namespace AyBorg.Web.Shared.Utils;

public static class UploadUtils
{
    const int MAX_FILE_SIZE = 51200000; // 50MB

    public static async ValueTask<ImageSource> CreateImageSourceAsync(IBrowserFile file, int MaxSize = 2160)
    {
        // Uploaded files should not be directly used in memory according to Microsoft, so we save them first on disk:
        // https://learn.microsoft.com/en-us/aspnet/core/blazor/file-uploads?view=aspnetcore-6.0&pivots=server
        string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            await using var fileStream = new FileStream(tempFilePath, FileMode.Create);
            await file.OpenReadStream(maxAllowedSize: MAX_FILE_SIZE).CopyToAsync(fileStream);
            fileStream.Position = 0;
            using ImageTorque.Image image = ImageTorque.Image.Load(fileStream);
            if (MaxSize > 0 && (image.Width > MaxSize || image.Height > MaxSize))
            {
                image.CalculateClampSize(MaxSize, out int newWidth, out int newHeight);
                using ImageTorque.Image resizedImage = image.Resize(newWidth, newHeight);
                await using var resizedFileStream = new FileStream(tempFilePath, FileMode.Create);
                if (file.ContentType.EndsWith("jpeg") || file.ContentType.EndsWith("jpg"))
                {
                    resizedImage.Save(resizedFileStream, ImageTorque.Processing.EncoderType.Jpeg);
                }
                else
                {
                    resizedImage.Save(resizedFileStream, ImageTorque.Processing.EncoderType.Png);
                }
                byte[] data = new byte[resizedFileStream.Length];
                resizedFileStream.Position = 0;
                await resizedFileStream.ReadAsync(data);
                resizedFileStream.Close();
                string hashValue = CreateHash(data);
                return new ImageSource(data, file.ContentType, hashValue) { Width = resizedImage.Width, Height = resizedImage.Height };
            }
            else
            {
                byte[] data = new byte[file.Size];
                fileStream.Position = 0;
                await fileStream.ReadAsync(data);
                fileStream.Close();
                string hashValue = CreateHash(data);
                return new ImageSource(data, file.ContentType, hashValue) { Width = image.Width, Height = image.Height };
            }
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    private static string CreateHash(byte[] data)
    {
        byte[] hashBytes = SHA256.HashData(data);
        var stringBuilder = new StringBuilder();
        for (int index = 0; index < hashBytes.Length; index++)
        {
            stringBuilder.Append(hashBytes[index].ToString("x2"));
        }

        string hashValue = stringBuilder.ToString();
        return hashValue;
    }
}
