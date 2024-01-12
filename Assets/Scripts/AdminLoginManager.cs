using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminLoginManager : MonoBehaviour
{
    public SchoolInfoData testData;

    private void Start()
    {
        testData = new SchoolInfoData(101, "Lyman High School", new List<string>(), new List<string>(), new Dictionary<string, ListWrapper<int>>(), new Dictionary<string, ListWrapper<int>>(), new List<string>());
    }

    [ContextMenu("Save")]
    public void SaveTestData()
    {
        Database.instance.SaveDataToFirebase(testData);
    }
}
