using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace osuBMParser
{
    public class OsuFileParser
    {

        [Flags]
        public enum OsuFileSection
        {
            NONE = 0,
            FORMAT = 1 << 0,
            GENERAL = 1 << 1,
            EDITOR = 1 << 2,
            METADATA = 1 << 3,
            DIFFICULTY = 1 << 4,
            EVENTS = 1 << 5,
            TIMINGPOINTS = 1 << 6,
            COLOURS = 1 << 7,
            HITOBJECTS = 1 << 8
        }

        public OsuFileSection entireFile = OsuFileSection.FORMAT | OsuFileSection.GENERAL |
            OsuFileSection.EDITOR | OsuFileSection.METADATA | OsuFileSection.DIFFICULTY | OsuFileSection.EVENTS |
            OsuFileSection.TIMINGPOINTS | OsuFileSection.COLOURS | OsuFileSection.HITOBJECTS;

        #region fields
        private Beatmap beatmap;
        private string path;
        #endregion

        #region constructors
        internal OsuFileParser(string path, Beatmap beatmap)
        {
            this.path = path;
            this.beatmap = beatmap;
        }
        #endregion

        #region methods
        internal void parse()
        {
            parse(entireFile);
        }

        internal void parse(OsuFileSection sections)
        {

            //Read in file. Exceptions here are to be handled by the devs who use this library.
            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (IOException)
            {
                throw;
            }

            //First line is always file format version
            OsuFileSection currentSection = OsuFileSection.FORMAT;

            foreach (string line in lines)
            {
                //Skip line if empty
                if (!string.IsNullOrWhiteSpace(line))
                {
                    //Test for new section, otherwise, parse normally.
                    OsuFileSection sectionTest = testNewSection(line);
                    if (sectionTest != OsuFileSection.NONE)
                    {
                        currentSection = sectionTest;
                    }
                    //If the currentSection is a member of sections (user-defined), parse it
                    else if ((currentSection & sections) == currentSection)
                    {
                        Debug.WriteLine("Parsing" + currentSection.ToString());
                        parseLine(currentSection, line);
                    }
                }
            }

            //Process default values (if they were not set)

            //General
            if (beatmap.PreviewTime == 0) beatmap.PreviewTime = -1;
            if (String.IsNullOrWhiteSpace(beatmap.SampleSet)) beatmap.SampleSet = "Normal";
            if (beatmap.StackLeniency == 0) beatmap.StackLeniency = 0.7f;

            //Editor
            if (beatmap.DistanceSpacing == 0) beatmap.DistanceSpacing = 1f;
            if (beatmap.BeatDivisor == 0) beatmap.BeatDivisor = 4;
            if (beatmap.GridSize == 0) beatmap.GridSize = 4;
            if (beatmap.TimelineZoom == 0) beatmap.TimelineZoom = 1;

            //Difficulty
            if (beatmap.HpDrainRate == 0) beatmap.HpDrainRate = 5f;
            if (beatmap.CircleSize == 0) beatmap.CircleSize = 5f;
            if (beatmap.OverallDifficulty == 0) beatmap.OverallDifficulty = 5f;
            if (beatmap.ApproachRate == 0) beatmap.ApproachRate = beatmap.OverallDifficulty;
            if (beatmap.SliderMultiplier == 0) beatmap.SliderMultiplier = 1.4f;
            if (beatmap.SliderTickRate == 0) beatmap.SliderTickRate = 1f;

            Debug.WriteLine("osuBMParser: Finished beatmap parsing");

        }

        private OsuFileSection testNewSection(string data)
        {
            OsuFileSection sectionEnum;
            return Enum.TryParse(data.Substring(1, data.Length - 2), true, out sectionEnum) ? sectionEnum : OsuFileSection.NONE;

        }

        //Send line data to the right parse method
        private void parseLine(OsuFileSection section, string data)
        {
            switch (section)
            {
                case OsuFileSection.FORMAT:
                    beatmap.FormatVersion = data;
                    break;
                case OsuFileSection.GENERAL:
                case OsuFileSection.EDITOR:
                case OsuFileSection.METADATA:
                case OsuFileSection.DIFFICULTY:
                    normalParse(data);
                    break;
                case OsuFileSection.TIMINGPOINTS:
                    timingPointParse(data);
                    break;
                case OsuFileSection.COLOURS:
                    colourParse(data);
                    break;
                case OsuFileSection.HITOBJECTS:
                    hitObjectParse(data);
                    break;
            }
        }

        #region parseMethods
        private void normalParse(string data)
        {
            string[] tokens = data.Split(':');

            switch (tokens[0].ToLower().Trim())
            {
                case "bookmarks":
                case "editorbookmarks": //From old file format (v5)
                    beatmap.Bookmarks.AddRange(Array.ConvertAll(tokens[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries), int.Parse));
                    break;

                case "tags":
                    if (tokens[1] != null) beatmap.Tags.AddRange(tokens[1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                    break;

                case "editordistancespacing": //From old file format (v5)
                    beatmap.DistanceSpacing = toFloat(tokens[1]);
                    break;

                default:
                    //Use reflection to set property values
                    PropertyInfo property = beatmap.GetType().GetProperty(tokens[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        //Convert.ChangeType() does not do string to boolean conversion
                        if (property.PropertyType == typeof(Boolean))
                        {
                            property.SetValue(beatmap, toBool(tokens[1].Trim()));
                        } else if (property.PropertyType == typeof(Int32))
                        {
                            string[] intTokens = tokens[1].Split('.');
                            property.SetValue(beatmap, Convert.ChangeType(intTokens[0].Trim(), property.PropertyType));
                        }
                        else
                        {
                            property.SetValue(beatmap, Convert.ChangeType(tokens[1].Trim(), property.PropertyType));
                        }
                    }
                    else
                    {
                        Debug.WriteLine("osuBMParser: Undefined property: " + tokens[0]);
                    }
                    break;
            }

        }


        private void timingPointParse(string data)
        {

            string[] tokens = data.Split(',');

            if (tokens.Length < 8)
            {
                Debug.WriteLine("osuBMParser: Invalid TimingPoint line, no further information available");
                return;
            }

            TimingPoint timingPoint = new TimingPoint();

            if (tokens[0] != null) timingPoint.Offset = toInt(tokens[0]);
            if (tokens[1] != null) timingPoint.MsPerBeat = toFloat(tokens[1]);
            if (tokens[2] != null) timingPoint.Meter = toInt(tokens[2]);
            if (tokens[3] != null) timingPoint.SampleType = toInt(tokens[3]);
            if (tokens[4] != null) timingPoint.SampleSet = toInt(tokens[4]);
            if (tokens[5] != null) timingPoint.Volume = toInt(tokens[5]);
            if (tokens[6] != null) timingPoint.Inherited = toBool(tokens[6]);
            if (tokens[7] != null) timingPoint.KiaiMode = toBool(tokens[7]);

            beatmap.TimingPoints.Add(timingPoint);

        }

        private void colourParse(string data)
        {
            string[] tokens = data.Split(':');
            if (tokens.Length >= 2) 
            {
                string[] colourValues = tokens[1].Split(',');
                if (colourValues.Length >= 3)
                {
                    beatmap.Colours.Add(new ComboColour(byte.Parse(colourValues[0]), byte.Parse(colourValues[1]), byte.Parse(colourValues[2])));
                }
            }
        }

        private void hitObjectParse(string data)
        {

            string[] tokens = data.Split(',');

            if (tokens.Length < 5)
            {
                Debug.WriteLine("osuBMParser: Invalid HitObject line, no further information available");
                return; //Not possible to have less arguments than this
            }

            //Create bit array for checking type
            BitArray typeBitArray = new BitArray(new int[] { toInt(tokens[3]) });
            bool[] typeBits = new bool[typeBitArray.Count];
            typeBitArray.CopyTo(typeBits, 0);

            //Create hitObject of correct type
            HitObject hitObject = null;

            if (typeBits[0])
            {
                hitObject = new HitCircle();
            }
            else if (typeBits[1])
            {
                hitObject = new HitSlider();
            }
            else if (typeBits[3])
            {
                hitObject = new HitSpinner();
            }
            else
            {
                Debug.WriteLine("osuBMParser: Invalid HitObject line at timestamp: " + tokens[2] + " | Type = " + tokens[3]);
                return; //This type does not exist
            }

            //Parse all information for the hitObject

            //Global stuff first
            hitObject.Position = new Vector2(toFloat(tokens[0]), toFloat(tokens[1]));
            hitObject.Time = toInt(tokens[2]);
            //hitObject.HitSound = toInt(tokens[4]);
            hitObject.IsNewCombo = typeBits[2];

            //Specific stuff

            if (hitObject is HitCircle)
            {

                if (tokens.Length >= 6 && tokens[5] != null) //Additions
                {
                    //hitObject.Addition = new List<int>(getAdditionsAsIntArray(tokens[5]));
                }

            }

            if (hitObject is HitSlider)
            {

                if (tokens.Length >= 6 && tokens[5] != null) //SliderType and HitSliderSegments
                {
                    string[] hitSliderSegments = tokens[5].Split('|');
                    ((HitSlider)hitObject).Type = HitSlider.parseSliderType(hitSliderSegments[0]);
                    foreach (string hitSliderSegmentPosition in hitSliderSegments.Skip(1))
                    {
                        string[] positionTokens = hitSliderSegmentPosition.Split(':');
                        if (positionTokens.Length == 2)
                        {
                            ((HitSlider)hitObject).HitSliderSegments.Add(new HitSliderSegment(new Vector2(toFloat(positionTokens[0]), toFloat(positionTokens[1]))));
                        }
                    }
                }

                if (tokens.Length >= 7 && tokens[6] != null)
                {
                    ((HitSlider)hitObject).Repeat = toInt(tokens[6]);
                }

                if (tokens.Length >= 8 && tokens[7] != null)
                {
                    ((HitSlider)hitObject).PixelLength = toFloat(tokens[7]);
                }

                if (tokens.Length >= 9 && tokens[8] != null)
                {
                    //((HitSlider)hitObject).EdgeHitSound = toInt(tokens[8]);
                }

                if (tokens.Length >= 10 && tokens[9] != null)
                {
                    //((HitSlider)hitObject).EdgeAddition = new List<int>(getAdditionsAsIntArray(tokens[9]));
                }

                if (tokens.Length >= 11 && tokens[10] != null)
                {
                    //hitObject.Addition = new List<int>(getAdditionsAsIntArray(tokens[10]));
                }

            }

            if (hitObject is HitSpinner)
            {

                if (tokens.Length >= 6 && tokens[5] != null)
                {
                    ((HitSpinner)hitObject).EndTime = toInt(tokens[5]);
                }

                if (tokens.Length >= 7 && tokens[6] != null)
                {
                    //hitObject.Addition = new List<int>(getAdditionsAsIntArray(tokens[6]));
                }

            }

            beatmap.HitObjects.Add(hitObject);

        }

        private int[] getAdditionsAsIntArray(string additionToken)
        {

            int[] additions = new int[0];
            try
            {
                additions = Array.ConvertAll(additionToken.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries), int.Parse);
            }
            catch
            {
                throw;
            }
            return additions;

        }
        #endregion
        
        private int toInt(string data)
        {
            int result;
            return int.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) ? result : 0;
        }

        private float toFloat(string data)
        {
            float result;
            return float.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ? result : 0f;
        }

        private bool toBool(string data)
        {
            return (data.Trim() == "1" || data.Trim().ToLower() == "true");
        }
        #endregion

    }
}

