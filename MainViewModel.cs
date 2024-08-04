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
        public ICommand ClearAll { get; set; }
        public ICommand SelectAll { get; set; }

        public ICommand SaveSelect { get; set; }
        public ICommand LoadSelect { get; set; }

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

        string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                SetProperty(ref _status, value);
            }
        }

        public MainViewModel()
        {
            var dirInfo = new DirectoryInfo("Spines");
            var extensions = new[] { "*.png", "*.jpg", "*.webp" };
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

            BuildImage = new DelegateCommand(() =>
            {
                Status = "Generating images";
                Task.Run(() =>
                {
                    var files = ImageBuilder.BuildImages(_spines.Where(s => s.Selected).Select(s => s.FullPath).ToArray(), 20, (cur, total) => { Status = $"Generating Images - {cur} of {total} complete"; });
                    Status = "Building PDF file";
                    ImageBuilder.BuildPdf(files);
                    Status = "Generation complete";
                });
            });


            ClearAll = new DelegateCommand(() =>
            {
                foreach (var item in _spines)
                {
                    item.Selected = false;
                }
                RaisePropertyChanged(nameof(SelectedSpines));
            });

            SelectAll = new DelegateCommand(() =>
            {
                foreach (var spine in Spines)
                {
                    spine.Selected = true;
                }
            });

            SaveSelect = new DelegateCommand(() =>
            {
                var selected = _spines.Where(s => s.Selected).Select(s => s.Filename).ToArray();
                File.WriteAllLines("save", selected);
                Status = "Selections saved";
            });

            LoadSelect = new DelegateCommand(() =>
            {
                if (File.Exists("save"))
                {
                    var selected = File.ReadAllLines("save");
                    foreach(var s in _spines)
                    {
                        if (selected.Contains(s.Filename))
                        {
                            s.Selected = true;
                        }
                        else
                        {
                            s.Selected = false;
                        }
                    }
                    Status = "Selections loaded";
                }
                else
                {
                    Status = "No save file found.";
                }
            });

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
