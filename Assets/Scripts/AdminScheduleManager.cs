using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AdminScheduleManager : MonoBehaviour
{
    [Header("Block Schedule")]
    [SerializeField] private GameObject blockScheduleContainer;
    [SerializeField] private GameObject blockScheduleColumnPrefab;
    private BlockScheduleColumn[] blockScheduleColumns;

    [Header("Schedule Overrides")]
    [SerializeField] private GameObject scheduleOverridesContainer;
    [SerializeField] private GameObject scheduleOverridePrefab;
    private List<ScheduleOverrideRow> scheduleOverrides;
    [SerializeField] private float defaultOverrideContentHeight;
    [SerializeField] private float distanceBetweenOverrides;

    public static AdminScheduleManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"Multiple {GetType()}s in the scene");
        }
    }

    private void OnEnable()
    {
        SetupBlockSchedule();
        SetUpOverrides();
    }

    private void ClearBlockContainer()
    {
        foreach (Transform t in blockScheduleContainer.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(blockScheduleContainer.transform)) continue;

            Destroy(t.gameObject);
        }

        blockScheduleColumns = new BlockScheduleColumn[7];
    }

    private void SetupBlockSchedule()
    {
        if (AdminHomeManager.instance == null || AdminHomeManager.instance.currentData == null) return;

        ClearBlockContainer();

        for (int i = 0; i < 7; i++)
        {
            blockScheduleColumns[i] = Instantiate(blockScheduleColumnPrefab, blockScheduleContainer.transform).GetComponent<BlockScheduleColumn>();

            blockScheduleColumns[i].Day = AdminHomeManager.instance.currentData.blockSchedule[i].date;
            blockScheduleColumns[i].AssignSchedule(AdminHomeManager.instance.currentData.blockSchedule[i].periods);
        }
    }

    public void UpdateBlockSchedule()
    {
        DesktopGraphics.instance.Loading(true);

        AdminHomeManager.instance.currentData.blockSchedule.Clear();

        for (int i = 0; i < 7; i++)
        {
            AdminHomeManager.instance.currentData.blockSchedule.Add(new ScheduledPeriods(blockScheduleColumns[i].Day, blockScheduleColumns[i].GetSchedule()));
        }

        Database.instance.SaveDataToFirebase(AdminHomeManager.instance.currentData);

        DesktopGraphics.instance.DisplayMessage("Success");
        DesktopGraphics.instance.Loading(false);
    }

    private void SetUpOverrides()
    {
        if (AdminHomeManager.instance == null || AdminHomeManager.instance.currentData == null) return;

        ClearOverrideContainer();

        for (int i = 0; i < AdminHomeManager.instance.currentData.scheduleOverrides.Count; i++)
        {
            ScheduleOverrideRow newOverride = Instantiate(scheduleOverridePrefab, scheduleOverridesContainer.transform).GetComponent<ScheduleOverrideRow>();

            string[] dates = AdminHomeManager.instance.currentData.scheduleOverrides[i].date.Split("*");

            if (dates.Length == 1)
            {
                newOverride.SetDate(dates[0]);
                newOverride.UpdateRangeToggle(false);
            }
            else if (dates.Length == 2)
            {
                newOverride.SetDate(dates[0], dates[1]);
                newOverride.UpdateRangeToggle(true);
            }
            else
            {
                Debug.LogError("Date was not 1 or 2");
            }

            newOverride.AssignSchedule(AdminHomeManager.instance.currentData.scheduleOverrides[i].periods);

            scheduleOverrides.Add(newOverride);
        }

        AdjustOverrideContainer();
    }

    private void ClearOverrideContainer()
    {
        foreach (Transform t in scheduleOverridesContainer.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(scheduleOverridesContainer.transform)) continue;

            Destroy(t.gameObject);
        }

        scheduleOverrides = new List<ScheduleOverrideRow>();

        AdjustOverrideContainer();
    }



    private void AdjustOverrideContainer()
    {
        scheduleOverridesContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(scheduleOverridesContainer.GetComponent<RectTransform>().sizeDelta.x, (distanceBetweenOverrides * scheduleOverrides.Count) + defaultOverrideContentHeight);
    }

    public void AddOverride()
    {
        ScheduleOverrideRow newOverride = Instantiate(scheduleOverridePrefab, scheduleOverridesContainer.transform).GetComponent<ScheduleOverrideRow>();

        newOverride.UpdateRangeToggle(false);
        newOverride.AssignSchedule(new List<int>() { 1, 2, 3, 4, 5, 6, 7 });

        scheduleOverrides.Add(newOverride);

        AdjustOverrideContainer();
    }

    public void RemoveOverride(ScheduleOverrideRow toBeRemoved)
    {
        scheduleOverrides.Remove(toBeRemoved);

        Destroy(toBeRemoved.gameObject);

        AdjustOverrideContainer();
    }

    public void UpdateOverrides()
    {
        DesktopGraphics.instance.Loading(true);

        List<ScheduledPeriods> tempList = new List<ScheduledPeriods>();

        for (int i = 0; i < scheduleOverrides.Count; i++)
        {
            string date = scheduleOverrides[i].GetDate();

            if (string.IsNullOrEmpty(date))
            {
                DesktopGraphics.instance.Loading(false);
                return;
            }

            tempList.Add(new ScheduledPeriods(date, scheduleOverrides[i].GetSchedule()));
        }

        AdminHomeManager.instance.currentData.scheduleOverrides.Clear();
        AdminHomeManager.instance.currentData.scheduleOverrides = tempList;

        Database.instance.SaveDataToFirebase(AdminHomeManager.instance.currentData);

        DesktopGraphics.instance.DisplayMessage("Success");
        DesktopGraphics.instance.Loading(false);
    }
}
