using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;

public class DatePicker : MonoBehaviour
{
    [SerializeField] private ScrollOptionSelector monthSelector;
    [SerializeField] private ScrollOptionSelector daySelector;
    [SerializeField] private ScrollOptionSelector yearSelector;

    private string selectedDate;
    public string SelectedDate
    {
        get
        {
            string day = daySelector.GetSelectedOption();
            if (day.Length <= 1) day = "0" + day;

            string month = monthSelector.GetSelectedOption();
            if (month.Length <= 1) month = "0" + month;

            selectedDate = yearSelector.GetSelectedOption() + "-" + month + "-" + day;

            return selectedDate;
        }
        set
        {
            DateTime result;
            if (!DateTime.TryParseExact(value, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out result)) return;

            selectedDate = value;

            string[] splitDate = selectedDate.Split('-');

            yearSelector.SetSelectedOption(splitDate[0]);
            monthSelector.SetSelectedOption(Int32.Parse(splitDate[1]).ToString());
            daySelector.SetSelectedOption(Int32.Parse(splitDate[2]).ToString());
        }
    }

    private void Start()
    {
        Invoke(nameof(ResetDate), 0.01f);
    }


    public void SetDate(string date)
    {
        SelectedDate = date;
    }

    public void ResetDate()
    {
        SelectedDate = DateTime.Today.ToString("yyyy-MM-dd");
    }
}
