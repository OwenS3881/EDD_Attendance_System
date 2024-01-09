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

    // Start is called before the first frame update
    void Start()
    {
        SetUpCamera();
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
                AdminCreator.MarkPresent(Int32.Parse(Database.instance.GetUsername()), Int32.Parse(splitFields[1]), splitFields[2], false);
                MobileGraphics.instance.DisplayMessage("Success");
            }
            else if (!string.IsNullOrEmpty(scannedResult))
            {
                string[] splitFields = scannedResult.Split("*");
                if (!splitFields[0].Equals("eddAttendance"))
                {
                    MobileGraphics.instance.DisplayMessage("Invalid QR Code, only use QR Codes generated by the system");
                    return;
                }
                AdminCreator.MarkPresent(Int32.Parse(Database.instance.GetUsername()), Int32.Parse(splitFields[1]), splitFields[2], false);
                MobileGraphics.instance.DisplayMessage("Success");
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
