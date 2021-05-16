using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingsManagement.Test.SampleSettings
{
    public class Settings1 : ISettings
    {
        public Settings1()
        {
            this.SetToDefault();
        }

        public int PublicIntProperty { get; set; }
        public int PublicIntPropertyInternalSetter { get; internal set; }
        public int PublicIntPropertyInternalGetter { internal get; set; }
        internal int InternalIntProperty { get; set; }
        protected int ProtectedIntProperty { get; set; }
        private int PrivateIntProperty { get; set; }

        public void SetToDefault()
        {
            this.PublicIntProperty = 0;
            this.InternalIntProperty = 0;
            this.ProtectedIntProperty = 0;
            this.PrivateIntProperty = 0;
        }
    }
}
