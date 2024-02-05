using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(AddressablesTools))]
public class AddressablesToolsInspector : Editor
{
    private bool _buildLinux;
    private bool _buildWindows;
    private bool _buildOSX;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Clear addressables cache") == true)
        {
            AddressablesTools cleaner = (AddressablesTools)target;
            cleaner.Clear();
        }
    }
}
#endif
