using System;
using System.Collections.Generic;
using System.IO;

namespace PakArchive
{
    /// <summary>
    /// Поток данных элемента PAK архива
    /// </summary>
    public class PakEntryStream : Stream
    {
        private long entryOffset;

        private long entryPosition;

        private readonly Stream pakFileStream;

        /// <summary>
        /// Открытый элемент PAK архива
        /// </summary>
        public PakEntry OpenEntry { get; }

        /// <summary>
        /// Можно ли произвести чтение
        /// </summary>
        public override bool CanRead => entryPosition < OpenEntry.Size;

        /// <summary>
        /// Число доступных байт для чтения
        /// </summary>
        public override long Length => OpenEntry.Size;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        /// <summary>
        /// Позиция чтения внутри PAK элемента
        /// </summary>
        public override long Position 
        {
            get => entryPosition;
            set => throw new NotImplementedException(); 
        }

        public PakEntryStream(Stream baseStream, PakEntry entry)
        {
            pakFileStream = baseStream;
            entryOffset = baseStream.Position;
            OpenEntry = entry;
        }

        /// <summary>
        /// Установить указатель позиции чтения внутри PAK элемента
        /// </summary>
        /// <param name="offset">Новая позиция</param>
        /// <param name="origin">Откуда отсчитывать позицию</param>
        /// <returns>Текущая позиция внутри PAK элемента</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset < 0) { throw new ArgumentException(nameof(offset)); }

            entryPosition = new Func<long>(() =>
            {
                switch(origin)
                {
                    case SeekOrigin.Begin: 
                        return Math.Min(offset, OpenEntry.Size);
                    case SeekOrigin.Current:
                        return Math.Min(entryPosition + offset, OpenEntry.Size);
                    default:
                        return Math.Max(OpenEntry.Size - offset, 0);
                }
            })();

            long baseOffset = entryPosition + entryOffset;
            pakFileStream.Seek(baseOffset, SeekOrigin.Begin);

            return entryPosition;
        }

        /// <summary>
        /// Прочитать данные PAK элемента
        /// </summary>
        /// <param name="buffer">Буфер, куда записывать данные</param>
        /// <param name="offset">Отступ в массиве-буфере</param>
        /// <param name="count">Число байт для чтения</param>
        /// <returns>Число прочтенных байт</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if(count < 0) { throw new ArgumentException(nameof(count)); }

            Seek(entryPosition, SeekOrigin.Begin);

            int bytesToRead = (int)Math.Min(count, OpenEntry.Size - entryPosition);
            int actualReadBytes = pakFileStream.Read(buffer, offset, bytesToRead);

            entryPosition += actualReadBytes;

            return actualReadBytes;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }
    }
}
