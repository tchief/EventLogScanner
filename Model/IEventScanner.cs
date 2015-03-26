namespace EventLogScanner
{
    using System;
    using System.Collections.Generic;

    public interface IEventScanner
    {
        IEnumerable<EventItem> Scan(DateTime fromDate, DateTime toDate, string logName = null);
        bool IsLogNameSupported(string logName);
    }
}
