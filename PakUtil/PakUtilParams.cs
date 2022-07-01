using System;
using System.Collections.Generic;
using System.IO;

namespace PakUtil
{
    /// <summary>
    /// Команда запрашиваемого действия
    /// </summary>
    public enum PakCommand
    {
        List,
        Pack,
        Unpack
    }

    /// <summary>
    /// Параметры приложения
    /// </summary>
    public class PakUtilParams
    {
        public const string ParamList = "-l";
        public const string ParamPack = "-p";
        public const string ParamUnpack = "-u";

        private static string[] verbs = { ParamList, ParamPack, ParamUnpack };

        /// <summary>
        /// Команда приложению
        /// </summary>
        public PakCommand? Command { get; private set; }

        /// <summary>
        /// Путь к объекту
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Параметры верны
        /// </summary>
        public bool IsParamsCorrect => Command.HasValue;

        public PakUtilParams(string[] args)
        {
            if(args.Length > 0 && args.Length < 3)
            {
                ParseArgs(args);
            }
        }

        // Разобрать аргументы программы
        private void ParseArgs(string[] args)
        {
            if(args.Length == 1)
            {
                Path = args[0];
                Command = GuessCommandByPath(Path);
            }
            else
            {
                string actionVerb = args[0].ToLower();
                if(Array.IndexOf(verbs, actionVerb) == -1) { return; }

                Path = args[1];
                var cmdByPath = GuessCommandByPath(Path);

                if(actionVerb.Equals(ParamList) && cmdByPath == PakCommand.List)
                {
                    Command = PakCommand.List;
                }
                else if(actionVerb.Equals(ParamPack))
                {
                    Command = PakCommand.Pack;
                }
                else if(actionVerb.Equals(ParamUnpack) && cmdByPath != PakCommand.Pack)
                {
                    Command = PakCommand.Unpack;
                }
            }
        }

        // Получить команду на основе пути к объекту
        private PakCommand? GuessCommandByPath(string path)
        {
            if (File.Exists(path))
            {
                return PakCommand.List;
            }
            else if (Directory.Exists(path))
            {
                return PakCommand.Pack;
            }
            else
            {
                return null;
            }
        }
    }
}
