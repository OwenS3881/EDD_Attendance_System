using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class Database : MonoBehaviour
{
    public static Database instance { get; private set; }

    public static readonly int freePeriodId = 0;

    [SerializeField] private string databaseURL = "";

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
        return CurrentUser.InfoResultPayload.AccountInfo.Username;
    }

    public string GetUserEmail()
    {
        return CurrentUser.InfoResultPayload.AccountInfo.PrivateInfo.Email;
    }
}
