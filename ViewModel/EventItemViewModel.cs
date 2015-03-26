namespace EventLogScanner.ViewModel
{
    using System;

    public interface IEventItemViewModel
    {
        DateTime Date { get; }        
    }

    public class EventItemViewModel : IEventItemViewModel
    {
        private readonly EventItem _model;

        public EventItemViewModel(EventItem eventItem)
        {
            _model = eventItem;
        }

        public int Count { get { return _model.Count; }}
        public DateTime Date { get { return _model.Date; } }
    }

    public class DummyEventItemViewModel : IEventItemViewModel
    {
        public DummyEventItemViewModel(DateTime date)
        {
            Date = date;
        }

        public DateTime Date { get; private set; }
    }
}
