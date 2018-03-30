﻿using ApplicationUpdater.Processes;
using Microsoft.Extensions.Configuration;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ApplicationUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            Consts.Header.WriteHeader();
            Console.CursorVisible = false;

            var di = new Di(null, ConsoleEvent, GetConfirmation, RezultEvent);

            di.Build();

            try
            {
                var updateModel = GetUpdateModel(args);

                Console.WriteLine(updateModel.UserParams.ToString());
                Console.WriteLine();

                var selgrosApplicationUpdateStrategy = di.GetService<SelgrosApplicationUpdateStrategy>();

                var iISAplicationUpdater = di.GetService<IISAplicationUpdater>() ;

                iISAplicationUpdater.Update(updateModel);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred during the update: {e.Message}");
            }

            Console.WriteLine("The application has been updated", null);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.CursorVisible = true;
        }

        public static string GetStopWatchString(DateTime ts)
        {
            return $"{ts.Hour:00}:{ts.Minute:00}:{ts.Second:00}:{(ts.Millisecond / 10):00}";
        }

        private static void RezultEvent(object sender, EventArgs e)
        {
            var cwp = new ConsoleWriteProcess
            {
                Msg = $"{((ProcesEventResult)sender).Result}",
                NewLine = true
            };

            ConsoleEvent(cwp, null);
        }

        private static void ConsoleEvent(object sender, EventArgs e)
        {
            var d = sender as ConsoleWriteProcess;

            if (d.NewLine)
            {
                Console.WriteLine($"{GetStopWatchString(DateTime.Now)}   {d.Msg}");
            }
            else
            {
                Console.Write($"{GetStopWatchString(DateTime.Now)}   {d.Msg}");
            }
        }

        private static void GetConfirmation(object sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            var pc = (ProcessConfirmation)sender;

            Console.Write($"{ GetStopWatchString(DateTime.Now)}   {pc.Question}");

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

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static UpdateModel GetUpdateModel(string[] args)
        {
            if (args.Length != 5)
            {
                throw new ArgumentException("No suitable parameters");
            }

            var updateModel = new UpdateModel
            {
                UserParams = new UserParams
                {
                    PathToZipFile = new FileInfo(GetParam(args, 0, "PathToZipFile")),
                    BackupDirectory = new DirectoryInfo(GetParam(args, 1, "BackupDirectory")),
                    IntepubDirectory = new DirectoryInfo(GetParam(args, 2, "IntepubDirectory")),
                    Version = GetParam(args, 3, "Version"),
                    IsUndoProcess = bool.Parse(GetParam(args, 4, "IsUndoProcess"))
                }
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
