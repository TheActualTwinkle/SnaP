using UnityEngine;

public class AddressablesTools : MonoBehaviour
{
    private void Awake()
    {
#if !UNITY_EDITOR
        Destroy(gameObject);
#endif
    }

    // Editor Button.
    public void Clear()
    {
        if (Caching.ClearCache() == true)
        {
            Debug.Log("Addressables cache cleared.");
        }
        else
        {
            Debug.LogError("Failed to clear Addressables cache. Cache was in use.");
        }
    }
}
