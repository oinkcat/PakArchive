using System;
using System.IO;
using Xunit;

namespace PakArchive.Tests
{
    /// <summary>
    /// Тестирование операций с файлом архива PAK
    /// </summary>
    public class PakFileTets
    {
        private const string TestArchivePath = @"D:\Games\Quake 2\baseq2\pak0.pak";

        private const string TestOutputDir = @"C:\Temp";

        /// <summary>
        /// Тестирование возможности открытия корректных PAK-файлов
        /// </summary>
        [Fact]
        public void TestOpenCorrectArchive()
        {
            using var testPak = new PakFile(TestArchivePath);

            testPak.Open();

            Assert.True(testPak.IsOpen);
            Assert.NotNull(testPak.Contents);
            Assert.NotEmpty(testPak.Contents);
        }

        [Fact]
        public void TestExtractRandomEntry()
        {
            using var testPak = new PakFile(TestArchivePath);

            testPak.Open();

            var rng = new Random();
            var entryToExtract = testPak.Contents[rng.Next(testPak.Contents.Length)];

            string testName = entryToExtract.Name.Replace("/", "#");
            var entryData = testPak.ReadEntryData(entryToExtract);

            File.WriteAllBytes(Path.Combine(TestOutputDir, testName), entryData);

            Assert.NotEmpty(entryData);
        }

        [Fact]
        public void TestReadRandomEntryStream()
        {
            const int BufferSize = 1024;

            using var testPak = new PakFile(TestArchivePath);

            testPak.Open();

            var rng = new Random();
            var entryToRead = testPak.Contents[rng.Next(testPak.Contents.Length)];

            using(var entryStream = testPak.OpenEntryStream(entryToRead))
            {
                int totalBytesRead = 0;
                int bytesRead = 0;
                var buffer = new byte[BufferSize];

                do
                {
                    bytesRead = entryStream.Read(buffer, 0, buffer.Length);
                    totalBytesRead += bytesRead;
                }
                while (bytesRead > 0);

                Assert.Equal(entryToRead.Size, totalBytesRead);
            }
        }

        [Fact]
        public void VerifyEntryReadsCorrectly()
        {
            const int BufferSize = 1024;

            using var testPak = new PakFile(TestArchivePath);

            testPak.Open();

            var rng = new Random();
            var entryToRead = testPak.Contents[rng.Next(testPak.Contents.Length)];

            var entryContentsWhole = testPak.ReadEntryData(entryToRead);
            var entryContetnsToTest = new byte[entryToRead.Size];

            using (var entryStream = testPak.OpenEntryStream(entryToRead))
            {
                int bytesRead = 0;
                int offset = 0;

                do
                {
                    bytesRead = entryStream.Read(entryContetnsToTest, offset, BufferSize);
                    offset += bytesRead;
                }
                while (bytesRead > 0);
            }

            for(int i = 0; i < entryToRead.Size; i++)
            {
                Assert.Equal(entryContetnsToTest[i], entryContentsWhole[i]);
            }
        }
    }
}
