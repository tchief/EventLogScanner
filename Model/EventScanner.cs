namespace EventLogScanner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class EventScanner : IEventScanner
    {
        private const string DefaultLogName = "Application";
        public IEnumerable<EventItem> Scan(DateTime fromDate, DateTime toDate, string logName = null)
        {
            var eventLog = new EventLog { Log = logName ?? DefaultLogName };
            var entries =
                eventLog.Entries.Cast<EventLogEntry>()
                        .Where(e => e.TimeGenerated >= fromDate && e.TimeGenerated <= toDate)
                        .GroupBy(e => e.TimeGenerated.Date)
                        .Select(e => new EventItem(e.Key, e.Count()));
            return entries;
        }

        public bool IsLogNameSupported(string logName)
        {
            try
            {
                var eventLog = new EventLog { Log = logName };
                var entries = eventLog.Entries;
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }
    }
}
