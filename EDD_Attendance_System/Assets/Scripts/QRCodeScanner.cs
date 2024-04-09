using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class QRCodeScanner : MonoBehaviour
{
    [SerializeField] private RawImage rawImageBackground;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;
    //[SerializeField] private TMP_Text outputText;
    [SerializeField] private RectTransform scanZone;
    //[SerializeField] private TMP_InputField studentIdInput;

    [SerializeField] private bool testingCam;

    private bool isCamAvailable;
    private WebCamTexture cameraTexture;

    private string scannedResult;
    [SerializeField] private float holdResultTime;
    private float currentHoldTime;

    [SerializeField] private Image scanGraphics;
    [SerializeField] private Color detectedColor;
    [SerializeField] private Color notDetectedColor;

    private StudentInfoData studentInfo;
    private SchoolInfoData schoolInfo;

    // Start is called before the first frame update
    void Start()
    {
        SetUpCamera();

        GetData();
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
                cameraTexture = new WebCamTexture(devices[i].name, (int)scanZone.rect.width, (int)scanZone.rect.height);
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

    public void Scan()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(cameraTexture.GetPixels32(), cameraTexture.width, cameraTexture.height);
            if (result != null)
            {
                scannedResult = result.Text;
            }
            else
            {
                MobileGraphics.instance.DisplayMessage("Failed to read QR Code, please try again");
            }
        }
        catch
        {
            MobileGraphics.instance.DisplayMessage("An error has occurred");
        }
    }

    public bool IsQRCode()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(cameraTexture.GetPixels32(), cameraTexture.width, cameraTexture.height);
            if (result != null)
            {
                scannedResult = result.Text;
                currentHoldTime = 0f;
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    public void MarkPresent()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(cameraTexture.GetPixels32(), cameraTexture.width, cameraTexture.height);
            if (result != null)
            {           
                string[] splitFields = result.Text.Split("*");
                if (!splitFields[0].Equals("eddAttendance"))
                {
                    MobileGraphics.instance.DisplayMessage("Invalid QR Code, only use QR Codes generated by the system");
                    return;
                }
                VerifyAndMarkPresent(splitFields);
            }
            else if (!string.IsNullOrEmpty(scannedResult))
            {
                string[] splitFields = scannedResult.Split("*");
                if (!splitFields[0].Equals("eddAttendance"))
                {
                    MobileGraphics.instance.DisplayMessage("Invalid QR Code, only use QR Codes generated by the system");
                    return;
                }
                VerifyAndMarkPresent(splitFields);
            }
            else
            {
                MobileGraphics.instance.DisplayMessage("Failed to read QR Code, please try again");
            }
        }
        catch (Exception e)
        {
            MobileGraphics.instance.DisplayMessage("An error has occurred");
            Debug.LogError(e);
        }
    }

    private void VerifyAndMarkPresent(string[] splitFields)
    {
        int period = Array.IndexOf(studentInfo.classList, Int32.Parse(splitFields[1])) + 1;
        List<int> periods = AdminHomeManager.GetPeriods(splitFields[2], schoolInfo);

        if (!periods.Contains(period))
        {
            MobileGraphics.instance.DisplayMessage("You don't have this class on this day");
            return;
        }

        MobileGraphics.instance.Loading(true);
        Database.instance.ReadData(Int32.Parse(Database.instance.GetUsername()) + "*" + splitFields[2], new Database.ReadDataCallbackParams<StudentAttendanceEntryData>(MarkPresentCallback), new object[] { Int32.Parse(Database.instance.GetUsername()), Int32.Parse(splitFields[1]), splitFields[2], false });
    }

    private void MarkPresentCallback(StudentAttendanceEntryData output, object[] additionalParams)
    {
        int studentId = (int)additionalParams[0];
        int teacherId = (int)additionalParams[1];
        string date = (string)additionalParams[2];
        bool tardy = (bool)additionalParams[3];

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
        if (!updatedEntry.presentList.Contains(teacherId.ToString())) updatedEntry.presentList.Add(teacherId.ToString());

        //tardyList
        if (tardy)
        {
            if (!updatedEntry.tardyList.Contains(teacherId.ToString())) updatedEntry.tardyList.Add(teacherId.ToString());
        }
        else
        {
            if (updatedEntry.tardyList.Contains(teacherId.ToString())) updatedEntry.tardyList.Remove(teacherId.ToString());
        }

        if (!studentInfo.attendanceObjects.Contains(updatedEntry.fileName))
        {
            studentInfo.attendanceObjects.Add(updatedEntry.fileName);
            Database.instance.SaveDataToFirebase(studentInfo);
        }


        Database.instance.SaveDataToFirebase(updatedEntry);

        MobileGraphics.instance.DisplayMessage("Success");
        MobileGraphics.instance.Loading(false);
    }

    private void UpdateCameraRenderer()
    {
        if (!isCamAvailable) return;

        float ratio = (float)cameraTexture.width / (float)cameraTexture.height;
        aspectRatioFitter.aspectRatio = ratio;

        int orientation = -cameraTexture.videoRotationAngle;
        rawImageBackground.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MobileHome");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraRenderer();

        if (!IsQRCode())
        {
            currentHoldTime += Time.deltaTime;
            if (currentHoldTime > holdResultTime)
            {
                scannedResult = "";
            }
        }

        scanGraphics.color = !string.IsNullOrEmpty(scannedResult) ? detectedColor : notDetectedColor;
    }
}
