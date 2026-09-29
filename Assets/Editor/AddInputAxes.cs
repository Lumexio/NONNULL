using UnityEngine;
using UnityEditor;

public class AddInputAxes
{
    [MenuItem("Tools/Add Missing Input Axes")]
    public static void AddAxes()
    {
        var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
        SerializedObject obj = new SerializedObject(inputManager);
        SerializedProperty axes = obj.FindProperty("m_Axes");

        AddAxis(axes, "Elbow",    "c",            "",             "joystick button 3");
        AddAxis(axes, "ElbowMod", "e",            "left shift",   "joystick button 5");
        AddAxis(axes, "Run",      "left shift",   "e",            "joystick button 4");

        obj.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        Debug.Log("Done — Elbow, ElbowMod, Run axes added.");
    }

    static void AddAxis(SerializedProperty axes, string name,
                        string positiveButton, string altPositiveButton,
                        string joystickButton)
    {
        for (int i = 0; i < axes.arraySize; i++)
        {
            if (axes.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue == name)
            {
                Debug.Log("Already exists, skipping: " + name);
                return;
            }
        }

        axes.arraySize++;
        SerializedProperty axis = axes.GetArrayElementAtIndex(axes.arraySize - 1);

        axis.FindPropertyRelative("m_Name").stringValue                  = name;
        axis.FindPropertyRelative("descriptiveName").stringValue         = "";
        axis.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
        axis.FindPropertyRelative("negativeButton").stringValue          = "";
        axis.FindPropertyRelative("positiveButton").stringValue          = positiveButton;
        axis.FindPropertyRelative("altNegativeButton").stringValue       = "";
        axis.FindPropertyRelative("altPositiveButton").stringValue       = altPositiveButton;
        axis.FindPropertyRelative("gravity").floatValue                  = 1000f;
        axis.FindPropertyRelative("dead").floatValue                     = 0.001f;
        axis.FindPropertyRelative("sensitivity").floatValue              = 1000f;
        axis.FindPropertyRelative("snap").boolValue                      = false;
        axis.FindPropertyRelative("invert").boolValue                    = false;
        axis.FindPropertyRelative("type").intValue                       = 0;
        axis.FindPropertyRelative("axis").intValue                       = 0;
        axis.FindPropertyRelative("joyNum").intValue                     = 0;
    }
}
