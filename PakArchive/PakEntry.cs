using System;
using System.Collections.Generic;

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
