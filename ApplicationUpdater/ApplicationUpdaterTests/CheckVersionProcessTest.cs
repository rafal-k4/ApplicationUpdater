using ApplicationUpdater;
using ApplicationUpdater.Processes;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace ApplicationUpdaterTests
{
    public class CheckVersionProcessTest
    {

        [Fact]
        public void CheckVersionShowDifferencesTest()
        {

            var OldFilePath = CreateFiles("TestOld");
            var NewFilePath = CreateFiles("TestNew", true);

            var model = new UpdateModel
            {
                UserParams = new UserParams
                {
                    IntepubDirectory = new DirectoryInfo(OldFilePath)
                },
                UnZipDirectory = new DirectoryInfo(NewFilePath)
            };

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

        [Fact]
        public void CheckVersionNullReferencePathTest()
        {
            var model = new UpdateModel
            {
                UserParams = new UserParams
                {
                    IntepubDirectory = null
                },
                UnZipDirectory = null
            };

            var result = new CheckVersionProcess(new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json", optional: true).Build());

            Assert.Throws<NullReferenceException>(() => result.Process(model));
        }

        [Fact]
        public void CheckVersionDirectoryNotFoundTest()
        {
            var NotExsistingDirectorPath = CreateFiles("random", havingNoDirectory: true);
            //var newPath = CreateFiles("new", havingNoDirectory, true);

            var model = new UpdateModel
            {
                UserParams = new UserParams
                {
                    IntepubDirectory = new DirectoryInfo(NotExsistingDirectorPath)
                },
                UnZipDirectory = new DirectoryInfo(NotExsistingDirectorPath)
            };

            var result = new CheckVersionProcess(new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json", optional: true).Build());

            Assert.Throws<DirectoryNotFoundException>(() => result.Process(model));

        }

        [Fact]
        public void CheckVersionFilesNotFoun()
        {
            var DirectoryWithoutFilesPath = CreateFiles("old", havingNoFiles: true);

            var model = new UpdateModel
            {
                UserParams = new UserParams
                {
                    IntepubDirectory = new DirectoryInfo(DirectoryWithoutFilesPath)
                },
                UnZipDirectory = new DirectoryInfo(DirectoryWithoutFilesPath)
            };

            var result = new CheckVersionProcess(new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json", optional: true).Build());

            try
            {
                Assert.Throws<Exception>(() => result.Process(model));
            }
            finally
            {
                Directory.Delete(DirectoryWithoutFilesPath);
            }
        }

        [Fact]
        public void CheckVersionNoFileInNewAppTest()
        {
            var oldPath = CreateFiles("TestOld");
            var NewPathWithoutFile = CreateFiles("TestNew",isNew: true, missFiles: true);

            var model = new UpdateModel
            {
                UserParams = new UserParams
                {
                    IntepubDirectory = new DirectoryInfo(oldPath)
                },
                UnZipDirectory = new DirectoryInfo(NewPathWithoutFile)
            };

            var result = new CheckVersionProcess(new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json", optional: true).Build());

            result.ProcessEvent += Result_ProcessEventMock;

            result.Process(model);



        }

        private void Result_ProcessEventMock(object sender, EventArgs e)
        {
            var item = sender as ConsoleWriteProcess;
            var expectedMessage = @"No file in the new application \test.txt";
            Assert.Contains(expectedMessage, item.Msg);
            
            //Assert.Equal(expectedMessage, item.Msg);
        }

        public string CreateFiles(string rootFileName, bool isNew = false, bool havingNoDirectory = false, bool havingNoFiles = false, bool missFiles = false)
        {
            var RootPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRootPath = Path.GetDirectoryName(appPathMatcher.Match(RootPath).Value);

            if (havingNoDirectory == true)
            {
                return Path.Combine(appRootPath, Guid.NewGuid().ToString());
            }

            appRootPath = Path.Combine(appRootPath, rootFileName);

            var fileName = "test";
            var fileNameSecond = fileName + "2";

            DirectoryInfo dirInfo = new DirectoryInfo(appRootPath);


            //creating directories and files
            dirInfo.Create();

            var path = dirInfo.FullName;

            if(havingNoFiles == true)
            {
                return path;
            }

            //new version of app contains Parent folder app, from which all files are copied to the target application directory
            if (isNew)
            {
                dirInfo.CreateSubdirectory("app");
                dirInfo = dirInfo.GetDirectories().Where(x => x.Name == "app").First();
            }

            if(missFiles == false)
            {
                File.WriteAllLines(Path.Combine(dirInfo.FullName, fileName + ".txt"), new[] { "" });
            }
            

            dirInfo.CreateSubdirectory(fileNameSecond);
            var subDirInfo = dirInfo.GetDirectories().Where(x => x.Name == fileNameSecond).First();
            File.WriteAllLines(Path.Combine(subDirInfo.FullName, fileNameSecond + ".config"), new[] { "" });


            return path;
        }
    }
}
