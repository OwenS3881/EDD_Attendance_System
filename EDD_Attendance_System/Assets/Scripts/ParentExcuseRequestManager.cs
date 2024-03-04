using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ParentExcuseRequestManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown teacherIdDropdown;
    [SerializeField] private DateButton dateButton;
    [SerializeField] private TMP_InputField reasonInput;

    private void OnEnable()
    {
        LoadTeacherIds();
    }

    private void LoadTeacherIds()
    {
        if (ParentHomeManager.instance == null || ParentHomeManager.instance.StudentInfo == null) return;

        teacherIdDropdown.ClearOptions();

        List<string> optionsList = new List<string>();

        foreach (int id in ParentHomeManager.instance.StudentInfo.classList)
        {
            optionsList.Add(id.ToString());
        }

        teacherIdDropdown.AddOptions(optionsList);

        for (int i = 0; i < ParentHomeManager.instance.StudentInfo.classList.Length; i++)
        {
            AddTeacherNameToDropdown(ParentHomeManager.instance.StudentInfo.classList[i], i);
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
            DesktopGraphics.instance.DisplayMessage("Please select a date");
            return;
        }

        if (string.IsNullOrEmpty(reasonInput.text))
        {
            DesktopGraphics.instance.DisplayMessage("Please enter a reason");
            return;
        }

        DesktopGraphics.instance.Loading(true);
        Database.instance.ReadData(ParentHomeManager.instance.StudentInfo.studentId + "*" + dateButton.CurrentDate, new Database.ReadDataCallback<StudentAttendanceEntryData>(GetAttendanceCallback));
    }

    private void GetAttendanceCallback(StudentAttendanceEntryData output)
    {
        int selectedTeacher = Int32.Parse(teacherIdDropdown.options[teacherIdDropdown.value].text.Split(" - ")[0]);

        if (output != null)
        {
            if (output.presentList.Contains(selectedTeacher.ToString()) && !output.tardyList.Contains(selectedTeacher.ToString()))
            {
                DesktopGraphics.instance.DisplayMessage("Already present for this day");
                DesktopGraphics.instance.Loading(false);
                return;
            }
        }

        List<int> periods = GetPeriods(dateButton.CurrentDate);

        int period = teacherIdDropdown.value + 1;

        if (!periods.Contains(period))
        {
            DesktopGraphics.instance.DisplayMessage("Don't have that class on this day");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        if (ParentHomeManager.instance.SchoolData.excuseRequests == null) ParentHomeManager.instance.SchoolData.excuseRequests = new List<AttendanceExcuseRequest>();

        AttendanceExcuseRequest newRequest = new AttendanceExcuseRequest(ParentHomeManager.instance.StudentInfo.studentId, selectedTeacher, dateButton.CurrentDate, reasonInput.text, false);
        ParentHomeManager.instance.SchoolData.excuseRequests.Add(newRequest);
        Database.instance.SaveDataToFirebase(ParentHomeManager.instance.SchoolData);
        DesktopGraphics.instance.DisplayMessage("Success");
        DesktopGraphics.instance.Loading(false);
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
}
