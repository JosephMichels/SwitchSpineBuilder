using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SwitchSpineBuilder
{

    class SpineViewModel : BindableBase
    {
        public string Filename { get; set; }
        public string FullPath { get; set; }

        bool _selected;
        public bool Selected { get => _selected; set => SetProperty(ref _selected, value); }

        public ICommand ClickCommand { get; set; }
        public SpineViewModel()
        {
            ClickCommand = new DelegateCommand(() => { Selected = !Selected; });
        }
    }

    internal class MainViewModel : BindableBase
    {
        List<SpineViewModel> _spines = new List<SpineViewModel>();
        public List<SpineViewModel> Spines { get; set; } = new List<SpineViewModel>();

        public List<SpineViewModel> SelectedSpines { get { return _spines.Where(s => s.Selected).ToList(); } }

        public ICommand BuildImage { get; set; }

        string _query;
        public string SearchQuery
        {
            get { return _query; }
            set
            {
                SetProperty(ref _query, value);
                UpdateDisplayedSpines();
            }
        }

        public MainViewModel()
        {
            var dirInfo = new DirectoryInfo("Spines");
            var extensions = new[] { "*.png", "*.jpg" };
            var spines = extensions.SelectMany(ext => dirInfo.GetFiles(ext));
            foreach (var spine in spines)
            {
                var s = new SpineViewModel
                {
                    Filename = spine.Name,
                    FullPath = spine.FullName
                };
                s.PropertyChanged += (obj, changed) =>
                {
                    if (changed == null) return;
                    if (changed.PropertyName == nameof(SpineViewModel.Selected))
                    {
                        RaisePropertyChanged(nameof(SelectedSpines));
                    }
                };

                _spines.Add(s);
            }
            Spines.AddRange(_spines);

            BuildImage = new DelegateCommand(() => ImageBuilder.BuildImage(_spines.Where(s => s.Selected).Select(s => s.FullPath).ToArray(), 20));
        }

        public void UpdateDisplayedSpines()
        {
            var newList = new List<SpineViewModel>();
            foreach(var sp in _spines)
            {
                if (sp.Filename.ToLower().Contains(SearchQuery.ToLower()))
                {
                    newList.Add(sp);
                }
            }
            Spines = newList;
            RaisePropertyChanged(nameof(Spines));
        }
    }
}
