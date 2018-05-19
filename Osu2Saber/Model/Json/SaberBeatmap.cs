using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu2Saber.Model.Json
{
    /// <summary>
    /// This class contains all the required information for [difficulty].json, namely BS beatmap file.
    /// The enum definitions are retrieved from some reverse engineering work.
    /// 
    /// Note:
    /// * the unit of time in BS is second
    /// * 
    /// </summary>
    public class SaberBeatmap
    {
        public string _version;
        public int _beatsPerMinute;
        public int _beatsPerBar;
        public int _noteJumpSpeed;
        public int _shuffle;
        public double _shufflePeriod;
        public List<Event> _events;
        public List<Note> _notes;
        public List<Obstacle> _obstacles;
    }

    public class Event
    {
        public double _time;
        public int _type;
        public int _value;
    }

    public class Note
    {
        public double _time;
        public int _lineIndex;
        public int _lineLayer;
        public int _type;
        public int _cutDirection;
    }

    public class Obstacle
    {
        public double _time;
        public int _lineIndex;
        public int _type;
        public int _duration;
        public int _width;
    }

    public enum Line
    {
        Bottom = 0,
        Middle,
        Top
    }

    public enum CutDirection
    {
        Up = 0,
        Down,
        Right,
        Left,
        BottomRight,
        BottomLeft,
        UpRight,
        UpLeft,
        Any
    }

    public enum NoteType
    {
        Red = 0,
        Blue,
        Mine
    }

    public enum ObstacleType
    {
        Wall = 0,
        Ceiling,
    }

    public enum EventType
    {
        BlackTopLasers = 0,
        TrackRingNeons,
        LeftLasers,
        RightLasers,
        BotBackSideLasers,

        AllTrackRings = 8,
        SmallTrackRings,

        RotatingLeftLasers = 12,
        RotatingRightLasers

    }

    public enum LineLayer
    {
        Bottom = 0,
        Middle,
        Top
    }

}
