namespace SettingsManagement
{
    using SettingsManagement.Options;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class SettingsManager<T>
        where T :
            ISettings,
            new()
    {
        public static readonly string DefaultSettingsFileRelativePath;
        private SettingsFilePathReferencePoint settingsFilePathReferencePoint;
        private string settingsFileAbsolutePath;
        private string settingsFileRelativePathFromStandardReferencePoints;

        static SettingsManager()
        {
            SettingsManager<T>.DefaultSettingsFileRelativePath = Path.Join("Settings", "Settings.json");
        }

        public SettingsManager()
        {
            this.Settings = this.GetNewDefaultSettingsInstance();
            this.ActionOnMissingFileOnLoad = ActionOnMissingFileOnLoad.CreateFileWithDefaultSettings;
            this.ActionOnFailedJsonDeserialization = ActionOnFailedJsonDeserialization.OverwriteOldFileWithDefaultSettings;
            this.ShouldThrowOnFailedToSave = true;

            // Synced properties. Order matters. 
            this.settingsFileRelativePathFromStandardReferencePoints 
                = SettingsManager<T>.DefaultSettingsFileRelativePath;
            this.settingsFilePathReferencePoint = SettingsFilePathReferencePoint.CallingAssembly;
            this.UpdateSettingsFileAbsolutePathIfStandard();
        }

        public T Settings { get; protected set; }

        public string SettingsFileAbsolutePath 
        {
            get
            {
                return this.settingsFileAbsolutePath;
            }

            set
            {
                this.settingsFileAbsolutePath = value;
                this.settingsFilePathReferencePoint = SettingsFilePathReferencePoint.Custom;
            }
        }

        public string SettingsFileRelativePathFromStandardReferencePoints
        {
            get
            {
                return this.settingsFileRelativePathFromStandardReferencePoints;
            }

            set
            {
                this.settingsFileRelativePathFromStandardReferencePoints = value;
                this.UpdateSettingsFileAbsolutePathIfStandard();
            }
        }

        public string SettingsFileDirectory
        {
            get
            {
                return Path.GetDirectoryName(this.SettingsFileAbsolutePath);
            }
        }

        public ActionOnMissingFileOnLoad ActionOnMissingFileOnLoad { get; set; }

        public SettingsFilePathReferencePoint SettingsFilePathReferencePoint
        {
            get
            {
                return this.settingsFilePathReferencePoint;
            }

            set
            {
                this.settingsFilePathReferencePoint = value;
                this.UpdateSettingsFileAbsolutePathIfStandard();
            }
        }

        public ActionOnFailedJsonDeserialization ActionOnFailedJsonDeserialization { get; set; }

        public bool ShouldThrowOnFailedToSave { get; set; }

        public void Load()
        {
            if (!File.Exists(SettingsFileAbsolutePath))
            {
                switch (this.ActionOnMissingFileOnLoad)
                {
                    case ActionOnMissingFileOnLoad.CreateFileWithDefaultSettings:
                        this.CreateNewFileWithDefaultSettings();
                        break;
                    case ActionOnMissingFileOnLoad.None:
                        break;
                    case ActionOnMissingFileOnLoad.Throw:
                        throw new FileNotFoundException($"The source file for the settings was not found. Exception is thrown because {nameof(ActionOnMissingFileOnLoad)} is set to {nameof(ActionOnMissingFileOnLoad.Throw)}", SettingsFileAbsolutePath);
                    default:
                        break;
                }
            }

            string jsonString = File.ReadAllText(this.SettingsFileAbsolutePath);
            try
            {
                T settingsFromFile = JsonSerializer.Deserialize<T>(jsonString);
                this.CopySettingsFromObject(settingsFromFile);
            }
            catch (Exception)
            {
                switch (this.ActionOnFailedJsonDeserialization)
                {
                    case ActionOnFailedJsonDeserialization.RenameOldFileAndCreateNewWithDefaultSettings:
                        string newNameForOldFile = this.GetNewNameForFileToBeOverwritten(this.SettingsFileAbsolutePath);
                        File.Move(this.SettingsFileAbsolutePath, newNameForOldFile);
                        this.CreateNewFileWithDefaultSettings();
                        this.Load();
                        break;
                    case ActionOnFailedJsonDeserialization.OverwriteOldFileWithDefaultSettings:
                        this.CreateNewFileWithDefaultSettings();
                        this.Load();
                        break;
                    case ActionOnFailedJsonDeserialization.Throw:
                        throw;
                    default:
                        throw;
                }
            }
        }

        public void Save()
        {
            this.Save(this.Settings);
        }

        public T GetCopy()
        {
            var copy = this.GetNewDefaultSettingsInstance();
            this.MapProperties(this.Settings, copy);
            return copy;
        }

        public void CopySettingsFromObject(T settingsObjectToCopyFrom)
        {
            this.MapProperties(settingsObjectToCopyFrom, this.Settings);
        }

        public string GetSettingsJsonString()
        {
            return this.GetSettingsJsonString(this.Settings);
        }

        public T GetNewDefaultSettingsInstance()
        {
            var result = Activator.CreateInstance<T>();
            result.SetToDefault();
            return result;
        }

        protected string GetNewNameForFileToBeOverwritten(string originalFileName)
        {
            DateTime now = DateTime.Now;
            string timeStamp = $"{now:yyyy-MM-dd-hh-mm-ss}";
            string fileNameNoExtension =
                    Path.GetFileNameWithoutExtension(originalFileName);
            string fileExtension = Path.GetExtension(originalFileName);
            string directory = Path.GetDirectoryName(originalFileName);

            int iterations = 1;
            while (true)
            {
                string iterationsStamp = $"-{iterations}";
                if (iterations == 1)
                {
                    iterationsStamp = string.Empty;
                }

                string fileName = $"OLD-{fileNameNoExtension}-{timeStamp}{iterationsStamp}{fileExtension}";
                string filePath = Path.Combine(directory, fileName);
                if (!File.Exists(filePath))
                {
                    return filePath;
                }

                iterations++;
            }
        }

        protected void MapProperties(T sourceObject, T targetObject)
        {
            Type type = typeof(T);
            List<PropertyInfo> propertiesToMap = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToList();

            foreach (var p in propertiesToMap)
            {
                var getMethod = p.GetGetMethod();
                if (getMethod == null || !getMethod.IsPublic)
                {
                    continue;
                }

                var setMethod = p.GetSetMethod();
                if (setMethod == null || !setMethod.IsPublic)
                {
                    continue;
                }

                object value = p.GetValue(sourceObject);
                p.SetValue(targetObject, value);
            }
        }

        protected void UpdateSettingsFileAbsolutePathIfStandard()
        {
            Assembly referenceAssembly;
            switch (this.settingsFilePathReferencePoint)
            {
                case SettingsFilePathReferencePoint.CallingAssembly:
                    referenceAssembly = Assembly.GetCallingAssembly();
                    break;
                case SettingsFilePathReferencePoint.ExecutingAssembly:
                    referenceAssembly = Assembly.GetExecutingAssembly();
                    break;
                case SettingsFilePathReferencePoint.Custom:
                    return;
                default:
                    break;
            }

            string referenceAssemblyPath = Assembly.GetCallingAssembly().Location;
            string referenceAssemblyDirectory = Path.GetDirectoryName(referenceAssemblyPath);

            this.settingsFileAbsolutePath = Path.Join(
                referenceAssemblyDirectory,
                this.settingsFileRelativePathFromStandardReferencePoints);
        }

        private void Save(T settings)
        {
            try
            {
                string jsonString = this.GetSettingsJsonString(settings);

                if (!Directory.Exists(this.SettingsFileDirectory))
                {
                    Directory.CreateDirectory(this.SettingsFileDirectory);
                }

                File.WriteAllText(this.SettingsFileAbsolutePath, jsonString);
            }
            catch (Exception)
            {
                if (this.ShouldThrowOnFailedToSave)
                {
                    throw;
                }
            }
        }

        private string GetSettingsJsonString(T settings)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve,
            };

            string jsonString = JsonSerializer.Serialize<T>(settings, options);
            return jsonString;
        }

        private void CreateNewFileWithDefaultSettings()
        {
            T defaultSettings = this.GetNewDefaultSettingsInstance();
            this.Save(defaultSettings);
        }
    }
}
