﻿using System.IO;
using System.IO.Compression;

namespace ApplicationUpdater.Processes
{
    public class UnzipProcess: ProcessBase, IProcess<UpdateModel>
    {
        public UnzipProcess() : base("Unzipping")
        {
        }


        public ProcesEventResult Process(UpdateModel model)
        {
            var unZipDirectory = Path.Combine(model.UserParams.BackupDirectory.FullName, Consts.DirectoriesNames.NewApplication);

            Directory.CreateDirectory(unZipDirectory);

            UpdateProcess($"Unzip {model.UserParams.PathToZipFile.FullName} to {unZipDirectory}");

            ZipFile.ExtractToDirectory(model.UserParams.PathToZipFile.FullName, unZipDirectory);

            model.UnZipDirectory = new DirectoryInfo(unZipDirectory);

            model.NewApplicationDirectory = new DirectoryInfo(Path.Combine(model.UnZipDirectory.FullName, "app"));

            return GetProcesEventResult(Consts.ProcesEventResult.Successful);
        }
    }
}
