using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class ParentAttendanceManager : MonoBehaviour
{
    [Header("UI Initialize Fields")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject[] otherScreens;

    [Header("Day Attendance Fields")]
    [SerializeField] private DateButton dayAttendanceDate;
    [SerializeField] private GameObject classAttendanceViewPrefab;
    [SerializeField] private GameObject classAttendanceViewContentParent;
    private StudentAttendanceEntryData currentDayStudentAttendance;
    [SerializeField] private GameObject noClassesGraphic;

    [Header("Date Range Attendance Fields")]
    [SerializeField] private GameObject dateRangeContentParent;
    [SerializeField] private float dateRangeContentDefaultHeight;
    [SerializeField] private float dateRangeContainerHeight;
    [SerializeField] private GameObject dateRangeAttendanceContainerPrefab;
    [SerializeField] private DateButton startDate;
    [SerializeField] private DateButton endDate;
    private List<string> dateRanges = new List<string>();
    private List<StudentAttendanceEntryData> attendanceRangeDatas = new List<StudentAttendanceEntryData>();

    [Header("Absent Range Fields")]
    [SerializeField] private GameObject absentDateRangeContentParent;
    [SerializeField] private float absentDateRangeContentDefaultHeight;
    [SerializeField] private float absentDateRangeContainerHeight;
    [SerializeField] private GameObject absentDateRangeAttendanceContainerPrefab;
    [SerializeField] private DateButton absentStartDate;
    [SerializeField] private DateButton absentEndDate;
    private List<string> absentDateRanges = new List<string>();
    private List<StudentAttendanceEntryData> absentAttendanceRangeDatas = new List<StudentAttendanceEntryData>();
    private int absentCount;
    private int tardyCount;
    private int totalCount;
    [SerializeField] private TMP_Text absentCountText;
    [SerializeField] private TMP_Text tardyCountText;

    private void Start()
    {
        mainScreen.SetActive(true);
        foreach (GameObject screen in otherScreens)
        {
            screen.SetActive(false);
        }

        ClearDayAttendanceContainer();
        ClearRangesContainer();
        ClearAbsentRangesContainer();

        absentCountText.gameObject.SetActive(false);
        tardyCountText.gameObject.SetActive(false);

        noClassesGraphic.SetActive(true);
    }

    public void SelectStudentDaySchedule()
    {
        Database.instance.ReadData(ParentHomeManager.instance.StudentInfo.studentId.ToString() + "*" + dayAttendanceDate.CurrentDate, new Database.ReadDataCallback<StudentAttendanceEntryData>(SelectStudentDayScheduleCallbackAttendance));
        DesktopGraphics.instance.Loading(true);
    }


    private void SelectStudentDayScheduleCallbackAttendance(StudentAttendanceEntryData output)
    {
        if (output == null)
        {
            currentDayStudentAttendance = new StudentAttendanceEntryData(ParentHomeManager.instance.StudentInfo.studentId, dayAttendanceDate.CurrentDate, new List<string>(), new List<string>());
        }
        else
        {
            currentDayStudentAttendance = output;
        }

        FoundStudent();
    }

    private void ClearDayAttendanceContainer()
    {
        foreach (Transform child in classAttendanceViewContentParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void FoundStudent()
    {
        DesktopGraphics.instance.Loading(false);

        ClearDayAttendanceContainer();

        List<int> periods = GetPeriods(dayAttendanceDate.CurrentDate);

        foreach (int p in periods)
        {

            ClassAttendanceView viewBox = Instantiate(classAttendanceViewPrefab, classAttendanceViewContentParent.transform).GetComponent<ClassAttendanceView>();

            viewBox.SetTeacherText(ParentHomeManager.instance.StudentInfo.classList[p - 1].ToString());
            AddTeacherNameToIDField(ParentHomeManager.instance.StudentInfo.classList[p - 1], viewBox);

            if (!currentDayStudentAttendance.presentList.Contains(ParentHomeManager.instance.StudentInfo.classList[p - 1].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Absent);
            }
            else if (currentDayStudentAttendance.tardyList.Contains(ParentHomeManager.instance.StudentInfo.classList[p - 1].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Tardy);
            }
            else if (currentDayStudentAttendance.presentList.Contains(ParentHomeManager.instance.StudentInfo.classList[p - 1].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Present);
            }
        }

        noClassesGraphic.SetActive(periods.Count == 0);
    }

    private List<int> GetPeriods(string date)
    {
        //check for overrides
        foreach (ScheduledPeriods sp in ParentHomeManager.instance.SchoolData.scheduleOverrides)
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
        foreach (ScheduledPeriods sp in ParentHomeManager.instance.SchoolData.blockSchedule)
        {
            if (sp.date.ToLower().Equals(dayOfWeek))
            {
                return sp.periods;
            }
        }

        return new List<int>();
    }

    private void AddTeacherNameToIDField(int teacherId, ClassAttendanceView viewBox)
    {
        Database.instance.ReadData(teacherId.ToString(), new Database.ReadDataCallbackParams<TeacherInfoData>(AddTeacherNameToIDFieldCallback), new object[] { viewBox });
    }

    private void AddTeacherNameToIDFieldCallback(TeacherInfoData output, object[] additionalParams)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find teacher");
            return;
        }

        ClassAttendanceView viewBox = (ClassAttendanceView)additionalParams[0];

        viewBox.SetTeacherText(output.teacherId + " - " + output.teacherName);
    }

    //Date Range Attendance
    public string GetDateRange()
    {
        if (string.IsNullOrEmpty(startDate.CurrentDate) || string.IsNullOrEmpty(endDate.CurrentDate))
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

    public void OnDateSet()
    {
        if (!string.IsNullOrEmpty(startDate.CurrentDate) && !string.IsNullOrEmpty(endDate.CurrentDate) && !MyFunctions.VerifyDateOrder(startDate.CurrentDate, endDate.CurrentDate))
        {
            DesktopGraphics.instance.DisplayMessage("Invalid date range, 1st date must come before 2nd date");
            endDate.CurrentDate = startDate.CurrentDate;
        }
    }

    private void ClearRangesContainer()
    {
        foreach (Transform t in dateRangeContentParent.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(dateRangeContentParent.transform)) continue;

            Destroy(t.gameObject);
        }

        attendanceRangeDatas.Clear();

        UpdateDateRangeContentParentHeight();
    }

    private void UpdateDateRangeContentParentHeight()
    {
        dateRangeContentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(dateRangeContentParent.GetComponent<RectTransform>().sizeDelta.x, dateRangeContentDefaultHeight + (attendanceRangeDatas.Count * dateRangeContainerHeight));
    }

    public void LoadDateRange()
    {
        if (string.IsNullOrEmpty(GetDateRange())) return;

        DesktopGraphics.instance.Loading(true);

        dateRanges = MyFunctions.GetDateRange(startDate.CurrentDate, endDate.CurrentDate);

        ClearRangesContainer();

        foreach (string date in dateRanges)
        {
            GetDayAttendance(date);
        }
    }

    public void GetDayAttendance(string date)
    {
        Database.instance.ReadData(ParentHomeManager.instance.StudentInfo.studentId + "*" + date, new Database.ReadDataCallbackParams<StudentAttendanceEntryData>(GetDayAttendanceCallback), new object[] { date });
    }

    private void GetDayAttendanceCallback(StudentAttendanceEntryData output, object[] additionalParams)
    {
        string date = (string)additionalParams[0];
        StudentAttendanceEntryData newData = null;

        if (output == null)
        {
            newData = new StudentAttendanceEntryData(ParentHomeManager.instance.StudentInfo.studentId, date, new List<string>(), new List<string>());
        }
        else
        {
            newData = output;
        }

        attendanceRangeDatas.Add(newData);

        LoadedAllDates();
    }

    private void LoadedAllDates()
    {
        if (attendanceRangeDatas.Count < dateRanges.Count) return;

        attendanceRangeDatas.Sort(delegate (StudentAttendanceEntryData s1, StudentAttendanceEntryData s2)
        {
            return s1.date.CompareTo(s2.date);
        });

        foreach (StudentAttendanceEntryData data in attendanceRangeDatas)
        {
            DateRangeAttendanceContainer newContainer = Instantiate(dateRangeAttendanceContainerPrefab, dateRangeContentParent.transform).GetComponent<DateRangeAttendanceContainer>();

            newContainer.AssignDate(data.date);

            List<int> periods = GetPeriods(data.date);
            int present = data.presentList.Count;
            int tardy = data.tardyList.Count;

            newContainer.SetPresentBar(present - tardy, periods.Count);
            newContainer.SetTardyBar(present, periods.Count);
        }

        UpdateDateRangeContentParentHeight();

        DesktopGraphics.instance.Loading(false);
    }

    //Absent Range Attendance
    public string GetAbsentDateRange()
    {
        if (string.IsNullOrEmpty(absentStartDate.CurrentDate) || string.IsNullOrEmpty(absentEndDate.CurrentDate))
        {
            DesktopGraphics.instance.DisplayMessage("Fill out all dates");
            return null;
        }

        if (!MyFunctions.VerifyDateOrder(absentStartDate.CurrentDate, absentEndDate.CurrentDate))
        {
            DesktopGraphics.instance.DisplayMessage("Invalid date range, 1st date must come before 2nd date");
            return null;
        }

        return absentStartDate.CurrentDate + "*" + absentEndDate.CurrentDate;
    }

    public void OnAbsentDateSet()
    {
        if (!string.IsNullOrEmpty(absentStartDate.CurrentDate) && !string.IsNullOrEmpty(absentEndDate.CurrentDate) && !MyFunctions.VerifyDateOrder(absentStartDate.CurrentDate, absentEndDate.CurrentDate))
        {
            DesktopGraphics.instance.DisplayMessage("Invalid date range, 1st date must come before 2nd date");
            absentEndDate.CurrentDate = absentStartDate.CurrentDate;
        }
    }


    private void ClearAbsentRangesContainer()
    {
        foreach (Transform t in absentDateRangeContentParent.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(absentDateRangeContentParent.transform)) continue;

            Destroy(t.gameObject);
        }

        absentAttendanceRangeDatas.Clear();

        UpdateAbsentDateRangeContentParentHeight();
    }

    private void UpdateAbsentDateRangeContentParentHeight()
    {
        absentDateRangeContentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(absentDateRangeContentParent.GetComponent<RectTransform>().sizeDelta.x, absentDateRangeContentDefaultHeight + ((absentCount + tardyCount) * absentDateRangeContainerHeight));
    }


    public void LoadAbsentDateRange()
    {
        if (string.IsNullOrEmpty(GetAbsentDateRange())) return;

        absentCount = 0;
        tardyCount = 0;
        totalCount = 0;

        absentCountText.gameObject.SetActive(false);
        tardyCountText.gameObject.SetActive(false);

        DesktopGraphics.instance.Loading(true);

        absentDateRanges = MyFunctions.GetDateRange(absentStartDate.CurrentDate, absentEndDate.CurrentDate);

        ClearAbsentRangesContainer();

        foreach (string date in absentDateRanges)
        {
            GetAbsentDayAttendance(date);
        }
    }

    public void GetAbsentDayAttendance(string date)
    {
        Database.instance.ReadData(ParentHomeManager.instance.StudentInfo.studentId + "*" + date, new Database.ReadDataCallbackParams<StudentAttendanceEntryData>(GetAbsentDayAttendanceCallback), new object[] { date });
    }

    private void GetAbsentDayAttendanceCallback(StudentAttendanceEntryData output, object[] additionalParams)
    {
        string date = (string)additionalParams[0];
        StudentAttendanceEntryData newData = null;

        if (output == null)
        {
            newData = new StudentAttendanceEntryData(ParentHomeManager.instance.StudentInfo.studentId, date, new List<string>(), new List<string>());
        }
        else
        {
            newData = output;
        }

        absentAttendanceRangeDatas.Add(newData);

        LoadedAllAbsentDates();
    }

    private void LoadedAllAbsentDates()
    {
        if (absentAttendanceRangeDatas.Count < absentDateRanges.Count) return;

        absentAttendanceRangeDatas.Sort(delegate (StudentAttendanceEntryData s1, StudentAttendanceEntryData s2)
        {
            return s1.date.CompareTo(s2.date);
        });

        foreach (StudentAttendanceEntryData data in absentAttendanceRangeDatas)
        {
            List<int> periods = GetPeriods(data.date);

            for (int i = 0; i < ParentHomeManager.instance.StudentInfo.classList.Length; i++)
            {
                if (!periods.Contains(i + 1)) continue; //dont have class on that day

                totalCount++;


                if (!data.presentList.Contains(ParentHomeManager.instance.StudentInfo.classList[i].ToString())) //absent
                {
                    absentCount++;

                    ClassAttendanceView newContainer = Instantiate(absentDateRangeAttendanceContainerPrefab, absentDateRangeContentParent.transform).GetComponent<ClassAttendanceView>();
                    newContainer.SetStatus(AttendanceStatus.Absent);
                    newContainer.SetTeacherText($"{ParentHomeManager.instance.StudentInfo.classList[i]} - {DateButton.ConvertToNiceDate(data.date)}");
                }
                else if (data.tardyList.Contains(ParentHomeManager.instance.StudentInfo.classList[i].ToString())) // tardy
                {
                    tardyCount++;

                    ClassAttendanceView newContainer = Instantiate(absentDateRangeAttendanceContainerPrefab, absentDateRangeContentParent.transform).GetComponent<ClassAttendanceView>();
                    newContainer.SetStatus(AttendanceStatus.Tardy);
                    newContainer.SetTeacherText($"{ParentHomeManager.instance.StudentInfo.classList[i]} - {DateButton.ConvertToNiceDate(data.date)}");
                }
            }
        }

        UpdateAbsentDateRangeContentParentHeight();

        absentCountText.text = $"Absent: {absentCount}/{totalCount}";
        tardyCountText.text = $"Tardy: {tardyCount}/{totalCount}";

        absentCountText.gameObject.SetActive(true);
        tardyCountText.gameObject.SetActive(true);

        DesktopGraphics.instance.Loading(false);
    }
}
