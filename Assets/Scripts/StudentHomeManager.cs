using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudentHomeManager : MonoBehaviour
{
    public void ScanQRCode()
    {
        SceneManager.LoadScene("QRScanner");
    }

    public void Logout()
    {
        Database.instance.LogoutUser();
        SceneManager.LoadScene("MobileLogin");
    }
}
