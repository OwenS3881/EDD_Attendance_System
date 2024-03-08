using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImageData : BasicData
{
    public byte[] image;

    public ImageData(string fileName, byte[] image)
    {
        this.fileName = fileName;
        this.image = image;
    }
}
