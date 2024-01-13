using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminScheduleManager : MonoBehaviour
{
    [SerializeField] private GameObject blockScheduleContainer;
    [SerializeField] private GameObject blockScheduleColumnPrefab;
    private BlockScheduleColumn[] blockScheduleColumns;

    private void OnEnable()
    {
        SetupBlockSchedule();
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
}
