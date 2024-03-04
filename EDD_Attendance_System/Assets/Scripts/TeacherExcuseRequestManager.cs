using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TeacherExcuseRequestManager : MonoBehaviour
{
    public static TeacherExcuseRequestManager instance { get; private set; }

    private SchoolInfoData schoolData;

    [SerializeField] private GameObject mainScrollView;
    [SerializeField] private GameObject decisionScrollView;

    [Header("Main View")]
    [SerializeField] private GameObject pendingExcuseRequestsContainer;
    [SerializeField] private GameObject pendingExcuseRequestPrefab;
    [SerializeField] private float distanceBetweenRequests;
    [SerializeField] private float defaultContainerHeight;

    [Header("Decision View")]
    [SerializeField] private TMP_Text studentField;
    [SerializeField] private TMP_Text dateField;
    [SerializeField] private TMP_Text reasonField;
    private AttendanceExcuseRequest currentRequest;

    private List<PendingTeacherExcuseRequest> requestObjects = new List<PendingTeacherExcuseRequest>();

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

    public void GetSchoolData()
    {
        if (TeacherHomeManager.instance == null || TeacherHomeManager.instance.GetSchoolId() == 0) return;

        Database.instance.ReadData(TeacherHomeManager.instance.GetSchoolId().ToString(), new Database.ReadDataCallback<SchoolInfoData>(GetSchoolDataCallback));
        DesktopGraphics.instance.Loading(true);
    }

    private void GetSchoolDataCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find school");
            return;
        }

        schoolData = output;

        DisplayRequests();

        DesktopGraphics.instance.Loading(false);
    }

    private void OnEnable()
    {
        mainScrollView.SetActive(true);
        decisionScrollView.SetActive(false);
        currentRequest = null;
        if (schoolData == null) GetSchoolData();
        DisplayRequests();
    }

    public void RequestSelected(AttendanceExcuseRequest selectedRequest)
    {
        mainScrollView.SetActive(false);
        decisionScrollView.SetActive(true);

        studentField.text = selectedRequest.studentId.ToString();
        dateField.text = DateButton.ConvertToNiceDate(selectedRequest.date);
        reasonField.text = selectedRequest.reason;

        AddStudentName(selectedRequest.studentId, studentField);

        currentRequest = selectedRequest;
    }

    private void AddStudentName(int studentId, TMP_Text field)
    {
        Database.instance.ReadData(studentId.ToString(), new Database.ReadDataCallbackParams<StudentInfoData>(AddStudentNameCallback), new object[] { field });
    }

    private void AddStudentNameCallback(StudentInfoData output, object[] additionalParams)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find student");
            return;
        }

        TMP_Text field = (TMP_Text)additionalParams[0];

        field.text = output.studentId + " - " + output.studentName;
    }

    private void ClearPendingContainer()
    {
        foreach (Transform t in pendingExcuseRequestsContainer.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(pendingExcuseRequestsContainer.transform)) continue;

            Destroy(t.gameObject);
        }

        requestObjects = new List<PendingTeacherExcuseRequest>();

        AdjustPendingContainer();
    }

    private void AdjustPendingContainer()
    {
        pendingExcuseRequestsContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(pendingExcuseRequestsContainer.GetComponent<RectTransform>().sizeDelta.x, (distanceBetweenRequests * requestObjects.Count) + defaultContainerHeight);
    }

    public void DisplayRequests()
    {
        if (schoolData == null) return;

        ClearPendingContainer();

        for (int i = 0; i < schoolData.excuseRequests.Count; i++)
        {
            if (!schoolData.excuseRequests[i].teacherId.ToString().Equals(Database.instance.GetUsername())) continue;

            if (schoolData.excuseRequests[i].teacherDenied) continue;

            PendingTeacherExcuseRequest newRequest = Instantiate(pendingExcuseRequestPrefab, pendingExcuseRequestsContainer.transform).GetComponent<PendingTeacherExcuseRequest>();

            newRequest.AssignRequest(schoolData.excuseRequests[i]);

            requestObjects.Add(newRequest);
        }

        AdjustPendingContainer();
    }

    public void Approve()
    {
        AdminCreator.MarkPresent(currentRequest.studentId, currentRequest.teacherId, currentRequest.date, false);

        schoolData.excuseRequests.Remove(currentRequest);
        Database.instance.SaveDataToFirebase(schoolData);

        OnEnable();
    }

    public void Defer()
    {
        OnEnable();
    }

    public void Deny()
    {
        schoolData.excuseRequests[schoolData.excuseRequests.IndexOf(currentRequest)].teacherDenied = true;
        Database.instance.SaveDataToFirebase(schoolData);

        OnEnable();
    }
}
