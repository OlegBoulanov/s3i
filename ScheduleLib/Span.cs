using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleLib
{
    public class Span
    {
        public DateTimeOffset Begin { get; set; }
        public DateTimeOffset End { get; set; }
        public bool IsEmpty { get { return End <= Begin; } }
        public bool IsNull { get { return IsEmpty && Begin == DateTimeOffset.MinValue; } }
        public Span(DateTimeOffset begin, DateTimeOffset end)
        {
            Begin = begin;
            End = end;
        }
        public bool Enter(DateTimeOffset dt, Plan plan)
        {
            if (Begin < dt && End <= dt && plan != null)
            {
                int days = 2;
                Span gate = plan.Next(dt, days);
            }
            return Begin <= dt && dt < End;
        }
        /// <summary>
        /// Strict precedence, may or may not overlap
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns>True, if first segment starts before second starts, and ends before second ends. May or may not overlap.</returns>
        public static bool operator ==(Span s1, Span s2) { return s1.Begin == s2.Begin && s1.End == s2.End; }
        public static bool operator !=(Span s1, Span s2) { return s1.Begin != s2.Begin || s1.End != s2.End; ; }
        public static bool operator <(Span s1, Span s2) { return s1.Begin < s2.Begin && s1.End < s2.End; }
        public static bool operator >(Span s1, Span s2) { return s2 < s1; }
        /// <summary>
        /// Soft precedence, meaning they may overlap, and can be joined to one
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns>True, if first segment starts before second starts, and ends before second ends, and segments overlap.</returns>
        public static bool operator <=(Span s1, Span s2) { return s1.Begin <= s2.Begin && s2.Begin <= s1.End && s1.End <= s2.End; }
        public static bool operator >=(Span t1, Span t2) { return t2 <= t1; }
        /// <summary>
        /// Joining two segments (commutative)
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns>Joined (or first of the two, if no intersection) segment</returns>
        public static Span operator +(Span s1, Span s2)
        {
            if (s2.IsEmpty) return s1;
            if (s1 <= s2) return new Span(s1.Begin, s2.End);
            if (s2 <= s1) return new Span(s2.Begin, s1.End);
            if (s2 < s1) return s2;
            return s1;
        }
        /// <summary>
        /// Subtracting segments
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns>Remainder of the first segment after removing overlap with the second, or <see cref="Span.Null"/>Span.Null</returns>
        public static Span operator-(Span s1, Span s2)
        {
            if (s2.IsEmpty) return s1;
            if (s2.Begin < s1.Begin && s1.End < s2.End) return Span.Null;
            if (s1.Begin < s2.Begin && s2.Begin < s1.End) return new Span(s1.Begin, s2.Begin);
            if (s1 <= s2) return new Span(s1.Begin, s2.Begin);
            if (s2 <= s1) return new Span(s2.End, s1.End);
            return s1;
        }
        public override bool Equals(object obj)
        {
            return obj is Span && this == (Span)obj;
        }
        public override int GetHashCode()
        {
            return Begin.GetHashCode() + End.GetHashCode();
        }
        public static Span Null { get; } = new Span(DateTimeOffset.MinValue, DateTimeOffset.MinValue);
    }
}
