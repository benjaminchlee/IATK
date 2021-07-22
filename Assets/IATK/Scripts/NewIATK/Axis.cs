using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace NewIATK
{
    public class Axis : MonoBehaviour
    {
        [SerializeField] [Tooltip("The tip (cone) of the axis.")]
        private Transform axisTip;
        [SerializeField] [Tooltip("The rod (cylinder) of the axis.")]
        private Transform axisRod;
        [SerializeField] [Tooltip("The main attribute label of this axis.")]
        private TextMeshPro attributeLabel;
        [SerializeField] [Tooltip("The GameObject which holds all of the axis value labels.")]
        private GameObject axisTickLabelHolder;
        [SerializeField] [Tooltip("The base axis tick label to duplicate and use.")]
        private GameObject axisTickLabelPrefab;
        [SerializeField] [Tooltip("The minimum normaliser handle.")]
        private Transform minNormaliserObject;
        [SerializeField] [Tooltip("The maximum normaliser handle.")]
        private Transform maxNormaliserObject;
        [SerializeField] [Tooltip("The minimum filter handle.")]
        private Transform minFilterObject;
        [SerializeField] [Tooltip("The maximum normaliser handle.")]
        private Transform maxFilterObject;

        public AbstractVisualisation AbstractVisualisation { get; private set; }
        public DataSource DataSource { get; private set; }
        public AttributeFilter AttributeFilter { get; private set; }
        public int AttributeDataSourceIndex { get; private set; }
        public IATKDimension Dimension { get; private set; }
        public float Length { get; private set; } = 1f;
        public float AxisTickSpacing { get; private set; } = 0.075f;

        public void Initialise(AbstractVisualisation abstractVisualisation, AttributeFilter attributeFilter, IATKDimension dimension)
        {
            AbstractVisualisation = abstractVisualisation;
            DataSource = abstractVisualisation.DataSource;
            AttributeFilter = attributeFilter;
            AttributeDataSourceIndex = Array.IndexOf(AbstractVisualisation.DataSource.Select(m => m.Identifier).ToArray(), attributeFilter.Name);

            SetDirection(dimension);
            attributeLabel.text = AttributeFilter.Name;
            UpdateTickLabels();

            axisTickLabelPrefab.SetActive(false);
        }

        public void UpdateAttribute(AttributeFilter attributeFilter)
        {
            AttributeFilter = attributeFilter;
            AttributeDataSourceIndex = Array.IndexOf(AbstractVisualisation.DataSource.Select(m => m.Identifier).ToArray(), attributeFilter.Name);
            attributeLabel.text = AttributeFilter.Name;
            UpdateTickLabels();
        }

        public void UpdateLength(float length)
        {
            Length = length;

            axisRod.localScale = new Vector3(axisRod.localScale.x, Length, axisRod.localScale.z);
            axisTip.localPosition = new Vector3(axisTip.localPosition.x, Length, axisTip.localPosition.z);
            axisTip.localEulerAngles = new Vector3(length >= 0 ? 0 : 180, -45, 0);

            UpdateMinFilter(AttributeFilter.MinFilter);
            UpdateMaxFilter(AttributeFilter.MaxFilter);
            UpdateMinNormaliser(AttributeFilter.MinScale);
            UpdateMaxNormaliser(AttributeFilter.MaxScale);

            // Update attribute label position
            SetYLocalPosition(attributeLabel.transform, Length * 0.5f);

            UpdateTickLabels();
        }

        public void UpdateMinFilter(float minFilter)
        {
            AttributeFilter.MinFilter = Mathf.Clamp(minFilter, 0, 1);
            SetYLocalPosition(minFilterObject, minFilter * Length);
        }

        public void UpdateMaxFilter(float maxFilter)
        {
            AttributeFilter.MaxFilter = Mathf.Clamp(maxFilter, 0, 1);
            SetYLocalPosition(maxFilterObject, maxFilter * Length);
        }

        public void UpdateMinNormaliser(float minNormaliser)
        {
            AttributeFilter.MinScale = Mathf.Clamp(minNormaliser, 0, 1);
            SetYLocalPosition(minNormaliserObject, minNormaliser * Length);
        }

        public void UpdateMaxNormaliser(float maxNormaliser)
        {
            AttributeFilter.MaxScale = Mathf.Clamp(maxNormaliser, 0, 1);
            SetYLocalPosition(maxNormaliserObject, maxNormaliser * Length);
        }

        private void UpdateTickLabels()
        {
            List<GameObject> axisTickLabels = GetAxisTickLabels();
            int currentNumberOfLabels = axisTickLabels.Count;
            int targetNumberOfLabels = CalculateNumAxisTickLabels();

            if (currentNumberOfLabels != targetNumberOfLabels)
            {
                DestroyAxisTickLabels();

                // Create new labels
                for (int i = 0; i < targetNumberOfLabels; i++)
                {
                    Instantiate(axisTickLabelPrefab, axisTickLabelHolder.transform);
                }
            }

            // Update label positions and text
            axisTickLabels = GetAxisTickLabels();
            for (int i = 0; i < targetNumberOfLabels; i++)
            {
                GameObject label = axisTickLabels[i];
                label.SetActive(true);
                float y = GetAxisTickLabelPosition(i, targetNumberOfLabels);
                SetYLocalPosition(label.transform, y * Length);

                TextMeshPro labelText = label.GetComponentInChildren<TextMeshPro>();
                labelText.gameObject.SetActive(y >= 0.0f && y <= 1.0f);
                labelText.text = GetAxisTickLabelText(i, targetNumberOfLabels);
                labelText.color = new Color(1, 1, 1, GetAxisTickLabelFiltered(i, targetNumberOfLabels) ? 0.4f : 1.0f);
            }
        }

        private void SetDirection(IATKDimension dimension)
        {
            Dimension = dimension;

            switch (Dimension)
            {
                case IATKDimension.X:
                    // Fix the alignment of the axis tick labels
                    foreach (Transform child in axisTickLabelHolder.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.gameObject.name.Contains("Text"))
                        {
                            TextMeshPro labelText = child.GetComponent<TextMeshPro>();
                            labelText.alignment = TextAlignmentOptions.MidlineLeft;
                            labelText.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                        }
                        else if (child.gameObject.name.Contains("Tick"))
                        {
                            SetXLocalPosition(child, -child.localPosition.x);
                        }
                    }
                    transform.localEulerAngles = new Vector3(0, 0, -90);
                    SetXLocalPosition(axisTickLabelHolder.transform, 0);
                    attributeLabel.alignment = TextAlignmentOptions.Top;
                    attributeLabel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                    break;

                case IATKDimension.Y:
                    transform.localEulerAngles = new Vector3(0, 0, 0);
                    SetXLocalPosition(minNormaliserObject, -minNormaliserObject.transform.localPosition.x);
                    SetXLocalPosition(maxNormaliserObject, -maxNormaliserObject.transform.localPosition.x);
                    minNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                    maxNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                    break;

                case IATKDimension.Z:
                    transform.localEulerAngles = new Vector3(90, 0, 0);
                    SetXLocalPosition(minNormaliserObject, -minNormaliserObject.transform.localPosition.x);
                    SetXLocalPosition(maxNormaliserObject, -maxNormaliserObject.transform.localPosition.x);
                    minNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                    maxNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                    break;
            }
        }

        /// <summary>
        /// Destroys all of the axis tick labels on this axis, excluding the referenced base label.
        /// </summary>
        public void DestroyAxisTickLabels()
        {
            foreach (GameObject label in GetAxisTickLabels())
            {
                #if !UNITY_EDITOR
                Destroy(label);
                #else
                DestroyImmediate(label);
                #endif
            }
        }

        #region Private helper functions

        private List<GameObject> GetAxisTickLabels()
        {
            List<GameObject> labels = new List<GameObject>();
            foreach (Transform t in axisTickLabelHolder.transform)
            {
                if (t.gameObject.name.Contains("Clone"))
                {
                    labels.Add(t.gameObject);
                }
            }
            return labels;
        }

        private int CalculateNumAxisTickLabels()
        {
            if (IsAttributeDiscrete())
            {
                // If this axis dimension has been rescaled at all, don't show any ticks
                if (AttributeFilter.MinScale > 0.001f || AttributeFilter.MaxScale < 0.999f)
                    return 0;

                // If this discrete dimension has less unique values than the maximum number of ticks allowed due to spacing, give an axis tick label for each unique value
                int numValues = DataSource[AttributeFilter.Name].MetaData.CategoryCount;
                int maxTicks = Mathf.CeilToInt(Length / AxisTickSpacing);
                if (numValues < maxTicks)
                    return numValues;

                // Otherwise just use 2 labels
                else
                {
                    return 2;
                }
            }
            else
            {
                return Mathf.CeilToInt(Length / AxisTickSpacing);
            }
        }

        private bool IsAttributeDiscrete()
        {
            var type = DataSource[AttributeFilter.Name].MetaData.Type;

            return (type == IATKDataType.String || type == IATKDataType.Date);
        }

        private float GetAxisTickLabelPosition(int labelIndex, int numLabels)
        {
            if (numLabels == 1)
                return 0;

            return (labelIndex / (float) (numLabels - 1));
        }

        private string GetAxisTickLabelText(int labelIndex, int numLabels)
        {
            object v = DataSource.GetValueApproximate(Mathf.Lerp(AttributeFilter.MinScale, AttributeFilter.MaxScale, labelIndex / (numLabels - 1f)), AttributeFilter.Name);

            if (v is float && v.ToString().Length > 4)
            {
                return ((float)v).ToString("#,##0.0");
            }
            else
            {
                return v.ToString();
            }
        }

        private bool GetAxisTickLabelFiltered(int labelIndex, int numLabels)
        {
            float n = labelIndex / (float)(numLabels - 1);
            float delta = Mathf.Lerp(AttributeFilter.MinScale, AttributeFilter.MaxScale, n);
            return delta < AttributeFilter.MinFilter || delta > AttributeFilter.MaxFilter;
        }


        private void SetXLocalPosition(Transform t, float value)
        {
            var p = t.localPosition;
            p.x = value;
            t.localPosition = p;
        }

        private void SetYLocalPosition(Transform t, float value)
        {
            var p = t.localPosition;
            p.y = value;
            t.localPosition = p;
        }

        #endregion // Private helper functions
    }
}