using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleLib
{
    public class Range
    {
        DateTimeOffset dbeg, dend;
        TimeSpan tbeg, tend;
        public DateTimeOffset Begin
        {
            get { return dbeg.AddTicks(tbeg.Ticks); }
            set { dbeg = value.Date; tbeg = value.TimeOfDay; }
        }
        public DateTimeOffset End
        {
            get { return dend.AddTicks(tend.Ticks); }
            set { dend = value.Date; tend = value.TimeOfDay; }
        }
        public byte WeekDays { get; set; } = 0;
        public uint MonthDays { get; set; } = 0;
        public ushort Months { get; set; } = 0;

        public Range()
        {
            Begin = End = DateTimeOffset.MinValue;
        }
        public Range(DateTimeOffset begin, DateTimeOffset end)
        {
            Begin = begin;
            End = end;
        }
        public Range(TimeSpan begin, TimeSpan end)
        {
            Begin = DateTimeOffset.MinValue.Add(begin);
            End = DateTimeOffset.MinValue.Add(end);
        }

        public DateTimeOffset NextOn(DateTimeOffset dt, int days)
        {
            var nd = dt.Date;
            for (var n = 0; n < days; n++, nd = nd.AddDays(1))
            {
                if (nd.Date < dbeg) continue;
                if (DateTimeOffset.MinValue < dend && dend < nd.Date) continue;
                if (0 < WeekDays && (WeekDays & (1 << (int)nd.DayOfWeek)) == 0) continue;
                if (0 < MonthDays && (MonthDays & (1 << (int)nd.Day - 1)) == 0) continue;
                if (0 < Months && (Months & (1 << (int)nd.Month - 1)) == 0) continue;
                if (tbeg == TimeSpan.Zero && tend == TimeSpan.Zero) return nd;
                var t = 0 < n ? dt.TimeOfDay : TimeSpan.Zero;
                if (tbeg <= tend)
                {
                    // daytime range, includes starting at midnight
                    if (tend <= t) continue;    // over for today
                    if (t < tbeg) return nd.AddTicks(tbeg.Ticks);
                }
                else if (tend != TimeSpan.Zero)
                {
                    // nighttime range
                    if (t < tend) return nd;    //active yet, return midnight
                    if (t < tbeg) return nd.AddTicks(tbeg.Ticks);
                }
                else
                {
                    // ending at midnight
                    if (t < tbeg) return nd.AddTicks(tbeg.Ticks);
                }
            }
            return DateTimeOffset.MinValue;
        }
        public DateTimeOffset NextOff(DateTimeOffset dt, int days)
        {
            var nt = DateTimeOffset.MinValue;
            if (DateTimeOffset.MinValue < dbeg || DateTimeOffset.MinValue < dend || TimeSpan.Zero < tend)
            {
                // inversed range
                if (tbeg == tend) { nt = dend.AddTicks(tbeg.Ticks); }
                else { nt = new Range(dbeg.AddTicks(tend.Ticks), dend.AddTicks(tbeg.Ticks)).NextOn(dt, days); }
            }
            if (0 != WeekDays)
            {
                var nd = new Range() { WeekDays = (byte)~WeekDays }.NextOn(dt, days);
                if (nt == DateTimeOffset.MinValue || (DateTimeOffset.MinValue < nd && nd < nt)) nt = nd;
            }
            if(0 != MonthDays)
            {
                var nd = new Range() { MonthDays = (uint)~MonthDays }.NextOn(dt, days);
                if (nt == DateTimeOffset.MinValue || (DateTimeOffset.MinValue < nd && nd < nt)) nt = nd;
            }
            if (0 != Months)
            {
                var nd = new Range() { Months = (ushort)~Months }.NextOn(dt, days);
                if (nt == DateTimeOffset.MinValue || ( DateTimeOffset.MinValue < nd && nd < nt)) nt = nd;
            }
            return nt;
        }
        public Span NextRange(DateTimeOffset dt, int days)
        {
            if (DateTimeOffset.MinValue == Begin && DateTimeOffset.MinValue == End && 0 == WeekDays && 0 == MonthDays && 0 == Months) return Span.Null;
            DateTimeOffset dateTimeOn = NextOn(dt, days);
            if (DateTimeOffset.MinValue == dateTimeOn) return Span.Null;
            DateTimeOffset dateTimeOff = NextOff(dateTimeOn.AddTicks(1), days);
            if (DateTimeOffset.MinValue == dateTimeOff) dateTimeOff = dateTimeOn.Date.AddDays(days);
            return new Span(dateTimeOn, dateTimeOff);
        }
    }
}
