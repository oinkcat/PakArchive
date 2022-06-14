using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PakArchive
{
    /// <summary>
    /// Представляет информацию о файле архива PAK
    /// </summary>
    public class PakFile : IDisposable
    {
        private const int EntryInfoSize = 64;

        private const int EntryNameLength = 56;

        private const string PakSignature = "PACK";

        private BinaryReader pakReader;

        private int[] entryOffsets;

        private bool disposedValue;

        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Признак открытия файла на чтение
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Элементы содержимого архива
        /// </summary>
        public PakEntry[] Contents { get; private set; }

        public PakFile(string fileName)
        {
            FileName = fileName;
        }
        
        /// <summary>
        /// Открыть файл и прочитать содержимое
        /// </summary>
        public void Open()
        {
            pakReader = new BinaryReader(new FileStream(FileName, FileMode.Open));

            try
            {
                var (tocOffset, entriesCount) = ReadHeaderInfo();
                ReadTableOfContents(tocOffset, entriesCount);
            }
            catch(Exception e)
            {
                pakReader.Close();
                throw new PakException(e.Message);
            }

            IsOpen = true;
        }

        // Прочитать заголовок файла
        private (int, int) ReadHeaderInfo()
        {
            string signature = new string(pakReader.ReadChars(PakSignature.Length));

            if(signature.Equals(PakSignature))
            {
                int tocOffset = pakReader.ReadInt32();
                int tocSize = pakReader.ReadInt32();
                int numOfEntries = tocSize / EntryInfoSize;

                return (tocOffset, numOfEntries);
            }
            else
            {
                throw new PakException("Invalid archive signature");
            }
        }

        // Прочитать таблицу содержимого
        private void ReadTableOfContents(int tocOffset, int numEntries)
        {
            pakReader.BaseStream.Seek(tocOffset, SeekOrigin.Begin);

            Contents = new PakEntry[numEntries];
            entryOffsets = new int[numEntries];

            for (int i = 0; i < numEntries; i++)
            {
                var entryNameBytes = pakReader.ReadBytes(EntryNameLength);
                string entryName = Encoding.ASCII.GetString(entryNameBytes).TrimEnd('\0');

                int entryDataOffset = pakReader.ReadInt32();
                int entrySize = pakReader.ReadInt32();

                Contents[i] = new PakEntry(entryName, entrySize);
                entryOffsets[i] = entryDataOffset;
            }
        }

        /// <summary>
        /// Получить прочитанные из архива данные элемента
        /// </summary>
        /// <param name="entry">Элемент архива PAK</param>
        /// <returns>Массив байтов данных элемента архива</returns>
        public byte[] ReadEntryData(PakEntry entry)
        {
            if(TrySeekEntryStart(entry))
            {
                return pakReader.ReadBytes(entry.Size);
            }
            else
            {
                throw new PakException("Incorrect archive entry");
            }
        }

        // Попробовать установить указатель чтения на начало данных PAK элемента
        private bool TrySeekEntryStart(PakEntry entry)
        {
            int offsetIndex = Array.IndexOf<PakEntry>(Contents, entry);
            
            if(offsetIndex > -1)
            {
                int entryDataOffset = entryOffsets[offsetIndex];
                pakReader.BaseStream.Seek(entryDataOffset, SeekOrigin.Begin);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Открыть поток данных элемента на чтение
        /// </summary>
        /// <param name="entry">Элемент архива PAK</param>
        /// <returns>Поток данных элемента</returns>
        public Stream OpenEntryStream(PakEntry entry)
        {
            if(TrySeekEntryStart(entry))
            {
                return new PakEntryStream(pakReader.BaseStream, entry);
            }
            else
            {
                throw new PakException("Incorrect archive entry");
            }
        }

        /// <summary>
        /// Закрыть файл
        /// </summary>
        public void Close()
        {
            if(IsOpen)
            {
                pakReader.Close();
                IsOpen = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
