namespace SettingsManagement.Options
{
    public class SettingsManagerOptions
    {
        public SettingsManagerOptions()
        {
            this.SetToDefault();
        }

        public SettingsFilePathReferencePoint ReferencePoint { get; set; }
        
        public ActionOnMissingFileOnLoad ActionOnMissingFile { get; set; }

        public ActionOnFailedJsonDeserialization ActionOnFailedJsonDeserialization { get; set; }

        public bool ShouldThrowOnFailedToSave { get; set; }

        public void SetToDefault()
        {
            this.ReferencePoint = SettingsFilePathReferencePoint.CallingAssembly;
            this.ActionOnMissingFile = ActionOnMissingFileOnLoad.CreateFileWithDefaultSettings;
            this.ActionOnFailedJsonDeserialization = 
                ActionOnFailedJsonDeserialization.RenameOldFileAndCreateNewWithDefaultSettings;
            this.ShouldThrowOnFailedToSave = true;
        }
    }
}
