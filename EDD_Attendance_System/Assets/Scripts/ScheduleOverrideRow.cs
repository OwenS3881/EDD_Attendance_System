using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScheduleOverrideRow : MonoBehaviour
{
    [SerializeField] private GameObject OverrideTogglePrefab;
    [SerializeField] private GameObject overrideTogglesContainer;

    [SerializeField] private DateButton startDate;
    [SerializeField] private DateButton endDate;
    [SerializeField] private Toggle rangeToggle;
    [SerializeField] private Image rangeToggleBG;
    [SerializeField] private Button endDateButton;
    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;

    private BlockToggleContainer[] toggles;

    public void OnRangeToggleChange(bool state)
    {
        if (state)
        {
            rangeToggleBG.color = onColor;
            endDateButton.interactable = true;
        }
        else
        {
            rangeToggleBG.color = offColor;
            endDateButton.interactable = false;
        }
    }

    public void UpdateRangeToggle(bool state)
    {
        rangeToggle.isOn = state;
        OnRangeToggleChange(state);
    }

    public void DestroySelf()
    {
        AdminScheduleManager.instance.RemoveOverride(this);
    }

    public void SetDate(string date)
    {
        startDate.CurrentDate = date;
    }

    public void SetDate(string firstDate, string lastDate)
    {
        startDate.CurrentDate = firstDate;
        endDate.CurrentDate = lastDate;
    }

    public string GetDate()
    {
        if (string.IsNullOrEmpty(startDate.CurrentDate))
        {
            DesktopGraphics.instance.DisplayMessage("Fill out all dates");
            return null;
        }

        if (rangeToggle.isOn)
        {
            if (string.IsNullOrEmpty(endDate.CurrentDate))
            {
                DesktopGraphics.instance.DisplayMessage("Fill out all dates");
                return null;
            }

            if (!MyFunctions.VerifyDateOrder(startDate.CurrentDate, endDate.CurrentDate))
            {
                DesktopGraphics.instance.DisplayMessage("Invalid date range, 1st date must come before 2nd date");
                return null;
            }

            return startDate.CurrentDate + "*" + endDate.CurrentDate;
        }
        else
        {
            return startDate.CurrentDate;
        }
    }

    public void OnDateSet()
    {
        if (rangeToggle.isOn && !string.IsNullOrEmpty(startDate.CurrentDate) && !string.IsNullOrEmpty(endDate.CurrentDate) && !MyFunctions.VerifyDateOrder(startDate.CurrentDate, endDate.CurrentDate))
        {
            DesktopGraphics.instance.DisplayMessage("Invalid date range, 1st date must come before 2nd date");
            endDate.CurrentDate = startDate.CurrentDate;
        }
    }

    private void ClearTogglesContainer()
    {
        foreach (Transform t in overrideTogglesContainer.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(overrideTogglesContainer.transform)) continue;

            Destroy(t.gameObject);
        }

        toggles = new BlockToggleContainer[7];
    }

    private void CreateNewToggles()
    {
        ClearTogglesContainer();

        for (int i = 0; i < 7; i++)
        {
            BlockToggleContainer newToggle = Instantiate(OverrideTogglePrefab, overrideTogglesContainer.transform).GetComponent<BlockToggleContainer>();
            newToggle.Period = i + 1;
            toggles[i] = newToggle;
        }
    }

    public void AssignSchedule(List<int> newSchedule)
    {
        if (newSchedule == null)
        {
            Debug.LogError("No Schedule given");
            return;
        }

        if (toggles == null || toggles.Length < 7)
        {
            CreateNewToggles();
        }

        for (int i = 0; i < 7; i++)
        {
            toggles[i].SetState(newSchedule.Contains(i + 1));
        }
    }

    public List<int> GetSchedule()
    {
        List<int> output = new List<int>();

        for (int i = 0; i < 7; i++)
        {
            if (toggles[i].IsOn())
            {
                output.Add(toggles[i].Period);
            }
        }

        return output;
    }
}
