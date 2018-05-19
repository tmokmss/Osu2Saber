using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuBMParser
{
    public abstract class HitObject
    {

        #region fields
        public Vector2 Position { get; set; }
        public int Time { get; set; }
        public int HitSound { get; set; }
        public List<int> Addition { get; set; }
        public bool IsNewCombo { get; set; }
        #endregion

        #region constructors
        public HitObject()
        {
            init();
        }
        
        public HitObject(Vector2 position, int time, int hitSound, int[] addition, bool isNewCombo) : this()
        {   
            this.Position = position;
            this.Time = time;
            this.HitSound = hitSound;
            this.Addition.AddRange(addition);
            this.IsNewCombo = isNewCombo;
        }
        #endregion

        #region methods
        private void init()
        {
            Position = new Vector2();
            Addition = new List<int>();
        }
        #endregion

    }
}
