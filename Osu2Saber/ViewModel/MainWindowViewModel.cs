using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using Osu2Saber.Model;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Osu2Saber.ViewModel
{
    class MainWindowViewModel : BindableBase, IDropTarget
    {
        string statusText = "Select an osz file first.";
        string workDir = "";
        bool canProcess = false;
        bool isWorkDirSpecified = false;

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
            var fbd = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select working folder",
            };
            var res = fbd.ShowDialog();
            if (res != System.Windows.Forms.DialogResult.OK) return;
            WorkDir = fbd.SelectedPath;
            isWorkDirSpecified = true;
        }

        private void ClearList()
        {
            OszFiles.Clear();
            WorkDir = "";
            CanProcess = false;
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
                if (!isWorkDirSpecified)
                    WorkDir = Path.GetDirectoryName(OszFiles[0]);
                CanProcess = true;
            }
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
            StatusText = "Started processing.";
            bp = new BatchProcessor(OszFiles.ToArray(), WorkDir);
            bp.PropertyChanged += ModelChanged;
            await bp.BatchProcess();
            bp.PropertyChanged -= ModelChanged;
            StatusText = "Batch process completed.";
            OpenOutputDir();
            CanProcess = true;
        }

        void OpenOutputDir()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = bp.OutputDir,
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }

        private void ModelChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }


        public void DragOver(IDropInfo dropInfo)
        {
            var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
            dropInfo.Effects = dragFileList.Any(item =>
            {
                var extension = Path.GetExtension(item);
                return extension != null && extension.Equals(".zip");
            }) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>().ToList();
            //dropInfo.Effects = dragFileList.Any(item =>
            //{
            //    var extension = Path.GetExtension(item);
            //    return extension != null && extension.Equals(".zip");
            //}) ? DragDropEffects.Copy : DragDropEffects.None;

            foreach (var file in dragFileList)
            {
                OszFiles.Add(file);
            }
            if (!isWorkDirSpecified)
                WorkDir = Path.GetDirectoryName(OszFiles[0]);
        }

    }
}
