using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class DateButton : MonoBehaviour
{
    [SerializeField] private TMP_Text dateLabel;

    public UnityEvent OnSelect;

    private static string[] monthAbbreviations = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};

    private string currentDate;
    public string CurrentDate
    {
        get
        {
            return currentDate;
        }
        set
        {
            if (string.IsNullOrEmpty(value) || !DatePicker.IsDateValid(value))
            {
                currentDate = null;
                dateLabel.text = "Select Date";
            }

            if (currentDate != null && currentDate.Equals(value)) return;

            currentDate = value;

            dateLabel.text = ConvertToNiceDate(currentDate);

            OnSelect.Invoke();
        }
    }

    public static string ConvertToNiceDate(string date)
    {
        string[] splitDate = date.Split('-');

        string year = splitDate[0];
        string month = monthAbbreviations[Int32.Parse(splitDate[1]) - 1];
        string day = Int32.Parse(splitDate[2]).ToString();

        return ($"{month} {day}, {year}");
    }

    public void SelectDateMobile()
    {
        MobileGraphics.instance.SelectDate(this, CurrentDate);
    }

    public void SelectDateDesktop()
    {
        DesktopGraphics.instance.SelectDate(this, CurrentDate);
    }

    public void ResetDate()
    {
        CurrentDate = null;
    }

}
