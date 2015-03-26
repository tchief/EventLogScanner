namespace EventLogScanner
{
    using System;

    public class EventItem
    {
        public EventItem(DateTime date, int count)
        {
            Date = date;
            Count = count;
        }

        public DateTime Date { get; private set; }
        public int Count { get; private set; }
    }
}
