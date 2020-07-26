using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Styles
{
    [DataContract]
    public class StyleSettings
    {
        [DataMember(Name = "trackColors")]
        public List<string> trackColors = null;

        public StyleSettings()
        {
            trackColors = new List<string> { "#00A8E8" };
        }
    }
}
