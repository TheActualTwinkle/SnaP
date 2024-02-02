using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public interface IAddressableContentUser
{
    public uint LoadedCount { get; }
    
    public uint AssetsCount { get; }
    
    Task LoadContent();
    
    void UnloadContent();
}
