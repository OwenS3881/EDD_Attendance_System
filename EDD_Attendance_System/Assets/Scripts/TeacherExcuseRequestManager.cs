using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    [SerializeField] private GameObject decisionContentParent;
    [SerializeField] private GameObject noImageContent;
    [SerializeField] private GameObject imageContent;
    [SerializeField] private RawImage excuseImage;
    [SerializeField] private AspectRatioFitter excuseImageFitter;
    [SerializeField] private float noImageContentHeight;
    [SerializeField] private float imageContentHeight;

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
        if (TeacherHomeManager.instance == null || TeacherHomeManager.instance.GetSchoolId().Equals("0")) return;

        Database.instance.ReadData(TeacherHomeManager.instance.GetSchoolId(), new Database.ReadDataCallback<SchoolInfoData>(GetSchoolDataCallback));
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

        studentField.text = selectedRequest.studentId;
        dateField.text = DateButton.ConvertToNiceDate(selectedRequest.date);
        reasonField.text = selectedRequest.reason;

        AddStudentName(selectedRequest.studentId, studentField);

        currentRequest = selectedRequest;

        if (!string.IsNullOrEmpty(currentRequest.imageName) && !string.IsNullOrEmpty(currentRequest.imageToken))
        {
            Database.instance.LoadImage(currentRequest.imageName, currentRequest.imageToken, new Database.LoadImageCallback(LoadImageCallback));

            imageContent.SetActive(true);
            noImageContent.SetActive(false);
            decisionContentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(decisionContentParent.GetComponent<RectTransform>().sizeDelta.x, imageContentHeight);
        }
        else
        {
            imageContent.SetActive(false);
            noImageContent.SetActive(true);
            decisionContentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(decisionContentParent.GetComponent<RectTransform>().sizeDelta.x, noImageContentHeight);
        }
    }

    private void LoadImageCallback(Texture2D downloadedTexture)
    {
        excuseImage.texture = downloadedTexture;
        excuseImageFitter.aspectRatio = (float)downloadedTexture.width / (float)downloadedTexture.height;
    }

    private void AddStudentName(string studentId, TMP_Text field)
    {
        Database.instance.ReadData(studentId, new Database.ReadDataCallbackParams<StudentInfoData>(AddStudentNameCallback), new object[] { field });
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
            if (!schoolData.excuseRequests[i].teacherId.Equals(Database.instance.GetUsername())) continue;

            if (schoolData.excuseRequests[i].teacherDenied) continue;

            PendingTeacherExcuseRequest newRequest = Instantiate(pendingExcuseRequestPrefab, pendingExcuseRequestsContainer.transform).GetComponent<PendingTeacherExcuseRequest>();

            newRequest.AssignRequest(schoolData.excuseRequests[i]);

            requestObjects.Add(newRequest);
        }

        AdjustPendingContainer();
    }

    public void Approve()
    {
        DesktopGraphics.instance.Loading(true);
        Database.instance.ReadData(currentRequest.studentId, new Database.ReadDataCallback<StudentInfoData>(ApproveCallback));
    }

    private void ApproveCallback(StudentInfoData output)
    {
        if (output == null)
        {
            DesktopGraphics.instance.DisplayMessage("An error has occurred");
            return;
        }

        Database.instance.ReadData(currentRequest.studentId + "*" + currentRequest.date, new Database.ReadDataCallbackParams<StudentAttendanceEntryData>(MarkPresentCallback), new object[] { currentRequest.studentId, currentRequest.teacherId, currentRequest.date, false, output });
    }

    private void MarkPresentCallback(StudentAttendanceEntryData output, object[] additionalParams)
    {
        string studentId = (string)additionalParams[0];
        string teacherId = (string)additionalParams[1];
        string date = (string)additionalParams[2];
        bool tardy = (bool)additionalParams[3];
        StudentInfoData studentInfo = (StudentInfoData)additionalParams[4];

        StudentAttendanceEntryData updatedEntry;

        if (output == null)
        {
            updatedEntry = new StudentAttendanceEntryData(studentId, date, new List<string>(), new List<string>());
        }
        else
        {
            updatedEntry = output;
        }

        //presentList
        if (!updatedEntry.presentList.Contains(teacherId)) updatedEntry.presentList.Add(teacherId);

        //tardyList
        if (tardy)
        {
            if (!updatedEntry.tardyList.Contains(teacherId)) updatedEntry.tardyList.Add(teacherId);
        }
        else
        {
            if (updatedEntry.tardyList.Contains(teacherId)) updatedEntry.tardyList.Remove(teacherId);
        }

        if (!studentInfo.attendanceObjects.Contains(updatedEntry.fileName))
        {
            studentInfo.attendanceObjects.Add(updatedEntry.fileName);
            Database.instance.SaveDataToFirebase(studentInfo);
        }

        Database.instance.SaveDataToFirebase(updatedEntry);

        if (imageContent.activeSelf)
        {
            Database.instance.DeleteImage(currentRequest.imageName, currentRequest.imageToken);
        }

        schoolData.excuseRequests.Remove(currentRequest);
        Database.instance.SaveDataToFirebase(schoolData);

        excuseImage.texture = null;

        DesktopGraphics.instance.Loading(false);
        OnEnable();
    }

    public void Defer()
    {
        excuseImage.texture = null;

        OnEnable();
    }

    public void Deny()
    {
        schoolData.excuseRequests[schoolData.excuseRequests.IndexOf(currentRequest)].teacherDenied = true;
        Database.instance.SaveDataToFirebase(schoolData);

        excuseImage.texture = null;

        OnEnable();
    }
}
