using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace osuBMParser
{
    public class HitCircle : HitObject
    {

        #region construcors
        public HitCircle() { }
        
        public HitCircle(Vector2 position, int time, int hitSound, int[] addition, bool isNewCombo) : base(position, time, hitSound, addition, isNewCombo) { }
        #endregion

    }
}
