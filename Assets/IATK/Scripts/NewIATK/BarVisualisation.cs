using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewIATK
{
    public enum IATKBarAggregation
    {
        None,
        Count,
        Sum,
        Average,
        Median,
        Min,
        Max
    }

    public class BarVisualisation : AbstractVisualisation
    {
        public override IATKVisualisationType VisualisationType { get { return IATKVisualisationType.Bar; } }

        [field: SerializeField] public int NumXBins { get; protected set; } = 1;
        [field: SerializeField] public int NumZBins { get; protected set; } = 1;
        [field: SerializeField] public IATKBarAggregation BarAggregation { get; protected set; } = IATKBarAggregation.None;

        public override void CreateView(Visualisation visualisationReference)
        {
            VisualisationReference = visualisationReference;
            SetDataSource(visualisationReference.DataSource);

            // Create view if it does not yet exist
            if (View == null)
            {
                GameObject viewHolder = new GameObject("View");
                viewHolder.transform.SetParent(transform);
                viewHolder.transform.localPosition = Vector3.zero;
                viewHolder.transform.localRotation = Quaternion.identity;
                View = viewHolder.AddComponent<View>();
            }

            // Create material and view
            Material material = new Material(Shader.Find("IATK/BarShader"));
            material.renderQueue = 3000;
            material.enableInstancing = true;

            View.CreateView(GenerateVertices(),
                            GenerateBarIndices(),
                            GenerateColours(),
                            GenerateVertices(),
                            GenerateUVs(),
                            MeshTopology.Points,
                            material
                            );

            if (XDimension.Name != "Undefined")
                CreateAxis(IATKDimension.X);
            if (YDimension.Name != "Undefined")
                CreateAxis(IATKDimension.Y);
            if (ZDimension.Name != "Undefined")
                CreateAxis(IATKDimension.Z);

            UpdateView(IATKProperty.X);
            UpdateView(IATKProperty.Z);
            UpdateView(IATKProperty.Size);

            isInitialised = true;
        }

        public override void UpdateView(IATKProperty property)
        {
            if (!isInitialised)
                return;

            if (View == null)
                return;

            switch (property)
            {
                case IATKProperty.X:
                    if (XDimension.Name == "Undefined")
                    {
                        View.SetVertices(new float[DataSource.DataCount], IATKDimension.X);
                        DestroyAxis(IATKDimension.X);
                    }
                    else
                    {
                        float[] xPositions = SetBinnedDataDimension(DataSource[XDimension.Name].Data, NumXBins, IsDimensionCategorical(XDimension.Name));
                        View.SetVertices(xPositions, IATKDimension.X);

                        if (XAxis == null)
                            CreateAxis(IATKDimension.X);
                        else
                            UpdateAxisDimensionAndRanges(IATKDimension.X);
                    }
                    UpdateView(IATKProperty.Y);
                    break;

                case IATKProperty.Y:
                    float[] yPositions;
                    if (YDimension.Name == "Undefined")
                    {
                        yPositions = SetAggregatedDimension(null, IATKBarAggregation.Count);
                    }
                    // If the aggregation type is not set, just use the raw position
                    else if (BarAggregation == IATKBarAggregation.None)
                    {
                        yPositions = DataSource[YDimension.Name].Data;

                        // Override the array such that all bars are shown
                        float[] oneArray = new float[DataSource.DataCount];
                        for (int i = 0; i < DataSource.DataCount; i++)
                            oneArray[i] = 1;
                        View.SetUVs(oneArray, IATKDimension.Y);
                    }
                    else
                    {
                        yPositions = SetAggregatedDimension(DataSource[YDimension.Name].Data, BarAggregation);
                    }

                    View.SetVertices(yPositions, IATKDimension.Y);
                    UpdateView(IATKProperty.Size);
                    break;

                case IATKProperty.Z:
                    if (ZDimension.Name == "Undefined")
                    {
                        View.SetVertices(new float[DataSource.DataCount], IATKDimension.Z);
                        DestroyAxis(IATKDimension.Z);
                    }
                    else
                    {
                        float[] zPositions = SetBinnedDataDimension(DataSource[ZDimension.Name].Data, NumZBins, IsDimensionCategorical(ZDimension.Name));
                        View.SetVertices(zPositions, IATKDimension.Z);

                        if (ZAxis == null)
                            CreateAxis(IATKDimension.Z);
                        else
                            UpdateAxisDimensionAndRanges(IATKDimension.Z);
                    }
                    UpdateView(IATKProperty.Y);
                    break;

                case IATKProperty.Size:
                    float xBins = NumXBins;
                    float zBins = NumZBins;

                    if (XDimension.Name != "Undefined" && IsDimensionCategorical(XDimension.Name))
                        xBins = DataSource[XDimension.Name].Data.Distinct().Count();
                    if (ZDimension.Name != "Undefined" && IsDimensionCategorical(ZDimension.Name))
                        zBins = DataSource[ZDimension.Name].Data.Distinct().Count();

                    if (XDimension.Name == "Undefined")
                        View.SetFloatProperty("_Width", 0.005f);
                    else
                        View.SetFloatProperty("_Width", 1 / xBins / 2f);
                    if (ZDimension.Name == "Undefined")
                        View.SetFloatProperty("_Depth", 0.005f);
                    else
                        View.SetFloatProperty("_Depth", 1 / zBins / 2f);
                    break;

                case IATKProperty.Colour:
                    if (ColourBy == "Undefined")
                    {
                        View.SetColour(Colour);
                    }
                    break;
            }
        }

        public void SetNumXBins(int numXBins)
        {
            NumXBins = numXBins;
            UpdateView(IATKProperty.X);
        }

        public void SetNumZBins(int numZBins)
        {
            NumZBins = numZBins;
            UpdateView(IATKProperty.Z);
        }

        public void SetBarAggregation(IATKBarAggregation barAggregation)
        {
            BarAggregation = barAggregation;
            UpdateView(IATKProperty.Y);
        }

        #region Visualisation Specific Methods

        private Vector3[] GenerateVertices()
        {
            Vector3[] vertices = new Vector3[DataSource.DataCount];
            int total = DataSource.DataCount;
            if (XDimension.Name != "Undefined")
            {
                float[] values = DataSource[XDimension.Name].Data;
                for (int i = 0; i < DataSource.DataCount; i++)
                {
                    vertices[i].x = values[i];
                }
            }
            if (YDimension.Name != "Undefined")
            {
                float[] values = DataSource[YDimension.Name].Data;
                for (int i = 0; i < DataSource.DataCount; i++)
                {
                    vertices[i].y = values[i];
                }
            }
            if (ZDimension.Name != "Undefined")
            {
                float[] values = DataSource[ZDimension.Name].Data;
                for (int i = 0; i < DataSource.DataCount; i++)
                {
                    vertices[i].z = values[i];
                }
            }
            return vertices;
        }

        private int[] GenerateBarIndices()
        {
            int[] indices = new int[DataSource.DataCount];
            for (int i = 0; i < DataSource.DataCount; i++)
            {
                indices[i] = i;
            }
            return indices;
        }

        private Color[] GenerateColours()
        {
            Color[] colours = new Color[DataSource.DataCount];
            for (int i = 0; i < DataSource.DataCount; i++)
            {
                colours[i] = Colour;
            }
            return colours;
        }

        private Vector4[] GenerateUVs()
        {
            Vector4[] uvs = new Vector4[DataSource.DataCount];
            for (int i = 0; i < DataSource.DataCount; i++)
            {
                uvs[i].x = i;
                uvs[i].y = Size;
                uvs[i].w = Size;
            }
            return uvs;
        }

        private bool IsDimensionCategorical(string dimension)
        {
            var type = DataSource[dimension].MetaData.Type;
            return (type == IATKDataType.String || type == IATKDataType.Date);
        }

        /// <summary>
        /// Creates an array of positions that are binned
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dimension"></param>
        /// <param name="numBins"></param>
        /// <param name="isCategorical"></param>
        /// <returns></returns>
        public float[] SetBinnedDataDimension(float[] data, int numBins, bool isCategorical = false)
        {
            IATK.DiscreteBinner binner = new IATK.DiscreteBinner();
            // If the dimension is categorical, numBins is fixed to the number of distinct values in it
            if (isCategorical)
                numBins = data.Distinct().Count();
            binner.MakeIntervals(data, numBins);

            float[] positions = new float[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                // Position such that the centre of the bar is never at 0 or 1
                float value = binner.Bin(data[i]);
                value = (value * 2 + 1) / (float)(numBins * 2);
                positions[i] = value;
            }

            return positions;
        }

        /// <summary>
        /// Creates an array of positions that are aggregated based on the given aggregation type.
        /// This MUST be called AFTER each time the other dimensions change.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dimension"></param>
        /// <param name="aggregation"></param>
        /// <returns></returns>
        public float[] SetAggregatedDimension(float[] yData, IATKBarAggregation aggregation)
        {
            // Extract independent arrays of the position values for the x and z dimensions
            Vector3[] vertices = View.GetVertices();
            float[] xData = new float[vertices.Length];
            float[] zData = new float[vertices.Length];
            for (int i = 0; i < DataSource.DataCount; i++)
            {
                xData[i] = vertices[i].x;
                zData[i] = vertices[i].z;
            }

            // Get the unique "categories" of the x and z dimensions (these are technically floats)
            var xCategories = xData.Distinct();
            var zCategories = zData.Distinct();

            // LAZY HACK: Set a value in the mesh's normal.y value to designate whether to show or hide the point to prevent z-fighting and mass lag
            float[] masterBars = new float[DataSource.DataCount];

            // Create a dictionary that will store the values assocatied with each (x, z) pairs of aggregating values (x bins * z bins = n lists)
            Dictionary<float, Dictionary<float, List<float>>> aggGroups = new Dictionary<float, Dictionary<float, List<float>>>();
            // Iterate through each position and assign the data values to the respective (x, z) pair
            for (int i = 0; i < DataSource.DataCount; i++)
            {
                Dictionary<float, List<float>> innerDict;
                if (!aggGroups.TryGetValue(xData[i], out innerDict))
                {
                    innerDict = new Dictionary<float, List<float>>();
                    aggGroups[xData[i]] = innerDict;
                }

                List<float> innerList;
                if (!innerDict.TryGetValue(zData[i], out innerList))
                {
                    innerList = new List<float>();
                    innerDict[zData[i]] = innerList;
                    masterBars[i] = 1;
                }

                // If the aggregation type is count, we don't need to use the y axis values
                if (aggregation == IATKBarAggregation.Count || yData == null)
                    innerList.Add(0);
                else
                    innerList.Add(yData[i]);
            }

            // LAZY HACK: Send the master values to the mesh now
            View.SetUVs(masterBars, IATKDimension.Y);

            // Create another dictionary that will store the aggregated value for each (x, z) pair group
            float max = 0;
            Dictionary<float, Dictionary<float, float>> aggregatedValues = new Dictionary<float, Dictionary<float, float>>();
            foreach (float xCategory in xCategories)
            {
                foreach (float zCategory in zCategories)
                {
                    // Calculate final aggregated value
                    if (!aggGroups[xCategory].ContainsKey(zCategory))
                        continue;

                    List<float> values = aggGroups[xCategory][zCategory];
                    float aggregated = 0;
                    switch (aggregation)
                    {
                        case IATKBarAggregation.Count:
                            aggregated = values.Count;
                            break;
                        case IATKBarAggregation.Average:
                            aggregated = values.Average();
                            break;
                        case IATKBarAggregation.Sum:
                            aggregated = values.Sum();
                            break;
                        case IATKBarAggregation.Median:
                            values.Sort();
                            float mid = (values.Count - 1) / 2f;
                            aggregated = (values[(int)(mid)] + values[(int)(mid + 0.5f)]) / 2;
                            break;
                        case IATKBarAggregation.Min:
                            aggregated = values.Min();
                            break;
                        case IATKBarAggregation.Max:
                            aggregated = values.Max();
                            break;
                    }

                    // Set value
                    Dictionary<float, float> innerDict;
                    if (!aggregatedValues.TryGetValue(xCategory, out innerDict))
                    {
                        innerDict = new Dictionary<float, float>();
                        aggregatedValues[xCategory] = innerDict;
                    }
                    innerDict[zCategory] = aggregated;

                    // We need to normalise back into 0..1 for these specific aggregations, so we collect the max value
                    if (aggregation == IATKBarAggregation.Count || aggregation == IATKBarAggregation.Sum)
                    {
                        if (max < aggregated)
                            max = aggregated;
                    }
                }
            }

            // Set y position based on newly aggregated values
            float[] positions = new float[DataSource.DataCount];
            for (int i = 0; i < DataSource.DataCount; i++)
            {
                // For specific aggregations, normalise
                if (aggregation == IATKBarAggregation.Count || aggregation == IATKBarAggregation.Sum)
                {
                    positions[i] = UtilMath.NormaliseValue(aggregatedValues[xData[i]][zData[i]], 0, max, 0, 1);
                }
                else
                {
                    positions[i] = aggregatedValues[xData[i]][zData[i]];
                }
            }

            return positions;
        }

        #endregion
    }
}