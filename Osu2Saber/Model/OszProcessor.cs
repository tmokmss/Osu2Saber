using Ionic.Zip;
using osuBMParser;
using System.IO;

namespace Osu2Saber.Model
{
    class OszProcessor
    {
        readonly static string OszDirName = "osz";
        static string workDir;
        public static string WorkDir
        {
            set
            {
                workDir = Path.Combine(value, OszDirName);
                Directory.CreateDirectory(workDir);
            }
            get => workDir;
        }

        readonly string oszPath;

        public string DefaultAudioExtension { set; get; } = "mp3";
        public string OutDir { private set; get; }
        public string[] OsuFiles { private set; get; }
        public string OszName { private set; get; }

        public OszProcessor(string oszPath)
        {
            this.oszPath = oszPath;
            OszName = Path.GetFileNameWithoutExtension(oszPath);
            OutDir = Path.Combine(WorkDir, Path.GetFileNameWithoutExtension(oszPath));
            Directory.CreateDirectory(OutDir);

            Decompress();
            ListOsuFiles();
        }

        void Decompress()
        {
            Directory.CreateDirectory(OutDir);
            using (var archive = ZipFile.Read(oszPath))
            {
                archive.FlattenFoldersOnExtract = true;
                foreach (var entry in archive)
                {
                    entry.Extract(OutDir, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        void ListOsuFiles()
        {
            var filesInFull = Directory.GetFiles(OutDir, "*.osu");
            var count = filesInFull.Length;
            OsuFiles = new string[count];
            for (var i = 0; i < count; i++)
            {
                OsuFiles[i] = Path.GetFileName(filesInFull[i]);
            }

        }

        public Beatmap LoadOsuFile(int index)
        {
            if (index < 0 || index >= OsuFiles.Length) return null;
            var filePath = Path.Combine(OutDir, OsuFiles[index]);
            var bm = new Beatmap(filePath);
            return bm;
        }

    }
}
