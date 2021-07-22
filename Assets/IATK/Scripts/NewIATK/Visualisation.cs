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
        public string UID;
        public DataSource DataSource;
        public AttributeFilter XDimension = new AttributeFilter { Name = "Undefined" };
        public AttributeFilter YDimension = new AttributeFilter { Name = "Undefined" };
        public AttributeFilter ZDimension = new AttributeFilter { Name = "Undefined" };
        public Color Colour = Color.white;
        public string ColourBy = "Undefined";
        public Gradient ColourGradient = new Gradient();
        [Range(0, 1)] public float Size = 0.3f;
        public string SizeBy = "Undefined";
        public Vector3 Scale = Vector3.one;

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