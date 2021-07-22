using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewIATK
{
    [ExecuteInEditMode]
    public class ScatterplotVisualisation : AbstractVisualisation
    {
        public override IATKVisualisationType VisualisationType { get { return IATKVisualisationType.Scatterplot; } }

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
            Material material = new Material(Shader.Find("IATK/OutlineDots"));
            material.mainTexture = Resources.Load("circle-outline-basic") as Texture2D;
            material.renderQueue = 3000;
            material.enableInstancing = true;

            View.CreateView(GenerateVertices(),
                            GenerateScatterplotIndices(),
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
                        View.SetVertices(DataSource[XDimension.Name].Data, IATKDimension.X);
                        if (XAxis == null)
                            CreateAxis(IATKDimension.X);
                        else
                            UpdateAxisDimensionAndRanges(IATKDimension.X);

                        View.SetFloatProperty("_MinNormX", XDimension.MinScale);
                        View.SetFloatProperty("_MaxNormX", XDimension.MaxScale);
                        View.SetFloatProperty("_MinX", XDimension.MinFilter);
                        View.SetFloatProperty("_MaxX", XDimension.MaxFilter);
                    }
                    break;

                case IATKProperty.Y:
                    if (YDimension.Name == "Undefined")
                    {
                        View.SetVertices(new float[DataSource.DataCount], IATKDimension.Y);
                        DestroyAxis(IATKDimension.Y);
                    }
                    else
                    {
                        View.SetVertices(DataSource[YDimension.Name].Data, IATKDimension.Y);
                        if (YAxis == null)
                            CreateAxis(IATKDimension.Y);
                        else
                            UpdateAxisDimensionAndRanges(IATKDimension.Y);

                        View.SetFloatProperty("_MinNormY", YDimension.MinScale);
                        View.SetFloatProperty("_MaxNormY", YDimension.MaxScale);
                        View.SetFloatProperty("_MinY", YDimension.MinFilter);
                        View.SetFloatProperty("_MaxY", YDimension.MaxFilter);
                    }
                    break;

                case IATKProperty.Z:
                    if (ZDimension.Name == "Undefined")
                    {
                        View.SetVertices(new float[DataSource.DataCount], IATKDimension.Z);
                        DestroyAxis(IATKDimension.Z);
                    }
                    else
                    {
                        View.SetVertices(DataSource[ZDimension.Name].Data, IATKDimension.Z);
                        if (ZAxis == null)
                            CreateAxis(IATKDimension.Z);
                        else
                            UpdateAxisDimensionAndRanges(IATKDimension.Z);

                        View.SetFloatProperty("_MinNormZ", ZDimension.MinScale);
                        View.SetFloatProperty("_MaxNormZ", ZDimension.MaxScale);
                        View.SetFloatProperty("_MinZ", ZDimension.MinFilter);
                        View.SetFloatProperty("_MaxZ", ZDimension.MaxFilter);
                    }
                    break;

                case IATKProperty.Colour:
                    if (ColourBy == "Undefined")
                    {
                        View.SetColour(Colour);
                    }
                    break;

                case IATKProperty.ColourBy:
                case IATKProperty.ColourGradient:
                    if (ColourBy != "Undefined")
                        View.SetColours(MapColoursContinuous(DataSource[ColourBy].Data));
                    else
                        UpdateView(IATKProperty.Colour);
                    break;

                case IATKProperty.Size:
                    View.SetUVs(Size, IATKDimension.Y);
                    break;

                case IATKProperty.SizeBy:
                    View.SetUVs(DataSource[SizeBy].Data, IATKDimension.Y);
                    break;

                case IATKProperty.Scale:
                    if (XAxis != null)
                        XAxis.UpdateLength(Scale.x);
                    if (YAxis != null)
                        YAxis.UpdateLength(Scale.y);
                    if (ZAxis != null)
                        ZAxis.UpdateLength(Scale.z);
                    View.transform.localScale = Scale;
                    break;

            }
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

        private int[] GenerateScatterplotIndices()
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

        private Color[] MapColoursContinuous(float[] data)
        {
            Color[] colours = new Color[data.Length];
            for (int i = 0; i < data.Length; ++i)
            {
                colours[i] = ColourGradient.Evaluate(data[i]);
            }
            return colours;
        }

        #endregion
    }
}