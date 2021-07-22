using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;


namespace NewIATK
{
    [ExecuteInEditMode]
    public class Visualisation : MonoBehaviour
    {
        [field:SerializeField] public string UID { get; private set; }
        [field:SerializeField] public DataSource DataSource { get; private set; }
        [field:SerializeField] public AttributeFilter XDimension { get; private set; } = new AttributeFilter { Name = "Undefined" };
        [field:SerializeField] public AttributeFilter YDimension { get; private set; } = new AttributeFilter { Name = "Undefined" };
        [field:SerializeField] public AttributeFilter ZDimension { get; private set; } = new AttributeFilter { Name = "Undefined" };
        [field:SerializeField] public Color Colour { get; private set; } = Color.white;
        [field:SerializeField] public string ColourBy { get; private set; } = "Undefined";
        [field:SerializeField] public Gradient ColourGradient { get; private set; } = new Gradient();
        [field:SerializeField] public float Size { get; private set; } = 0.3f;
        [field:SerializeField] public string SizeBy { get; private set; } = "Undefined";
        [field:SerializeField] public Vector3 Scale { get; private set; } = Vector3.one;

        public List<AbstractVisualisation> SubVisualisations = new List<AbstractVisualisation>();

        public void CreateScatterplot()
        {
            GameObject holder = new GameObject("ScatterplotVisualisation");
            holder.transform.parent = transform;
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;

            ScatterplotVisualisation scatterplot = holder.AddComponent<ScatterplotVisualisation>();
            scatterplot.TransferProperties(this);
            scatterplot.CreateView(this);

            SubVisualisations.Add(scatterplot);
        }

        public void CreateBarchart()
        {
            GameObject holder = new GameObject("BarVisualisation");
            holder.transform.parent = transform;
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;

            BarVisualisation bar = holder.AddComponent<BarVisualisation>();
            bar.TransferProperties(this);
            bar.CreateView(this);

            SubVisualisations.Add(bar);
        }

        public void VisualisationDestroyed(AbstractVisualisation abstractVisualisation)
        {
            SubVisualisations.Remove(abstractVisualisation);
        }

        public void SetXDimension(AttributeFilter xDimension)
        {
            XDimension = xDimension;
            foreach (var vis in SubVisualisations)
                vis.SetXDimension(xDimension, false);
        }

        public void SetYDimension(AttributeFilter yDimension)
        {
            YDimension = yDimension;
            foreach (var vis in SubVisualisations)
                vis.SetYDimension(yDimension, false);
        }

        public void SetZDimension(AttributeFilter zDimension)
        {
            ZDimension = zDimension;
            foreach (var vis in SubVisualisations)
                vis.SetZDimension(zDimension, false);
        }

        public void SetColour(Color colour)
        {
            Colour = colour;
            foreach (var vis in SubVisualisations)
                vis.SetColour(colour, false);
        }

        public void SetColourBy(string attribute)
        {
            ColourBy = attribute;
            foreach (var vis in SubVisualisations)
                vis.SetColourBy(attribute, false);
        }

        public void SetColourGradient(Gradient colourGradient)
        {
            ColourGradient = colourGradient;
            foreach (var vis in SubVisualisations)
                vis.SetColourGradient(colourGradient, false);
        }

        public void SetSize(float size)
        {
            Size = size;
            foreach (var vis in SubVisualisations)
                vis.SetSize(size, false);
        }

        public void SetSizeBy(string attribute)
        {
            SizeBy = attribute;
            foreach (var vis in SubVisualisations)
                vis.SetSizeBy(attribute, false);
        }

        public void SetScale(Vector3 scale)
        {
            Scale = scale;
            foreach (var vis in SubVisualisations)
                vis.SetScale(scale, false);
        }
    }
}