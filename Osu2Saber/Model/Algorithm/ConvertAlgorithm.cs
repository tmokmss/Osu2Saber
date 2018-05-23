using Osu2Saber.Model.Json;
using osuBMParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Osu2Saber.Model.Algorithm
{
    class ConvertAlgorithm
    {
        public static bool HandleHitSlider = false;

        protected const float OsuScreenXMax = 512, OsuScreenYMax = 384;
        
        const int NegligibleTimeDiffMs = 500; // is used for the first note and event time gap
        const int EnoughIntervalMs = 1500;  // is used to determine whether to reset cut direction
        const int EnoughIntervalForSymMs = 800; // is used to determine to set a symmetric note
        const float EnoughIntervalBetweenNotes = 0.25f;

        protected Beatmap org;
        protected SaberBeatmap dst;

        protected List<Event> events = new List<Event>();
        protected List<Obstacle> obstacles = new List<Obstacle>();
        protected List<Note> notes = new List<Note>();
        protected int bpm;
        protected int offset;

        Random rnd4dir;

        public List<Event> Events => events;
        public List<Obstacle> Obstacles => obstacles;
        public List<Note> Notes => notes;

        public ConvertAlgorithm(Beatmap osu, SaberBeatmap bs)
        {
            org = osu;
            dst = bs;
            bpm = dst._beatsPerMinute;
            offset = osu.TimingPoints[0].Offset;
            rnd4dir = new Random(offset);
        }

        // You may override this method for better map generation
        public void Convert()
        {
            MakeHitObjects();
            RemoveExcessNotes();
            SetCutDirection();
            AddSymmetryNotes();
            MakeLightEffect();
        }

        void MakeHitObjects()
        {
            foreach (var obj in org.HitObjects)
            {
                if (obj.Time < NegligibleTimeDiffMs) continue;
                if (obj is HitSlider && HandleHitSlider)
                {
                    var temp = (HitSlider)obj;

                    var beatDuration = GetBeatDurationIn(temp.Time);
                    var sliderMultiplier = org.SliderMultiplier;
                    var durationMs = (int)Math.Round(temp.PixelLength / (100.0 * sliderMultiplier) * beatDuration);
                    var lastPos = temp.HitSliderSegments[temp.HitSliderSegments.Count - 1].position;

                    for (var i = 0; i < temp.Repeat; i++)
                    {
                        if (i == 0)
                        {
                            // add beginning and final point
                            AddNote(temp.Time, temp.Position.x, obj.Position.y);
                            AddNote(temp.Time + durationMs, lastPos.x, lastPos.y);
                        }
                        else if (i % 2 == 0)
                        {
                            AddNote(temp.Time + durationMs * (i + 1), temp.Position.x, obj.Position.y);
                        }
                        else
                        {
                            AddNote(temp.Time + durationMs * (i + 1), lastPos.x, lastPos.y);
                        }
                    }
                }
                else if (obj is HitSpinner)
                {
                    var temp = (HitSpinner)obj;
                    AddNote(temp.Time, temp.Position.x, temp.Position.y);
                    
                    // this is just a nuisance at many times
                    // AddNote(temp.EndTime, temp.Position.x, temp.Position.y);
                }
                else
                {
                    AddNote(obj.Time, obj.Position.x, obj.Position.y);
                }

            }
        }

        void RemoveExcessNotes()
        {
            var rightNotes = Notes.Where(note => note._type == (int)(NoteType.Blue)).ToList();
            var leftNotes = Notes.Where(note => note._type == (int)(NoteType.Red)).ToList();
            var newRight = RemoveExcessNotes(rightNotes);
            var newLeft = RemoveExcessNotes(leftNotes);
            foreach (var note in newLeft)
            {
                newRight.Add(note);
            }
            notes = newRight.OrderBy(note => note._time).ToList();
        }

        List<Note> RemoveExcessNotes(List<Note> notes)
        {
            float lastX = 0, lastY = 0;
            double lastTime = -100;
            var newNotes = new List<Note>();
            foreach (var note in notes)
            {
                var distance = Math.Sqrt(Math.Pow(lastX - note._lineIndex, 2) * 9.0 / 16 + Math.Pow(lastY - note._lineLayer, 2));
                var timeGap = note._time - lastTime;

                if (timeGap > EnoughIntervalBetweenNotes * (1 + 0.4 * distance))
                {
                    // the timeGap is long enough
                    newNotes.Add(note);

                    lastTime = note._time;
                    lastX = note._lineIndex;
                    lastY = note._lineLayer;
                }
            }
            return newNotes;
        }

        void AddNote(int timeMs, float posx, float posy)
        {
            var (line, layer) = DeterminePosition(posx, posy);
            var note = new Note(ConvertTime(timeMs), line, layer, DetermineColor(line, layer), CutDirection.Any);
            notes.Add(note);
        }

        double GetBeatDurationIn(int timeMs)
        {
            var baseMsPerBeat = org.TimingPoints[0].MsPerBeat;
            var fac = 1.0;
            foreach (var point in org.TimingPoints)
            {
                if (point.MsPerBeat > 0)
                {
                    baseMsPerBeat = point.MsPerBeat;
                    fac = 1;
                }
                else
                    fac = -point.MsPerBeat / 100.0;

                // TimingPoints is supposed to be sorted by default
                if (point.Offset >= timeMs)
                {
                    break;
                }
            }
            return baseMsPerBeat * fac;
        }

        protected double ConvertTime(int timeMs)
        {
            var unit = 60.0 / bpm / 8.0;
            var sectionIdx = (int)Math.Round(((timeMs) / 1000.0 / unit));
            return Math.Round(sectionIdx / 8.0, 3, MidpointRounding.AwayFromZero);
        }

        protected int ConvertBeat(double timeBeat)
        {
            return (int)Math.Round(timeBeat / bpm * 60 * 1000);
        }

        (int line, int layer) DeterminePosition(float x, float y)
        {
            if (x < 0) x = 0;
            else if (x > OsuScreenXMax) x = OsuScreenXMax;

            if (y < 0) y = 0;
            else if (y > OsuScreenYMax) y = OsuScreenYMax;

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
            var fineSection = 8;
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

        void MakeLightEffect()
        {
            var lastVolume = org.TimingPoints[0].Volume;
            var lastObjTimeMs = org.HitObjects[org.HitObjects.Count - 1].Time;
            foreach (var bp in org.BreakPeriods)
            {
                // turn off lights while break
                var startTime = ConvertTime(bp.BeginTime);
                var endTime = ConvertTime(bp.EndTime);

                // pseudo-randomly choose color
                var color = (bp.BeginTime / 100) % 2 == 0 ? EventLightValue.BlueOn : EventLightValue.RedOn;

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
                ev = new Event(endTime, EventType.LightBottomBackSideLasers, color + 1);
                events.Add(ev);
            }

            var lastTpTime = -1000000;
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

                if (tp.Offset > lastObjTimeMs + EnoughIntervalMs)
                    continue;

                if (tp.KiaiMode)
                {
                    ev = new Event(time, EventType.LightTrackRingNeons, Inverse(color) + 2);
                    events.Add(ev);
                    ev = new Event(time, EventType.LightBackTopLasers, color + 2);
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

                else if (tp.Offset - lastTpTime < NegligibleTimeDiffMs)
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

        void SetCutDirection()
        {
            var n = Notes.Count;
            if (n == 0) return;
            var rightNotes = Notes.Where(note => note._type == (int)(NoteType.Blue)).ToList();
            var leftNotes = Notes.Where(note => note._type == (int)(NoteType.Red)).ToList();

            SetCutDirection(rightNotes);
            SetCutDirection(leftNotes);
        }

        void SetCutDirection(List<Note> notes)
        {
            var n = notes.Count;
            if (n < 1) return;

            // the first cut direction can be determined independently
            notes[0]._cutDirection = (int)PickBestDirectionSingle(notes[0]._lineIndex, notes[0]._lineLayer);

            // the other notes should be determined depending on one before note.
            for (var i = 1; i < n; i++)
            {
                SetCutDirection(notes[i - 1], notes[i]);
            }
        }

        void SetCutDirection(Note before, Note now)
        {
            var swingFac = Math.Pow(ConvertBeat(now._time - before._time) * 1.0 / EnoughIntervalMs, 1.0 / 2);
            if (swingFac > 1)
            {
                now._cutDirection = (int)PickBestDirectionSingle(now._lineIndex, now._lineLayer);
                //now._cutDirection = (int)PickBestDirectionCont(now, before, swingFac);
            }
            else
            {
                now._cutDirection = (int)PickBestDirectionCont(now, before, swingFac);
            }
        }

        // the best cut direction for each section below
        //  8  9 10 11
        //  4  5  6  7
        //  0  1  2  3 
        int[] bestDir = new int[] { 6, 1, 1, 7, 2, 8, 8, 3, 4, 0, 0, 5 };
        CutDirection PickBestDirectionSingle(int line, int layer)
        {
            var idx = (int)Line.MaxNum * layer + line;
            var best = (CutDirection)bestDir[idx];
            if (best == CutDirection.Any) return PickRandomDirection();
            return best;
        }

        CutDirection PickRandomDirection(DirectionRandomMode mode = DirectionRandomMode.Any)
        {
            int min = 0, max = (int)CutDirection.Any;
            switch (mode)
            {
                case DirectionRandomMode.OnlyNormal:
                    max = (int)CutDirection.Left + 1;
                    break;
                case DirectionRandomMode.OnlyDiagonal:
                    min = (int)CutDirection.DownRight;
                    break;
            }
            return (CutDirection)rnd4dir.Next(min, max);
        }

        // positon difference in each axis after the specified direction of cut
        static float Sqr2 = (float)Math.Sqrt(2);
        float[] lineDiff = new float[] { 0, 0, -1, 1, -Sqr2, Sqr2, -Sqr2, Sqr2 };
        float[] layerDiff = new float[] { 1, -1, 0, 0, Sqr2, Sqr2, -Sqr2, -Sqr2 };

        CutDirection PickBestDirectionCont(Note now, Note before, double swingAmount)
        {
            var lastcut = before._cutDirection;
            if (lastcut == (int)CutDirection.Any)
                lastcut = (int)PickRandomDirection();

            // limit factor
            swingAmount = Math.Max(swingAmount, 2.5);

            // this is where player's hand supposed to be
            var nowline = before._lineIndex + lineDiff[lastcut] * swingAmount;
            var nowlayer = before._lineLayer + layerDiff[lastcut] * swingAmount;

            var linegap = now._lineIndex - nowline;
            var layergap = now._lineLayer - nowlayer;
            var deg = Math.Atan2(layergap, linegap * 3.0 / 4) * 180 / Math.PI;
            return PickDirectionFromDeg(deg);
        }

        CutDirection PickDirectionFromDeg(double deg)
        {
            // divide a circle into 8 sections
            const double Div = 45;
            if (deg >= 180 - Div / 2) return CutDirection.Left;
            if (deg >= 180 - Div * 3 / 2) return CutDirection.UpLeft;
            if (deg >= 180 - Div * 5 / 2) return CutDirection.Up;
            if (deg >= 180 - Div * 7 / 2) return CutDirection.UpRight;
            if (deg >= 180 - Div * 9 / 2) return CutDirection.Right;
            if (deg >= 180 - Div * 11 / 2) return CutDirection.DownRight;
            if (deg >= 180 - Div * 13 / 2) return CutDirection.Down;
            if (deg >= 180 - Div * 15 / 2) return CutDirection.DownLeft;
            return CutDirection.Left;
        }

        void AddSymmetryNotes()
        {
            var n = Notes.Count;
            if (n < 2) return;

            var addingNotes = new List<Note>();
            SymmetryMode symmode = SymmetryMode.Line;

            AddSymmetryNote(null, Notes[0], Notes[1], addingNotes, symmode);
            for (var i = 1; i < Notes.Count - 1; i++)
            {
                var now = Notes[i];
                AddSymmetryNote(Notes[i - 1], now, Notes[i + 1], addingNotes, symmode);
            }
            AddSymmetryNote(Notes[n - 2], Notes[n - 1], null, addingNotes, symmode);

            foreach (var note in addingNotes)
            {
                notes.Add(note);
            }
            notes = Notes.OrderBy(note => note._time).ToList();
        }

        void AddSymmetryNote(Note before, Note now, Note after, List<Note> addition, SymmetryMode symmode)
        {
            double lastInterval = 0, nextInterval = 0;
            if (before == null)
                lastInterval = EnoughIntervalForSymMs * 2;
            else
                lastInterval = ConvertBeat(now._time - before._time);

            if (after == null)
                nextInterval = EnoughIntervalForSymMs * 2;
            else
                nextInterval = ConvertBeat(after._time - now._time);

            if (nextInterval > EnoughIntervalForSymMs && lastInterval > EnoughIntervalForSymMs)
            {
                var note = GetMirrorNote(now, symmode);
                addition.Add(note);
            }
        }

        Note GetMirrorNote(Note note, SymmetryMode mode)
        {
            int line = 0, layer = 0;
            switch (mode)
            {
                case SymmetryMode.Line:
                    line = (int)(-note._lineIndex + (int)Line.Right);
                    layer = note._lineLayer;
                    break;
                default:
                    line = (int)(-note._lineIndex + (int)Line.Right);
                    layer = (int)(-note._lineLayer + (int)Layer.Top);
                    break;
            }
            var dir = PickOppositeDirection(note._cutDirection, mode);
            var type = note._type == (int)NoteType.Blue ? NoteType.Red : NoteType.Blue;
            return new Note(note._time, line, layer, type, dir);
        }

        // the cut direction for symmetrically placed note
        int[] lineSym = new int[] { 0, 1, 3, 2, 5, 4, 7, 6 };
        int[] pointSym = new int[] { 1, 0, 3, 2, 7, 6, 5, 4 };
        CutDirection PickOppositeDirection(int dir, SymmetryMode mode)
        {
            if (dir < 0 || dir >= (int)CutDirection.Any)
                return CutDirection.Any;

            switch (mode)
            {
                case SymmetryMode.Line:
                    return (CutDirection)lineSym[dir];
                default:
                    return (CutDirection)pointSym[dir];
            }
        }
    }

    enum DirectionRandomMode
    {
        Any,
        OnlyNormal,
        OnlyDiagonal
    }

    enum SymmetryMode
    {
        Line,
        Point
    }
}
