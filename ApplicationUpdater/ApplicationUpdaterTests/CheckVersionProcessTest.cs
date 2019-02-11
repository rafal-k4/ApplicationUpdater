using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.IO;
using System.Text.RegularExpressions;
using ApplicationUpdater;
using System.Linq;
using ApplicationUpdater.Processes;
using Microsoft.Extensions.Configuration;

namespace ApplicationUpdaterTests
{
    public class CheckVersionProcessTest
    {

        [Fact]
        public void CheckVersionShowDifferencesTest()
        {

            var OldFilePath = CreateFiles("TestOld");
            var NewFilePath = CreateFiles("TestNew", true);

            var model = new UpdateModel();

            model.UserParams = new UserParams();
            model.UserParams.IntepubDirectory = new DirectoryInfo(OldFilePath);
            model.UnZipDirectory = new DirectoryInfo(NewFilePath);

            var result = new CheckVersionProcess(new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json", optional: true).Build()).Process(model);

            try
            {
                Assert.Equal("[Checking the files] " + Consts.ProcesEventResult.Successful, result.Result);
            }
            finally
            {
                Directory.Delete(OldFilePath, true);
                Directory.Delete(NewFilePath, true);
            }
            
        }


        public string CreateFiles(string rootFileName, bool isNew = false)
        {
            var RootPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRootPath = Path.GetDirectoryName(appPathMatcher.Match(RootPath).Value);
            appRootPath = Path.Combine(appRootPath, rootFileName);

            var fileName = "test";
            var fileNameSecond = fileName +"2";

            DirectoryInfo dirInfo = new DirectoryInfo(appRootPath);


            //creating directories and files
            dirInfo.Create();

            var path = dirInfo.FullName;

            //new version of app contains Parent folder app, from which all files are copied to the target application directory
            if (isNew)
            {
                dirInfo.CreateSubdirectory("app");
                dirInfo = dirInfo.GetDirectories().Where(x => x.Name == "app").First();
            }

            File.WriteAllLines(Path.Combine(dirInfo.FullName, fileNameSecond + ".txt"), new[] { "" });

            dirInfo.CreateSubdirectory(fileNameSecond);
            var subDirInfo = dirInfo.GetDirectories().Where(x => x.Name == fileNameSecond).First();
            File.WriteAllLines(Path.Combine(subDirInfo.FullName, fileNameSecond + ".config"), new[] { "" });


            return path;
        }
    }
}
