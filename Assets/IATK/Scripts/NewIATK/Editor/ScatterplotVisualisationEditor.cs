using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NewIATK
{
    [CustomEditor(typeof(ScatterplotVisualisation))]
    [CanEditMultipleObjects]
    public class ScatterplotVisualisationEditor : IATKEditor
    {
        private SerializedProperty colourPaletteProperty;
        private SerializedProperty colourPaletteByProperty;

        private ScatterplotVisualisation targetScatterplot;

        protected override void OnEnable()
        {
            base.OnEnable();

            targetScatterplot = (ScatterplotVisualisation)serializedObject.targetObject;
            colourPaletteProperty = serializedObject.FindProperty("<ColourPalette>k__BackingField");
            colourPaletteByProperty = serializedObject.FindProperty("<ColourPaletteBy>k__BackingField");

            LoadDataSource();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            dirtyFlag = IATKProperty.None;

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
                if (EnumPopup("Colour palette by", dimensions, colourPaletteByProperty))
                {
                    dirtyFlag = IATKProperty.ColourPaletteBy;
                }
                // Colour palette
                if (colourPaletteByProperty.stringValue != "Undefined")
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(colourPaletteProperty);
                    if (EditorGUI.EndChangeCheck())
                        dirtyFlag = IATKProperty.ColourPalette;
                }
            }

            serializedObject.ApplyModifiedProperties();

            UpdateScatterplotProperty(dirtyFlag);
        }

        private void UpdateScatterplotProperty(IATKProperty property)
        {
            switch (property)
            {
                case IATKProperty.ColourPaletteBy:
                    targetScatterplot.SetColourPaletteBy(targetScatterplot.ColourPaletteBy);
                    break;

                case IATKProperty.ColourPalette:
                    targetScatterplot.SetColourPalette(targetScatterplot.ColourPalette);
                    break;
            }
        }
    }
}