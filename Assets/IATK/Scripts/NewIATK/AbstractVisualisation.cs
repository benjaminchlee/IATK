using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewIATK
{
    public enum IATKDimension
    {
        X,
        Y,
        Z,
        W
    }

    public enum IATKProperty
    {
        None,
        DataSource,
        X,
        Y,
        Z,
        Colour,
        ColourBy,
        ColourGradient,
        Size,
        SizeBy,
        DimensionChange,
        AttributeFiltering,
        Scale
        // GeometryType,
        // LinkingDimension,
        // OriginDimension,
        // DestinationDimension,
        // GraphDimension,
        // DimensionFiltering,
        // Scaling,
        // BlendSourceMode,
        // BlendDestinationMode,
        // AttributeFiltering,
        // DimensionChange,
        // VisualisationType,
        // SizeValues,
        // DimensionChangeFiltering,
        // VisualisationWidth,
        // VisualisationHeight,
        // VisualisationLength
    }

    public enum IATKVisualisationType
    {
        Scatterplot
    }

    public abstract class AbstractVisualisation : MonoBehaviour
    {
        [HideInInspector] public Visualisation VisualisationReference;
        [HideInInspector] public View View;
        [HideInInspector] public Axis XAxis, YAxis, ZAxis;

        public DataSource DataSource { get; private set; }
        public AttributeFilter XDimension { get; private set; } = new AttributeFilter() { Name = "Undefined" };
        public AttributeFilter YDimension { get; private set; } = new AttributeFilter() { Name = "Undefined" };
        public AttributeFilter ZDimension { get; private set; } = new AttributeFilter() { Name = "Undefined" };
        public Color Colour { get; private set; } = Color.white;
        public string ColourBy { get; private set; } = "Undefined";
        public Gradient ColourGradient { get; private set; } = new Gradient();
        public float Size { get; private set; } = 0.3f;
        public string SizeBy { get; private set; } = "Undefined";
        public Vector3 Scale { get; private set; } = Vector3.one;

        protected Dictionary<IATKProperty, bool> inheritanceDictionary;
        protected bool isInitialised = false;

        protected static string serialisedObjectPath = "SerializedFields";

        public abstract IATKVisualisationType VisualisationType { get; }

        protected void Awake()
        {
            inheritanceDictionary = new Dictionary<IATKProperty, bool>();
            foreach (var value in (IATKProperty[])System.Enum.GetValues(typeof(IATKProperty)))
            {
                inheritanceDictionary[value] = true;
            }
        }

        public abstract void CreateView(Visualisation visualisationReference);

        public abstract void UpdateView(IATKProperty property);

        public void TransferProperties(Visualisation visualisationReference)
        {
            DataSource = visualisationReference.DataSource;
            XDimension = visualisationReference.XDimension;
            YDimension = visualisationReference.YDimension;
            ZDimension = visualisationReference.ZDimension;
            Colour = visualisationReference.Colour;
            ColourBy = visualisationReference.ColourBy;
            ColourGradient = visualisationReference.ColourGradient;
            Size = visualisationReference.Size;
            SizeBy = visualisationReference.SizeBy;
        }

        protected Axis CreateAxis(IATKDimension dimension)
        {
            GameObject axisHolder;
            axisHolder = (GameObject)Instantiate(Resources.Load("NewAxis"));
            axisHolder.transform.parent = transform;
            axisHolder.transform.localPosition = Vector3.zero;
            axisHolder.transform.localRotation = Quaternion.identity;

            Axis axis = axisHolder.GetComponent<Axis>();
            switch (dimension)
            {
                case IATKDimension.X:
                    axis.Initialise(this, XDimension, dimension);
                    XAxis = axis;
                    break;
                case IATKDimension.Y:
                    axis.Initialise(this, YDimension, dimension);
                    YAxis = axis;
                    break;
                case IATKDimension.Z:
                    axis.Initialise(this, ZDimension, dimension);
                    ZAxis = axis;
                    break;
            }
            return axis;
        }

        protected void UpdateAxisDimensionAndRanges(IATKDimension dimension)
        {
            Axis axis = null;
            AttributeFilter attributeFilter = null;

            switch (dimension)
            {
                case IATKDimension.X:
                    axis = XAxis;
                    attributeFilter = XDimension;
                    break;
                case IATKDimension.Y:
                    axis = YAxis;
                    attributeFilter = YDimension;
                    break;
                case IATKDimension.Z:
                    axis = ZAxis;
                    attributeFilter = ZDimension;
                    break;
            }

            if (axis == null || attributeFilter.Name == "Undefined")
                return;

            axis.UpdateAttribute(attributeFilter);
            axis.UpdateMinFilter(attributeFilter.MinFilter);
            axis.UpdateMaxFilter(attributeFilter.MaxFilter);
            axis.UpdateMinNormaliser(attributeFilter.MinScale);
            axis.UpdateMaxNormaliser(attributeFilter.MaxScale);
        }

        protected void UpdateAxisLength(IATKDimension length)
        {

        }

        protected void DestroyAxis(IATKDimension dimension)
        {
            switch (dimension)
            {
                case IATKDimension.X:
                    Destroy(XAxis.gameObject);
                    XAxis = null;
                    return;
                case IATKDimension.Y:
                    Destroy(YAxis.gameObject);
                    YAxis = null;
                    return;
                case IATKDimension.Z:
                    Destroy(ZAxis.gameObject);
                    ZAxis = null;
                    return;
            }
        }

        protected void OnDestroy()
        {
            VisualisationReference.VisualisationDestroyed(this);
        }

        public void SetDataSource(DataSource dataSource, bool breakInheritance = true)
        {
            if (inheritanceDictionary[IATKProperty.DataSource] && breakInheritance)
                inheritanceDictionary[IATKProperty.DataSource] = false;
            if (!inheritanceDictionary[IATKProperty.DataSource] && !breakInheritance)
                return;
            DataSource = dataSource;
        }

        public void SetXDimension(AttributeFilter xDimension, bool breakInheritance = true)
        {
            if (!InheritFromParent(IATKProperty.X, breakInheritance))
            {
                XDimension = xDimension;
                UpdateView(IATKProperty.X);
            }
        }

        public void SetYDimension(AttributeFilter yDimension, bool breakInheritance = true)
        {
            if (!InheritFromParent(IATKProperty.X, breakInheritance))
            {
                YDimension = yDimension;
                UpdateView(IATKProperty.Y);
            }
        }

        public void SetZDimension(AttributeFilter zDimension, bool breakInheritance = true)
        {
            if (!InheritFromParent(IATKProperty.Z, breakInheritance))
            {
                ZDimension = zDimension;
                UpdateView(IATKProperty.Z);
            }
        }

        public void SetColour(Color colour, bool breakInheritance = true)
        {
            if (!InheritFromParent(IATKProperty.Colour, breakInheritance))
            {
                Colour = colour;
                UpdateView(IATKProperty.Colour);
            }
        }

        public void SetColourBy(string attribute, bool breakInheritance = true)
        {
            if (!InheritFromParent(IATKProperty.ColourBy, breakInheritance))
            {
                ColourBy = attribute;
                UpdateView(IATKProperty.ColourBy);
            }
        }

        public void SetColourGradient(Gradient colourGradient, bool breakInheritance = true)
        {
            if (!InheritFromParent(IATKProperty.ColourGradient, breakInheritance))
            {
                ColourGradient = colourGradient;
                UpdateView(IATKProperty.ColourGradient);
            }
        }

        public void SetSize(float size, bool breakInheritance = true)
        {
            if (!InheritFromParent(IATKProperty.Size, breakInheritance))
            {
                Size = size;
                UpdateView(IATKProperty.Size);
            }
        }

        public void SetSizeBy(string attribute, bool breakInheritance = true)
        {
            if (!InheritFromParent(IATKProperty.SizeBy, breakInheritance))
            {
                SizeBy = attribute;
                UpdateView(IATKProperty.SizeBy);
            }
        }

        public void SetScale(Vector3 scale, bool breakInheritance = true)
        {
            if (!InheritFromParent(IATKProperty.Scale, breakInheritance))
            {
                Scale = scale;
                UpdateView(IATKProperty.Scale);
            }
        }

        protected bool InheritFromParent(IATKProperty property, bool breakInheritance)
        {
            if (inheritanceDictionary[property] && breakInheritance)
                inheritanceDictionary[property] = false;
            if (!inheritanceDictionary[property] && !breakInheritance)
                return true;
            return false;
        }

    }
}