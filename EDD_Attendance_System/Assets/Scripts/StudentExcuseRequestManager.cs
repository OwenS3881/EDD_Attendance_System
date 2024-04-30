using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public class StudentExcuseRequestManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown teacherIdDropdown;
    [SerializeField] private DateButton dateButton;
    [SerializeField] private TMP_InputField reasonInput;

    [SerializeField] private RawImage rawImageBackground;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;
    [SerializeField] private bool testingCam;
    private bool isCamAvailable;
    private WebCamTexture cameraTexture;
    private bool camActive;

    private StudentInfoData studentInfo;
    private SchoolInfoData schoolInfo;

    private byte[] currentImage;
    private AttendanceExcuseRequest currentRequest;

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

        Database.instance.ReadData(studentInfo.schoolId, new Database.ReadDataCallback<SchoolInfoData>(GetSchoolDataCallback));
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

        foreach (string id in studentInfo.classList)
        {
            optionsList.Add(id);
        }

        teacherIdDropdown.AddOptions(optionsList);

        for (int i = 0; i < studentInfo.classList.Length; i++)
        {
            AddTeacherNameToDropdown(studentInfo.classList[i], i);
        }
    }

    private void AddTeacherNameToDropdown(string teacherId, int dropdownListIndex)
    {
        Database.instance.ReadData(teacherId, new Database.ReadDataCallbackParams<TeacherInfoData>(AddTeacherNameToDropdownCallback), new object[] { dropdownListIndex });
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
        Database.instance.ReadData(studentInfo.schoolId, new Database.ReadDataCallback<SchoolInfoData>(ReloadSchoolDataCallback));
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

        schoolInfo = output;

        Database.instance.ReadData(Database.instance.GetUsername() + "*" + dateButton.CurrentDate, new Database.ReadDataCallback<StudentAttendanceEntryData>(GetAttendanceCallback));
    }

    private void GetAttendanceCallback(StudentAttendanceEntryData output)
    {
        string selectedTeacher = teacherIdDropdown.options[teacherIdDropdown.value].text.Split(" - ")[0];

        if (output != null)
        {
            if (output.presentList.Contains(selectedTeacher) && !output.tardyList.Contains(selectedTeacher))
            {
                MobileGraphics.instance.DisplayMessage("Already present for this day");
                MobileGraphics.instance.Loading(false);
                return;
            }
        }

        List<int> periods = AdminHomeManager.GetPeriods(dateButton.CurrentDate, schoolInfo);

        int period = teacherIdDropdown.value + 1;

        if (!periods.Contains(period))
        {
            MobileGraphics.instance.DisplayMessage("Don't have that class on this day");
            MobileGraphics.instance.Loading(false);
            return;
        }

        if (schoolInfo.excuseRequests == null) schoolInfo.excuseRequests = new List<AttendanceExcuseRequest>();

        currentRequest = new AttendanceExcuseRequest(Database.instance.GetUsername(), selectedTeacher, dateButton.CurrentDate, reasonInput.text, false);

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
        MobileGraphics.instance.Loading(true);
        Database.instance.ReadData(studentInfo.schoolId, new Database.ReadDataCallback<SchoolInfoData>(SaveRequestCallback));
    }

    private void SaveRequestCallback(SchoolInfoData output)
    {
        if (output == null)
        {
            MobileGraphics.instance.Loading(false);
            MobileGraphics.instance.DisplayMessage("An error has occurred");
            Debug.LogWarning("Couldn't find SchoolInfo");
            return;
        }

        schoolInfo = output;

        schoolInfo.excuseRequests.Add(currentRequest);
        Database.instance.SaveDataToFirebase(schoolInfo);
        MobileGraphics.instance.DisplayMessage("Success");
        MobileGraphics.instance.Loading(false);
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
            SetUpCamera();
            if (isCamAvailable) camActive = true;  
        }
        else
        {
            camActive = false;
            DisableCamera();
        }
        
    }

    private void SetUpCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            isCamAvailable = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == testingCam)
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
    }

    private void DisableCamera()
    {
        if (!isCamAvailable) return;

        isCamAvailable = false;

        
        //first Make sure you're using RGB24 as your texture format
        Texture2D texture = new Texture2D(cameraTexture.width, cameraTexture.height, TextureFormat.RGB24, false);

        texture.SetPixels(cameraTexture.GetPixels());
        texture.Apply();

        texture = RotateTexture(texture);

        cameraTexture.Stop();

        currentImage = texture.EncodeToPNG();
    }

    public static Texture2D RotateTexture(Texture2D t)
    {
        Texture2D newTexture = new Texture2D(t.height, t.width, t.format, false);

        for (int i = 0; i < t.width; i++)
        {
            for (int j = 0; j < t.height; j++)
            {
                newTexture.SetPixel(j, i, t.GetPixel(t.width - i, j));
            }
        }
        newTexture.Apply();
        return newTexture;
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
