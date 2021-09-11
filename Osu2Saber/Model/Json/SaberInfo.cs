using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu2Saber.Model.Json
{
    /// <summary>
    /// This class contains all the required information for info.json.
    /// The format of the json file is described partially in the following documents:
    /// https://steamcommunity.com/groups/ModSaber#announcements/detail/1653262779995763262
    /// </summary>
    public class SaberInfo
    {
        List<DifficultyBeatmapSet> diffSets = new List<DifficultyBeatmapSet>() { 
			new DifficultyBeatmapSet() {
				_beatmapCharacteristicName = "Standard"
			}
		};

		public string _version;
        public string _songName;
        public string _songSubName;
        public string _songAuthorName;
		public string _levelAuthorName = "osu2saber";

		public int _beatsPerMinute;
        public int _previewStartTime;
        public int _previewDuration;
        public string _coverImageFilename;
        public string _environmentName;

		public string _comment = "Map created with Osu2Saber by tmokmss (fixed by Ivan_Alone)";

		public int _shuffle;
		public double _shufflePeriod;

		public int _songTimeOffset;

		public string _songFilename;

		public IReadOnlyList<DifficultyBeatmapSet> _difficultyBeatmapSets => diffSets;

        public void AddDifficultyLevels(
            string difficulty, int difficultyRank, 
            string jsonPath, int offset, string difficultyName)
        {
            var difficultyLevel = new DifficultyLevel
            {
                _difficulty = difficulty,
                _difficultyRank = difficultyRank,
                _beatmapFilename = jsonPath,
                _noteJumpStartBeatOffset = offset
            };
			difficultyLevel.setDifficultyName(difficultyName.Length > 0 ? difficultyName : difficulty);
			diffSets[0].Add(difficultyLevel);
        }

        public void ChangeAudioPath(string audioPath)
        {
			this._songFilename = audioPath;
		}
    }

    public class DifficultyLevel
    {
        public string _difficulty;
        public int _difficultyRank;
        public string _beatmapFilename;
        public int _noteJumpStartBeatOffset;
		public int _noteJumpSpeed = 10;

		public CustomData _customData = new CustomData();
		public void setDifficultyName(String name)
		{
			_customData._difficultyLabel = name;
		}
	}

	public class DifficultyBeatmapSet
	{
		List<DifficultyLevel> diffLevels = new List<DifficultyLevel>();
		public string _beatmapCharacteristicName;
		public IReadOnlyList<DifficultyLevel> _difficultyBeatmaps => diffLevels;
		public void Add(DifficultyLevel lvl) {
			diffLevels.Add(lvl);
		}
	}
	public class CustomData
	{
		public string _difficultyLabel = "";
	}
}

