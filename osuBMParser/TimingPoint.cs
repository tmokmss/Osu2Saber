namespace osuBMParser
{
    public class TimingPoint
    {

        #region fields
        public int Offset { get; set; }
        public float MsPerBeat { get; set; }
        public int Meter { get; set; }
        public int SampleType { get; set; }
        public int SampleSet { get; set; }
        public int Volume { get; set; } = 100;
        public bool Inherited { get; set; } = false;
        public bool KiaiMode { get; set; } = false;
        #endregion

        #region constructors
        public TimingPoint() { }

        public TimingPoint(int offset, float msPerBeat, int meter, int sampleType, int sampleSet, int volume, bool inherited, bool kiaiMode)
        {
            this.Offset = offset;
            this.MsPerBeat = msPerBeat;
            this.Meter = meter;
            this.SampleType = sampleType;
            this.SampleSet = sampleSet;
            this.Volume = volume;
            this.Inherited = inherited;
            this.KiaiMode = kiaiMode;
        }
        #endregion

    }
}
