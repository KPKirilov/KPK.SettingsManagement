﻿namespace SettingsManagement.Test.SettingsManager
{
    using NUnit.Framework;
    using SettingsManagement.Test.Exposers;
    using SettingsManagement.Test.SampleSettings;

    public class MapPropertiesShould
    {
        public SettingsManagerExposer<Settings1> SettingsManager { get; set; }

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
            Settings1 s1 = new()
            {
                PublicIntProperty = mappedInt,
            };

            Settings1 s2 = new();

            this.SettingsManager.MapProperties(s1, s2);
            Assert.AreEqual(mappedInt, s2.PublicIntProperty);
        }

        [Test]
        public void NotMapInternalProperties()
        {
            int mappedInt = 1;
            Settings1 s1 = new()
            {
                InternalIntProperty = mappedInt,
            };

            Settings1 s2 = new();

            this.SettingsManager.MapProperties(s1, s2);
            Assert.AreNotEqual(mappedInt, s2.InternalIntProperty);
        }

        [Test]
        public void NotMapPublicPropertiesWithNonPublicGetters()
        {
            int mappedInt = 1;
            Settings1 s1 = new()
            {
                PublicIntPropertyInternalGetter = mappedInt,
            };

            Settings1 s2 = new();

            this.SettingsManager.MapProperties(s1, s2);
            Assert.AreNotEqual(mappedInt, s2.PublicIntPropertyInternalGetter);
        }

        [Test]
        public void NotMapPublicPropertiesWithNonPublicSetters()
        {
            int mappedInt = 1;
            Settings1 s1 = new()
            {
                PublicIntPropertyInternalSetter = mappedInt,
            };

            Settings1 s2 = new();

            this.SettingsManager.MapProperties(s1, s2);
            Assert.AreNotEqual(mappedInt, s2.PublicIntPropertyInternalSetter);
        }
    }
}