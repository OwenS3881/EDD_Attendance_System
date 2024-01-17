using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class StudentExcuseRequestManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown teacherIdDropdown;
    [SerializeField] private DateButton dateButton;
    [SerializeField] private TMP_InputField reasonInput;

    private StudentInfoData studentInfo;
    private SchoolInfoData schoolInfo;

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

        LoadTeacherIds();

        Database.instance.ReadData(studentInfo.schoolId.ToString(), new Database.ReadDataCallback<SchoolInfoData>(GetSchoolDataCallback));
    }

    private void GetSchoolDataCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            MobileGraphics.instance.Loading(false);
            MobileGraphics.instance.DisplayMessage("An error has occurred");
            Debug.LogWarning("Couldn't find SchoolInfo");
            return;
        }

        schoolInfo = output;

        MobileGraphics.instance.Loading(false);
    }

    private void LoadTeacherIds()
    {
        if (studentInfo == null) return;

        teacherIdDropdown.ClearOptions();

        List<string> optionsList = new List<string>();

        foreach (int id in studentInfo.classList)
        {
            optionsList.Add(id.ToString());
        }

        teacherIdDropdown.AddOptions(optionsList);

        for (int i = 0; i < studentInfo.classList.Length; i++)
        {
            AddTeacherNameToDropdown(studentInfo.classList[i], i);
        }
    }

    private void AddTeacherNameToDropdown(int teacherId, int dropdownListIndex)
    {
        Database.instance.ReadData(teacherId.ToString(), new Database.ReadDataCallbackParams<TeacherInfoData>(AddTeacherNameToDropdownCallback), new object[] { dropdownListIndex });
    }

    private void AddTeacherNameToDropdownCallback(TeacherInfoData output, object[] additionalParams)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find teacher");
            return;
        }

        int dropdownListIndex = (int)additionalParams[0];

        teacherIdDropdown.options[dropdownListIndex].text = output.teacherId + " - " + output.teacherName;

        if (dropdownListIndex == 0)
        {
            teacherIdDropdown.RefreshShownValue();
        }
    }

    public void SubmitRequest()
    {
        if (string.IsNullOrEmpty(dateButton.CurrentDate))
        {
            MobileGraphics.instance.DisplayMessage("Please select a date");
            return;
        }

        if (string.IsNullOrEmpty(reasonInput.text))
        {
            MobileGraphics.instance.DisplayMessage("Please enter a reason");
            return;
        }

        MobileGraphics.instance.Loading(true);
        Database.instance.ReadData(Database.instance.GetUsername() + "*" + dateButton.CurrentDate, new Database.ReadDataCallback<StudentAttendanceEntryData>(GetAttendanceCallback));
    }

    private void GetAttendanceCallback(StudentAttendanceEntryData output)
    {
        int selectedTeacher = Int32.Parse(teacherIdDropdown.options[teacherIdDropdown.value].text.Split(" - ")[0]);

        if (output != null)
        {
            if (output.presentList.Contains(selectedTeacher.ToString()))
            {
                MobileGraphics.instance.DisplayMessage("Already present for this day");
                MobileGraphics.instance.Loading(false);
                return;
            }
        }

        List<int> periods = GetPeriods(dateButton.CurrentDate);

        int period = teacherIdDropdown.value + 1;

        if (!periods.Contains(period))
        {
            MobileGraphics.instance.DisplayMessage("Don't have that class on this day");
            MobileGraphics.instance.Loading(false);
            return;
        }

        if (schoolInfo.excuseRequests == null) schoolInfo.excuseRequests = new List<AttendanceExcuseRequest>();

        AttendanceExcuseRequest newRequest = new AttendanceExcuseRequest(Int32.Parse(Database.instance.GetUsername()), selectedTeacher, dateButton.CurrentDate, reasonInput.text, false);
        schoolInfo.excuseRequests.Add(newRequest);
        Database.instance.SaveDataToFirebase(schoolInfo);
        MobileGraphics.instance.DisplayMessage("Success");
        MobileGraphics.instance.Loading(false);
    }

    private List<int> GetPeriods(string date)
    {
        //check for overrides
        foreach (ScheduledPeriods sp in schoolInfo.scheduleOverrides)
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
        foreach (ScheduledPeriods sp in schoolInfo.blockSchedule)
        {
            if (sp.date.ToLower().Equals(dayOfWeek))
            {
                return sp.periods;
            }
        }

        return new List<int>();
    }
}
