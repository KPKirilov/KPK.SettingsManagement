namespace SettingsManagement.Options
{
    public enum ActionOnFailedJsonDeserialization
    {
        RenameOldFileAndCreateNewWithDefaultSettings = 0,
        OverwriteOldFileWithDefaultSettings = 1,
        Throw = 2,
    }
}
