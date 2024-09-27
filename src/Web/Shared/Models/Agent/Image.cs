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

using AyBorg.SDK.Common.Models;
using ImageTorque;

namespace AyBorg.Web.Shared.Models;

public sealed record Image
{
    /// <summary>
    /// Gets the width of the image.
    /// </summary>
    public int Width => Meta.Width;

    /// <summary>
    /// Gets the height of the image.
    /// </summary>
    public int Height => Meta.Height;

    /// <summary>
    /// Gets the scaled width of the image.
    /// </summary>
    public int ScaledWidth { get; init; }

    /// <summary>
    /// Gets the scaled height of the image.
    /// </summary>
    public int ScaledHeight { get; init; }

    /// <summary>
    /// Gets the pixel format of the image.
    /// </summary>
    public PixelFormat PixelFormat => Meta.PixelFormat;

    /// <summary>
    /// Gets the meta data of the image.
    /// </summary>
    public ImageMeta Meta { get; init; }

    /// <summary>
    /// Gets the base64 encoded image.
    /// </summary>
    public string Base64 { get; init; } = string.Empty;

    /// <summary>
    /// Gets the encoder type.
    /// </summary>
    public string EncoderType  { get; init; } = "png";
}
