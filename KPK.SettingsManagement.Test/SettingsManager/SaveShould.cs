namespace KPK.SettingsManagement.Test.SettingsManager
{
    using NUnit.Framework;
    using KPK.SettingsManagement.Test.Exposers;
    using KPK.SettingsManagement.Test.SampleSettings;
    using System;
    using System.IO;

    public class SaveShould
    {
        public SettingsManagerExposer<ValidSettings> ValidSettingsManager { get; set; }

        [SetUp]
        public void Setup()
        {
            this.ValidSettingsManager = new();
            this.CleanUpExistingFilesAndDirectories();
        }

        [TearDown]
        public void TearDown()
        {
            this.CleanUpExistingFilesAndDirectories();
        }

        [Test]
        public void CreateAFileIfItDoesNotExist()
        {
            this.ValidSettingsManager.Save();
            if (File.Exists(this.ValidSettingsManager.SettingsFileAbsolutePath))
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void ThrowIfFailedToSaveAndOptionIsOn()
        {
            this.ValidSettingsManager.ShouldThrowOnFailedToSave = true;
            bool shouldPass = false;
            try
            {
                this.FailToSave();
                shouldPass = false;
            }
            catch (Exception)
            {
                shouldPass = true;
            }

            Assert.True(shouldPass);
        }


        [Test]
        public void NotThrowIfFailedToSaveAndOptionIsOff()
        {
            this.ValidSettingsManager.ShouldThrowOnFailedToSave = false;
            Assert.DoesNotThrow(this.FailToSave);
        }

        private void FailToSave()
        {
            if (!Directory.Exists(this.ValidSettingsManager.SettingsFileDirectory))
            {
                Directory.CreateDirectory(this.ValidSettingsManager.SettingsFileDirectory);
            }

            using (FileStream fs = File.Create(this.ValidSettingsManager.SettingsFileAbsolutePath))
            {
                this.ValidSettingsManager.Save();
            }
        }

        private void CleanUpExistingFilesAndDirectories()
        {
            if (File.Exists(this.ValidSettingsManager.SettingsFileAbsolutePath))
            {
                File.Delete(this.ValidSettingsManager.SettingsFileAbsolutePath);
            }

            string directory = Path.GetDirectoryName(this.ValidSettingsManager.SettingsFileAbsolutePath);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory);
            }
        }
    }
}
