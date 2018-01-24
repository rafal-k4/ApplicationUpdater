﻿using ApplicationUpdater.Processes;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationUpdater
{
    public class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetLogger("ApplicationUpdater");

            ConsoleEvent("Aktualizacja aplikacji", null);

            try
            {
                Console.CursorVisible = false;
                var updateModel = GetUpdateModel(args);

                ConsoleEvent("Przygotowywanie modelu danych", null);

                var selgrosApplicationUpdateStrategy = new SelgrosApplicationUpdateStrategy(logger);

                selgrosApplicationUpdateStrategy.UpdateEvent += ConsoleEvent;
                selgrosApplicationUpdateStrategy.ConfirmEvent += GetConfirmation;

                var iISAplicationUpdater = new IISAplicationUpdater(selgrosApplicationUpdateStrategy, logger);

                iISAplicationUpdater.Update(updateModel);
            }
            catch (Exception e)
            {
                logger.Error(e);

                Console.WriteLine($"Wystąpił błąd podczas aktualizacji: {e.Message}");
            }

            ConsoleEvent("Aplikacja została zaaktalizowana", null);

            Console.WriteLine("Naciśnij dowolny klawisz aby kontynuować...");
            Console.ReadKey();
            Console.CursorVisible = true;
        }

        public static string GetStopWatchString(DateTime ts)
        {
            return $"{ts.Hour:00}:{ts.Minute:00}:{ts.Second:00}:{(ts.Millisecond / 10):00}";
        }

        private static void ConsoleEvent(object sender, EventArgs e)
        {
            Console.WriteLine($"{GetStopWatchString(DateTime.Now)}...{(string)sender}.");
        }

        private static void GetConfirmation(object sender, EventArgs e)
        {
            var pc = (ProcessConfirmation)sender;

            Console.Write($"--> {pc.Question}");

            var allowKeys = new List<ConsoleKey>
            {
                ConsoleKey.Y,
                ConsoleKey.N,
                ConsoleKey.C
            };

            var key = ConsoleKey.Clear;

            while (allowKeys.Contains(key) == false)
            {
                key = Console.ReadKey(true).Key;
            }

            if (key == ConsoleKey.C)
            {
                Environment.Exit(0);
            }

            pc.Key = key;

            Console.WriteLine(pc.Key.ToString());
        }

        private static UpdateModel GetUpdateModel(string[] args)
        {
            if (args.Length != 5)
            {
                throw new ArgumentException("Brak odpowiednich parametrów");
            }

            var updateModel = new UpdateModel
            {
                PathToZipFile = new FileInfo(GetParam(args, 0, "PathToZipFile")),
                BackupDirectory = new DirectoryInfo(GetParam(args, 1, "BackupDirectory")),
                IntepubDirectory = new DirectoryInfo(GetParam(args, 2, "IntepubDirectory")),
                Version = GetParam(args, 3, "Version"),
                IsUndoProcess =  bool.Parse(GetParam(args, 4, "IsUndoProcess"))
            };

            return updateModel;
        }

        private static string GetParam(string[] args, int index, string name)
        {
            if (string.IsNullOrEmpty(args[index]))
            {
                throw new ArgumentException(name);
            }

            return args[index];
        }
    }
}
