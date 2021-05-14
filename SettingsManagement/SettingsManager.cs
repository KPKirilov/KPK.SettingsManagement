using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingsManagement
{
    public class SettingsManager<T>
        where T: 
            ISettings, 
            new()
    {
        public T Settings { get; private set; }
    }
}
