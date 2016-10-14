using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShapeBehaviour))]
public class ShapeBehaviourInspector : Editor {

    public override void OnInspectorGUI() {
        ShapeBehaviour gb = (ShapeBehaviour)target;

        gb.isEnabled = EditorGUILayout.Toggle(new GUIContent("Is enabled", "Disable or enable shape recognition"), gb.isEnabled);
        gb.forceCopy = EditorGUILayout.Toggle(new GUIContent("Force copy", "Overwrite the XML file in persistent data path"), gb.forceCopy);
        gb.useProtractor = EditorGUILayout.Toggle(new GUIContent("Use protractor", "Use the faster algorithm, however default (slower) algorithm has a better scoring system"), gb.useProtractor);
        gb.libraryToLoad = EditorGUILayout.TextField(new GUIContent("Library to load", "The name of the shape library to load. Do NOT include '.xml'"), gb.libraryToLoad);
        gb.distanceBetweenPoints = EditorGUILayout.FloatField(new GUIContent("Distance between points", "A new point will be placed if it is this further than the last point."), gb.distanceBetweenPoints);
        gb.minimumPointsToRecognize = EditorGUILayout.IntField(new GUIContent("Minimum points to recognize", "Minimum amount of points required to recognize a gesture."), gb.minimumPointsToRecognize);
        gb.lineMaterial = (Material)EditorGUILayout.ObjectField(new GUIContent("Line material", "Material for the line renderer."), gb.lineMaterial, typeof(Material), false);
        gb.startThickness = EditorGUILayout.FloatField(new GUIContent("Start thickness", "Start thickness of the gesture."), gb.startThickness);
        gb.endThickness = EditorGUILayout.FloatField(new GUIContent("End thickness", "End thickness of the gesture."), gb.endThickness);
        gb.startColor = EditorGUILayout.ColorField(new GUIContent("Start color", "Start color of the gesture."), gb.startColor);
        gb.endColor = EditorGUILayout.ColorField(new GUIContent("End color", "End color of the gesture."), gb.endColor);
        gb.shapeLimitType = (ShapeDetector.ShapeLimitType)EditorGUILayout.EnumPopup(new GUIContent("Gesture limit type", "Limits gesture drawing to a specific area"), gb.shapeLimitType);
        
        if (gb.shapeLimitType == ShapeDetector.ShapeLimitType.RectBoundsClamp || gb.shapeLimitType == ShapeDetector.ShapeLimitType.RectBoundsIgnore) {
            gb.shapeLimitRectBounds = (RectTransform)EditorGUILayout.ObjectField(new GUIContent("Gesture limit rect bounds", "RectTransform to limit gesture"), gb.shapeLimitRectBounds, typeof(RectTransform), true);
        }

        if (GUI.changed)
            EditorUtility.SetDirty(gb);
    }
}