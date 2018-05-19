using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuBMParser
{
    public class HitSpinner : HitObject
    {

        #region fields
        public int EndTime { get; set; }
        #endregion

        #region constructors
        public HitSpinner() { }

        public HitSpinner(Vector2 position, int time, int hitSound, int endTime, int[] addition, bool isNewCombo) : base(position, time, hitSound, addition, isNewCombo)
        {
            this.EndTime = endTime;
        }
        #endregion

    }
}
