using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Osu2Saber.Model
{
    class BatchProcessor : BindableBase
    {
        object progressLock = new object();
        double progress;

        public string[] TargetFiles { private set; get; }
        public string WorkDir { private set; get; }

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
        }

        public Task BatchProcess()
        {
            var tasks = new List<Task>();
            for (var i = 0; i < TargetFiles.Length; i++)
            {
                var x = i;
                var task = Task.Run(() => Process(x));
                tasks.Add(task);
            }
            return Task.WhenAll(tasks);
        }

        void Process(int index)
        {
            var oszp = DecompressOsz(index);
            if (oszp == null) return;

            var o2b = ConvertBeatmap(oszp);
            ConvertImgAudio(o2b);
        }

        OszProcessor DecompressOsz(int index)
        {
            OszProcessor.WorkDir = WorkDir;
            var oszPath = TargetFiles[index];
            var oszp = new OszProcessor(oszPath);
            if (!oszPath.EndsWith("osz") && !oszPath.EndsWith("zip"))
                return null;

            var files = oszp.OsuFiles;
            ReportProgress(0.3);
            return oszp;
        }

        Osu2BsConverter ConvertBeatmap(OszProcessor oszp)
        {
            Osu2BsConverter.WorkDir = WorkDir;
            var o2b = new Osu2BsConverter(oszp.OutDir, oszp.OszName);
            var output = oszp.OsuFiles
                .Select((e, i) => oszp.LoadOsuFile(i))
                .Select(osuFile => o2b.AddBeatmap(osuFile))
                .Where(jsonPath => jsonPath != null).ToArray();
            ReportProgress(0.2);
            return o2b;
        }

        void ConvertImgAudio(Osu2BsConverter o2b)
        {
            var audioFileName = Mp3toOggConverter.ConvertMp3toOgg(o2b.Mp3Path, o2b.OutDir);
            ThumbnailGenerator.GenerateThumbnail(o2b.ImagePath, o2b.OutDir);
            o2b.GenerateInfoFile(audioFileName);
            ReportProgress(0.5);
            return;
        }

        void ReportProgress(double add)
        {
            lock(progressLock)
            {
                Progress += add;
            }
        }
    }
}
