﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using FoodLog.Wpf.Api;
using FoodLog.Wpf.Properties;

namespace FoodLog.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApiWrapper _api;

        public ObservableCollection<EntryViewModel> Entries { get; } = new ObservableCollection<EntryViewModel>();

        private EntryViewModel _selectedEntryViewModel;
        public EntryViewModel SelectedEntryViewModel
        {
            get { return _selectedEntryViewModel;}
            set
            {
                _selectedEntryViewModel = value;
                OnPropertyChanged();
                OnPropertyChanged("EntryDate");
            }
        }
        
        public DateTime EntryDate
        {
            get => _selectedEntryViewModel?.Date ?? DateTime.Now.Date;

            set => GoToDate(value);
        }

        public  ICommand SaveCommand => new AsyncCommand(Save);
        public ICommand RefreshCommand => new AsyncCommand(Refresh);
        public DelegateCommand ForwardCommand => new DelegateCommand(Forward);
        public DelegateCommand BackCommand => new DelegateCommand(Back);
        public DelegateCommand ClearCommand => new DelegateCommand(Clear);
        public ICommand DeleteCommand => new AsyncCommand(Delete);

        public MainViewModel(ApiWrapper api)
        {
            _api = api;
        }

        public async Task Start()
        {
            await Refresh();
            
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

        public async Task Refresh()
        {
            Entries.Clear();

            foreach (var e in await _api.GetEntries())
                Entries.Add(e);

            if (Entries.Count > 0)
            {
                SelectedEntryViewModel = Entries.OrderByDescending(x => x.Date).First();
            }
        }

        public async Task Save()
        {
            var updatedEntries = Entries.Where(x => x.Updated);

            foreach (var entry in updatedEntries)
            {
                var response = await _api.Save(entry);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        string.Format("Error saving record {0}{1}{2}{3}", entry, Environment.NewLine,
                            Environment.NewLine, response.ReasonPhrase), "Save Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                }
            }
        }

        public async Task Delete()
        {
            var toDeleteEntries = Entries.Where(x => x.ToDelete);

            foreach (var entry in toDeleteEntries)
            {
                var response = await _api.Delete(entry);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        string.Format("Error deleting record {0}{1}{2}{3}", entry, Environment.NewLine,
                            Environment.NewLine, response.ReasonPhrase), "Save Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                }
            }
        }

        public void Clear()
        {
            SelectedEntryViewModel = new EntryViewModel(SelectedEntryViewModel.Date);

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
