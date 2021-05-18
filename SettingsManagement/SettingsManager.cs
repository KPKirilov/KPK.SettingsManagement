namespace SettingsManagement
{
    using SettingsManagement.Options;
    using SettingsManagement.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class SettingsManager<T>
        where T :
            ISettings,
            new()
    {
        public static readonly string DefaultSettingsFileRelativePath;
        private string settingsFileRelativePath;

        static SettingsManager()
        {
            SettingsManager<T>.DefaultSettingsFileRelativePath = Path.Join("Settings", "Settings.json");
        }


        public SettingsManager()
            : this(null)
        {
        }

        public SettingsManager(ISerializer<T> serializer)
        {
            if (serializer == default(ISerializer<T>))
            {
                serializer = new NewtonsoftJsonSerializer<T>();
            }

            this.Serializer = serializer;
            this.Settings = this.GetNewDefaultSettingsInstance();
            this.ActionOnMissingFileOnLoad = ActionOnMissingFileOnLoad.CreateFileWithDefaultSettings;
            this.ActionOnFailedDeserialization = ActionOnFailedDeserialization.OverwriteOldFileWithDefaultSettings;
            this.ShouldThrowOnFailedToSave = true;

            // Synced properties. Order matters. 
            this.settingsFileRelativePath
                = SettingsManager<T>.DefaultSettingsFileRelativePath;
            this.UpdateSettingsFileAbsolutePath();
        }


        public T Settings { get; protected set; }

        public string SettingsFileAbsolutePath { get; set; }

        public string SettingsFileRelativePath
        {
            get
            {
                return this.settingsFileRelativePath;
            }

            set
            {
                this.settingsFileRelativePath = value;
                this.UpdateSettingsFileAbsolutePath();
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

        public ActionOnFailedDeserialization ActionOnFailedDeserialization { get; set; }

        public bool ShouldThrowOnFailedToSave { get; set; }

        protected ISerializer<T> Serializer { get; set; }

        public void Load()
        {
            if (!File.Exists(SettingsFileAbsolutePath))
            {
                switch (this.ActionOnMissingFileOnLoad)
                {
                    case ActionOnMissingFileOnLoad.CreateFileWithDefaultSettings:
                        this.CreateNewFileWithDefaultSettings();
                        break;
                    case ActionOnMissingFileOnLoad.Throw:
                        throw new FileNotFoundException($"The source file for the settings was not found. Exception is thrown because {nameof(ActionOnMissingFileOnLoad)} is set to {nameof(ActionOnMissingFileOnLoad.Throw)}", SettingsFileAbsolutePath);
                    default:
                        break;
                }
            }

            byte[] bytesFromFile = File.ReadAllBytes(this.SettingsFileAbsolutePath);
            try
            {
                T settingsFromFile = this.Serializer.Deserialize(bytesFromFile);
                this.CopySettingsFromObject(settingsFromFile);
            }
            catch (Exception)
            {
                switch (this.ActionOnFailedDeserialization)
                {
                    case ActionOnFailedDeserialization.RenameOldFileAndCreateNewWithDefaultSettings:
                        string newNameForOldFile = this.GetNewNameForFileToBeOverwritten(this.SettingsFileAbsolutePath);
                        File.Move(this.SettingsFileAbsolutePath, newNameForOldFile);
                        this.CreateNewFileWithDefaultSettings();
                        this.Load();
                        break;
                    case ActionOnFailedDeserialization.OverwriteOldFileWithDefaultSettings:
                        this.CreateNewFileWithDefaultSettings();
                        this.Load();
                        break;
                    case ActionOnFailedDeserialization.Throw:
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

        public T GetNewDefaultSettingsInstance()
        {
            var result = Activator.CreateInstance<T>();
            result.SetToDefault();
            return result;
        }

        public byte[] GetSerializedSettings()
        {
            return this.GetSerializedSettings(this.Settings);
        }

        public byte[] GetSerializedSettings(T settings)
        {
            byte[] result = this.Serializer.Serialize(settings);
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

        protected void UpdateSettingsFileAbsolutePath()
        {
            var referenceAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            this.SettingsFileAbsolutePath = Path.Join(
                referenceAssemblyDirectory,
                this.settingsFileRelativePath);
        }

        private void Save(T settings)
        {
            try
            {
                byte[] bytes = this.Serializer.Serialize(settings);

                if (!Directory.Exists(this.SettingsFileDirectory))
                {
                    Directory.CreateDirectory(this.SettingsFileDirectory);
                }

                File.WriteAllBytes(this.SettingsFileAbsolutePath, bytes);
            }
            catch (Exception)
            {
                if (this.ShouldThrowOnFailedToSave)
                {
                    throw;
                }
            }
        }

        private void CreateNewFileWithDefaultSettings()
        {
            T defaultSettings = this.GetNewDefaultSettingsInstance();
            this.Save(defaultSettings);
        }
    }
}
