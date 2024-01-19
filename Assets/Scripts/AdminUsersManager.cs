using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminUsersManager : MonoBehaviour
{
    [Header("UI Initialize Fields")]
    [SerializeField] private GameObject mainScroll;
    [SerializeField] private GameObject[] otherScrolls;

    private void OnEnable()
    {
        mainScroll.SetActive(true);
        foreach (GameObject scroll in otherScrolls)
        {
            scroll.SetActive(false);
        }
    }
}
