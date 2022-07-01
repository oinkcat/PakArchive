using System;
using System.IO;
using PakArchive;

using Assembly = System.Reflection.Assembly;

namespace PakUtil
{
    /// <summary>
    /// Утилита для работы с файлами PAK архивов
    /// </summary>
    public class Program
    {
        private readonly PakUtilParams actionParams;

        public static void Main(string[] args)
        {
            Console.WriteLine("PAK Archive utility");
            Console.WriteLine();

            var utilParams = new PakUtilParams(args);

            if(utilParams.IsParamsCorrect)
            {
                var utilApp = new Program(utilParams);
                utilApp.ExecutePakAction();
            }
            else
            {
                PrintUsage();
            }
        }

        // Вывести параметры утилиты на консоль
        private static void PrintUsage()
        {
            string exeName = Assembly.GetExecutingAssembly().GetName().Name;
            string appName = Path.GetFileNameWithoutExtension(exeName);

            Console.WriteLine("Usage:");

            Console.Write($"{appName} [{PakUtilParams.ParamList}]\t<Pak archive path>");
            Console.WriteLine("\t\t List archived files");

            Console.Write($"{appName} [{PakUtilParams.ParamPack}]\t<Directory path>");
            Console.WriteLine("\t\t Pack directory contents");

            Console.Write($"{appName} {PakUtilParams.ParamUnpack}\t<Pak archive path>");
            Console.WriteLine("\t\t Unpack archived files");
        }

        public Program(PakUtilParams aParams)
        {
            actionParams = aParams;
        }

        /// <summary>
        /// Выполнить действие архивации
        /// </summary>
        public void ExecutePakAction()
        {
            if (!actionParams.Command.HasValue) 
            {
                throw new ArgumentNullException(nameof(actionParams.Command));
            }

            switch(actionParams.Command.Value)
            {
                case PakCommand.List:
                    ListArchiveFiles();
                    break;
                case PakCommand.Pack:
                    PackDirectoryFiles();
                    break;
                case PakCommand.Unpack:
                    UnpackArchiveFiles();
                    break;
            }
        }

        // Вывести список файлов архива на консоль
        private void ListArchiveFiles()
        {
            Console.WriteLine($"Listing files in {actionParams.Path}:");

            using var pakFile = new PakFile(actionParams.Path);
            pakFile.Open();

            foreach(var pakEntry in pakFile.Contents)
            {
                Console.WriteLine($"{pakEntry.Name}, {pakEntry.Size} bytes");
            }

            Console.WriteLine($"{pakFile.Contents.Length} files total");
        }

        // Упаковать файлы в каталоге
        private void PackDirectoryFiles()
        {
            Console.WriteLine($"Packing files in {actionParams.Path}:");

            var archiver = new PakArchiver(actionParams.Path);
            archiver.ProgressChanged += Archiver_ProgressChanged;

            archiver.Pack();
        }

        // Прогресс архивирования изменен
        private void Archiver_ProgressChanged(object sender, PakProgressEventArgs e)
        {
            if(e.IsFinished)
            {
                Console.WriteLine($"Done. Total files processed: {e.FilesProcessed}");
            }
            else
            {
                string progress = String.Concat(e.FilesProcessed + 1, '/', e.TotalFiles);
                Console.WriteLine($"{e.CurrentFileName} ({progress})...");
            }
        }

        // Распаковать файлы из архива
        private void UnpackArchiveFiles()
        {
            Console.WriteLine($"Unpacking files from {actionParams.Path}:");

            using var archiveFile = new PakFile(actionParams.Path);
            archiveFile.Open();

            string pakName = Path.GetFileNameWithoutExtension(actionParams.Path);
            string outDir = Path.Combine(Path.GetDirectoryName(actionParams.Path), pakName);

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            int numTotal = archiveFile.Contents.Length;
            int numProcessed = 0;

            foreach (var entryToUnpack in archiveFile.Contents)
            {
                string progress = String.Concat(numProcessed + 1, '/', numTotal);
                Console.WriteLine($"{entryToUnpack.Name} ({progress})...");

                string safeName = entryToUnpack.Name.Replace('/', '#');
                string outFilePath = Path.Combine(outDir, safeName);

                using(var outFileStream = File.Create(outFilePath))
                using(var entryStream = archiveFile.OpenEntryStream(entryToUnpack))
                {
                    entryStream.CopyTo(outFileStream);
                }

                numProcessed++;
            }

            Console.WriteLine($"Done. Total files processed: {numTotal}");
        }
    }
}
