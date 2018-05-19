using System;
using System.Collections.Generic;
using System.Reflection;

namespace osuBMParser
{
    public class Beatmap
    {
        
        #region fields
        #region extra
        public string FormatVersion { get; set; }
        #endregion

        #region general        
        public string AudioFileName { get; set; }
        public int AudioLeadIn { get; set; }
        public int PreviewTime { get; set; }
        public bool Countdown { get; set; }
        public string SampleSet { get; set; }
        public float StackLeniency { get; set; }
        public int Mode { get; set; }
        public bool LetterBoxInBreaks { get; set; }
        public bool WidescreenStoryboard { get; set; }
        #endregion

        #region editor
        public List<int> Bookmarks { get; set; }
        public float DistanceSpacing { get; set; }
        public int BeatDivisor { get; set; }
        public int GridSize { get; set; }
        public int TimelineZoom { get; set; }
        #endregion

        #region metadata
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Creator { get; set; }
        public string Version { get; set; }
        public string Source { get; set; }
        public List<string> Tags { get; set; }        
        public int BeatmapID { get; set; }
        public int BeatmapSetID { get; set; }
        #endregion

        #region difficulty
        public float HpDrainRate { get; set; }
        public float CircleSize { get; set; }
        public float OverallDifficulty { get; set; }
        public float ApproachRate { get; set; }
        public float SliderMultiplier { get; set; }
        public float SliderTickRate { get; set; }
        #endregion

        #region events
        // I'll do this later :p
        #endregion

        #region timingPoints
        public List<TimingPoint> TimingPoints { get; set; }        
        #endregion

        #region colours
        public List<ComboColour> Colours { get; set; }      
        #endregion

        #region hitObjects 
        public List<HitObject> HitObjects { get; set; }
        #endregion
        #endregion

        #region constructors
        private Beatmap()
        {
            init();
        }
        
        public Beatmap(string path) : this()
        {
            OsuFileParser parser = new OsuFileParser(path, this);
            parser.parse();
        }

        public Beatmap(string path, OsuFileParser.OsuFileSection sections) : this()
        {
            OsuFileParser parser = new OsuFileParser(path, this);
            parser.parse(sections);
        }
        #endregion

        #region methods
        public void init()
        {
            Bookmarks = new List<int>();
            Tags = new List<string>();
            TimingPoints = new List<TimingPoint>();
            Colours = new List<ComboColour>();
            HitObjects = new List<HitObject>();
        }
        #endregion

    }
}
