using Microsoft.Win32;
using Osu2Saber.Model;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Osu2Saber.ViewModel
{
    class MainWindowViewModel : BindableBase
    {
        string statusText = "Select an osz file first.";
        string workDir = "";
        bool canProcess = false;

        BatchProcessor bp;

        public string StatusText
        {
            private set
            {
                statusText = value;
                RaisePropertyChanged();
            }
            get => statusText;
        }

        public string WorkDir
        {
            private set
            {
                workDir = value;
                RaisePropertyChanged();
            }
            get => workDir;
        }

        public bool CanProcess
        {
            private set
            {
                canProcess = value;
                RaisePropertyChanged();
            }
            get => canProcess;
        }

        public ObservableCollection<string> OsuFiles { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> OszFiles { get; } = new ObservableCollection<string>();
        public int Progress { get => CalcProgress(); }


        public DelegateCommand SelectFileCommand => new DelegateCommand(SelectFile);
        public DelegateCommand ProcessCommand => new DelegateCommand(ProcessBatch);
        public DelegateCommand ClearCommand => new DelegateCommand(ClearList);
        public DelegateCommand SelectWorkFolderCommand => new DelegateCommand(SelectWorkFolder);

        private void SelectWorkFolder()
        {
            var fbd = new FolderBrowserDialog
            {
                Description = "Select working folder",
            };
            var res = fbd.ShowDialog();
            if (res != DialogResult.OK) return;
            WorkDir = fbd.SelectedPath;
        }

        private void ClearList()
        {
            OszFiles.Clear();
            WorkDir = "";
        }

        void SelectFile()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open osu zip file",
                FilterIndex = 2,
                Filter = "osu zip files | *.osz;*.zip",
                Multiselect = true,

            };
            bool? res = ofd.ShowDialog();
            if (res == true)
            {
                foreach (var fileName in ofd.FileNames) OszFiles.Add(fileName);
                StatusText = "Now, process the osz file.";
            }
            WorkDir = Path.GetDirectoryName(OszFiles[0]);
            CanProcess = true;
        }

        int CalcProgress()
        {
            if (bp == null) return 0;
            var pf = bp.Progress;
            var progress = (int)Math.Ceiling(pf / bp.TargetFiles.Length * 100);
            return progress;
        }

        async void ProcessBatch()
        {
            CanProcess = false;
            bp = new BatchProcessor(OszFiles.ToArray(), WorkDir);
            bp.PropertyChanged += ModelChanged;
            await bp.BatchProcess();
            bp.PropertyChanged -= ModelChanged;
            StatusText = "Batch process completed.";
            CanProcess = true;
        }

        private void ModelChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }
    }
}
