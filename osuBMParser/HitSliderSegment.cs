using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuBMParser
{
    public class HitSliderSegment
    {

        #region fields 
        public Vector2 position { get; set; }
        public int time { get; set; }
        #endregion

        #region constructors
        public HitSliderSegment(Vector2 position)
        {
            init();
            this.position = position;
        }
        #endregion

        #region methods
        private void init()
        {
            position = new Vector2();
        }
        #endregion

    }
}
