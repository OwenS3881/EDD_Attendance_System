using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AdminExcuseRequestManager : MonoBehaviour
{
    public static AdminExcuseRequestManager instance { get; private set; }

    [SerializeField] private GameObject mainScrollView;
    [SerializeField] private GameObject decisionScrollView;

    [Header("Main View")]
    [SerializeField] private GameObject pendingExcuseRequestsContainer;
    [SerializeField] private GameObject pendingExcuseRequestPrefab;
    [SerializeField] private float distanceBetweenRequests;
    [SerializeField] private float defaultContainerHeight;

    [Header("Decision View")]
    [SerializeField] private TMP_Text studentField;
    [SerializeField] private TMP_Text teacherField;
    [SerializeField] private TMP_Text dateField;
    [SerializeField] private TMP_Text reasonField;
    [SerializeField] private GameObject teacherDenyDisplay;
    private AttendanceExcuseRequest currentRequest;

    [SerializeField] private GameObject decisionContentParent;
    [SerializeField] private GameObject noImageContent;
    [SerializeField] private GameObject imageContent;
    [SerializeField] private RawImage excuseImage;
    [SerializeField] private AspectRatioFitter excuseImageFitter;
    [SerializeField] private float noImageContentHeight;
    [SerializeField] private float imageContentHeight;

    private List<PendingExcuseRequest> requestObjects = new List<PendingExcuseRequest>();

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

    private void OnEnable()
    {
        mainScrollView.SetActive(true);
        decisionScrollView.SetActive(false);
        currentRequest = null;
        DisplayRequests();
    }

    public void RequestSelected(AttendanceExcuseRequest selectedRequest)
    {
        mainScrollView.SetActive(false);
        decisionScrollView.SetActive(true);

        studentField.text = selectedRequest.studentId.ToString();
        teacherField.text = selectedRequest.teacherId.ToString();
        dateField.text = DateButton.ConvertToNiceDate(selectedRequest.date);
        reasonField.text = selectedRequest.reason;
        teacherDenyDisplay.SetActive(selectedRequest.teacherDenied);

        AddStudentName(selectedRequest.studentId, studentField);
        AddTeacherName(selectedRequest.teacherId, teacherField);

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

    private void AddTeacherName(int teacherId, TMP_Text field)
    {
        Database.instance.ReadData(teacherId.ToString(), new Database.ReadDataCallbackParams<TeacherInfoData>(AddTeacherNameCallback), new object[] { field });
    }

    private void AddTeacherNameCallback(TeacherInfoData output, object[] additionalParams)
    {
        if (output == null)
        {
            Debug.LogWarning("Couldn't find teacher");
            return;
        }

        TMP_Text field = (TMP_Text)additionalParams[0];

        field.text = output.teacherId + " - " + output.teacherName;
    }

    private void ClearPendingContainer()
    {
        foreach (Transform t in pendingExcuseRequestsContainer.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(pendingExcuseRequestsContainer.transform)) continue;

            Destroy(t.gameObject);
        }

        requestObjects = new List<PendingExcuseRequest>();

        AdjustPendingContainer();
    }

    private void AdjustPendingContainer()
    {
        pendingExcuseRequestsContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(pendingExcuseRequestsContainer.GetComponent<RectTransform>().sizeDelta.x, (distanceBetweenRequests * requestObjects.Count) + defaultContainerHeight);
    }

    public void DisplayRequests()
    {
        if (AdminHomeManager.instance == null || AdminHomeManager.instance.currentData == null) return;

        ClearPendingContainer();

        for (int i = 0; i < AdminHomeManager.instance.currentData.excuseRequests.Count; i++)
        {
            PendingExcuseRequest newRequest = Instantiate(pendingExcuseRequestPrefab, pendingExcuseRequestsContainer.transform).GetComponent<PendingExcuseRequest>();

            newRequest.AssignRequest(AdminHomeManager.instance.currentData.excuseRequests[i]);

            requestObjects.Add(newRequest);
        }

        AdjustPendingContainer();
    }

    public void Approve()
    {
        AdminCreator.MarkPresent(currentRequest.studentId, currentRequest.teacherId, currentRequest.date, false);

        if (imageContent.activeSelf)
        {
            Database.instance.DeleteImage(currentRequest.imageName, currentRequest.imageToken);
        }

        AdminHomeManager.instance.currentData.excuseRequests.Remove(currentRequest);
        Database.instance.SaveDataToFirebase(AdminHomeManager.instance.currentData);

        excuseImage.texture = null;

        OnEnable();
    }

    public void Defer()
    {
        excuseImage.texture = null;

        OnEnable();
    }

    public void Deny()
    {
        AdminHomeManager.instance.currentData.excuseRequests.Remove(currentRequest);
        Database.instance.SaveDataToFirebase(AdminHomeManager.instance.currentData);

        if (imageContent.activeSelf)
        {
            Database.instance.DeleteImage(currentRequest.imageName, currentRequest.imageToken);
        }

        excuseImage.texture = null;

        OnEnable();
    }
}
