using Osu2Saber.Model.Json;
using osuBMParser;
using System;
using System.Collections.Generic;

namespace Osu2Saber.Model.Algorithm
{
    class ConvertAlgorithm
    {
        protected Beatmap org;
        protected SaberBeatmap dst;

        protected List<Event> events = new List<Event>();
        protected List<Obstacle> obstacles = new List<Obstacle>();
        protected List<Note> notes = new List<Note>();

        public List<Event> Events => events;
        public List<Obstacle> Obstacles => obstacles;
        public List<Note> Notes => notes;

        public ConvertAlgorithm(Beatmap osu, SaberBeatmap bs)
        {
            org = osu;
            dst = bs;
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
                var note = new Note()
                {
                    _time = ConvertTime(dst._beatsPerMinute, obj.Time),
                    _lineIndex = line,
                    _lineLayer = layer,
                    _type = (int)DetermineColor(line, layer),
                    _cutDirection = (int)CutDirection.Any,
                };
                notes.Add(note);
            }
        }

        void MakeLightEffect()
        {
            foreach (var timingPoint in org.TimingPoints)
            {

            }
        }

        protected double ConvertTime(int bpm, int timeMs)
        {
            var unit = 60.0 / bpm / 8.0;
            var sectionIdx = (int)(timeMs / 1000.0 / unit);
            return sectionIdx / 8.0;
        }

        (int line, int layer) DeterminePosition(float x, float y)
        {
            // just map notes position to BS screen
            const int XMax = 512, YMax = 384;
            var line = (int)Math.Floor(x * 3.0 / XMax);
            var layer = (int)Math.Floor(y * 3.0 / YMax);
            return (line: line, layer: layer);
        }

        NoteType DetermineColor(int line, int layer)
        {
            if (line < 2) return NoteType.Red;
            return NoteType.Blue;
        }
    }
}
