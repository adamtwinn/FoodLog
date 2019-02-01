using FoodLog.Common.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FoodLog.Common
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly IApiWrapper _api;

        public ObservableCollection<EntryViewModel> Entries { get; } = new ObservableCollection<EntryViewModel>();

        private EntryViewModel _selectedEntryViewModel;
        private DateTime _entryDate = DateTime.Now.Date;
        private bool _started;

        public EntryViewModel SelectedEntryViewModel
        {
            get { return _selectedEntryViewModel; }
            set
            {
                if (value.Equals(_selectedEntryViewModel)) return;
                _selectedEntryViewModel = value;
                OnPropertyChanged();
            }
        }

        public DateTime EntryDate
        {
            get => _entryDate;
            set
            {
                if (value.Equals(_entryDate)) return;
                _entryDate = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand => new AsyncCommand(Save);
        public ICommand RefreshCommand => new AsyncCommand(Refresh);
        public ICommand GoToDateCommand => new Command<DateTime>(DateSelectedCommand);
        public ICommand ForwardCommand => new Command(Forward);
        public ICommand BackCommand => new Command(Back);
        public ICommand ClearCommand => new Command(Clear);
        public ICommand AddCommand => new Command(AddNew);
        public ICommand DeleteCommand => new AsyncCommand(Delete);

        public MainViewModel(IApiWrapper api)
        {
            _api = api;
        }

        public async Task Start()
        {
            Messenger.Instance.NotifyColleagues("Log", new LogEvent("Starting MainViewModel...", new Dictionary<string, string>()));
            await Refresh();
        }

        private void AddNew()
        {
            EntryDate = Entries.OrderByDescending(x => x.Date).First().Date.AddDays(1);
            GoToDate();
        }

        private void Forward()
        {
            EntryDate = EntryDate.AddDays(1);
            GoToDate();
        }

        private void Back()
        {
            EntryDate = EntryDate.AddDays(-1);
            GoToDate();
        }
        private void DateSelectedCommand(DateTime dt)
        {
            EntryDate = dt;
            GoToDate();
        }

        private void GoToDate()
        {
            Messenger.Instance.NotifyColleagues("Log",
                new LogEvent("Running Go-To-Date...",
                    new Dictionary<string, string> { { "Date", EntryDate.ToString(CultureInfo.InvariantCulture) } }));

            var entry = Entries.FirstOrDefault(x => DateTime.Compare(x.Date.Date, EntryDate) == 0);

            if (entry == null)
            {
                SelectedEntryViewModel = new EntryViewModel(EntryDate);
                Entries.Add(SelectedEntryViewModel);
            }
            else
            {
                SelectedEntryViewModel = entry;
            }
        }

        private async Task Refresh()
        {
            Messenger.Instance.NotifyColleagues("Log", new LogEvent("Running Refresh...", new Dictionary<string, string> { { "Date", EntryDate.ToString(CultureInfo.InvariantCulture) } }));
            try
            {
                Entries.Clear();

                foreach (var e in await _api.GetEntries())
                    Entries.Add(e);

                if (Entries.Count > 0)
                {
                    SelectedEntryViewModel = Entries.Where(x => x.EntryId != 0).OrderByDescending(x => x.Date).First();
                    EntryDate = SelectedEntryViewModel.Date;
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
                var successfulUpdates = 0;

                foreach (var entry in updatedEntries)
                {
                    if (await _api.Save(entry))
                    {
                        successfulUpdates++;

                        entry.Updated = false;

                        if (!Entries.Contains(SelectedEntryViewModel))
                            Entries.Add(SelectedEntryViewModel);
                    }

                }

                var message = string.Format("{0} out of {1} entries updated", successfulUpdates, updatedEntries.Count);

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
