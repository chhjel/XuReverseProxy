using System.Diagnostics;
using System.Globalization;

namespace XuReverseProxy.Core.Systems.ScheduledTasks.Cron;

/// <summary>
/// Represents a schedule initialized from the crontab expression.
/// </summary>
[Serializable]
public sealed class CrontabSchedule
{
    private static readonly char[] _separators = { ' ' };
    private readonly CrontabField _days;
    private readonly CrontabField _daysOfWeek;
    private readonly CrontabField _hours;
    private readonly CrontabField _minutes;
    private readonly CrontabField _seconds;
    private readonly CrontabField _months;

    private CrontabSchedule(string expression)
    {
        Debug.Assert(expression != null);

        string[] fields = expression.Split(_separators, StringSplitOptions.RemoveEmptyEntries);

        if (fields.Length != 6)
        {
            throw new FormatException(string.Format(
                "'{0}' is not a valid crontab expression. It must contain at least 6 components of a schedule "
                + "(in the sequence of seconds, minutes, hours, days, months, days of week).",
                expression));
        }

        _seconds = CrontabField.Seconds(fields[0]);
        _minutes = CrontabField.Minutes(fields[1]);
        _hours = CrontabField.Hours(fields[2]);
        _days = CrontabField.Days(fields[3]);
        _months = CrontabField.Months(fields[4]);
        _daysOfWeek = CrontabField.DaysOfWeek(fields[5]);
    }

    private static Calendar Calendar => CultureInfo.InvariantCulture.Calendar;

    public static CrontabSchedule Parse(string expression)
    {
        return expression == null ? throw new ArgumentNullException(nameof(expression)) : new CrontabSchedule(expression);
    }

    public IEnumerable<DateTime> GetNextOccurrences(DateTime baseTime, DateTime endTime)
    {
        for (DateTime occurrence = GetNextOccurrence(baseTime, endTime);
            occurrence < endTime;
            occurrence = GetNextOccurrence(occurrence, endTime))
        {
            yield return occurrence;
        }
    }

    public DateTime GetNextOccurrence(DateTime baseTime)
    {
        return GetNextOccurrence(baseTime, DateTime.MaxValue);
    }

    public DateTime GetNextOccurrence(DateTime baseTime, DateTime endTime)
    {
        const int nil = -1;

        int baseYear = baseTime.Year;
        int baseMonth = baseTime.Month;
        int baseDay = baseTime.Day;
        int baseHour = baseTime.Hour;
        int baseMinute = baseTime.Minute;
        int baseSecond = baseTime.Second;

        int endYear = endTime.Year;
        int endMonth = endTime.Month;
        int endDay = endTime.Day;

        int year = baseYear;
        int month = baseMonth;
        int day = baseDay;
        int hour = baseHour;
        int minute = baseMinute;
        int second = baseSecond + 1;

        //
        // Second
        //

        second = _seconds.Next(second);

        if (second == nil)
        {
            second = _seconds.GetFirst();
            minute++;
        }

        //
        // Minute
        //

        minute = _minutes.Next(minute);

        if (minute == nil)
        {
            second = _seconds.GetFirst();
            minute = _minutes.GetFirst();
            hour++;
        }

        //
        // Hour
        //

        hour = _hours.Next(hour);

        if (hour == nil)
        {
            second = _seconds.GetFirst();
            minute = _minutes.GetFirst();
            hour = _hours.GetFirst();
            day++;
        }
        else if (hour > baseHour)
        {
            second = _seconds.GetFirst();
            minute = _minutes.GetFirst();
        }

        //
        // Day
        //

        day = _days.Next(day);

    RetryDayMonth:

        if (day == nil)
        {
            second = _seconds.GetFirst();
            minute = _minutes.GetFirst();
            hour = _hours.GetFirst();
            day = _days.GetFirst();
            month++;
        }
        else if (day > baseDay)
        {
            second = _seconds.GetFirst();
            minute = _minutes.GetFirst();
            hour = _hours.GetFirst();
        }

        //
        // Month
        //

        month = _months.Next(month);

        if (month == nil)
        {
            second = _seconds.GetFirst();
            minute = _minutes.GetFirst();
            hour = _hours.GetFirst();
            day = _days.GetFirst();
            month = _months.GetFirst();
            year++;
        }
        else if (month > baseMonth)
        {
            second = _seconds.GetFirst();
            minute = _minutes.GetFirst();
            hour = _hours.GetFirst();
            day = _days.GetFirst();
        }

        //
        // The day field in a cron expression spans the entire range of days
        // in a month, which is from 1 to 31. However, the number of days in
        // a month tend to be variable depending on the month (and the year
        // in case of February). So a check is needed here to see if the
        // date is a border case. If the day happens to be beyond 28
        // (meaning that we're dealing with the suspicious range of 29-31)
        // and the date part has changed then we need to determine whether
        // the day still makes sense for the given year and month. If the
        // day is beyond the last possible value, then the day/month part
        // for the schedule is re-evaluated. So an expression like "0 0
        // 15,31 * *" will yield the following sequence starting on midnight
        // of Jan 1, 2000:
        //
        //  Jan 15, Jan 31, Feb 15, Mar 15, Apr 15, Apr 31, ...
        //

        bool dateChanged = day != baseDay || month != baseMonth || year != baseYear;

        if (day > 28 && dateChanged && day > Calendar.GetDaysInMonth(year, month))
        {
            if (year >= endYear && month >= endMonth && day >= endDay)
            {
                return endTime;
            }

            day = nil;
            goto RetryDayMonth;
        }

        DateTime nextTime = new(year, month, day, hour, minute, second, 0, baseTime.Kind);

        if (nextTime >= endTime)
        {
            return endTime;
        }

        //
        // Day of week
        //

        return _daysOfWeek.Contains((int)nextTime.DayOfWeek)
            ? nextTime
            : GetNextOccurrence(new DateTime(year, month, day, 23, 59, 59, 0, baseTime.Kind), endTime);
    }

    public override string ToString()
    {
        StringWriter writer = new(CultureInfo.InvariantCulture);

        _seconds.Format(writer, true);
        writer.Write(' ');
        _minutes.Format(writer, true);
        writer.Write(' ');
        _hours.Format(writer, true);
        writer.Write(' ');
        _days.Format(writer, true);
        writer.Write(' ');
        _months.Format(writer, true);
        writer.Write(' ');
        _daysOfWeek.Format(writer, true);

        return writer.ToString();
    }
}