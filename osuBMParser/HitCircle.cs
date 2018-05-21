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
