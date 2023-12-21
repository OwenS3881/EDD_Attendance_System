using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ListWrapper<T>
{
    public List<T> internalList = new List<T>();

    public T this[int key]
    {
        get
        {
            return internalList[key];
        }
        set
        {
            internalList[key] = value;
        }
    }

    public void Add(T item)
    {
        internalList.Add(item);
    }
}
