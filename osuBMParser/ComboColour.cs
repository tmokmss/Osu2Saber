using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuBMParser
{
    public class ComboColour
    {

        #region fields
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }
        #endregion

        #region construcors
        public ComboColour() { }

        public ComboColour(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
        #endregion

        #region methods
        #endregion

    }
}
