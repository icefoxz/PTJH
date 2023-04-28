using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoDebug : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log($"{name}: MonoDebug OnEnable");
    }
}
