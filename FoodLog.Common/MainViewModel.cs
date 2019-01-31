﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FoodLog.Common.Annotations;


namespace FoodLog.Common
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApiWrapper _api;

        public ObservableCollection<EntryViewModel> Entries { get; } = new ObservableCollection<EntryViewModel>();

        private EntryViewModel _selectedEntryViewModel;
        public EntryViewModel SelectedEntryViewModel
        {
            get { return _selectedEntryViewModel; }
            set
            {
                _selectedEntryViewModel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EntryDate));
            }
        }

        public DateTime EntryDate
        {
            get => _selectedEntryViewModel?.Date ?? DateTime.Now.Date;

            set => GoToDate(value);
        }

        public ICommand SaveCommand => new AsyncCommand(Save);
        public ICommand RefreshCommand => new AsyncCommand(Refresh);
        public ICommand ForwardCommand => new Command(Forward);
        public ICommand BackCommand => new Command(Back);
        public ICommand ClearCommand => new Command(Clear);
        public ICommand AddCommand => new Command(AddNew);
        public ICommand DeleteCommand => new AsyncCommand(Delete);

        public MainViewModel(ApiWrapper api)
        {
            _api = api;
        }

        public async Task Start()
        {
            await Refresh();
        }

        public void AddNew()
        {
            GoToDate(Entries.OrderByDescending(x => x.Date).First().Date.AddDays(1));
        }
        public void Forward()
        {
            GoToDate(SelectedEntryViewModel.Date.AddDays(1));
        }

        public void Back()
        {
            GoToDate(SelectedEntryViewModel.Date.AddDays(-1));
        }

        public void GoToDate(DateTime dt)
        {
            var shortDate = dt.Date;

            var entry = Entries.FirstOrDefault(x => DateTime.Compare(x.Date.Date, shortDate) == 0);

            if (entry == null)
            {
                SelectedEntryViewModel = new EntryViewModel(shortDate);
                Entries.Add(SelectedEntryViewModel);
            }
            else
            {
                SelectedEntryViewModel = entry;
            }
        }

        private async Task Refresh()
        {
            try
            {
                Entries.Clear();

                foreach (var e in await _api.GetEntries())
                    Entries.Add(e);

                if (Entries.Count > 0)
                {
                    SelectedEntryViewModel = Entries.OrderByDescending(x => x.Date).First();
                }
            }
            catch (Exception e)
            {
                Messenger.Instance.NotifyColleagues("Exception", e);
            }
        }

        private async Task Save()
        {
            try
            {


                var updatedEntries = Entries.Where(x => x.Updated).ToList();

                foreach (var entry in updatedEntries)
                {
                    await _api.Save(entry);
                    entry.Updated = false;

                    if (!Entries.Contains(SelectedEntryViewModel))
                        Entries.Add(SelectedEntryViewModel);
                }

                var message = string.Format("{0} {1} updated", updatedEntries.Count, updatedEntries.Count == 1 ? "entry" : "entries");

                Messenger.Instance.NotifyColleagues("Notification", new Notification("Save", message));
            }
            catch (Exception e)
            {
                Messenger.Instance.NotifyColleagues("Exception", e);
            }

        }

        private async Task Delete()
        {
            try
            {
                var toDeleteEntries = Entries.Where(x => x.ToDelete);

                foreach (var entry in toDeleteEntries)
                {
                    await _api.Delete(entry);
                }
            }
            catch (Exception e)
            {
                Messenger.Instance.NotifyColleagues("Exception", e);
            }


        }

        private void Clear()
        {
            SelectedEntryViewModel = new EntryViewModel(SelectedEntryViewModel.Date);

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
