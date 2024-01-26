using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StudentFreePeriodManager : MonoBehaviour
{
    private StudentInfoData studentInfo;

    [SerializeField] private GameObject freePeriodParent;
    [SerializeField] private GameObject freePeriodSelectionPrefab;
    [SerializeField] private GameObject noFreePeriodsGraphic;

    private void Start()
    {
        GetData();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MobileHome");
    }

    private void GetData()
    {
        MobileGraphics.instance.Loading(true);
        Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<StudentInfoData>(GetStudentDataCallback));
    }

    private void GetStudentDataCallback(StudentInfoData output)
    {
        if (output == null)
        {
            MobileGraphics.instance.Loading(false);
            MobileGraphics.instance.DisplayMessage("An error has occurred");
            Debug.LogWarning("Couldn't find StudentInfo");
            return;
        }

        studentInfo = output;

        MobileGraphics.instance.Loading(false);

        CheckForFreePeriods();
    }

    private void CheckForFreePeriods()
    {
        List<int> freePeriods = new List<int>();

        foreach (int id in studentInfo.classList)
        {
            if (Database.freePeriodIds.Contains(id))
            {
                freePeriods.Add(id);
            }
        }

        ClearSelectionContainer();

        if (freePeriods.Count == 0) //no free periods (womp womp)
        {
            noFreePeriodsGraphic.SetActive(true);
        }
        else //free period(s)!!!!
        {
            noFreePeriodsGraphic.SetActive(false);
            CreateFreePeriodSelections(freePeriods);
        }
    }

    private void CreateFreePeriodSelections(List<int> periods)
    {

        foreach (int period in periods)
        {
            GameObject newSelection = Instantiate(freePeriodSelectionPrefab, freePeriodParent.transform);

            newSelection.GetComponentInChildren<TMP_Text>().text = $"Period {period}";

            newSelection.GetComponent<Button>().onClick.AddListener(delegate { SelectedPeriod(period); });
        }
    }

    private void ClearSelectionContainer()
    {
        foreach (Transform t in freePeriodParent.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(freePeriodParent.transform)) continue;

            Destroy(t.gameObject);
        }
    }

    public void SelectedPeriod(int period)
    {
        Debug.Log($"Period {period}");
    }
}
