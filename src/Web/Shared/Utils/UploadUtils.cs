/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
                    resizedImage.Save(resizedFileStream, "jpeg");
                }
                else
                {
                    resizedImage.Save(resizedFileStream, "png");
                }
                byte[] data = new byte[resizedFileStream.Length];
                resizedFileStream.Position = 0;
                _ = await resizedFileStream.ReadAsync(data);
                resizedFileStream.Close();
                string hashValue = CreateHash(data);
                return new ImageSource(data, file.ContentType, hashValue) { Width = resizedImage.Width, Height = resizedImage.Height };
            }
            else
            {
                byte[] data = new byte[file.Size];
                fileStream.Position = 0;
                _ = await fileStream.ReadAsync(data);
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
