using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NewIATK
{
    public abstract class IATKEditor : Editor
    {
        protected SerializedProperty dataSourceProperty;

        protected DataSource dataSource;
        protected string undefinedString = "Undefined";
        protected string[] dimensions;
        protected IATKProperty dirtyFlag;

        protected virtual void OnEnable()
        {
            dataSourceProperty = serializedObject.FindProperty("<DataSource>k__BackingField");

            LoadDataSource();
        }

        protected virtual IATKProperty DrawAttributeFilterEditor(SerializedProperty element, Rect rect)
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

        protected virtual void DrawMinMaxSlider(Rect rect, SerializedProperty minFilterProp, SerializedProperty maxFilterProp, string attributeid, DataSource dataSource)
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
        protected virtual bool EnumPopup(string label, string[] enumArray, SerializedProperty selected)
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
        protected virtual int EnumIndexOf(string[] stringArray, string toFind)
        {
            int index = stringArray.ToList().IndexOf(toFind);

            return (index >= 0) ? index : 0;        // Return the "Undefined"
        }

        protected virtual void LoadDataSource()
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

        protected virtual bool IsDataSourceLoaded()
        {
            return dataSourceProperty.objectReferenceValue != null && dataSource.IsLoaded;
        }
    }
}
