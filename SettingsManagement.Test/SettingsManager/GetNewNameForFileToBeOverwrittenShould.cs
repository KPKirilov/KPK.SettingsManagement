namespace SettingsManagement.Test.SettingsManager
{
    using NUnit.Framework;
    using SettingsManagement.Test.Exposers;
    using SettingsManagement.Test.SampleSettings;
    using System;
    using System.IO;

    public class GetNewNameForFileToBeOverwrittenShould
    {
        public SettingsManagerExposer<ValidSettings> SettingsManager { get; set; }
        public string testFilesDirectory = "TestFiles";
        public string originalFilePath;

        [SetUp]
        public void Setup()
        {
            this.SettingsManager = new();
            this.originalFilePath = Path.Combine(this.testFilesDirectory, "SampleFile.txt");
        }

        [TearDown]
        public void TearDown()
        {
            CleanupGeneratedFiles();
        }

        [Test]
        public void ProduceADifferentFileNameFromTheOriginal()
        {
            string sampleFileName = originalFilePath;
            string newFileName = this.SettingsManager.GetNewNameForFileToBeOverwritten(sampleFileName);
            Assert.AreNotEqual(sampleFileName, newFileName);
        }

        [Test]
        public void GiveDifferentFileNamesForFilesGeneratedInTheSameSecondIfTheyExist()
        {
            int attemptCount = 20;
            string sampleText = "sample text";
            Directory.CreateDirectory(this.testFilesDirectory);
            File.WriteAllText(originalFilePath, sampleText);

            for (int i = 0; i < attemptCount; i++)
            {
                DateTime start = DateTime.Now;
                string firstFileName = this.SettingsManager.GetNewNameForFileToBeOverwritten(this.originalFilePath);
                File.WriteAllText(firstFileName, sampleText);

                string secondFileName = this.SettingsManager.GetNewNameForFileToBeOverwritten(this.originalFilePath);
                File.WriteAllText(secondFileName, sampleText);
                string thirdFileName = this.SettingsManager.GetNewNameForFileToBeOverwritten(this.originalFilePath);
                DateTime end = DateTime.Now;
                if (start.Hour == end.Hour
                    && start.Minute == end.Minute
                    && start.Second == end.Second)
                {
                    if (firstFileName != secondFileName
                        && secondFileName != thirdFileName
                        && thirdFileName != firstFileName)
                    {
                        Assert.Pass();
                    }
                    else
                    {
                        Assert.Fail();
                    }
                }
            }

            Assert.Warn("Could not be determined. Could not create three files in the same second.");
        }

        private void CleanupGeneratedFiles()
        {
            if (Directory.Exists(this.testFilesDirectory))
            {
                var files = Directory.GetFiles(this.testFilesDirectory);
                foreach (var f in files)
                {
                    try
                    {
                        File.Delete(f);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
