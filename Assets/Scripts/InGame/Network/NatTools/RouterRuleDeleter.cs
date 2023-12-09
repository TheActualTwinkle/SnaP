using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouterRuleDeleter : MonoBehaviour
{
    public ushort PortToDelete => _portToDelete;
    [SerializeField] private ushort _portToDelete;

    private void Awake()
    {
#if !UNITY_EDITOR
        Destroy(gameObject);
#endif
    }

    // Editor Button.
    public void DeleteRule()
    {
        UPnP.DeleteRuleAsync(_portToDelete);
    }
}
