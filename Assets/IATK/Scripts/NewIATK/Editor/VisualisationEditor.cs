using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NewIATK
{
    /// <summary>
    /// Visualisation editor. Custom editor for inspector in Visualisation component.
    /// </summary>
    [CustomEditor(typeof(Visualisation))]
    [CanEditMultipleObjects]
    public class VisualisationEditor : IATKEditor
    {
        private SerializedProperty uidProperty;
        private SerializedProperty xDimensionProperty;
        private SerializedProperty yDimensionProperty;
        private SerializedProperty zDimensionProperty;
        private SerializedProperty colourProperty;
        private SerializedProperty colourByProperty;
        private SerializedProperty colourGradientProperty;
        private SerializedProperty sizeProperty;
        private SerializedProperty sizeByProperty;
        private SerializedProperty scaleProperty;

        private Visualisation targetVisualisation;
        private Dictionary<AbstractVisualisation, Editor> abstractVisualisationEditors;

        protected override void OnEnable()
        {
            base.OnEnable();

            targetVisualisation = (Visualisation)serializedObject.targetObject;
            uidProperty = serializedObject.FindProperty("<UID>k__BackingField");
            xDimensionProperty = serializedObject.FindProperty("<XDimension>k__BackingField");
            yDimensionProperty = serializedObject.FindProperty("<YDimension>k__BackingField");
            zDimensionProperty = serializedObject.FindProperty("<ZDimension>k__BackingField");
            colourProperty = serializedObject.FindProperty("<Colour>k__BackingField");
            colourByProperty = serializedObject.FindProperty("<ColourBy>k__BackingField");
            colourGradientProperty = serializedObject.FindProperty("<ColourGradient>k__BackingField");
            sizeProperty = serializedObject.FindProperty("<Size>k__BackingField");
            sizeByProperty = serializedObject.FindProperty("<SizeBy>k__BackingField");
            scaleProperty = serializedObject.FindProperty("<Scale>k__BackingField");

            abstractVisualisationEditors = new Dictionary<AbstractVisualisation, Editor>();

            LoadDataSource();
        }

        /// <summary>
        /// Draw the inspector and update Visualisation when a property changes
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            dirtyFlag = IATKProperty.None;

            // UID
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(uidProperty);
            EditorGUI.EndDisabledGroup();

            // Data Source
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(dataSourceProperty);
            if (EditorGUI.EndChangeCheck())
            {
                LoadDataSource();
                dirtyFlag = IATKProperty.DataSource;
            }

            // Only show these if there is a data source which is loaded
            if (IsDataSourceLoaded())
            {
                // Dimension properties
                EditorGUILayout.PrefixLabel(new GUIContent("X Axis"));
                var rect = EditorGUILayout.GetControlRect(true, 55);
                var dirty = DrawAttributeFilterEditor(xDimensionProperty, rect);
                if (dirty == IATKProperty.DimensionChange)
                    dirtyFlag = IATKProperty.X;

                EditorGUILayout.PrefixLabel(new GUIContent("Y Axis"));
                rect = EditorGUILayout.GetControlRect(true, 55);
                dirty = DrawAttributeFilterEditor(yDimensionProperty, rect);
                if (dirty == IATKProperty.DimensionChange)
                    dirtyFlag = IATKProperty.Y;

                EditorGUILayout.PrefixLabel(new GUIContent("Z Axis"));
                rect = EditorGUILayout.GetControlRect(true, 55);
                dirty = DrawAttributeFilterEditor(zDimensionProperty, rect);
                if (dirty == IATKProperty.DimensionChange)
                    dirtyFlag = IATKProperty.Z;


                // Colour properties
                if (colourByProperty.stringValue == "Undefined")
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(colourProperty);
                    if (EditorGUI.EndChangeCheck())
                        dirtyFlag = IATKProperty.Colour;
                }
                if (colourByProperty.stringValue != "Undefined")
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(colourGradientProperty);
                    if (EditorGUI.EndChangeCheck())
                        dirtyFlag = IATKProperty.ColourGradient;
                }
                if (EnumPopup("Colour by", dimensions, colourByProperty))
                {
                    dirtyFlag = IATKProperty.ColourBy;
                }

                // Size properties
                if (sizeByProperty.stringValue == "Undefined")
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(sizeProperty);
                    if (EditorGUI.EndChangeCheck())
                        dirtyFlag = IATKProperty.Size;
                }
                if (EnumPopup("Size by", dimensions, sizeByProperty))
                {
                    dirtyFlag = IATKProperty.SizeBy;
                }

                if (GUILayout.Button("Create Scatterplot"))
                {
                    targetVisualisation.CreateScatterplot();
                }

                for (int i = 0; i < targetVisualisation.SubVisualisations.Count; i++)
                {
                    if (targetVisualisation.SubVisualisations[i] == null)
                    {
                        EditorGUILayout.LabelField("Null visualisation");
                    }
                    else
                    {
                        var vis = targetVisualisation.SubVisualisations[i];
                        EditorGUILayout.LabelField(vis.VisualisationType.ToString());
                        EditorGUI.indentLevel++;
                        FindEditorFor(vis).OnInspectorGUI();
                        EditorGUI.indentLevel--;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            UpdateVisualisationProperty(dirtyFlag);

        }

        private Editor FindEditorFor(AbstractVisualisation visualisation) {
            Editor visEditor;
            if(!abstractVisualisationEditors.TryGetValue(visualisation, out visEditor)) {
                visEditor = CreateEditor(visualisation);
                abstractVisualisationEditors[visualisation] = visEditor;
            }
            return visEditor;
        }

        private void UpdateVisualisationProperty(IATKProperty property)
        {
            switch (property)
            {
                case IATKProperty.X:
                    targetVisualisation.SetXDimension(targetVisualisation.XDimension);
                    break;
                case IATKProperty.Y:
                    targetVisualisation.SetYDimension(targetVisualisation.YDimension);
                    break;
                case IATKProperty.Z:
                    targetVisualisation.SetZDimension(targetVisualisation.ZDimension);
                    break;
                case IATKProperty.Colour:
                    targetVisualisation.SetColour(targetVisualisation.Colour);
                    break;
                case IATKProperty.ColourBy:
                    targetVisualisation.SetColourBy(targetVisualisation.ColourBy);
                    break;
                case IATKProperty.ColourGradient:
                    targetVisualisation.SetColourGradient(targetVisualisation.ColourGradient);
                    break;
                case IATKProperty.Size:
                    targetVisualisation.SetSize(targetVisualisation.Size);
                    break;
                case IATKProperty.SizeBy:
                    targetVisualisation.SetSizeBy(targetVisualisation.SizeBy);
                    break;
                case IATKProperty.Scale:
                    targetVisualisation.SetScale(targetVisualisation.Scale);
                    break;
            }
        }
    }

}   // Namespace
