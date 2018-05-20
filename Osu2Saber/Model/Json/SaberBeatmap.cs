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
        Left = 0,
        MiddleLeft,
        MiddleRight,
        Right,
        MaxNum
    }

    public enum Layer
    {
        Bottom = 0,
        Middle,
        Top,
        MaxNum
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
        Mine = 3
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

    public enum EventLightValue
    {
        Off = 0,
        BlueOn,
        BlueFlashStay,
        BlueFlashFade,
        RedOn = 5,
        RedFlashStay,
        RedFlashFade
    }

    public enum EventRotationValue
    {
        Stop = 0,
        Speed1,
        Speed2,
        Speed3,
        Speed4,
        Speed5,
        Speed6,
        Speed7,
        Speed8
    }


}
