namespace SettingsManagement
{
    using SettingsManagement.Options;
    using SettingsManagement.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides functionality to handle settings.
    /// </summary>
    /// <typeparam name="T">The type of the settings.</typeparam>
    public class SettingsManager<T>
        where T :
            ISettings,
            new()
    {
        /// <summary>
        /// The default relative path for the settings file.
        /// </summary>
        public static readonly string DefaultSettingsFileRelativePath;

        private string settingsFileRelativePath;

        static SettingsManager()
        {
            SettingsManager<T>.DefaultSettingsFileRelativePath = Path.Join("Settings", "Settings.json");
        }


        /// <summary>
        /// Initializes a new instance of the SettingsManager class using a NewtonsoftJsonSerializer.
        /// </summary>
        public SettingsManager()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SettingsManager class using a provided serializer.
        /// </summary>
        /// <param name="serializer">The serializer to use.</param>
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

        /// <summary>
        /// Gets the managed settings object.
        /// </summary>
        public T Settings { get; protected set; }

        /// <summary>
        /// The absolute path to the settings file. Gets updated by setting <cref>SettingsFileRelativePath</cref>.
        /// </summary>
        public string SettingsFileAbsolutePath { get; set; }

        /// <summary>
        /// The relative path to the settings file. Setting this property updates <cref>SettingsFileAbsolutePath</cref>.
        /// </summary>
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

        /// <summary>
        /// Gets the directory of the settings file. Is a product of <cref>SettingsFileAbsolutePath</cref>.
        /// </summary>
        public string SettingsFileDirectory
        {
            get
            {
                return Path.GetDirectoryName(this.SettingsFileAbsolutePath);
            }
        }

        /// <summary>
        /// Gets or sets what action should be taken when trying to load a missing file
        /// </summary>
        public ActionOnMissingFileOnLoad ActionOnMissingFileOnLoad { get; set; }

        /// <summary>
        /// Gets or sets what action should be taken when a deserialization fails.
        /// </summary>
        public ActionOnFailedDeserialization ActionOnFailedDeserialization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the manager should throw an exception if it fails to save the settings file.
        /// </summary>
        public bool ShouldThrowOnFailedToSave { get; set; }

        /// <summary>
        /// Gets or sets the used serializer.
        /// </summary>
        protected ISerializer<T> Serializer { get; set; }

        /// <summary>
        /// Loads the settings file.
        /// </summary
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

        /// <summary>
        /// Saves the settings file.
        /// </summary>
        public void Save()
        {
            this.Save(this.Settings);
        }

        /// <summary>
        /// Gets a copy of the settings object.
        /// </summary>
        /// <returns>The copy of the settings object.</returns>
        public T GetCopy()
        {
            var copy = this.GetNewDefaultSettingsInstance();
            this.MapProperties(this.Settings, copy);
            return copy;
        }

        /// <summary>
        /// Copies the properties from the provided object to the managed Settings object.
        /// </summary>
        /// <param name="settingsObjectToCopyFrom">Object to copy from.</param>
        public void CopySettingsFromObject(T settingsObjectToCopyFrom)
        {
            this.MapProperties(settingsObjectToCopyFrom, this.Settings);
        }

        /// <summary>
        /// Gets new settings object with default properties.
        /// </summary>
        /// <returns>A default settings object.</returns>
        public T GetNewDefaultSettingsInstance()
        {
            var result = Activator.CreateInstance<T>();
            result.SetToDefault();
            return result;
        }

        /// <summary>
        /// Gets a byte array that represents the managed settings object.
        /// </summary>
        /// <returns>The serialized object.</returns>
        public byte[] GetSerializedSettings()
        {
            return this.GetSerializedSettings(this.Settings);
        }

        /// <summary>
        /// Gets a byte array that represents a settings object.
        /// </summary>
        /// <param name="settings">The settings object to be serialized.</param>
        /// <returns>The serialized settings object.</returns>
        public byte[] GetSerializedSettings(T settings)
        {
            byte[] result = this.Serializer.Serialize(settings);
            return result;
        }

        /// <summary>
        /// Gets a file name which is not taken.
        /// </summary>
        /// <param name="originalFileName">The file name of the original file.</param>
        /// <returns>The file name.</returns>
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

        /// <summary>
        /// Maps the properties from one object to another.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="targetObject">The target object.</param>
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

        /// <summary>
        /// Updates SettingsFileAbsolutePath.
        /// </summary>
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
