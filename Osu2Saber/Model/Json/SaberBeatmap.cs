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
    /// Reference: 
    /// https://pastebin.com/cTPGrxWY
    /// https://docs.google.com/spreadsheets/d/1vCTlDvx0ZW8NkkZBYW6ecvXaVRxDUKX7QIoah9PCp_c/htmlview
    /// </summary>
    public class SaberBeatmap
    {
        public string madeby = "osu2saber";
        public string origin;
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

        public Event(double time, EventType type, EventLightValue value)
        {
            _time = time; _type = (int)type; _value = (int)value;
        }

        public Event(double time, EventType type, EventRotationValue value)
        {
            _time = time; _type = (int)type; _value = (int)value;
        }

        public Event(double time, EventType type, int value)
        {
            _time = time; _type = (int)type; _value = value;
        }
    }

    public class Note
    {
        public double _time;
        public int _lineIndex;
        public int _lineLayer;
        public int _type;
        public int _cutDirection;

        public Note(double time, Line line, Layer layer, NoteType type, CutDirection cutDirection)
        {
            _time = time; _lineIndex = (int)line; _lineLayer = (int)layer; _type = (int)type;
            _cutDirection = (int)cutDirection;
        }

        public Note(double time, int line, int layer, NoteType type, CutDirection cutDirection)
        {
            _time = time; _lineIndex = line; _lineLayer = layer; _type = (int)type; _cutDirection = (int)cutDirection;
        }
    }

    public class Obstacle
    {
        public double _time;
        public int _lineIndex;
        public int _type;
        public float _duration;
        public int _width;
        
        public Obstacle(double time, Line line, ObstacleType type, float duration, int width)
        {
            _time = time; _lineIndex = (int)line; _type = (int)type; _duration = duration; _width = width;
        }
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
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
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
        LightBackTopLasers = 0,
        LightTrackRingNeons,
        LightLeftLasers,
        LightRightLasers,
        LightBottomBackSideLasers,

        RotationAllTrackRings = 8,
        RotationSmallTrackRings,

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
