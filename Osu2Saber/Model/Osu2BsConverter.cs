using Newtonsoft.Json;
using Osu2Saber.Model.Algorithm;
using Osu2Saber.Model.Json;
using osuBMParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Osu2Saber.Model
{
	/// <summary>
	/// This class generates info.dat and [difficulty].dat files from an.osu file.
	/// </summary>
	class Osu2BsConverter
	{
		public static string SystemVersion { get; } = "0.7.3";
		public static string WindowName { get; } = "Osu2Saber v" + SystemVersion + " by tmokmss (fixed by Ivan_Alone)";

		static readonly string MapDirName = "output";
        static Formatting formatting = Formatting.Indented;
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
        public static double MinimumDifficulty { set; get; } = 0;
        public static double MaximumDifficulty { set; get; } = 10;
		// Enables difficulty selector in configuration menu
		// Difficulti calculation is not realized yet! Must be "false" untill will realized!
		public static bool IsDifficultyEnabled { get; } = false;
		public static bool PreferHarder { set; get; } = true;


		const string InfoFileName = "info.dat";
        const int MaxNumOfBeatmap = 5;

        SaberInfo info;
        List<Beatmap> beatmaps = new List<Beatmap>();

        public string OrgDir { private set; get; }
        public string OutDir { private set; get; }
        public string AudioPath { private set; get; }
        public string ImagePath { private set; get; }

        public Osu2BsConverter(string orgDir, string outputDirName)
        {
            OrgDir = orgDir;
            OutDir = Path.Combine(workDir, outputDirName);
            Directory.CreateDirectory(OutDir);
        }

        public void AddBeatmap(Beatmap org)
        {
            beatmaps.Add(org);
        }

        public void ProcessAll()
        {
            SortAndPickBeatmaps();
            InitializeInfo(beatmaps[0]);
            for (var i=0; i<beatmaps.Count; i++)
            {
                ConvertBeatmap(i);
            }
        }

        public void GenerateInfoFile(string audioFileName)
        {
            info.ChangeAudioPath(audioFileName);
            var infoDat = JsonConvert.SerializeObject(info, formatting);
            var infoPath = Path.Combine(OutDir, InfoFileName);
            using (var sw = new StreamWriter(infoPath, false, Encoding.UTF8))
            {
                sw.Write(infoDat);
            }
        }

        void SortAndPickBeatmaps()
        {
            beatmaps = beatmaps.OrderBy(beatmap => beatmap.HitObjects.Count).ToList();
            if (beatmaps.Count <= MaxNumOfBeatmap) return;

            if (PreferHarder)
                beatmaps = beatmaps.Skip(beatmaps.Count - MaxNumOfBeatmap).ToList();
            else
                beatmaps = beatmaps.Take(MaxNumOfBeatmap).ToList();
        }

        void ConvertBeatmap(int index)
        {
            var org = beatmaps[index];
            var map = GenerateMap(org);
            var datPath = AddDifficulty(org, index);
            var mapPath = Path.Combine(OutDir, datPath);
            using (var sw = new StreamWriter(mapPath, false, Encoding.UTF8))
            {
                sw.Write(map);
            }
        }

        void InitializeInfo(Beatmap org)
        {
            if (org.ImageFileName == null)
                org.ImageFileName = "default.jpg";

            info = new SaberInfo
            {
				_version = "2.0.0",
                _songName = org.Title,
                _songSubName = org.Source,
                _songAuthorName = org.Artist,
                _beatsPerMinute = CalcOriginalBPM(org),
                _previewStartTime = org.PreviewTime / 1000,
                _previewDuration = 10,
                _coverImageFilename = Path.ChangeExtension(org.ImageFileName, ThumbnailGenerator.DefaultExtension),
				//_environmentName = "DefaultEnvironment", // There is "NiceEnvironment" too
				_environmentName = "NiceEnvironment", // I personally prefer this
				_shuffle = 0,
				_shufflePeriod = 0.5,
				_songTimeOffset = 0,
			};

            AudioPath = Path.Combine(OrgDir, org.AudioFileName);
            ImagePath = Path.Combine(OrgDir, org.ImageFileName);
        }

		string AddDifficulty(Beatmap org, int index)
		{
			var (diff, rank) = DetermineMapDifficulty(index);
			var difficulty = diff;
			var difficultyRank = rank;
			var datPath = diff + ".dat";
			var offset = 0;
			info.AddDifficultyLevels(difficulty, difficultyRank, datPath, offset, org.Version);
			return datPath;
		}

        string GenerateMap(Beatmap org)
        {
            var map = new SaberBeatmap()
            {
				_osuOriginName = org.Version,
                _version = "2.0.0",
            };
			map.linkInfo(info);
            var ca = new ConvertAlgorithm(org, map);
            ca.Convert();

            map._events = ca.Events;
            map._obstacles = ca.Obstacles;
            map._notes = ca.Notes;
			var dat = JsonConvert.SerializeObject(map, formatting);
            return dat;
        }

        int CalcOriginalBPM(Beatmap org)
        {
            var tp = org.TimingPoints[0];
            var mpb = tp.MsPerBeat;
            return (int)Math.Round(1000.0 / mpb * 60);
        }

        (string str, int rank) DetermineMapDifficulty(int idx)
        {
            // prefer harder difficulty for display, cuz it looks more cool!
            idx += MaxNumOfBeatmap - beatmaps.Count;
            if (idx == 0) return ("Easy", 1);
            if (idx == 1) return ("Normal", 2);
            if (idx == 2) return ("Hard", 3);
            if (idx == 3) return ("Expert", 4);
            if (idx == 4) return ("ExpertPlus", 5);
            return ("Easy", 1);
        }
    }
}
