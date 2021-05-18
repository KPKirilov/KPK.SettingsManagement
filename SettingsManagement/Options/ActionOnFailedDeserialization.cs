namespace SettingsManagement.Options
{
    public enum ActionOnFailedDeserialization
    {
        RenameOldFileAndCreateNewWithDefaultSettings = 0,
        OverwriteOldFileWithDefaultSettings = 1,
        Throw = 2,
    }
}
