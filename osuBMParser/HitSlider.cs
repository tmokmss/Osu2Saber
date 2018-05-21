using System.Collections.Generic;

namespace osuBMParser
{
    public class HitSlider : HitObject
    {

        public enum SliderType
        {
            NONE,
            BREZIER,
            CATMULL,
            LINEAR,
            PASSTHROUGH
        }

        #region fields
        public SliderType Type { get; set; }
        public List<HitSliderSegment> HitSliderSegments { get; set; }
        public int Repeat { get; set; }
        public float PixelLength { get; set; }
        public int EdgeHitSound { get; set; }
        public List<int> EdgeAddition { get; set; }
        #endregion

        #region constructors
        public HitSlider()
        {
            init();
        }
        
        public HitSlider(Vector2 position, int time, int hitSound, SliderType type, HitSliderSegment[] hitSliderSegments, int repeat, float pixelLength, int edgeHitSound, int[] edgeAddition, int[] addition, bool isNewCombo) : base(position, time, hitSound, addition, isNewCombo)
        {
            init();
            this.Type = type;
            this.HitSliderSegments.AddRange(hitSliderSegments);
            this.Repeat = repeat;
            this.PixelLength = pixelLength;
            this.EdgeHitSound = edgeHitSound;
            this.EdgeAddition.AddRange(edgeAddition);
        }
        #endregion

        #region methods
        private void init()
        {
            HitSliderSegments = new List<HitSliderSegment>();
            EdgeAddition = new List<int>();
        }

        public static SliderType parseSliderType(string data)
        {
            switch (data.Trim().ToLower())
            {
                case "b":
                    return SliderType.BREZIER;
                case "c":
                    return SliderType.CATMULL;
                case "l":
                    return SliderType.LINEAR;
                case "p":
                    return SliderType.PASSTHROUGH;
                default:
                    return SliderType.NONE;
            }
        }
        #endregion

    }
}
