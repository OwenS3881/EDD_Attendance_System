using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class AdminCreator : MonoBehaviour
{
    [Header("UI Initialize Fields")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject[] otherScreens;

    [Header("Teacher Creation Fields")]
    [SerializeField] private TMP_InputField teacherIdInput;
    [SerializeField] private TMP_InputField teacherNameInput;

    [Header("Student Creation methods")]
    [SerializeField] private TMP_InputField studentIdInput;
    [SerializeField] private TMP_InputField studentNameInput;
    [SerializeField] private TMP_InputField[] studentClassesInput;

    [Header("Present Fields")]
    [SerializeField] private TMP_InputField presentTeacherField;
    [SerializeField] private TMP_InputField presentStudentField;
    [SerializeField] private TMP_InputField presentDateField;

    [Header("Day Student Schedule Fields")]
    [SerializeField] private TMP_InputField dayScheduleStudentIdField;
    [SerializeField] private TMP_InputField dayScheduleDateField;
    [SerializeField] private StudentInfoData currentDayStudent;
    private StudentAttendanceEntryData currentDayStudentAttendance;
    private bool foundStudentInfo;
    private bool foundStudentAttendance;
    [SerializeField] private GameObject searchMenu;
    [SerializeField] private GameObject editMenu;
    [SerializeField] private TMP_Text editTitle;
    [SerializeField] private GameObject[] classAttendanceParents;

    [Header("Teacher Attendance Fields")]
    [SerializeField] private GameObject teacherSearchMenu;
    [SerializeField] private GameObject teacherEditMenu;
    [SerializeField] private TMP_InputField teacherAttendanceIdField;
    [SerializeField] private TMP_InputField teacherAttendanceDateField;
    [SerializeField] private TMP_InputField teacherAttendancePeriodField;
    [SerializeField] private TMP_Text teacherAttendanceTitle;
    [SerializeField] private GameObject studentsParentContainer;
    [SerializeField] private GameObject studentAttendanceCheckboxPrefab;
    private TeacherInfoData currentTeacherAttendance;
    private List<StudentAttendanceEntryData> teacherRosterAttendances;

    private void Start()
    {
        mainScreen.SetActive(true);
        foreach (GameObject screen in otherScreens)
        {
            screen.SetActive(false);
        }

        searchMenu.SetActive(true);
        editMenu.SetActive(false);

        teacherSearchMenu.SetActive(true);
        teacherEditMenu.SetActive(false);
    }

    //Teacher Creation Methods
    public void CreateTeacher()
    {
        TeacherInfoData newTeacher = new TeacherInfoData(Int32.Parse(teacherIdInput.text), teacherNameInput.text, new List<ListWrapper<string>>());
        Database.instance.SaveDataToFirebase(newTeacher);
    }

    //Student Creation Methods
    public void CreateStudent()
    {
        int[] teacherIds = new int[7];
        for (int i = 0; i < studentClassesInput.Length; i++)
        {
            if (string.IsNullOrEmpty(studentClassesInput[i].text))
            {
                Debug.LogError("Fill out all classes");
                return;
            }
            teacherIds[i] = Int32.Parse(studentClassesInput[i].text);
        }

        int studentId = Int32.Parse(studentIdInput.text);
        string studentName = studentNameInput.text;

        StudentInfoData newStudent = new StudentInfoData(studentId, studentName, teacherIds);
        Database.instance.SaveDataToFirebase(newStudent);

        StartCoroutine(AddStudentToRoster(studentId, teacherIds, 0));

        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest
        {
            Username = studentIdInput.text,
            Password = "123456",
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Registered and logged in!");
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    IEnumerator AddStudentToRoster(int studentId, int[] teacherIds, int index)
    {
        yield return new WaitForSeconds(0.1f);
        Database.instance.ReadData(teacherIds[index].ToString(), new Database.ReadDataCallbackParams<TeacherInfoData>(AddStudentToRosterCallback), new object[] { studentId, teacherIds, index });
    }

    private void AddStudentToRosterCallback(TeacherInfoData output, object[] additionalParams)
    {

        int studentId = (int)additionalParams[0];
        int[] teacherIds = (int[])additionalParams[1];
        int index = (int)additionalParams[2];

        TeacherInfoData updatedInfo;

        if (output == null)
        {
            Debug.LogError("No teacher found with id: " + teacherIds[index]);
            return;
        }
        else
        {
            updatedInfo = new TeacherInfoData(output);
        }

        updatedInfo.roster[index].Add(studentId.ToString());

        if (updatedInfo.roster[index][0] == "Default")
        {
            updatedInfo.roster[index].internalList.RemoveAt(0);
        }


        Database.instance.SaveDataToFirebase(updatedInfo);

        if (index < 6)
        {
            StartCoroutine(AddStudentToRoster(studentId, teacherIds, index + 1));
        }
    }

    //Present Methods
    public void ManuallyMarkPresent()
    {
        string date = presentDateField.text;

        if (string.IsNullOrEmpty(date))
        {
            date = DateTime.Today.ToString("yyyy-MM-dd");
        }

        MarkPresent(Int32.Parse(presentStudentField.text), Int32.Parse(presentTeacherField.text), date);
    }

    public static void MarkPresent(int studentId, int teacherId, string date)
    {
        Database.instance.ReadData(studentId + "*" + date, new Database.ReadDataCallbackParams<StudentAttendanceEntryData>(MarkPresentCallback), new object[] { studentId, teacherId, date });
    }

    private static void MarkPresentCallback(StudentAttendanceEntryData output, object[] additionalParams)
    {
        int studentId = (int)additionalParams[0];
        int teacherId = (int)additionalParams[1];
        string date = (string)additionalParams[2];

        StudentAttendanceEntryData updatedEntry;

        if (output == null)
        {
            updatedEntry = new StudentAttendanceEntryData(studentId, date, new List<string>());
        }
        else
        {
            updatedEntry = output;
        }

        if (!updatedEntry.presentList.Contains(teacherId.ToString())) updatedEntry.presentList.Add(teacherId.ToString());

        Database.instance.SaveDataToFirebase(updatedEntry);
    }

    //Day Student Schedule Methods
    public void SelectStudentDaySchedule()
    {
        Database.instance.ReadData(dayScheduleStudentIdField.text, new Database.ReadDataCallback<StudentInfoData>(SelectStudentDayScheduleCallbackInfo));
        Database.instance.ReadData(dayScheduleStudentIdField.text + "*" + dayScheduleDateField.text, new Database.ReadDataCallback<StudentAttendanceEntryData>(SelectStudentDayScheduleCallbackAttendance));
    }

    private void SelectStudentDayScheduleCallbackInfo(StudentInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find StudentInfo");
            return;
        }

        currentDayStudent = output;
        foundStudentInfo = true;
        FoundStudent();
    }

    private void SelectStudentDayScheduleCallbackAttendance(StudentAttendanceEntryData output)
    {
        if (output == null)
        {
            currentDayStudentAttendance = new StudentAttendanceEntryData(Int32.Parse(dayScheduleStudentIdField.text), dayScheduleDateField.text, new List<string>());
        }
        else
        {
            currentDayStudentAttendance = output;
        }


        foundStudentAttendance = true;
        FoundStudent();
    }

    private void FoundStudent()
    {
        if (!foundStudentAttendance || !foundStudentInfo) return;

        foundStudentAttendance = false;
        foundStudentInfo = false;

        searchMenu.SetActive(false);
        editMenu.SetActive(true);

        editTitle.text = $"{currentDayStudent.studentName}'s ({currentDayStudent.studentId}) Attendance for {currentDayStudentAttendance.date}";

        for (int i = 0; i < 7; i++)
        {
            classAttendanceParents[i].GetComponentInChildren<TMP_Text>().text = currentDayStudent.classList[i].ToString();
            AddTeacherNameToIDField(currentDayStudent.classList[i], i);
            classAttendanceParents[i].GetComponentInChildren<Toggle>().isOn = currentDayStudentAttendance.presentList.Contains(currentDayStudent.classList[i].ToString());
        }
    }

    private void AddTeacherNameToIDField(int teacherId, int index)
    {
        Database.instance.ReadData(teacherId.ToString(), new Database.ReadDataCallbackParams<TeacherInfoData>(AddTeacherNameToIDFieldCallback), new object[] { index });
    }

    private void AddTeacherNameToIDFieldCallback(TeacherInfoData output, object[] additionalParams)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find teacher");
            return;
        }

        int index = (int)additionalParams[0];

        classAttendanceParents[index].GetComponentInChildren<TMP_Text>().text = output.teacherId + " - " + output.teacherName;
    }

    public void UpdateDayAttendance()
    {
        currentDayStudentAttendance.presentList.Clear();

        for (int i = 0; i < 7; i++)
        {
            if (classAttendanceParents[i].GetComponentInChildren<Toggle>().isOn)
            {
                currentDayStudentAttendance.presentList.Add(currentDayStudent.classList[i].ToString());
            }
        }
        Database.instance.SaveDataToFirebase(currentDayStudentAttendance);
    }

    //Teacher Attendance Methods
    public void SelectTeacherAttendance()
    {
        currentTeacherAttendance = null;
        teacherRosterAttendances = new List<StudentAttendanceEntryData>();
        Database.instance.ReadData(teacherAttendanceIdField.text, new Database.ReadDataCallback<TeacherInfoData>(SelectTeacherAttendanceCallback));
    }

    private void SelectTeacherAttendanceCallback(TeacherInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("No teacher found");
            return;
        }

        currentTeacherAttendance = output;

        int period = Int32.Parse(teacherAttendancePeriodField.text);
        for (int i = 0; i < currentTeacherAttendance.roster[period - 1].internalList.Count; i++)
        {
            Database.instance.ReadData(currentTeacherAttendance.roster[period - 1].internalList[i] + "*" + teacherAttendanceDateField.text, new Database.ReadDataCallbackParams<StudentAttendanceEntryData>(SelectTeacherGetStudentCallback), new object[] { Int32.Parse(currentTeacherAttendance.roster[period - 1].internalList[i]) });
        }
    }

    private void SelectTeacherGetStudentCallback(StudentAttendanceEntryData output, object[] additionalParams)
    {
        int studentId = (int)additionalParams[0];

        StudentAttendanceEntryData student;

        if (output == null)
        {
            student = new StudentAttendanceEntryData(studentId, teacherAttendanceDateField.text, new List<string>());
        }
        else
        {
            student = output;
        }

        teacherRosterAttendances.Add(student);
        FoundStudents();
    }

    private void FoundStudents()
    {
        if (teacherRosterAttendances.Count < currentTeacherAttendance.roster[Int32.Parse(teacherAttendancePeriodField.text) - 1].internalList.Count)
        {
            return;
        }

        teacherRosterAttendances.Sort();

        teacherSearchMenu.SetActive(false);
        teacherEditMenu.SetActive(true);

        teacherAttendanceTitle.text = $"{currentTeacherAttendance.teacherName}'s ({currentTeacherAttendance.teacherId}) Roster Attendance for {teacherRosterAttendances[0].date}, Period {teacherAttendancePeriodField.text}";

        foreach (Transform t in studentsParentContainer.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(studentsParentContainer.transform)) continue;

            Destroy(t.gameObject);
        }

        for (int i = 0; i < teacherRosterAttendances.Count; i++)
        {
            GameObject presentBox = Instantiate(studentAttendanceCheckboxPrefab, studentsParentContainer.transform);

            TMP_Text field = presentBox.GetComponentInChildren<TMP_Text>();

            field.text = teacherRosterAttendances[i].studentId.ToString();

            AddStudentNameToIDField(teacherRosterAttendances[i].studentId, field);

            presentBox.GetComponentInChildren<Toggle>().isOn = teacherRosterAttendances[i].presentList.Contains(teacherAttendanceIdField.text);
        }
         
    }

    private void AddStudentNameToIDField(int studentId, TMP_Text field)
    {
        Database.instance.ReadData(studentId.ToString(), new Database.ReadDataCallbackParams<StudentInfoData>(AddStudentNameToIDFieldCallback), new object[] { field });
    }

    private void AddStudentNameToIDFieldCallback(StudentInfoData output, object[] additionalParams)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find teacher");
            return;
        }

        TMP_Text field = (TMP_Text)additionalParams[0];

        field.text = output.studentId + " - " + output.studentName;
    }

    public void UpdateTeacherAttendance()
    {
        List<GameObject> filteredPresentBoxes = new List<GameObject>();
        for (int i = 0; i < studentsParentContainer.transform.childCount; i++)
        {
            filteredPresentBoxes.Add(studentsParentContainer.transform.GetChild(i).gameObject);
        }

        filteredPresentBoxes.Sort(delegate (GameObject p1, GameObject p2)
        {
            return p1.GetComponentInChildren<TMP_Text>().text.CompareTo(p2.GetComponentInChildren<TMP_Text>().text);
        });

        string teacherId = teacherAttendanceIdField.text;

        for (int i = 0; i < teacherRosterAttendances.Count; i++)
        {
            if (filteredPresentBoxes[i].GetComponentInChildren<Toggle>().isOn)
            {
                if (!teacherRosterAttendances[i].presentList.Contains(teacherId))
                {
                    teacherRosterAttendances[i].presentList.Add(teacherId);
                    Database.instance.SaveDataToFirebase(teacherRosterAttendances[i]);
                }
            }
            else
            {
                if (teacherRosterAttendances[i].presentList.Contains(teacherId))
                {
                    teacherRosterAttendances[i].presentList.Remove(teacherId);
                    Database.instance.SaveDataToFirebase(teacherRosterAttendances[i]);
                }
            }
        }
    }
}