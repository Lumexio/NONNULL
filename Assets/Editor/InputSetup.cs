using UnityEditor;
using UnityEngine;

public class InputSetup {
    [MenuItem("Game/Setup Inputs")]
    public static void SetupInputs() {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
        if (assets == null || assets.Length == 0) return;
        SerializedObject inputManager = new SerializedObject(assets[0]);
        SerializedProperty axesProperty = inputManager.FindProperty("m_Axes");
        if (axesProperty == null) return;

        // PS Vita Dual Analog Sticks
        AddOrUpdateAxis(axesProperty, "RightStickX", "", "", "", "", 2, 3, 0.19f, 1f, 0f, false, false);
        AddOrUpdateAxis(axesProperty, "RightStickY", "", "", "", "", 2, 4, 0.19f, 1f, 0f, true, false);
        AddOrUpdateAxis(axesProperty, "Look X", "", "", "", "", 2, 3, 0.19f, 1f, 0f, false, false);
        AddOrUpdateAxis(axesProperty, "Look Y", "", "", "", "", 2, 4, 0.19f, 1f, 0f, true, false);

        // PS Vita Action Buttons (Cross=0, Circle=1, Square=2, Triangle=3)
        AddOrUpdateAxis(axesProperty, "Cross", "joystick button 0", "", "return", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "Circle", "joystick button 1", "", "escape", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "Square", "joystick button 2", "", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "Triangle", "joystick button 3", "", "space", "", 0, 0, 0.001f, 1000f, 1000f, false, false);

        // PS Vita Shoulder Buttons (L=4, R=5)
        AddOrUpdateAxis(axesProperty, "LeftBumper", "joystick button 4", "", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "L", "joystick button 4", "", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "Run", "joystick button 4", "", "left shift", "", 0, 0, 0.001f, 1000f, 1000f, false, false);

        AddOrUpdateAxis(axesProperty, "RightBumper", "joystick button 5", "", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "R", "joystick button 5", "", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "ElbowMod", "joystick button 5", "", "e", "", 0, 0, 0.001f, 1000f, 1000f, false, false);

        // PS Vita System Buttons (Select=6, Start=7)
        AddOrUpdateAxis(axesProperty, "Select", "joystick button 6", "", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "Start", "joystick button 7", "", "return", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "Pause", "joystick button 7", "", "escape", "", 0, 0, 0.001f, 1000f, 1000f, false, false);

        // PS Vita D-Pad Buttons (8-11)
        AddOrUpdateAxis(axesProperty, "DPadUp", "joystick button 8", "", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "DPadRight", "joystick button 9", "", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "DPadDown", "joystick button 10", "", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "DPadLeft", "joystick button 11", "", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);

        // PS Vita D-Pad Combined Axes
        AddOrUpdateAxis(axesProperty, "DPadX", "joystick button 9", "joystick button 11", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);
        AddOrUpdateAxis(axesProperty, "DPadY", "joystick button 8", "joystick button 10", "", "", 0, 0, 0.001f, 1000f, 1000f, false, false);

        inputManager.ApplyModifiedProperties();
        Debug.Log("PS Vita Input mappings successfully configured in InputManager!");
    }

    private static void AddOrUpdateAxis(
        SerializedProperty axesProperty,
        string name,
        string positiveButton,
        string negativeButton,
        string altPositiveButton,
        string altNegativeButton,
        int type,
        int axisNum,
        float dead,
        float sensitivity,
        float gravity,
        bool invert,
        bool snap
    ) {
        SerializedProperty targetAxis = null;
        for (int i = 0; i < axesProperty.arraySize; ++i) {
            SerializedProperty axis = axesProperty.GetArrayElementAtIndex(i);
            if (axis.FindPropertyRelative("m_Name").stringValue == name &&
                axis.FindPropertyRelative("type").intValue == type &&
                axis.FindPropertyRelative("axis").intValue == axisNum) {
                targetAxis = axis;
                break;
            }
        }

        if (targetAxis == null) {
            axesProperty.InsertArrayElementAtIndex(axesProperty.arraySize);
            targetAxis = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
        }

        targetAxis.FindPropertyRelative("m_Name").stringValue = name;
        targetAxis.FindPropertyRelative("descriptiveName").stringValue = "";
        targetAxis.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
        targetAxis.FindPropertyRelative("negativeButton").stringValue = negativeButton;
        targetAxis.FindPropertyRelative("positiveButton").stringValue = positiveButton;
        targetAxis.FindPropertyRelative("altNegativeButton").stringValue = altNegativeButton;
        targetAxis.FindPropertyRelative("altPositiveButton").stringValue = altPositiveButton;
        targetAxis.FindPropertyRelative("gravity").floatValue = gravity;
        targetAxis.FindPropertyRelative("dead").floatValue = dead;
        targetAxis.FindPropertyRelative("sensitivity").floatValue = sensitivity;
        targetAxis.FindPropertyRelative("snap").boolValue = snap;
        targetAxis.FindPropertyRelative("invert").boolValue = invert;
        targetAxis.FindPropertyRelative("type").intValue = type;
        targetAxis.FindPropertyRelative("axis").intValue = axisNum;
        targetAxis.FindPropertyRelative("joyNum").intValue = 0;
    }
}
