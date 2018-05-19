using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu2Saber.Model.Json
{
    /// <summary>
    /// This class contains all the required information for info.json.
    /// The format of the json file is described partially in the following document:
    /// https://steamcommunity.com/groups/ModSaber#announcements/detail/1653262779995763262
    /// </summary>
    public class SaberInfo
    {
        List<DifficultyLevel> diffLevels = new List<DifficultyLevel>();

        public string songName;
        public string songSubName;
        public string authorName;
        public int beatsPerMinute;
        public int previewStartTime;
        public int previewDuration;
        public string coverImagePath;
        public string environmentName;
        public IReadOnlyList<DifficultyLevel> difficultyLevels => diffLevels;

        public void AddDifficultyLevels(
            string difficulty, int difficultyRank, string audioPath,
            string jsonPath, int offset)
        {
            var difficultyLevel = new DifficultyLevel
            {
                difficulty = difficulty,
                difficultyRank = difficultyRank,
                audioPath = audioPath,
                jsonPath = jsonPath,
                offset = offset
            };
            diffLevels.Add(difficultyLevel);
        }

        public void ChangeAudioPath(string audioPath)
        {
            foreach (var diffLevel in diffLevels)
            {
                diffLevel.audioPath = audioPath;
            }
        }
    }

    public class DifficultyLevel
    {
        public string difficulty;
        public int difficultyRank;
        public string audioPath;
        public string jsonPath;
        public int offset;
    }
}
