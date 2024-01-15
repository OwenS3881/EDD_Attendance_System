using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class MyFunctions
{
    public static string GetDayOfWeek(string date)
    {
        return DateTime.Parse(date).DayOfWeek.ToString();
    }

    public static List<string> GetDateRange(string startDate, string endDate)
    {
        List<string> output = new List<string>();

        DateTime start = DateTime.Parse(startDate);
        DateTime end = DateTime.Parse(endDate);

        if (start.CompareTo(end) > 0)
        {
            Debug.LogError("startDate must occur before endDate");
            return null;
        }

        for (var dt = start; dt <= end; dt = dt.AddDays(1))
        {
            output.Add(dt.ToString("yyyy-MM-dd"));
        }

        return output;
    }

    public static bool IsDateInRange(string inputDate, string lowerDateBound, string upperDateBound)
    {
        DateTime input = DateTime.Parse(inputDate);
        DateTime lower = DateTime.Parse(lowerDateBound);
        DateTime upper = DateTime.Parse(upperDateBound);

        return inputDate.CompareTo(lowerDateBound) >= 0 && inputDate.CompareTo(upperDateBound) <= 0;
    }

    public static bool VerifyDateOrder(string earlyDate, string lateDate)
    {
        DateTime early = DateTime.Parse(earlyDate);
        DateTime late = DateTime.Parse(lateDate);

        return early.CompareTo(late) <= 0;
    }
}
