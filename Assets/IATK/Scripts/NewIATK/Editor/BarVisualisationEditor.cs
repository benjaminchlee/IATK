using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NewIATK
{
    [CustomEditor(typeof(BarVisualisation))]
    [CanEditMultipleObjects]
    public class BarVisualisationEditor : IATKEditor
    {
        private SerializedProperty numXBinsProperty;
        private SerializedProperty numZBinsProperty;
        private SerializedProperty barAggregationProperty;

        private BarVisualisation targetBarchart;

        protected override void OnEnable()
        {
            base.OnEnable();

            targetBarchart = (BarVisualisation)serializedObject.targetObject;
            numXBinsProperty = serializedObject.FindProperty("<NumXBins>k__BackingField");
            numZBinsProperty = serializedObject.FindProperty("<NumZBins>k__BackingField");
            barAggregationProperty = serializedObject.FindProperty("<BarAggregation>k__BackingField");
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
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(numXBinsProperty);
                if (EditorGUI.EndChangeCheck())
                    dirtyFlag = IATKProperty.NumXBins;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(numZBinsProperty);
                if (EditorGUI.EndChangeCheck())
                    dirtyFlag = IATKProperty.NumZBins;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(barAggregationProperty);
                if (EditorGUI.EndChangeCheck())
                    dirtyFlag = IATKProperty.BarAggregation;
            }

            serializedObject.ApplyModifiedProperties();

            UpdateBarProperty(dirtyFlag);
        }

        private void UpdateBarProperty(IATKProperty property)
        {
            switch (property)
            {
                case IATKProperty.NumXBins:
                    targetBarchart.SetNumXBins(targetBarchart.NumXBins);
                    break;
                case IATKProperty.NumZBins:
                    targetBarchart.SetNumZBins(targetBarchart.NumZBins);
                    break;
                case IATKProperty.BarAggregation:
                    targetBarchart.SetBarAggregation(targetBarchart.BarAggregation);
                    break;
            }
        }
    }
}