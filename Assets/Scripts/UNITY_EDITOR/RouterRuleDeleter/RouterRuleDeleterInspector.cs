using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(RouterRuleDeleter))]
public class RouterRuleDeleterInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RouterRuleDeleter deleter = (RouterRuleDeleter)target;
        if (GUILayout.Button($"Delete router rule with port {deleter.PortToDelete}"))
        {
            deleter.DeleteRule();
        }
    }
}
#endif
