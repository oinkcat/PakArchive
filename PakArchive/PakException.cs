using System;
using System.Collections.Generic;

namespace PakArchive
{
    /// <summary>
    /// Исключение при работе с PAK-файлом
    /// </summary>
    public class PakException : Exception
    {
        public PakException(string message) : base(message)
        { }
    }
}
