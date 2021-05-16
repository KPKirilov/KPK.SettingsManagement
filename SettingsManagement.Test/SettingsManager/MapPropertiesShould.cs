namespace SettingsManagement.Test.SettingsManager
{
    using NUnit.Framework;
    using SettingsManagement.Test.Exposers;
    using SettingsManagement.Test.SampleSettings;

    public class MapPropertiesShould
    {
        public SettingsManagerExposer<ValidSettings> SettingsManager { get; set; }

        [SetUp]
        public void Setup()
        {
            this.SettingsManager = new();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void MapPublicPropertiesWithPublicGettersAndSetters()
        {
            int mappedInt = 1;
            ValidSettings s1 = new()
            {
                PublicIntProperty = mappedInt,
            };

            ValidSettings s2 = new();

            this.SettingsManager.MapProperties(s1, s2);
            Assert.AreEqual(mappedInt, s2.PublicIntProperty);
        }

        [Test]
        public void NotMapInternalProperties()
        {
            int mappedInt = 1;
            ValidSettings s1 = new()
            {
                InternalIntProperty = mappedInt,
            };

            ValidSettings s2 = new();

            this.SettingsManager.MapProperties(s1, s2);
            Assert.AreNotEqual(mappedInt, s2.InternalIntProperty);
        }

        [Test]
        public void NotMapPublicPropertiesWithNonPublicGetters()
        {
            int mappedInt = 1;
            ValidSettings s1 = new()
            {
                PublicIntPropertyInternalGetter = mappedInt,
            };

            ValidSettings s2 = new();

            this.SettingsManager.MapProperties(s1, s2);
            Assert.AreNotEqual(mappedInt, s2.PublicIntPropertyInternalGetter);
        }

        [Test]
        public void NotMapPublicPropertiesWithNonPublicSetters()
        {
            int mappedInt = 1;
            ValidSettings s1 = new()
            {
                PublicIntPropertyInternalSetter = mappedInt,
            };

            ValidSettings s2 = new();

            this.SettingsManager.MapProperties(s1, s2);
            Assert.AreNotEqual(mappedInt, s2.PublicIntPropertyInternalSetter);
        }
    }
}
