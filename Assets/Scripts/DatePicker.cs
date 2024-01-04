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

    private DateButton currentButton;
    private string resetDate;

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
            if (!IsDateValid(value)) return;

            selectedDate = value;

            string[] splitDate = selectedDate.Split('-');

            yearSelector.SetSelectedOption(splitDate[0]);
            monthSelector.SetSelectedOption(Int32.Parse(splitDate[1]).ToString());
            daySelector.SetSelectedOption(Int32.Parse(splitDate[2]).ToString());
        }
    }

    public static bool IsDateValid(string date)
    {
        DateTime result;
        return DateTime.TryParseExact(date, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out result);
    }

    private void Start()
    {
        resetDate = DateTime.Today.ToString("yyyy-MM-dd");
        Invoke(nameof(ResetDate), 0.01f);
    }


    public void SetDate(string date)
    {
        SelectedDate = date;
    }

    public void ResetDate()
    {
        SelectedDate = resetDate;
    }

    public void Initialize(DateButton button, string initialDate)
    {
        if (string.IsNullOrEmpty(initialDate))
        {
            resetDate = DateTime.Today.ToString("yyyy-MM-dd");
        }
        else
        {
            resetDate = initialDate;
        }

        ResetDate();

        currentButton = button;
    }

    public void Select()
    {
        currentButton.CurrentDate = SelectedDate;
        gameObject.SetActive(false);
    }
}
