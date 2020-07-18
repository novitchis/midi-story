using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Styles
{
    public class StyleSettings
    {
        public string LeftColor { get; set; }

        public string RightColor { get; set; }

        public StyleSettings()
        {
            LeftColor = "#00A8E8";
            RightColor = "#00A8E8";
        }
    }
}
