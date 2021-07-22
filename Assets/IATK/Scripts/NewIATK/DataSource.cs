using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewIATK
{
    /// <summary>
    /// Dimension data class
    /// </summary>
    public class DimensionData
    {
        /// <summary>
        /// Metadata.
        /// </summary>
        public struct Metadata
        {
            public IATKDataType Type;
            public float Min;
            public float Max;
            public int BinCount;
            public float[] Categories;
            public int CategoryCount;
        }

        public string Identifier { get; private set; }          // Textual identifier for this dimension
        public int Index { get; private set; }                  // Integer indentifier for this dimension
        public Metadata MetaData { get; private set; }          // The MetaData for this dimension
        public float[] Data { get; private set; }               // The data array for this dimension
        public Dictionary<string, Dictionary<int, string>> StringTable { get; private set; }     // String lookup table for dimension data

        /// <summary>
        /// Initializes a new instance of the <see cref="IATK.DataSource+DimensionData"/> class.
        /// </summary>
        /// <param name="identifier">Identifier.</param>
        /// <param name="data">Data.</param>
        public DimensionData(string identifier, int index, Metadata metaData)
        {
            Identifier = identifier;
            Index = index;
            MetaData = metaData;
        }

        /// <summary>
        /// Sets the data.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="stringTable">String table.</param>
        public void SetData(float[] data, Dictionary<string, Dictionary<int, string>> stringTable)
        {
            Data = data;
            StringTable = stringTable;

        }

        /// <summary>
        /// Sets the metadata.
        /// </summary>
        /// <param name="metadate">Metadate.</param>
        public void SetMetadata(Metadata metadata)
        {
            MetaData = metadata;
        }
    }

    /// <summary>
    /// Data source base class.
    /// Concrete classes will need to derive from this to provide an implementation.
    /// </summary>
    public abstract class DataSource : MonoBehaviour, IEnumerable<DimensionData>
    {

        /// <summary>
        /// Event fired when the data is loaded. Load the data using the <see cref="Load"/> function.
        /// </summary>
        public event DataLoad DataLoaded;
        public delegate void DataLoad();

        /// <summary>
        /// Gets a value indicating whether the data is loaded. Load the data using the <see cref="Load"/> function.
        /// </summary>
        /// <value><c>true</c> if if the data has been loaded, otherwise <c>false</c>.</value>
        public abstract bool IsLoaded
        {
            get;
        }

        /// <summary>
        /// Gets the dimension count in the data.
        /// </summary>
        /// <value>The number of dimensions.</value>
        public abstract int DimensionCount
        {
            get;
        }

        /// <summary>
        /// Gets the data count in the data.
        /// </summary>
        /// <value>The number of observations in the data.</value>
        public abstract int DataCount
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="DimensionData"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the dimension to get the data of.</param>
        /// <returns>The <see cref="DimensionData"/> of the dimension at the given index.</returns>
        public abstract DimensionData this[int index]
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="DimensionData"/> with the specified identifier.
        /// </summary>
        /// <param name="identifier">The string identifier of the dimension to get the data of.</param>
        /// <returns>The <see cref="DimensionData"/> of the dimension at the given identifier.</returns>
        public abstract DimensionData this[string identifier]
        {
            get;
        }

        /// <summary>
        /// Gets the data value at the given normalised value for a specified data dimension. In other words, this converts a normalised float representation of the data back into its original value.
        /// This function allows for approximate normalised values to be given.
        /// </summary>
        /// <param name="normalisedValue">A float within the range 0..1 to get the data value of. If no exact match is found, returns the closest approximate value.</param>
        /// <param name="identifier">The string identifier of the data dimension.</param>
        /// <returns>The de-normalised, original value of the data dimension.</returns>
        public abstract object GetValueApproximate(float normalisedValue, string identifier);

        /// <summary>
        /// Gets the data value at the given normalised value for a specified data dimension. In other words, this converts a normalised float representation of the data back into its original value.
        /// This function allows for approximate normalised values to be given.
        /// </summary>
        /// <param name="normalisedValue">A value within the range 0..1 to get the data value of. If no exact match is found, returns the closest approximate value.</param>
        /// <param name="identifier">The integer identifier of the data dimension.</param>
        /// <returns>The de-normalised, original value of the data dimension.</returns>
        public abstract object GetValueApproximate(float normalisedValue, int identifier);

        /// <summary>
        /// Gets the data value at the given normalised value for a specified data dimension. In other words, this converts a normalised float representation of the data back into its original value.
        /// </summary>
        /// <param name="normalisedValue">A value within the range 0..1 to get the data value of.</param>
        /// <param name="identifier">The string identifier of the data dimension.</param>
        /// <returns>The de-normalised, original value of the data dimension.</returns>
        public abstract object GetValuePrecise(float normalisedValue, string identifier);

        /// <summary>
        /// Gets the data value at the given normalised value for a specified data dimension. In other words, this converts a normalised float representation of the data back into its original value.
        /// </summary>
        /// <param name="normalisedValue">A value within the range 0..1 to get the data value of.</param>
        /// <param name="identifier">The integer identifier of the data dimension.</param>
        /// <returns>The de-normalised, original value of the data dimension.</returns>
        public abstract object GetValuePrecise(float normalisedValue, int identifier);


        /// <summary>
        /// Gets the number of categories for the specified data dimension. This is only particularly relevant for categorical dimensions.
        /// </summary>
        /// <param name="identifier">The string identifier of the data dimension.</param>
        /// <returns>The number of categories of this data dimension.</returns>
        public abstract int GetNumberOfCategories(int identifier);

        /// <summary>
        /// Gets the number of categories for the specified data dimension. This is only particularly relevant for categorical dimensions.
        /// </summary>
        /// <param name="identifier">The integer identifier of the data dimension.</param>
        /// <returns>The number of categories of this data dimension.</returns>
        public abstract int GetNumberOfCategories(string identifier);

        /// <summary>
        /// Loads the data from the given source(s). The specific sources depends on the concrete implementation.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Loads the header information from the data. This is available here so that it can be called from the Unity Editor.
        /// </summary>
        public abstract void LoadHeader();

        /// <summary>
        /// Ensures that the data is loaded whenever this script is loaded or when values are changed in the inspector.
        /// </summary>
        public void OnValidate()
        {
            Load();
        }

        /// <summary>
        /// Raises the on load event.
        /// </summary>
        protected void OnDataLoaded()
        {
            if (DataLoaded != null)
                DataLoaded.Invoke();
        }

        public IEnumerator<DimensionData> GetEnumerator()
        {
            for (int i = 0; i < DimensionCount; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < DimensionCount; i++)
            {
                yield return this[i];
            }
        }
    }
}