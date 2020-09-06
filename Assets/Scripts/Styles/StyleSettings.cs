using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Styles
{
    [Serializable]
    public class StyleSettings
    {
        public List<TrackStyle> tracks = null;

        public StyleSettings()
        {
            tracks = new List<TrackStyle> { };
        }
    }
    
    [Serializable]
    public class TrackStyle
    {
        public string color = "#00A8E8";
    }
}
