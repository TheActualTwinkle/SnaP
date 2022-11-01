using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveLoadSystem
{
    void Save(ISaveLoadData saveLoadData);
    T Load<T>() where T : ISaveLoadData;
}
