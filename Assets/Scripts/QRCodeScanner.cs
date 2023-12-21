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
                MobileGraphics.instance.DisplayMessage("Success");
                string[] splitFields = result.Text.Split("*");
                AdminCreator.MarkPresent(Int32.Parse(Database.instance.CurrentUser.InfoResultPayload.AccountInfo.Username), Int32.Parse(splitFields[0]), splitFields[1]);
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
        scanGraphics.color = IsQRCode() ? detectedColor : notDetectedColor;
    }
}
