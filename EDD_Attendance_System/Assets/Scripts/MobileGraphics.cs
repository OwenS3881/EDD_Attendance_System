using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileGraphics : MonoBehaviour
{
    public static MobileGraphics instance { get; private set; }

    [SerializeField] private GameObject mobileMessageObject;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private DatePicker datePicker;

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

        loadingScreen.SetActive(false);
    }

    public void DisplayMessage(string message)
    {
        Instantiate(mobileMessageObject, mobileMessageObject.transform.position, Quaternion.identity).GetComponent<MobileMessage>().Initialize(message);
    }

    public void Loading(bool isLoading)
    {
        loadingScreen.SetActive(isLoading);
    }

    public void SelectDate(DateButton button, string initialDate)
    {
        datePicker.gameObject.SetActive(true);
        datePicker.Initialize(button, initialDate);
    }
}