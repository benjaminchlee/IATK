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
    public class VisualisationEditor : Editor
    {
        private SerializedProperty uidProperty;
        private SerializedProperty dataSourceProperty;
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
        private DataSource dataSource;
        private string undefinedString = "Undefined";

        private string[] dimensions;
        private IATKProperty dirtyFlag;

        void OnEnable()
        {
            targetVisualisation = (Visualisation)serializedObject.targetObject;
            uidProperty = serializedObject.FindProperty("UID");
            dataSourceProperty = serializedObject.FindProperty("DataSource");
            xDimensionProperty = serializedObject.FindProperty("XDimension");
            yDimensionProperty = serializedObject.FindProperty("YDimension");
            zDimensionProperty = serializedObject.FindProperty("ZDimension");
            colourProperty = serializedObject.FindProperty("Colour");
            colourByProperty = serializedObject.FindProperty("ColourBy");
            colourGradientProperty = serializedObject.FindProperty("ColourGradient");
            sizeProperty = serializedObject.FindProperty("Size");
            sizeByProperty = serializedObject.FindProperty("SizeBy");
            scaleProperty = serializedObject.FindProperty("Scale");

            LoadDataSource();
        }

        /// <summary>
        /// Draw the inspector and update Visualisation when a property changes
        /// </summary>
        public override void OnInspectorGUI()
        {
            dirtyFlag = IATKProperty.None;

            serializedObject.Update();

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
            }

            serializedObject.ApplyModifiedProperties();

            UpdateVisualisationProperty(dirtyFlag);

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

        private IATKProperty DrawAttributeFilterEditor(SerializedProperty element, Rect rect)
        {
            IATKProperty dirtyFlag = IATKProperty.None;

            Rect contentRect = rect;
            contentRect.height = EditorGUIUtility.singleLineHeight;

            Rect copyrect = rect;
            copyrect.height -= copyrect.height / 10f;

            EditorGUI.BeginChangeCheck();
            var attributeProp = element.FindPropertyRelative("Name");
            int attributeIndex = Array.IndexOf(dimensions, attributeProp.stringValue);

            if (attributeIndex >= 0)
            {
                attributeIndex = EditorGUI.Popup(contentRect, attributeIndex, dimensions);
                attributeProp.stringValue = dimensions[attributeIndex];
                if (EditorGUI.EndChangeCheck())
                {
                    dirtyFlag = IATKProperty.DimensionChange;
                }
            }

            EditorGUI.BeginDisabledGroup(attributeIndex < 1);

            contentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginChangeCheck();
            var minFilterProp = element.FindPropertyRelative("MinFilter");
            var maxFilterProp = element.FindPropertyRelative("MaxFilter");
            DrawMinMaxSlider(contentRect, minFilterProp, maxFilterProp, attributeProp.stringValue, dataSource);
            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlag = IATKProperty.DimensionChange;
            }

            contentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginChangeCheck();

            var minScaleProp = element.FindPropertyRelative("MinScale");
            var maxScaleProp = element.FindPropertyRelative("MaxScale");
            DrawMinMaxSlider(contentRect, minScaleProp, maxScaleProp, attributeProp.stringValue, dataSource);

            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlag = IATKProperty.DimensionChange;
            }

            EditorGUI.EndDisabledGroup();

            return dirtyFlag;
        }

        private void DrawMinMaxSlider(Rect rect, SerializedProperty minFilterProp, SerializedProperty maxFilterProp, string attributeid, DataSource dataSource)
        {
            bool isUndefined = dataSource == null || attributeid == "Undefined";
            int idx = Array.IndexOf(dataSource.Select(m => m.Identifier).ToArray(), attributeid);

            // get the normalized value
            float minValue = !isUndefined ? dataSource[attributeid].MetaData.Min : 0.0f;
            float maxValue = !isUndefined ? dataSource[attributeid].MetaData.Max : 1.0f;

            // calculate the real value
            float min = UtilMath.NormaliseValue(minFilterProp.floatValue, 0, 1, minValue, maxValue);
            float max = UtilMath.NormaliseValue(maxFilterProp.floatValue, 0, 1, minValue, maxValue);

            // get the string representation
            string minLogical = isUndefined ? "" : dataSource.GetValueApproximate(minFilterProp.floatValue, idx).ToString();
            string maxLogical = isUndefined ? "" : dataSource.GetValueApproximate(maxFilterProp.floatValue, idx).ToString();

            EditorGUI.TextField(new Rect(rect.x, rect.y, 75, rect.height), minLogical);
            EditorGUI.MinMaxSlider(new Rect(rect.x + 75, rect.y, rect.width - 150, rect.height), GUIContent.none, ref min, ref max, minValue, maxValue);
            EditorGUI.TextField(new Rect(rect.x + rect.width - 78, rect.y, 75, rect.height), maxLogical);

            minFilterProp.floatValue = UtilMath.NormaliseValue(min, minValue, maxValue, 0, 1);
            maxFilterProp.floatValue = UtilMath.NormaliseValue(max, minValue, maxValue, 0, 1);
        }

        /// <summary>
        /// Enums the popup.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <param name="enumArray">Enum array.</param>
        /// <param name="selected">Selected.</param>
        private bool EnumPopup(string label, string[] enumArray, SerializedProperty selected)
        {
            string oldSelected = selected.stringValue;

            if (enumArray.Length > 0)
            {
                selected.stringValue = enumArray[EditorGUILayout.Popup(label, EnumIndexOf(enumArray, selected.stringValue), enumArray)];
            }

            return selected.stringValue != oldSelected;
        }

        /// <summary>
        /// Find the enum index of an array
        /// </summary>
        /// <returns>The index of.</returns>
        /// <param name="stringArray">String array.</param>
        /// <param name="toFind">To find.</param>
        private int EnumIndexOf(string[] stringArray, string toFind)
        {
            int index = stringArray.ToList().IndexOf(toFind);

            return (index >= 0) ? index : 0;        // Return the "Undefined"
        }

        private void LoadDataSource()
        {
            if (dataSourceProperty != null)
            {
                dataSource = (DataSource)dataSourceProperty.objectReferenceValue;

                // Load the data source and its dimensions
                if (dataSource != null)
                {
                    if (!dataSource.IsLoaded)
                        dataSource.Load();

                    dimensions = new string[dataSource.DimensionCount + 1];
                    dimensions[0] = undefinedString;
                    for (int i = 0; i < dataSource.DimensionCount; i++)
                        dimensions[i + 1] = dataSource[i].Identifier;
                }
            }
        }

        private bool IsDataSourceLoaded()
        {
            return dataSourceProperty.objectReferenceValue != null && dataSource.IsLoaded;
        }
    }

}   // Namespace
