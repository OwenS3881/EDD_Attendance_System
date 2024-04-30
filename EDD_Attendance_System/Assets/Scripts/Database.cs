using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class Database : MonoBehaviour
{
    public static Database instance { get; private set; }

    [SerializeField] private string databaseURL = "";
    [SerializeField] private string storageURL = "";

    public static readonly List<string> freePeriodIds = new List<string>() { "1", "2", "3", "4", "5", "6", "7" };
    public static readonly List<string> checkInIds = new List<string>() { "1", "2", "3", "4" };
    public static readonly List<string> checkOutIds = new List<string>() { "5", "6", "7" };

    private LoginResult currentUser;
    public LoginResult CurrentUser
    {
        set
        {
            if (currentUser != null) return;

            currentUser = value;
        }
        get
        {
            if (currentUser == null)
            {
                if (SceneManager.GetActiveScene().name.Contains("Admin"))
                {
                    SceneManager.LoadScene("AdminLogin");
                    return null;
                }
                else if (SceneManager.GetActiveScene().name.Contains("Teacher"))
                {
                    SceneManager.LoadScene("TeacherLogin");
                    return null;
                }
                else if (SceneManager.GetActiveScene().name.Contains("Mobile"))
                {
                    SceneManager.LoadScene("MobileLogin");
                    return null;
                }
                else if (SceneManager.GetActiveScene().name.Contains("Parent"))
                {
                    SceneManager.LoadScene("ParentLogin");
                    return null;
                }
                else
                {
                    return null;
                }
            }

            return currentUser;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void LogoutUser()
    {
        currentUser = null;
    }

    
    public void SaveDataToFirebase(BasicData data)
    {
        
        RestClient.Put(databaseURL + "/" + data.fileName + ".json", JsonUtility.ToJson(data)).Then(response =>
        {
            //Debug.Log("Success");
        });
        
    }

    public void DeleteData(string fileName)
    {
        RestClient.Delete(databaseURL + "/" + fileName + ".json").Then(response =>
        {
            Debug.Log("Deleted " + fileName);
        });
    }

    public delegate void ReadDataCallback<T>(T output);
    public void ReadData<T>(string fileName, ReadDataCallback<T> callback)
    {
        RestClient.Get<T>(databaseURL + "/" + fileName + ".json").Then(response =>
        {
            callback(response);

        }).Catch(error =>
        {
            Debug.LogWarning(error);
            callback(default(T));
        });
    }

    public delegate void ReadDataCallbackParams<T>(T output, object[] additionalParamters);
    public void ReadData<T>(string fileName, ReadDataCallbackParams<T> callback, object[] additionalParamters)
    {
        RestClient.Get<T>(databaseURL + "/" + fileName + ".json").Then(response =>
        {
            callback(response, additionalParamters);

        }).Catch(error =>
        {
            Debug.LogWarning(error);
            callback(default(T), additionalParamters);
        });
    }

    public string GetUsername()
    {
        try
        {
            return CurrentUser.InfoResultPayload.AccountInfo.Username;
        }
        catch
        {
            Debug.Log("Loading login scene...");
            return "";
        }
    }

    public string GetUserEmail()
    {
        return CurrentUser.InfoResultPayload.AccountInfo.PrivateInfo.Email;
    }

    public delegate void LoadImageCallback(Texture2D downloadedTexture);
    public void LoadImage(string fileName, string mediaToken, LoadImageCallback callback)
    {
        StartCoroutine(LoadImageCoroutine(fileName, mediaToken, callback));
    }

    IEnumerator LoadImageCoroutine(string fileName, string mediaToken, LoadImageCallback callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture($"{storageURL}{fileName}?alt=media&token={mediaToken}");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            callback(((DownloadHandlerTexture)request.downloadHandler).texture);
        }
    }

    public delegate void PutImageCallback(PostImageResponseData responseData);
    public void PutImage(string fileName, byte[] image, PutImageCallback callback)
    {
        StartCoroutine(PutImageCoroutine(fileName, image, callback));
    }

    IEnumerator PutImageCoroutine(string fileName, byte[] image, PutImageCallback callback)
    {
        //UnityWebRequest request = UnityWebRequest.Put(url, image);
        var request = new UnityWebRequest(storageURL + fileName, UnityWebRequest.kHttpVerbPOST);
        request.SetRequestHeader("Content-Type", "image/png");
        request.uploadHandler = new UploadHandlerRaw(image);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            PostImageResponseData responseData = JsonUtility.FromJson<PostImageResponseData>(request.downloadHandler.text);
            callback(responseData);
        }
    }

    public void DeleteImage(string fileName, string mediaToken)
    {
        StartCoroutine(DeleteImageCoroutine(fileName, mediaToken));
    }

    IEnumerator DeleteImageCoroutine(string fileName, string mediaToken)
    {
        var request = new UnityWebRequest($"{storageURL}{fileName}?alt=media&token={mediaToken}", UnityWebRequest.kHttpVerbDELETE);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            //Debug.Log("Success");
        }
    }
}
