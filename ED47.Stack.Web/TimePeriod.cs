using System;
using System.Collections.Generic;

namespace ED47.Stack.Web
{
    public class TimePeriod
    {

        public bool IsEmpty { get;  private set; }

        public TimePeriod(DateTime? start, DateTime? end, IEnumerable<DateTime> publicHolidays = null)
        {
            Start = start;
            End = end;

            if (publicHolidays != null)
                AddPublicHoliday(publicHolidays);

            Update();
        }

        public DateTime? Start { get; private set; }
        public DateTime? End { get; private set; }

        public int BusinessDayCount
        {
            get;
            private set;
        }

        public int DayCount
        {
            get;
            private set;
        }


        public bool IsIn(DateTime? dateTime)
        {
          
            return dateTime.HasValue && dateTime.Value >= Start && dateTime.Value <= End;
        }

        public TimePeriod UnionWith(DateTime? start, DateTime? end)
        {
            return UnionWith(new TimePeriod(start, end));
        }

        public TimePeriod UnionWith(TimePeriod period)
        {
            if (IsEmpty)
            {
                IsEmpty = period.IsEmpty;
                Start = period.Start;
                End = period.End;
                return this;
            }

            if (period.Start <= End && period.End > End)
                End = period.End;

            if (period.End >= Start && period.Start < Start)
                Start = period.Start;

            Update();
            return this;
        }

        public TimePeriod IntersectWith(DateTime? start, DateTime? end)
        {
            return IntersectWith(new TimePeriod(start, end));
        }

        public TimePeriod IntersectWith(TimePeriod period)
        {
            if(IsEmpty) return this;

            if (period.IsEmpty)
            {
                IsEmpty = true;
                return this;
            }

            if (IsIn(period.Start))
                Start = period.Start;

            if (IsIn(period.End))
                End = period.End;

            if (period.End < Start || period.Start > End)
            {
                IsEmpty = true;
            }
         
            Update();

            return this;
        }


        private HashSet<DateTime> _publicHolidays = new HashSet<DateTime>();

        public TimePeriod AddPublicHoliday(IEnumerable<DateTime> dates)
        {
            if (dates == null)
                return this;

            _publicHolidays.UnionWith(dates);
            Update();
            return this;
        }

        private void Update()
        {

            if (!Start.HasValue || !End.HasValue)
                IsEmpty = true;

            if (Start > End)
                IsEmpty = true;


            if (IsEmpty)
            {
                BusinessDayCount = 0;
                DayCount = 0;
                return;
            }
            DayCount = (int)(End.Value - Start.Value).TotalDays + 1;
            BusinessDayCount = GetBusinessDays(Start.Value, End.Value);
            foreach (var holiday in _publicHolidays)
            {
                if (IsIn(holiday) && holiday.DayOfWeek != DayOfWeek.Saturday && holiday.DayOfWeek != DayOfWeek.Sunday)
                {
                    BusinessDayCount--;
                }
            }

            

        }

        private static int GetBusinessDays(DateTime start, DateTime end)
        {
            if (start.DayOfWeek == DayOfWeek.Saturday)
            {
                start = start.AddDays(2);
            }
            else if (start.DayOfWeek == DayOfWeek.Sunday)
            {
                start = start.AddDays(1);
            }

            if (end.DayOfWeek == DayOfWeek.Saturday)
            {
                end = end.AddDays(-1);
            }
            else if (end.DayOfWeek == DayOfWeek.Sunday)
            {
                end = end.AddDays(-2);
            }

            int diff = (int)end.Subtract(start).TotalDays + 1;

            int result = diff / 7 * 5 + diff % 7;

            if (end.DayOfWeek < start.DayOfWeek)
            {
                return result - 2;
            }

            return Math.Max(result,0);
        }


    }
}