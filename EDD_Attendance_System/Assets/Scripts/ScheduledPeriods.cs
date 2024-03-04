using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class ScheduledPeriods
{
    public string date;
    public List<int> periods;

    public ScheduledPeriods(string date, List<int> periods)
    {
        this.date = date;
        this.periods = periods;
    }

    public ScheduledPeriods(string date)
    {
        this.date = date;
        periods = new List<int>();
    }

    public ScheduledPeriods(ScheduledPeriods clone)
    {
        date = clone.date;
        periods = clone.periods;
    }
}
