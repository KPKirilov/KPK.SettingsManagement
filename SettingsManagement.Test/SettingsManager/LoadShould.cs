using NUnit.Framework;
using SettingsManagement.Test.SampleSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SettingsManagement.Test.SettingsManager
{
    public class LoadShould
    {
        public SettingsManager<ValidSettings> settingsManager;

        [SetUp]
        public void SetUp()
        {
            this.settingsManager = new();
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
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve,
            };

            var jsonString = JsonSerializer.Serialize<ValidSettings>(settings, options);
            Directory.CreateDirectory(this.settingsManager.SettingsFileDirectory);
            File.WriteAllText(this.settingsManager.SettingsFileAbsolutePath, jsonString);
            this.settingsManager.Load();
            Assert.AreEqual(valueToCompare, this.settingsManager.Settings.PublicIntProperty);
        }
    }
}
