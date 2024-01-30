using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DateRangeAttendanceContainer : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private RectTransform absentBar;
    [SerializeField] private RectTransform tardyBar;
    [SerializeField] private RectTransform presentBar;
    [SerializeField] private float defaultBarWidth;
    [SerializeField] private TMP_Text presentPercentText;

    public void AssignDate(string date)
    {
        dateText.text = DateButton.ConvertToNiceDate(date);
    }

    public void SetPresentBar(int periodsPresent, int totalPeriods)
    {
        if (totalPeriods <= 0)
        {
            absentBar.sizeDelta = new Vector2(0, absentBar.sizeDelta.y);
            tardyBar.sizeDelta = new Vector2(0, tardyBar.sizeDelta.y);
            presentBar.sizeDelta = new Vector2(0, presentBar.sizeDelta.y);
            presentPercentText.text = "100%";
            return;
        }

        if (periodsPresent > totalPeriods) periodsPresent = totalPeriods;

        absentBar.sizeDelta = new Vector2(defaultBarWidth, absentBar.sizeDelta.y);
        presentBar.sizeDelta = new Vector2(defaultBarWidth * ((float)periodsPresent / (float)totalPeriods), presentBar.sizeDelta.y);
        presentPercentText.text = $"{(int)(((float)periodsPresent / (float)totalPeriods) * 100)}%";
    }

    public void SetTardyBar(int periodsPresentAndTardy, int totalPeriods)
    {
        if (totalPeriods <= 0)
        {
            absentBar.sizeDelta = new Vector2(0, absentBar.sizeDelta.y);
            tardyBar.sizeDelta = new Vector2(0, tardyBar.sizeDelta.y);
            presentBar.sizeDelta = new Vector2(0, presentBar.sizeDelta.y);
            return;
        }

        if (periodsPresentAndTardy > totalPeriods) periodsPresentAndTardy = totalPeriods;

        absentBar.sizeDelta = new Vector2(defaultBarWidth, absentBar.sizeDelta.y);
        tardyBar.sizeDelta = new Vector2(defaultBarWidth * ((float)periodsPresentAndTardy / (float)totalPeriods), tardyBar.sizeDelta.y);
    }
}
