using UnityEditor;
using UnityEngine;

[System.Serializable]
public class HDRGradient
{
    public Gradient gradient = new Gradient();
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(HDRGradient))]
public class HDRGradientDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty gradientProp = property.FindPropertyRelative("gradient");

        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.BeginChangeCheck();
        Gradient newGradient = EditorGUI.GradientField(position, label, gradientProp.gradientValue, true); // HDR = true
        if (EditorGUI.EndChangeCheck())
        {
            gradientProp.gradientValue = newGradient;
        }

        EditorGUI.EndProperty();
    }
}
#endif
