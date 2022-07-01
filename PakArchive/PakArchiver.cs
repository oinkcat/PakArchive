using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PakArchive
{
    /// <summary>
    /// Архивирует файлы в формат PAK
    /// </summary>
    public class PakArchiver
    {
        private const int DefaultTocOffset = 12;

        private readonly string dirToPackPath;

        private BinaryWriter pakWriter;

        private int dataStartOffset;

        private List<(FileInfo, PakEntry)> entriesToPack;

        /// <summary>
        /// Путь к созданному файлу архива
        /// </summary>
        public string PakArchivePath { get; private set; }

        /// <summary>
        /// Индикация прогресса архивирования файлов
        /// </summary>
        public event EventHandler<PakProgressEventArgs> ProgressChanged;

        public PakArchiver(string dirPath)
        {
            dirToPackPath = dirPath;
        }

        /// <summary>
        /// Упаковать файлы из каталога в файл архива
        /// </summary>
        public void Pack()
        {
            CollectEntriesToPackInfo();

            string outFilePath = GetOutputArchivePath();

            using (pakWriter = new BinaryWriter(File.Create(outFilePath)))
            {
                WriteHeader();

                dataStartOffset = DefaultTocOffset + GetTocSize();

                WriteTableOfContents();
                WriteFilesData();
            }

            PakArchivePath = outFilePath;
        }

        // Получить размер таблицы содержимого
        private int GetTocSize()
        {
            int entryInfoSize = PakFile.EntryNameLength + sizeof(int) * 2;
            return entryInfoSize * entriesToPack.Count;
        }

        // Собрать информацию о файлах для архивирования
        private void CollectEntriesToPackInfo()
        {
            entriesToPack = new DirectoryInfo(dirToPackPath)
                .GetFiles("*.*", SearchOption.TopDirectoryOnly)
                .OrderBy(fi => fi.Name)
                .Select(fi => (fi, PakEntry.CreateFromFileInfo(fi)))
                .ToList();
        }

        // Получить путь к выходному файлу
        private string GetOutputArchivePath()
        {
            string parentDirPath = Path.Combine(dirToPackPath, "..");
            string dirName = Path.GetFileNameWithoutExtension(dirToPackPath);
            return Path.Combine(parentDirPath, $"{dirName}.pak");
        }

        // Записать заголовок файла
        private void WriteHeader()
        {
            pakWriter.Write(PakFile.PakSignature.ToCharArray());
            pakWriter.Write(DefaultTocOffset);
            pakWriter.Write(GetTocSize());
        }

        // Записать таблицу содержимого
        private void WriteTableOfContents()
        {
            int fileDataOffset = dataStartOffset;

            foreach((FileInfo _, PakEntry entry) in entriesToPack)
            {
                var nameBuffer = new byte[PakFile.EntryNameLength];
                var fiNameBytes = Encoding.ASCII.GetBytes(entry.Name);
                fiNameBytes.CopyTo(nameBuffer, 0);
                
                pakWriter.Write(nameBuffer);
                pakWriter.Write(fileDataOffset);
                pakWriter.Write(entry.Size);

                fileDataOffset += entry.Size;
            }
        }

        // Записать данные файлов
        private void WriteFilesData()
        {
            int numProcessed = 0;
            int numTotal = entriesToPack.Count;

            foreach((FileInfo fi, PakEntry _) in entriesToPack)
            {
                using (var fileStream = fi.OpenRead())
                {
                    var progressInfo = new PakProgressEventArgs(numProcessed, numTotal)
                    {
                        CurrentFileName = fi.Name
                    };
                    ProgressChanged?.Invoke(this, progressInfo);

                    fileStream.CopyTo(pakWriter.BaseStream);
                }

                numProcessed++;
            }

            ProgressChanged?.Invoke(this, new PakProgressEventArgs(numTotal, numTotal));
        }
    }
}
