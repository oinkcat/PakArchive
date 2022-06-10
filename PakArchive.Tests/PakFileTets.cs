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
    }
}
