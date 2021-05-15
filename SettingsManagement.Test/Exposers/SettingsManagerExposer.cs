namespace SettingsManagement.Test.Exposers
{
    using SettingsManagement.Options;

    public class SettingsManagerExposer<T>: SettingsManager<T>
        where T :
            ISettings,
            new()
    {
        public SettingsManagerExposer()
            : this(new SettingsManagerOptions())
        {
        }

        public SettingsManagerExposer(SettingsManagerOptions options)
            : base(options)
        {
        }

        public new string GetAbsoluteNameForBrokenOldFile()
        {
            return base.GetAbsoluteNameForBrokenOldFile();
        }

        public new void MapProperties(T sourceObject, T targetObject)
        {
            base.MapProperties(sourceObject, targetObject);
        }

        public new string GetDefaultSettingsFileAbsolutePath()
        {
            return base.GetDefaultSettingsFileAbsolutePath();
        }
    }
}
