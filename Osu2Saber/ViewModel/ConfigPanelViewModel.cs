using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Osu2Saber.Model;

namespace Osu2Saber.ViewModel
{
    class ConfigPanelViewModel : BindableBase
    {
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
        
        public bool PreferHarder
        {
            set { Osu2BsConverter.PreferHarder = value; }
            get => Osu2BsConverter.PreferHarder;
        }
    }
}
