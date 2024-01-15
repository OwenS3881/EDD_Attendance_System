using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class StudentViewAttendanceManager : MonoBehaviour
{
    [Header("UI Initialize Fields")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject[] otherScreens;

    [Header("Day Attendance Fields")]
    [SerializeField] private DateButton dayAttendanceDate;
    [SerializeField] private GameObject classAttendanceViewPrefab;
    [SerializeField] private GameObject classAttendanceViewContentParent;
    private StudentInfoData studentInfo;
    private StudentAttendanceEntryData currentDayStudentAttendance;
    [SerializeField] private GameObject noClassesGraphic;

    private SchoolInfoData schoolData;

    private void Start()
    {
        mainScreen.SetActive(true);
        foreach (GameObject screen in otherScreens)
        {
            screen.SetActive(false);
        }

        noClassesGraphic.SetActive(true);

        GetData();
    }

    private void GetData()
    {
        MobileGraphics.instance.Loading(true);
        Database.instance.ReadData(Database.instance.GetUsername(), new Database.ReadDataCallback<StudentInfoData>(GetStudentInfo));
    }

    private void GetStudentInfo(StudentInfoData output)
    {
        if (output == null)
        {
            MobileGraphics.instance.Loading(false);
            Debug.LogWarning("Couldn't find StudentInfo");
            return;
        }

        studentInfo = output;

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

        MobileGraphics.instance.Loading(false);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MobileHome");
    }

    //Day Attendance
    public void SelectStudentDaySchedule()
    {
        Database.instance.ReadData(Database.instance.GetUsername() + "*" + dayAttendanceDate.CurrentDate, new Database.ReadDataCallback<StudentAttendanceEntryData>(SelectStudentDayScheduleCallbackAttendance));
        MobileGraphics.instance.Loading(true);
    }


    private void SelectStudentDayScheduleCallbackAttendance(StudentAttendanceEntryData output)
    {
        if (output == null)
        {
            currentDayStudentAttendance = new StudentAttendanceEntryData(Int32.Parse(Database.instance.GetUsername()), dayAttendanceDate.CurrentDate, new List<string>(), new List<string>());
        }
        else
        {
            currentDayStudentAttendance = output;
        }

        FoundStudent();
    }

    private void FoundStudent()
    {
        MobileGraphics.instance.Loading(false);

        foreach (Transform child in classAttendanceViewContentParent.transform)
        {
            Destroy(child.gameObject);
        }

        List<int> periods = GetPeriods(dayAttendanceDate.CurrentDate);

        foreach (int p in periods)
        {

            ClassAttendanceView viewBox = Instantiate(classAttendanceViewPrefab, classAttendanceViewContentParent.transform).GetComponent<ClassAttendanceView>();

            viewBox.SetTeacherText(studentInfo.classList[p - 1].ToString());
            AddTeacherNameToIDField(studentInfo.classList[p - 1], viewBox);

            if (!currentDayStudentAttendance.presentList.Contains(studentInfo.classList[p - 1].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Absent);
            }
            else if (currentDayStudentAttendance.tardyList.Contains(studentInfo.classList[p - 1].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Tardy);
            }
            else if (currentDayStudentAttendance.presentList.Contains(studentInfo.classList[p - 1].ToString()))
            {
                viewBox.SetStatus(AttendanceStatus.Present);
            }
        }

        noClassesGraphic.SetActive(periods.Count == 0);
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
}
