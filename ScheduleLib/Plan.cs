using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleLib
{
    public class Plan
    {
        public List<Range> Enable { get; protected set; } = new List<Range>();
        public List<Range> Disable { get; protected set; } = new List<Range>();
        public Span Next(DateTimeOffset dt, int days)
        {
            var span = Span.Null;
            if (DateTimeOffset.MinValue == dt) return span;
            foreach(var r in Enable)
            {
                var next = r.NextRange(dt, days);
                if (next.IsEmpty) continue;
                if (span.IsEmpty) span = next;
                else span += next;
            }
            foreach(var r in Disable)
            {
                var next = r.NextRange(dt, days);
                if (next.IsEmpty) continue;
                span -= next;
                if (span.IsEmpty || span.End <= dt) return Next(next.End.AddTicks(1), days);
            }
            return span;
        }
    }
}
