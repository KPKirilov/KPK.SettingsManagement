namespace SettingsManagement.Options
{
    /// <summary>
    /// Enum indicating what action should be taken when trying to load a missing file.
    /// </summary>
    public enum ActionOnMissingFileOnLoad
    {
        /// <summary>
        /// Creates a file with default settings.
        /// </summary>
        CreateFileWithDefaultSettings = 0,

        /// <summary>
        /// Throws an exception.
        /// </summary>
        Throw = 1,
    }
}
