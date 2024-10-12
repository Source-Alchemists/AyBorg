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

using AyBorg.Plugins.ZXing.Models;
using AyBorg.Types.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AyBorg.Plugins.ZXing.Tests
{
    public sealed class ImageCodeReadTests
    {
        private readonly NullLogger<ImageCodeRead> _logger = new();

        [Fact]
        public async Task Test_TryRunAsync_Success()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/Code-128.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.CODE_128;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal("ABC-abc-1234", (readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value.First());
        }

        [Fact]
        public async Task Test_TryRunAsync_ReadAllFormats_Success()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/Code-128.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.All;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal("ABC-abc-1234", (readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value.First());
        }

        [Fact]
        public async Task Test_TryRunAsync_ReadAllBarcodeFormats_Success()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/Code-128.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.All_Barcodes;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal("ABC-abc-1234", (readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value.First());
        }

        [Fact]
        public async Task Test_TryRunAsync_ReadAllMatrixBarcodeFormats_Success()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/qr-code.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.All_MatrixBarcodes;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal("https://123TestTest567", (readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value.First());
        }

        [Fact]
        public async Task Test_TryRunAsync_WrongFormat_Fail()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/qr-code.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.CODE_128;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Empty((readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value);
        }

        [Fact]
        public async Task Test_TryRunAsync_NoBarcode_Fail()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/Stickman.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.CODE_128;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Empty((readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value);
        }
    }
}
