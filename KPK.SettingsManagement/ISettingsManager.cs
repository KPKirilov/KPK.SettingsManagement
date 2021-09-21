namespace KPK.SettingsManagement
{
    using KPK.SettingsManagement.Options;

    /// <summary>
    /// Provides functionality to handle settings.
    /// </summary>
    /// <typeparam name="T">The type of the settings.</typeparam>
    public interface ISettingsManager<T>
        where T :
            ISettings,
            new()
    {
        /// <summary>
        /// The default relative path for the settings file.
        /// </summary>
        static readonly string DefaultSettingsFileRelativePath;

        /// <summary>
        /// Gets the managed settings object.
        /// </summary>
        T Settings { get; }

        /// <summary>
        /// The absolute path to the settings file. Gets updated by setting <see cref="SettingsFileRelativePath"/>.
        /// </summary>
        string SettingsFileAbsolutePath { get; set; }

        /// <summary>
        /// The relative path to the settings file. Setting this property updates <see cref="SettingsFileAbsolutePath"/>.
        /// </summary>
        string SettingsFileRelativePath { get; set; }

        /// <summary>
        /// Gets the directory of the settings file. Is a product of <see cref="SettingsFileAbsolutePath"/>.
        /// </summary>
        string SettingsFileDirectory { get; }

        /// <summary>
        /// Gets or sets what action should be taken when trying to load a missing file
        /// </summary>
        ActionOnMissingFileOnLoad ActionOnMissingFileOnLoad { get; set; }

        /// <summary>
        /// Gets or sets what action should be taken when a deserialization fails.
        /// </summary>
        ActionOnFailedDeserialization ActionOnFailedDeserialization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the manager should throw an exception if it fails to save the settings file.
        /// </summary>
        bool ShouldThrowOnFailedToSave { get; set; }

        /// <summary>
        /// Loads the settings file from the path provided in SettingsFileAbsolutePath.
        /// </summary
        void Load();

        /// <summary>
        /// Saves the settings file.
        /// </summary>
        void Save();

        /// <summary>
        /// Gets a copy of the settings object.
        /// </summary>
        /// <returns>The copy of the settings object.</returns>
        T GetCopy();

        /// <summary>
        /// Copies the properties from the provided object to the managed Settings object.
        /// </summary>
        /// <param name="settingsObjectToCopyFrom">Object to copy from.</param>
        void CopySettingsFromObject(T settingsObjectToCopyFrom);

        /// <summary>
        /// Gets new settings object with default properties.
        /// </summary>
        /// <returns>A default settings object.</returns>
        T GetNewDefaultSettingsInstance();

        /// <summary>
        /// Gets a byte array that represents the managed settings object.
        /// </summary>
        /// <returns>The serialized object.</returns>
        byte[] GetSerializedSettings();

        /// <summary>
        /// Gets a byte array that represents a settings object.
        /// </summary>
        /// <param name="settings">The settings object to be serialized.</param>
        /// <returns>The serialized settings object.</returns>
        byte[] GetSerializedSettings(T settings);
    }
}
