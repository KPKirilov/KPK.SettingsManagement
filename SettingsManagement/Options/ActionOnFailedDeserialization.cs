namespace SettingsManagement.Options
{
    /// <summary>
    /// Enum indicating what action should be taken when a deserialization fails.
    /// </summary>
    public enum ActionOnFailedDeserialization
    {
        /// <summary>
        /// Keeps the old file under a different name.
        /// Creates a new file with default settings. Loads default settings.
        /// </summary>
        RenameOldFileAndCreateNewWithDefaultSettings = 0,

        /// <summary>
        /// Overwrites the old file with a new one with default settings. Loads default settings.
        /// </summary>
        OverwriteOldFileWithDefaultSettings = 1,

        /// <summary>
        /// Throws an exception.
        /// </summary>
        Throw = 2,
    }
}
