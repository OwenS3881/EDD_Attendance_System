using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockScheduleColumn : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private GameObject blockPeriodTogglePrefab;
    [SerializeField] private GameObject blockTogglesContainer;

    private BlockToggleContainer[] toggles;

    [SerializeField] private string day;
    public string Day
    {
        get
        {
            return day;
        }
        set
        {
            day = value;
            label.text = day;
        }
    }

    private void Start()
    {
        Day = day;

        toggles = new BlockToggleContainer[7];

        for (int i = 0; i < 7; i++)
        {
            BlockToggleContainer newToggle = Instantiate(blockPeriodTogglePrefab, blockTogglesContainer.transform).GetComponent<BlockToggleContainer>();
            newToggle.Period = i + 1;
            toggles[i] = newToggle;
        }
    }

    public void AssignSchedule(List<int> newSchedule)
    {
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
