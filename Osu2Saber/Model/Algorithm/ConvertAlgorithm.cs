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
        protected int offset;

        public List<Event> Events => events;
        public List<Obstacle> Obstacles => obstacles;
        public List<Note> Notes => notes;

        public ConvertAlgorithm(Beatmap osu, SaberBeatmap bs)
        {
            org = osu;
            dst = bs;
            bpm = dst._beatsPerMinute;
            offset = osu.TimingPoints[0].Offset;
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
                var color = (bp.BeginTime / 100) % 2 == 0 ? EventLightValue.BlueOn : EventLightValue.RedOn;  // pseudo-randomly choose color

                var ev = new Event(startTime, EventType.LightTrackRingNeons, EventLightValue.Off);
                events.Add(ev);
                ev = new Event(startTime, EventType.LightBottomBackSideLasers, EventLightValue.Off);
                events.Add(ev);
                ev = new Event(startTime, EventType.LightRightLasers, EventLightValue.Off);
                events.Add(ev);
                ev = new Event(startTime, EventType.LightLeftLasers, EventLightValue.Off);
                events.Add(ev);
                ev = new Event(startTime, EventType.RotationAllTrackRings, 1);
                events.Add(ev);

                ev = new Event(endTime, EventType.RotationAllTrackRings, EventRotationValue.Stop);
                events.Add(ev);
                ev = new Event(endTime, EventType.LightTrackRingNeons, color + 1);
                events.Add(ev);
            }

            var lastTpTime = -1000000;
            const int NegligibleTimeDiff = 500;
            foreach (var tp in org.TimingPoints)
            {
                var color = (tp.Offset / 100) % 2 == 0 ? EventLightValue.BlueOn : EventLightValue.RedOn;  // pseudo-randomly choose color
                var bpmFac = tp.MsPerBeat > 0 ? 1 : -tp.MsPerBeat / 100.0;
                var speed = CalcSpeedFromBpm(bpmFac);   // maybe used for rotation speed
                var time = ConvertTime(tp.Offset);  // time in BS

                var ev = new Event(time, EventType.LightBottomBackSideLasers, color);
                events.Add(ev);
                ev = new Event(time, EventType.RotationSmallTrackRings, 1);
                events.Add(ev);


                if (tp.KiaiMode)
                {
                    ev = new Event(time, EventType.LightTrackRingNeons, Inverse(color));
                    events.Add(ev);
                    ev = new Event(time, EventType.LightRightLasers, color);
                    events.Add(ev);
                    ev = new Event(time, EventType.LightLeftLasers, Inverse(color));
                    events.Add(ev);
                    ev = new Event(time, EventType.RotatingLeftLasers, speed);
                    events.Add(ev);
                    ev = new Event(time, EventType.RotatingRightLasers, speed);
                    events.Add(ev);
                }

                else if (tp.Offset - lastTpTime < NegligibleTimeDiff)
                {
                    // do nothing because the last event is too close
                }

                else if (tp.Volume > lastVolume)
                {
                    ev = new Event(time, EventType.LightBackTopLasers, color);
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
                lastTpTime = tp.Offset;
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

        EventLightValue Inverse(EventLightValue color)
        {
            if ((int)color >= 4)
                return color - 4;
            else
                return color + 4;
        }

        protected double ConvertTime(int timeMs)
        {
            var unit = 60.0 / bpm / 8.0;
            var sectionIdx = (int)Math.Round(((timeMs) / 1000.0 / unit));
            return Math.Round(sectionIdx / 8.0, 3, MidpointRounding.AwayFromZero);
        }

        (int line, int layer) DeterminePosition(float x, float y)
        {
            // just map notes position to BS screen
            var line = (int)Math.Floor(x / (OsuScreenXMax + 1) * (double)Line.MaxNum);
            var layer = (int)Math.Floor(y / (OsuScreenYMax + 1) * (double)Layer.MaxNum);
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
            var layerIdx = (int)Math.Floor(y / (OsuScreenYMax + 1) * fineSection);
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
