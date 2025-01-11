using System;
using System.IO;
using Xunit;

namespace PakArchive.Tests
{
    /// <summary>
    /// Тестирование операций с файлом архива PAK
    /// </summary>
    public class PakFileTests
    {
        private const string TestArchivePath = @"D:\Games\Quake 2\baseq2\pak0.pak";

        private const string TestOutputDir = @"C:\Temp";

        private const string TestSourceDir = @"..\..\..\TestData";

        /// <summary>
        /// Тестирование возможности открытия корректных PAK-файлов
        /// </summary>
        [Fact]
        public void TestOpenCorrectArchive()
        {
            using var testPak = new PakFile(TestArchivePath);

            testPak.Open();

            Assert.True(testPak.IsOpen);
            Assert.True(testPak.IsFromFile);
            Assert.NotNull(testPak.Contents);
            Assert.NotEmpty(testPak.Contents);
        }

        /// <summary>
        /// Тестирование открытия корректного PAK-архива из потока данных
        /// </summary>
        [Fact]
        public void TestOpenCorrectArchiveStream()
        {
            using var archiveStream = File.OpenRead(TestArchivePath);
            using var testPak = new PakFile(archiveStream);

            testPak.Open();

            Assert.True(testPak.IsOpen);
            Assert.False(testPak.IsFromFile);
            Assert.NotNull(testPak.Contents);
            Assert.NotEmpty(testPak.Contents);
        }

        /// <summary>
        /// Тестирование извлечения случайного файла
        /// </summary>
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

        /// <summary>
        /// Тестирование чтения потока случайного файла
        /// </summary>
        [Fact]
        public void TestReadRandomEntryStream()
        {
            const int BufferSize = 1024;

            using var testPak = new PakFile(TestArchivePath);

            testPak.Open();

            var rng = new Random();
            var entryToRead = testPak.Contents[rng.Next(testPak.Contents.Length)];

            using var entryStream = testPak.OpenEntryStream(entryToRead);

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

        /// <summary>
        /// Проверить, что потоковое чтение и извлечение выдают те же данные
        /// </summary>
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

            Assert.Equal(entryContentsWhole, entryContetnsToTest);
        }

        /// <summary>
        /// Тестирование создания архива PAK
        /// </summary>
        [Fact]
        public void TestCreatePakArchive()
        {
            using var createdPakArchive = PakFile.CreateFromDirectory(TestSourceDir);

            createdPakArchive.Open();

            Assert.True(createdPakArchive.IsOpen);
            Assert.NotEmpty(createdPakArchive.Contents);
        }
    }
}
