#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(DebugRecoilPatternMaker))]

public class DebugRecoilPatternMakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        if (Application.isPlaying == false)
        {
            GUILayout.TextArea("RecoilPatternMaker only works fully in Play Mode");
        }

        DebugRecoilPatternMaker recoilMaker = (DebugRecoilPatternMaker)target;

        if (GUILayout.Button("Load Recoilpattern From File"))
        {
            _ = recoilMaker.LoadRecoilPatternFromFile();
        }
        if (GUILayout.Button("Save Recoilpattern To File"))
        {
            _ = recoilMaker.SaveRecoilPatternToFile();
        }
        GUILayout.Space(10);

        if (Application.isPlaying && GUILayout.Button("Create Visual Recoilpattern"))
        {
            recoilMaker.StartShootingSequence();
        }
        if (Application.isPlaying && GUILayout.Button("Clear Visual Recoilpattern"))
        {
            recoilMaker.ClearVisualRecoilPattern();
        }
        GUILayout.Space(10);

        if (Application.isPlaying && GUILayout.Button("Load Recoilpattern from Visuals"))
        {
            recoilMaker.LoadRecoilPatternFromVisuals();
        }
        GUILayout.Space(10);

        if (GUILayout.Button("Load Recoilpattern From ScriptableObject"))
        {
            recoilMaker.LoadRecoilPatternFromScriptableObject();
        }
        if (GUILayout.Button("Save Recoilpattern To ScriptableObject"))
        {
            recoilMaker.SaveRecoilPatternToScriptableObject();
        }
    }
}
#endif