using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class ParentExcuseRequestManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown teacherIdDropdown;
    [SerializeField] private DateButton dateButton;
    [SerializeField] private TMP_InputField reasonInput;

    [SerializeField] private RawImage rawImageBackground;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;
    private bool isCamAvailable;
    private WebCamTexture cameraTexture;
    private bool camActive;

    private byte[] currentImage;
    private AttendanceExcuseRequest currentRequest;

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
        Database.instance.ReadData(ParentHomeManager.instance.StudentInfo.schoolId.ToString(), new Database.ReadDataCallback<SchoolInfoData>(ReloadSchoolDataCallback));
    }

    private void ReloadSchoolDataCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            MobileGraphics.instance.Loading(false);
            MobileGraphics.instance.DisplayMessage("An error has occurred");
            Debug.LogWarning("Couldn't find SchoolInfo");
            return;
        }

        ParentHomeManager.instance.SchoolData = output;

        Database.instance.ReadData(Database.instance.GetUsername() + "*" + dateButton.CurrentDate, new Database.ReadDataCallback<StudentAttendanceEntryData>(GetAttendanceCallback));
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

        List<int> periods = AdminHomeManager.GetPeriods(dateButton.CurrentDate, ParentHomeManager.instance.SchoolData);

        int period = teacherIdDropdown.value + 1;

        if (!periods.Contains(period))
        {
            DesktopGraphics.instance.DisplayMessage("Don't have that class on this day");
            DesktopGraphics.instance.Loading(false);
            return;
        }

        if (ParentHomeManager.instance.SchoolData.excuseRequests == null) ParentHomeManager.instance.SchoolData.excuseRequests = new List<AttendanceExcuseRequest>();

        currentRequest = new AttendanceExcuseRequest(ParentHomeManager.instance.StudentInfo.studentId, selectedTeacher, dateButton.CurrentDate, reasonInput.text, false);

        if (currentImage == null) //no image submission
        {
            SaveRequest();
        }
        else //image submission
        {
            AddImageToRequest();
        }
    }

    private void SaveRequest()
    {
        ParentHomeManager.instance.SchoolData.excuseRequests.Add(currentRequest);
        Database.instance.SaveDataToFirebase(ParentHomeManager.instance.SchoolData);
        DesktopGraphics.instance.DisplayMessage("Success");
        DesktopGraphics.instance.Loading(false);
        currentRequest = null;
    }

    private void AddImageToRequest()
    {
        SaveImage();
    }

    private void SaveImage()
    {
        if (currentImage == null) return;

        Database.instance.PutImage($"excuseRequest_{ Database.instance.GetUsername()}_{ System.DateTime.Now.ToString("yyyy_MM_dd_T_HH_mm_ss")}.png", currentImage, new Database.PutImageCallback(SaveImageCallback));
    }

    private void SaveImageCallback(PostImageResponseData data)
    {
        currentRequest.imageName = data.name;
        currentRequest.imageToken = data.downloadTokens;

        SaveRequest();
    }

    public void ActivateCamera()
    {
        if (!camActive)
        {
            StartCoroutine(SetUpCamera());
        }
        else
        {
            camActive = false;
            DisableCamera();
        }

    }

    IEnumerator SetUpCamera()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                DesktopGraphics.instance.DisplayMessage("Camera Access Not Granted");
                yield break;
            }
        }

        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            isCamAvailable = false;
            DesktopGraphics.instance.DisplayMessage("No Camera Found");
            yield break;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing)
            {
                cameraTexture = new WebCamTexture(devices[i].name, (int)rawImageBackground.rectTransform.sizeDelta.x, (int)rawImageBackground.rectTransform.sizeDelta.y);
            }
        }

        if (cameraTexture != null)
        {
            cameraTexture.Play();
            rawImageBackground.texture = cameraTexture;
            isCamAvailable = true;
        }
        else
        {
            isCamAvailable = false;
        }

        if (isCamAvailable)
        {
            camActive = true;
        }
        else
        {
            DesktopGraphics.instance.DisplayMessage("No Camera Found");
        }
    }

    private void DisableCamera()
    {
        if (!isCamAvailable) return;

        isCamAvailable = false;


        //first Make sure you're using RGB24 as your texture format
        Texture2D texture = new Texture2D(cameraTexture.width, cameraTexture.height, TextureFormat.RGB24, false);

        texture.SetPixels(cameraTexture.GetPixels());
        texture.Apply();

        cameraTexture.Stop();

        currentImage = texture.EncodeToPNG();
    }

    private void UpdateCameraRenderer()
    {
        if (!isCamAvailable) return;

        float ratio = (float)cameraTexture.width / (float)cameraTexture.height;
        aspectRatioFitter.aspectRatio = ratio;

        int orientation = -cameraTexture.videoRotationAngle;
        rawImageBackground.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    private void Update()
    {
        if (camActive) UpdateCameraRenderer();
    }
}
