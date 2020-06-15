using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler
{
    public class AdvancedTextWriterTraceListener : TextWriterTraceListener
    {
        private const string dateTimeFormat = @"MM\/dd\/yyyy HH:mm:ss";

        public AdvancedTextWriterTraceListener(string logFilePath) : base(logFilePath) { }
        public void WriteHeader(TraceEventCache eventCache, String source, TraceEventType eventType, int id)
        {
            if (this.TraceOutputOptions.HasFlag(TraceOptions.DateTime) && eventCache != null)
                Write(eventCache.DateTime.ToString(AdvancedTextWriterTraceListener.dateTimeFormat));

            Write($" [{eventType}] ");

            if (this.TraceOutputOptions.HasFlag(TraceOptions.ThreadId) && eventCache != null)
                Write($" ({eventCache.ThreadId}) ");
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
                return;

            this.WriteHeader(eventCache, source, eventType, id);
            base.WriteLine(message);
        }
    }
}
