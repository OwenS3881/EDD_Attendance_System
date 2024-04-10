using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class StudentFreePeriodManager : MonoBehaviour
{
    private StudentInfoData studentInfo;
    private SchoolInfoData schoolData;
    private StudentAttendanceEntryData attendanceData;
    private List<int> todaySchedule = new List<int>();
    private List<int> freePeriods = new List<int>();

    [SerializeField] private GameObject freePeriodParent;
    [SerializeField] private GameObject freePeriodSelectionPrefab;
    [SerializeField] private GameObject noFreePeriodsGraphic;
    [SerializeField] private ParticleSystem confettiParticles;
    [SerializeField] private GameObject mainScrollView;
    [SerializeField] private GameObject passScrollView;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text periodText;
    [SerializeField] private GameObject checkInButton;
    [SerializeField] private GameObject checkOutButton;
    [SerializeField] private GameObject passIssuedGraphic;
    private int selectedPeriod;

    private string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

    private void Start()
    {
        mainScrollView.SetActive(true);
        passScrollView.SetActive(false);

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

        GetAttendanceData();
    }

    private void GetAttendanceData()
    {
        Database.instance.ReadData(studentInfo.studentId.ToString() + "*" + currentDate, new Database.ReadDataCallback<StudentAttendanceEntryData>(GetAttendanceDataCallback));
    }

    private void GetAttendanceDataCallback(StudentAttendanceEntryData output)
    {
        if (output == null)
        {
            attendanceData = new StudentAttendanceEntryData(Int32.Parse(Database.instance.GetUsername()), currentDate, new List<string>(), new List<string>());
        }
        else
        {
            attendanceData = output;
        }

        GetSchoolData();
    }

    private void GetSchoolData()
    {
        Database.instance.ReadData(studentInfo.schoolId.ToString(), new Database.ReadDataCallback<SchoolInfoData>(GetSchoolDataCallback));
    }

    private void GetSchoolDataCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            MobileGraphics.instance.Loading(false);
            Debug.LogWarning("Couldn't find school");
            return;
        }

        schoolData = output;

        todaySchedule = GetPeriods(currentDate);

        CheckForFreePeriods();

        MobileGraphics.instance.Loading(false);
    }

    private void CheckForFreePeriods()
    {
        foreach (int id in studentInfo.classList)
        {
            /*
             * Conditions:
             * -Is a valid free period
             * -That free period occurs on this day
             */
            if (Database.freePeriodIds.Contains(id) && todaySchedule.Contains(id))
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
        selectedPeriod = period;

        checkInButton.SetActive(Database.checkInIds.Contains(period) && !attendanceData.presentList.Contains(selectedPeriod.ToString()));
        checkOutButton.SetActive(Database.checkOutIds.Contains(period) && !attendanceData.presentList.Contains(selectedPeriod.ToString()));

        mainScrollView.SetActive(false);
        passScrollView.SetActive(true);

        dateText.text = DateButton.ConvertToNiceDate(currentDate);
        periodText.text = period.ToString();

        passIssuedGraphic.SetActive(attendanceData.presentList.Contains(selectedPeriod.ToString()));     
    }

    public void CheckIn()
    {
        MobileGraphics.instance.DisplayMessage("Enjoy your day!");
        MarkPass();
    }

    public void Checkout()
    {
        confettiParticles.Play();
        MobileGraphics.instance.DisplayMessage("Bye!");
        MarkPass();
    }

    public void PassPressed()
    {
        if (!Database.checkOutIds.Contains(Int32.Parse(periodText.text))) return;

        confettiParticles.Play();
    }

    private void MarkPass()
    {
        if (attendanceData.presentList.Contains(selectedPeriod.ToString())) return;

        attendanceData.presentList.Add(selectedPeriod.ToString());

        passIssuedGraphic.SetActive(true);
        checkInButton.SetActive(false);
        checkOutButton.SetActive(false);

        if (!studentInfo.attendanceObjects.Contains(attendanceData.fileName))
        {
            studentInfo.attendanceObjects.Add(attendanceData.fileName);
            Database.instance.SaveDataToFirebase(studentInfo);
        }

        Database.instance.SaveDataToFirebase(attendanceData);
    }

    private List<int> GetPeriods(string date)
    {
        //check for overrides
        foreach (ScheduledPeriods sp in schoolData.scheduleOverrides)
        {
            string[] splitDate = sp.date.Split("*");
            if (splitDate.Length == 1) //single date
            {
                if (date.Equals(splitDate[0]))
                {
                    return sp.periods;
                }
            }
            else if (splitDate.Length == 2) //date range
            {
                if (MyFunctions.IsDateInRange(date, splitDate[0], splitDate[1]))
                {
                    return sp.periods;
                }
            }
        }

        //check for block schedule
        string dayOfWeek = MyFunctions.GetDayOfWeek(date).ToLower();
        foreach (ScheduledPeriods sp in schoolData.blockSchedule)
        {
            if (sp.date.ToLower().Equals(dayOfWeek))
            {
                return sp.periods;
            }
        }

        return new List<int>();
    }
}
