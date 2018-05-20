using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osuBMParser;
using Newtonsoft.Json;
using System.IO;
using Osu2Saber.Model.Algorithm;
using Osu2Saber.Model.Json;

namespace Osu2Saber.Model
{
    /// <summary>
    /// This class generates info.json and [difficulty].json files from an.osu file.
    /// </summary>
    class Osu2BsConverter
    {
        static readonly string MapDirName = "output";
        static string workDir;
        public static string WorkDir
        {
            set
            {
                workDir = Path.Combine(value, MapDirName);
                Directory.CreateDirectory(workDir);
            }
            get => workDir;
        }

        const string InfoFileName = "info.json";
        const int MaxNumOfBeatmap = 5;

        SaberInfo info;

        public string OrgDir { private set; get; }
        public string OutDir { private set; get; }
        public string Mp3Path { private set; get; }
        public string ImagePath { private set; get; }

        public Osu2BsConverter(string orgDir, string outputDirName)
        {
            OrgDir = orgDir;
            OutDir = Path.Combine(workDir, outputDirName);
            Directory.CreateDirectory(OutDir);
        }

        public string AddBeatmap(Beatmap org)
        {
            if (info == null)
            {
                InitializeInfo(org);
            }
            if (info.difficultyLevels.Count >= MaxNumOfBeatmap) return null;
            var map = GenerateMap(org);
            var jsonPath = AddDifficulty(org);
            var mapPath = Path.Combine(OutDir, jsonPath);
            using (var sw = new StreamWriter(mapPath, false, Encoding.UTF8))
            {
                sw.Write(map);
            }
            return jsonPath;
        }

        public void GenerateInfoFile(string audioFileName)
        {
            info.ChangeAudioPath(audioFileName);
            var infoJson = JsonConvert.SerializeObject(info, Formatting.Indented);
            var infoPath = Path.Combine(OutDir, InfoFileName);
            using (var sw = new StreamWriter(infoPath, false, Encoding.UTF8))
            {
                sw.Write(infoJson);
            }
        }

        void InitializeInfo(Beatmap org)
        {
            info = new SaberInfo
            {
                songName = org.Title,
                songSubName = org.Source,
                authorName = org.Artist,
                beatsPerMinute = CalcOriginalBPM(org),
                previewStartTime = org.PreviewTime / 1000,
                previewDuration = 10,
                coverImagePath = Path.ChangeExtension(org.ImageFileName, ThumbnailGenerator.DefaultExtension),
                environmentName = "NiceEnvironment",
            };

            Mp3Path = Path.Combine(OrgDir, org.AudioFileName);
            ImagePath = Path.Combine(OrgDir, org.ImageFileName);
        }

        string AddDifficulty(Beatmap org)
        {
            var diffi = DetermineMapDifficulty(org.Version);
            var difficulty = diffi.str;
            var difficultyRank = diffi.rank;
            var audioPath = org.AudioFileName.Replace("mp3", "wav");
            var jsonPath = diffi.str + ".json";
            var offset = 0;
            info.AddDifficultyLevels(difficulty, difficultyRank, audioPath, jsonPath, offset);
            return jsonPath;
        }

        string GenerateMap(Beatmap org)
        {
            var map = new SaberBeatmap()
            {
                _version = "1.0.0",
                _beatsPerMinute = CalcOriginalBPM(org),
                _beatsPerBar = 16,
                _noteJumpSpeed = 10,
                _shuffle = 0,
                _shufflePeriod = 0.5,
            };
            var ca = new ConvertAlgorithm(org, map);
            ca.Convert();

            map._events = ca.Events;
            map._obstacles = ca.Obstacles;
            map._notes = ca.Notes;
            var json = JsonConvert.SerializeObject(map, Formatting.Indented);
            return json;
        }

        int CalcOriginalBPM(Beatmap org)
        {
            var tp = org.TimingPoints[0];
            var mpb = tp.MsPerBeat;
            return (int)Math.Ceiling(1000.0 / mpb * 60);
        }

        bool isExpertTaken = false;
        (string str, int rank) DetermineMapDifficulty(string mapName)
        {
            if (mapName.Contains("Easy"))
            {
                return ("Easy", 1);
            }
            if (mapName.Contains("Normal"))
            {
                return ("Normal", 2);
            }
            if (mapName.Contains("Hard"))
            {
                return ("Hard", 3);
            }
            if (mapName.Contains("Insane"))
            {
                if (isExpertTaken) return ("ExpertPlus", 5);
                isExpertTaken = true;
                return ("Expert", 4);
            }
            return ("Easy", 1);
        }

    }
}
