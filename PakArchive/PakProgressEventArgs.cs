using System;
using System.Collections.Generic;
using System.Text;

namespace PakArchive
{
    /// <summary>
    /// Тип параметра события действия с PAK файлом
    /// </summary>
    public class PakProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Имя текущего обрабатываемого файла
        /// </summary>
        public string CurrentFileName { get; set; }

        /// <summary>
        /// Число обработанных файлов
        /// </summary>
        public int FilesProcessed { get; }

        /// <summary>
        /// Общее число файлов
        /// </summary>
        public int TotalFiles { get; }

        /// <summary>
        /// Действие завершено
        /// </summary>
        public bool IsFinished => FilesProcessed == TotalFiles;

        public PakProgressEventArgs(int numArchived, int numTotal)
        {
            FilesProcessed = numArchived;
            TotalFiles = numTotal;
        }
    }
}
