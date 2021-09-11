using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Osu2Saber.Model;
using Osu2Saber.Model.Algorithm;

namespace Osu2Saber.ViewModel
{
    class ConfigPanelViewModel : BindableBase
	{
		public static ConfigPanelViewModel instance = new ConfigPanelViewModel();;

		public ConfigPanelViewModel() {
			instance = this;
		}

		bool isConfigPanelActive = true;
		public bool IsPanelActive
		{
			set
			{

				isConfigPanelActive = value;
				RaisePropertyChanged();
			}
			get => isConfigPanelActive;
		}

		public double MaximumDifficulty
        {
            set
            {
                Osu2BsConverter.MaximumDifficulty = value;
                RaisePropertyChanged();
            }
            get => Osu2BsConverter.MaximumDifficulty;
        }

        public double MinimumDifficulty
        {
            set
            {
                Osu2BsConverter.MinimumDifficulty = value;
                RaisePropertyChanged();
            }
            get => Osu2BsConverter.MinimumDifficulty;
		}
		public bool IsDifficultyEnabled
		{
			get => Osu2BsConverter.IsDifficultyEnabled;
		}

		public bool PreferHarder
        {
            set { Osu2BsConverter.PreferHarder = value; }
            get => Osu2BsConverter.PreferHarder;
        }

        public bool HandleHitSlider
        {
            set { ConvertAlgorithm.HandleHitSlider = value; }
            get => ConvertAlgorithm.HandleHitSlider;
        }

        public bool NoDirectionAndPlacement
        {
            set { ConvertAlgorithm.NoDirectionAndPlacement = value; }
            get => ConvertAlgorithm.NoDirectionAndPlacement;
        }

        public bool IncludeTaiko
        {
            set { BatchProcessor.IncludeTaiko = value; }
            get => BatchProcessor.IncludeTaiko;
        }

        public bool IncludeCtB
        {
            set { BatchProcessor.IncludeCtB = value; }
            get => BatchProcessor.IncludeCtB;
		}

		public bool IncludeMania
		{
			set { BatchProcessor.IncludeMania = value; }
			get => BatchProcessor.IncludeMania;
		}

		public bool DontOpenDirectory
		{
			set { BatchProcessor.DontOpenDirectory = value; }
			get => BatchProcessor.DontOpenDirectory;
		}
	}
}
