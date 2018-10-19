using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Osu2Saber.Model
{
    class BatchProcessor : BindableBase
    {
        public static bool IncludeTaiko { set; get; } = false;
        public static bool IncludeCtB { set; get; } = true;
        public static bool IncludeMania { set; get; } = true;

        object progressLock = new object();
        double progress;
        Logger logger;

        public string[] TargetFiles { private set; get; }
        public string WorkDir { private set; get; }
        public string OutputDir { private set; get; }

        public double Progress
        {
            private set
            {
                progress = value;
                RaisePropertyChanged();
            }
            get => progress;
        }

        public BatchProcessor(string[] targetFiles, string workDir)
        {
            TargetFiles = targetFiles;
            WorkDir = workDir;
            logger = new Logger();
        }

        public Task BatchProcess()
        {
            var tasks = TargetFiles.Select(file => Task.Run(() => Process(file)));
            return Task.WhenAll(tasks);
        }

        void Process(string oszPath)
        {
            try
            {
                var oszp = DecompressOsz(oszPath);
                if (oszp == null) return;

                var o2b = ConvertBeatmap(oszp);
                ConvertImgAudio(o2b);
            }
            catch (Exception e)
            {
                logger.AddException(e, oszPath);
                logger.Write();
            }
        }

        OszProcessor DecompressOsz(string oszPath)
        {
            OszProcessor.WorkDir = WorkDir;
            if (!oszPath.EndsWith("osz") && !oszPath.EndsWith("zip"))
                return null;

            var oszp = new OszProcessor(oszPath);
            if (oszp.OsuFiles.Length == 0) return null;

            ReportProgress(0.3);
            return oszp;
        }

        Osu2BsConverter ConvertBeatmap(OszProcessor oszp)
        {
            Osu2BsConverter.WorkDir = WorkDir;
            OutputDir = Osu2BsConverter.WorkDir;
            var o2b = new Osu2BsConverter(oszp.OutDir, oszp.OszName);

            oszp.LoadOsuFiles()
                .Where(osuFile => IncludeTaiko || osuFile.Mode != 1)
                .Where(osuFile => IncludeCtB || osuFile.Mode != 2)
                .Where(osuFile => IncludeMania || osuFile.Mode != 3)
                .ToList().ForEach(osufile => o2b.AddBeatmap(osufile));

            o2b.ProcessAll();

            ReportProgress(0.2);
            return o2b;
        }

        void ConvertImgAudio(Osu2BsConverter o2b)
        {
            var audioFileName = Mp3toOggConverter.ConvertToOgg(o2b.AudioPath, o2b.OutDir);
            ThumbnailGenerator.GenerateThumbnail(o2b.ImagePath, o2b.OutDir);
            o2b.GenerateInfoFile(audioFileName);
            ReportProgress(0.5);
            return;
        }

        void ReportProgress(double add)
        {
            lock (progressLock)
            {
                Progress += add;
            }
        }
    }
}
