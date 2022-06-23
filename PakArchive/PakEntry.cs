using System;
using System.IO;

namespace PakArchive
{
    /// <summary>
    /// Элемент содержимого PAK-архива
    /// </summary>
    public class PakEntry
    {
        /// <summary>
        /// Имя элемента архива
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Размер элемента архива
        /// </summary>
        public int Size { get; set; }

        public static PakEntry CreateFromFileInfo(FileInfo fi)
        {
            if(fi.Length > int.MaxValue)
            {
                throw new OverflowException("File size too large");
            }

            string pakFileName = fi.Name;

            if(pakFileName.Length > PakFile.EntryNameLength)
            {
                string nameOnly = Path.GetFileNameWithoutExtension(fi.Name);
                string fileExt = Path.GetExtension(fi.Name);

                int nameLength = PakFile.EntryNameLength - fileExt.Length;
                pakFileName = String.Concat(nameOnly.Substring(0, nameLength), fileExt);
            }

            return new PakEntry(pakFileName, (int)fi.Length);
        }

        public PakEntry(string name, int size)
        {
            Name = name;
            Size = size;
        }

        /// <summary>
        /// Строковое представление информации элемента PAK-архива
        /// </summary>
        /// <returns>Имя элемента и размер в байтах в строке</returns>
        public override string ToString() => $"{Name} : {Size}";
    }
}
