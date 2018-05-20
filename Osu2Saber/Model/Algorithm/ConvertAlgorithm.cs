using Osu2Saber.Model.Json;
using osuBMParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Osu2Saber.Model.Algorithm
{
    class ConvertAlgorithm
    {
        protected const double OsuScreenXMax = 512, OsuScreenYMax = 384;

        protected Beatmap org;
        protected SaberBeatmap dst;

        protected List<Event> events = new List<Event>();
        protected List<Obstacle> obstacles = new List<Obstacle>();
        protected List<Note> notes = new List<Note>();
        protected int bpm;

        public List<Event> Events => events;
        public List<Obstacle> Obstacles => obstacles;
        public List<Note> Notes => notes;

        public ConvertAlgorithm(Beatmap osu, SaberBeatmap bs)
        {
            org = osu;
            dst = bs;
            bpm = dst._beatsPerMinute;
        }

        // You may override this method for better map generation
        public void Convert()
        {
            MakeHitObjects();
            MakeLightEffect();
        }

        void MakeHitObjects()
        {
            foreach (var obj in org.HitObjects)
            {
                var (line, layer) = DeterminePosition(obj.Position.x, obj.Position.y);
                var note = new Note(ConvertTime(obj.Time), line, layer, DetermineColor(line, layer), CutDirection.Any);
                notes.Add(note);
            }
        }

        void MakeLightEffect()
        {
            var lastVolume = org.TimingPoints[0].Volume;
            var lastObjTime = org.HitObjects[org.HitObjects.Count - 1].Time;
            foreach (var bp in org.BreakPeriods)
            {
                // turn off lights while break
                var startTime = ConvertTime(bp.BeginTime);
                var endTime = ConvertTime(bp.EndTime);
                var colorOfs = (bp.BeginTime / 100) % 2 == 0 ? 4 : 0;  // pseudo-randomly choose color

                var ev = new Event(startTime, EventType.LightTrackRingNeons, EventLightValue.Off);
                events.Add(ev);
                ev = new Event(startTime, EventType.LightBottomBackSideLasers, EventLightValue.Off);
                events.Add(ev);
                ev = new Event(startTime, EventType.LightRightLasers, EventLightValue.Off);
                events.Add(ev);
                ev = new Event(startTime, EventType.LightLeftLasers, EventLightValue.Off);
                events.Add(ev);
                ev = new Event(startTime, EventType.RotationAllTrackRings, EventRotationValue.Speed3);
                events.Add(ev);

                ev = new Event(endTime, EventType.RotationAllTrackRings, EventRotationValue.Stop);
                events.Add(ev);
                ev = new Event(endTime, EventType.LightBottomBackSideLasers, EventLightValue.BlueOn + colorOfs);
                events.Add(ev);
            }


            foreach (var tp in org.TimingPoints)
            {
                var colorOfs = (tp.Offset / 100) % 2 == 0 ? 4 : 0;  // pseudo-randomly choose color
                var bpmFac = tp.MsPerBeat > 0 ? 1 : -tp.MsPerBeat / 100.0;
                var speed = CalcSpeedFromBpm(bpmFac);   // maybe use for rotation speed
                var time = ConvertTime(tp.Offset);  // time in BS

                var ev = new Event(time, EventType.LightBottomBackSideLasers, EventLightValue.BlueOn + colorOfs);
                events.Add(ev);


                if (tp.KiaiMode)
                {
                    ev = new Event(time, EventType.LightTrackRingNeons, EventLightValue.BlueOn + colorOfs);
                    events.Add(ev);
                    ev = new Event(time, EventType.LightRightLasers, EventLightValue.BlueOn + colorOfs);
                    events.Add(ev);
                    ev = new Event(time, EventType.LightLeftLasers, EventLightValue.BlueOn + colorOfs);
                    events.Add(ev);
                    ev = new Event(time, EventType.RotatingLeftLasers, speed);
                    events.Add(ev);
                    ev = new Event(time, EventType.RotatingRightLasers, speed);
                    events.Add(ev);
                }

                if (tp.Volume > lastVolume)
                {
                    ev = new Event(time, EventType.LightBackTopLasers, EventLightValue.BlueOn + colorOfs);
                    events.Add(ev);
                }
                else
                {
                    ev = new Event(time, EventType.LightRightLasers, EventLightValue.Off);
                    events.Add(ev);
                    ev = new Event(time, EventType.LightLeftLasers, EventLightValue.Off);
                    events.Add(ev);
                }

                lastVolume = tp.Volume;
            }
            events = events.OrderBy(ev => ev._time).ToList();
        }

        int CalcSpeedFromBpm(double bpmFactor)
        {
            double a = 200, b = 60;
            var c = 1 - (8 - 1) * b / (a - b);
            var speedf = (8 - 1) / (a - b) * bpm * bpmFactor + c;
            var speed = (int)Math.Ceiling(speedf);
            if (speed < 1) speed = 1;
            if (speed > 8) speed = 8;
            return speed;
        }

        protected double ConvertTime(int timeMs)
        {
            var unit = 60.0 / bpm / 8.0;
            var sectionIdx = (int)(timeMs / 1000.0 / unit);
            return sectionIdx / 8.0;
        }

        (int line, int layer) DeterminePosition(float x, float y)
        {
            // just map notes position to BS screen
            var line = (int)Math.Floor(x / OsuScreenXMax * (double)Line.MaxNum);
            var layer = (int)Math.Floor(y / OsuScreenYMax * (double)Layer.MaxNum);
            layer = SlideLayer(line, layer, y);
            return (line: line, layer: layer);
        }

        int SlideLayer(int line, int layer, float y)
        {
            // don't want notes come to right front of our eyes so often
            if (layer != (int)Layer.Middle) return layer;
            if (line == (int)Line.Left || line == (int)Line.Right) return layer;

            // The larger this value is, the less likely notes appear in center middle.
            var fineSection = 12;
            var layerIdx = (int)Math.Floor(y / OsuScreenYMax * fineSection);
            if (layerIdx == fineSection / 2) return layer;
            if (layerIdx < fineSection / 2) return (int)Layer.Bottom;
            return (int)Layer.Top;
        }

        NoteType DetermineColor(int line, int layer)
        {
            if (line < 2) return NoteType.Red;
            return NoteType.Blue;
        }
    }
}
