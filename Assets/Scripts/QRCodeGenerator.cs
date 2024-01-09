using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using UnityEngine.UI;
using TMPro;

public class QRCodeGenerator : MonoBehaviour
{
    [SerializeField] private RawImage rawImageReceiver;
    [SerializeField] private DateButton dateInput;

    private Texture2D storeEncodedTexture;

    // Start is called before the first frame update
    void Start()
    {
        storeEncodedTexture = new Texture2D(256, 256);
    }

    public void GenerateCode()
    {
        EncodeTextToStoreTexture($"eddAttendance*{Database.instance.GetUsername()}*{dateInput.CurrentDate}");
        rawImageReceiver.texture = storeEncodedTexture;
    }


    private void EncodeTextToStoreTexture(string input)
    {
        string textWrite = string.IsNullOrEmpty(input) ? "N/A" : input;

        Color32[] convertPixelToTexture = Encode(textWrite, storeEncodedTexture.width, storeEncodedTexture.height);

        storeEncodedTexture.SetPixels32(convertPixelToTexture);
        storeEncodedTexture.Apply();   
    }

    private Color32[] Encode(string textForEncoding, int width, int height)
    {
        BarcodeWriter writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }
}
