﻿namespace SettingsManagement.Test.Exposers
{
    using SettingsManagement.Options;

    public class SettingsManagerExposer<T>: SettingsManager<T>
        where T :
            ISettings,
            new()
    {
        public SettingsManagerExposer()
            : base()
        {
        }

        public new string GetNewNameForFileToBeOverwritten(string originalFileName)
        {
            return base.GetNewNameForFileToBeOverwritten(originalFileName);
        }

        public new void MapProperties(T sourceObject, T targetObject)
        {
            base.MapProperties(sourceObject, targetObject);
        }

        public new void UpdateSettingsFileAbsolutePathIfStandard()
        {
            base.UpdateSettingsFileAbsolutePathIfStandard();
        }
    }
}
