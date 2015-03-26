namespace EventLogScanner.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using System.Windows.Input;
    using Probel.Mvvm.DataBinding;

    public class EventItemsViewModel : ObservableObject
    {
        #region Fields & Constructors

        private readonly IEventScanner _eventScanner;
        private readonly TaskScheduler _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        private bool _isBusy;
        private string _statusMessage;
        private bool _isScanCompleted;
        private bool _isScanFailed;

        public EventItemsViewModel(IEventScanner eventScanner)
        {
            _eventScanner = eventScanner;
            ScanCommand = new RelayCommand(Scan, () => !IsBusy);

            Events = new ObservableCollection<IEventItemViewModel>();
            SortedEvents = new CollectionViewSource { Source = Events };
            SortedEvents.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));           
            
            FromDate = DateTime.Today.AddMonths(-6);
            ToDate = DateTime.Now;
            LogName = "Application";

            Scan();
        }

        #endregion

        #region Properties

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string LogName { get; set; }
        
        public ObservableCollection<IEventItemViewModel> Events { get; private set; }       
        public CollectionViewSource SortedEvents { get; set; }

        public int DummyEventsCount { get; set; }
        public ICommand ScanCommand { get; private set; }
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged(() => IsBusy);
            }
        }

        public bool IsScanCompleted
        {
            get { return _isScanCompleted; }
            set
            {
                _isScanCompleted = value;
                OnPropertyChanged(() => IsScanCompleted);
                OnPropertyChanged(() => DummyEventsCount);
            }
        }

        public bool IsScanFailed
        {
            get { return _isScanFailed; }
            set { _isScanFailed = value; OnPropertyChanged(() => IsScanFailed); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged(() => StatusMessage);
                OnPropertyChanged(() => IsScanFailed);
            }
        }

        #endregion

        #region Private methods

        private void Scan()
        {
            IsBusy = true;
            Events.Clear();
            StatusMessage = "Scanning...";

            var scanTask = Task<List<IEventItemViewModel>>.Factory.StartNew(GetEvents, TaskCreationOptions.LongRunning);
            scanTask.ContinueWith(r => OnSuccesfulScan(r.Result), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, _uiScheduler);
            scanTask.ContinueWith(r => OnFailedScan(), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, _uiScheduler);
        }

        private void OnSuccesfulScan(IEnumerable<IEventItemViewModel> r)
        {
            Events.AddRange(r);
            DummyEventsCount = Events.OfType<DummyEventItemViewModel>().Count();
            StatusMessage = null;
            IsBusy = false;
            IsScanCompleted = true;
            IsScanFailed = false;
        }

        private void OnFailedScan()
        {
            StatusMessage = "Can't find log name.";
            IsBusy = false;
            IsScanCompleted = false;
            IsScanFailed = true;
        }

        private List<IEventItemViewModel> GetEvents()
        {
            var items = _eventScanner.Scan(FromDate, ToDate, LogName);
            var viewModels = items.Select(item => new EventItemViewModel(item));
            return PopulateWithDummyEvents(viewModels).ToList();
        }

        private IEnumerable<IEventItemViewModel> PopulateWithDummyEvents(IEnumerable<EventItemViewModel> items)
        {
            var dummyItems =
                Enumerable.Range(0, (ToDate.Date - FromDate.Date).Days)
                          .Select(x => new DummyEventItemViewModel(FromDate.AddDays(x)));
            var result = new List<IEventItemViewModel>();

            result.AddRange(dummyItems);
            result.AddRange(items);
            var filteredResult =
                result.GroupBy(i => i.Date)
                .Select(g => g.FirstOrDefault(i => i is EventItemViewModel) ?? g.First())
                      .ToList();

            return filteredResult;
        }

        private IEnumerable<IEventItemViewModel> PopulateWithDummyEventsSmoothly(IEnumerable<EventItemViewModel> items)
        {
            var sortedItems = items.OrderBy(vm => vm.Date).ToList();
            var result = new List<IEventItemViewModel>();
            var currentDate = FromDate;
            for (int i = 0; i < sortedItems.Count; i++)
            {
                while (sortedItems[i].Date.Date >= currentDate.Date)
                {
                    result.Add(new DummyEventItemViewModel(currentDate));
                    currentDate = currentDate.AddDays(1);
                }
            }

            while (currentDate <= ToDate)
            {
                result.Add(new DummyEventItemViewModel(currentDate));
                currentDate = currentDate.AddDays(1);
            }

            result.AddRange(items);

            return result;
        }

        #endregion
    }
}
