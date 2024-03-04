using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NumericTextCreator : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject parentObject;
    [SerializeField] private Vector2Int range;

    private void Awake()
    {
        for (int i = range.x; i <= range.y; i++)
        {
            Instantiate(textPrefab, parentObject.transform).GetComponentInChildren<TMP_Text>().text = i.ToString();
        }
    }
}
