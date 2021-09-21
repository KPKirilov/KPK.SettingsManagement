using NUnit.Framework;
using KPK.SettingsManagement.Test.SampleSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using KPK.SettingsManagement.Options;
using KPK.SettingsManagement.Serialization;
using KPK.SettingsManagement.Exceptions;

namespace KPK.SettingsManagement.Test.SettingsManager
{
    public class LoadShould
    {
        public SettingsManager<ValidSettings> settingsManager;
        public NewtonsoftJsonSerializer<ValidSettings> serializer;
        public NewtonsoftJsonSerializer<ValidSettings2> serializer2;

        [SetUp]
        public void SetUp()
        {
            this.settingsManager = new();
            serializer = new();
            serializer2 = new();

            this.CleanUpExistingFiles();
        }

        [TearDown]
        public void TearDown()
        {
            this.CleanUpExistingFiles();
        }

        public void CleanUpExistingFiles()
        {
            if (Directory.Exists(this.settingsManager.SettingsFileDirectory))
            {
                var files = Directory.GetFiles(this.settingsManager.SettingsFileDirectory);
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

                try
                {
                    Directory.Delete(this.settingsManager.SettingsFileDirectory);
                }
                catch (Exception)
                {
                }
            }
        }

        [Test]
        public void SuccessfullyLoadAValidExistingFile()
        {
            int valueToCompare = 5;
            ValidSettings settings = new();
            settings.PublicIntProperty = valueToCompare;
            this.CreateASettingsFile(settings);

            this.settingsManager.Load();
            Assert.AreEqual(valueToCompare, this.settingsManager.Settings.PublicIntProperty);
        }

        [Test]
        public void CreateAFileWithDefaultSettingsIfAFileIsMissingAndSettingIsOn()
        {
            this.settingsManager.ActionOnMissingFileOnLoad
                = ActionOnMissingFileOnLoad.CreateFileWithDefaultSettings;
            var defaultSettings = this.settingsManager.GetNewDefaultSettingsInstance();
            var defaultSettingsBytes = this.serializer.Serialize(defaultSettings);
            this.settingsManager.Load();
            if (!File.Exists(this.settingsManager.SettingsFileAbsolutePath))
            {
                Assert.Fail("File not created");
            }

            var bytesFromFile = File.ReadAllBytes(this.settingsManager.SettingsFileAbsolutePath);
            bool areBytesTheSame = this.AreByteArraysEqual(defaultSettingsBytes, bytesFromFile);
            Assert.True(areBytesTheSame);
        }

        [Test]
        public void ThrowFileNotFoundExceptionIfFileIsMissingAndSettingIsOn()
        {
            this.settingsManager.ActionOnMissingFileOnLoad
                = ActionOnMissingFileOnLoad.Throw;
            try
            {
                this.settingsManager.Load();
                Assert.Fail();
            }
            catch (SettingsFileAccessException exc)
            {
                if (exc.InnerException.GetType() == typeof(FileNotFoundException))
                {
                    Assert.Pass();
                }
            }

            Assert.Fail();
        }

        [Test]
        public void RenameOldBrokenFilesWithDefaultSettingsIfSettingIsOn()
        {
            int attempts = 20;
            this.settingsManager.ActionOnFailedDeserialization
                = ActionOnFailedDeserialization.RenameOldFileAndCreateNewWithDefaultSettings;
            ValidSettings2 s2 = new() { PublicStringProperty = "sample text" };
            byte[] invalidSettingsBytes = this.serializer2.Serialize(s2);

            for (int i = 0; i < attempts; i++)
            {
                DateTime start = DateTime.Now;
                this.CreateASettings2File(s2);
                this.settingsManager.Load();
                this.CreateASettings2File(s2);
                this.settingsManager.Load();
                var defaultSettings = this.settingsManager.GetNewDefaultSettingsInstance();
                var defaultSettingsBytes = this.settingsManager.GetSerializedSettings(defaultSettings);

                DateTime end = DateTime.Now;
                if (start.Hour == end.Hour
                    && start.Minute == end.Minute
                    && start.Second == end.Second)
                {
                    string[] files = Directory.GetFiles(this.settingsManager.SettingsFileDirectory);

                    // Valid file.
                    if (files.Contains(this.settingsManager.SettingsFileAbsolutePath))
                    {
                        var validFileBytes = File.ReadAllBytes(this.settingsManager.SettingsFileAbsolutePath);
                        if (!this.AreByteArraysEqual(defaultSettingsBytes, validFileBytes))
                        {
                            Assert.Fail("'Valid' file has wrong content.");
                        }
                    }
                    else
                    {
                        Assert.Fail("No 'Valid' file with the expected name.");
                    }

                    // Broken file 1
                    string brokenFileName1 = Path.Join(
                        this.settingsManager.SettingsFileDirectory,
                        $"OLD-Settings-{start:yyyy-MM-dd-hh-mm-ss}.json");

                    if (files.Contains(brokenFileName1))
                    {
                        var brokenFile1Bytes = File.ReadAllBytes(brokenFileName1);
                        if (!this.AreByteArraysEqual(invalidSettingsBytes, brokenFile1Bytes))
                        {
                            Assert.Fail("Broken file 1 has wrong content.");
                        }
                    }
                    else
                    {
                        Assert.Fail("Missing Broken file 1 with the expected name.");
                    }

                    // Broken file 1
                    string brokenFileName2 = Path.Join(
                        this.settingsManager.SettingsFileDirectory,
                        $"OLD-Settings-{start:yyyy-MM-dd-hh-mm-ss}-2.json");

                    if (files.Contains(brokenFileName2))
                    {
                        var brokenFile2Bytes = File.ReadAllBytes(brokenFileName2);
                        if (!this.AreByteArraysEqual(invalidSettingsBytes, brokenFile2Bytes))
                        {
                            Assert.Fail("Broken file 2 has wrong content.");
                        }
                    }
                    else
                    {
                        Assert.Fail("Missing Broken file 2 with the expected name.");
                    }

                    Assert.Pass();
                }

                this.CleanUpExistingFiles();
            }

            Assert.Warn("Could not create three files in the same second");
        }

        [Test]
        public void OverwriteExistingInvalidFileWithDefaultSettingsIfSettingIsOn()
        {
            this.settingsManager.ActionOnFailedDeserialization
                = ActionOnFailedDeserialization.OverwriteOldFileWithDefaultSettings;
            ValidSettings2 s2 = new();
            s2.PublicStringProperty = "sample text";
            this.CreateASettings2File(s2);
            this.settingsManager.Load();
            var defaultSettings = this.settingsManager.GetNewDefaultSettingsInstance();
            byte[] defaultSettingsBytes = this.serializer.Serialize(defaultSettings);
            var newFileContent = File.ReadAllBytes(this.settingsManager.SettingsFileAbsolutePath);
            bool isFileContentDefault = AreByteArraysEqual(defaultSettingsBytes, newFileContent);
            var files = Directory.GetFiles(this.settingsManager.SettingsFileDirectory);
            bool isThereOnlyOneFile = files.Length == 1;
            Assert.True(isFileContentDefault && isThereOnlyOneFile);
        }

        [Test]
        public void ThrowIfSettingsFileIsInvalidAndSettingIsOn()
        {
            this.settingsManager.ActionOnFailedDeserialization
                = ActionOnFailedDeserialization.Throw;
            ValidSettings2 s2 = new() { PublicStringProperty = "sample text" };
            this.CreateASettings2File(s2);
            try
            {
                this.settingsManager.Load();
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
        }

        private void CreateASettingsFile(ValidSettings settings)
        {
            byte[] bytes = this.serializer.Serialize(settings);
            Directory.CreateDirectory(this.settingsManager.SettingsFileDirectory);
            File.WriteAllBytes(this.settingsManager.SettingsFileAbsolutePath, bytes);
        }

        private void CreateASettings2File(ValidSettings2 settings)
        {
            byte[] bytes = this.serializer2.Serialize(settings);
            Directory.CreateDirectory(this.settingsManager.SettingsFileDirectory);
            File.WriteAllBytes(this.settingsManager.SettingsFileAbsolutePath, bytes);
        }

        private bool AreByteArraysEqual(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
