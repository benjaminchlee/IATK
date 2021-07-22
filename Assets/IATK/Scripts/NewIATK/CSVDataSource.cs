using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

namespace NewIATK
{
    /// <summary>
    /// CSV file data source class
    /// </summary>
    [ExecuteInEditMode]
    public class CSVDataSource : DataSource
    {
        [Tooltip("Text asset containing the data that is to be loaded. This can either be in comma, semicolon, or tab separated format.")]
        public TextAsset Data;

        public Dictionary<int, List<int>> GraphEdges = new Dictionary<int, List<int>>();

        /// <summary>
        /// The list of <see cref="DimensionData"/> that contains the values loaded from <see cref="Data"/>.
        /// </summary>
        private List<DimensionData> dimensionData = new List<DimensionData>();
        /// <summary>
        /// A dictionary that provides a look-up for textual dimensions and their integer representations. The key is the string identifier for the requested dimension, and the value is
        /// the dictionary for that dimension. This dictionary's key is an integer index, and corresponding value is the original string value from the data for that index.
        /// </summary>
        private Dictionary<string, Dictionary<int, string>> textualDimensionsList = new Dictionary<string, Dictionary<int, string>>();
        /// <summary>
        /// A dictionary that provides a look-up for textual dimensions and their integer representations. The key is the string identifier for the requested dimension, and the value is
        /// the dictionary for that dimension. This dictionary's key is a string value from the data, and corresponding value is the integer index that represents that string.
        /// </summary>
        private Dictionary<string, Dictionary<string, int>> textualDimensionsListReverse = new Dictionary<string, Dictionary<string, int>>();

        /// <summary>
        /// Cached count of the number of rows in the loaded <see cref="Data"/> file.
        /// </summary>
        private int dataCount;

        /// <summary>
        /// Characters to split the <see cref="Data"/> by. Supports CSV, TSV, and BSV.
        /// </summary>
        private char[] split = new char[] { ',', '\t', ';'};

        /// <inheritdoc/>
        public override bool IsLoaded
        {
            get { return DimensionCount > 0; }
        }

        /// <inheritdoc/>
        public override int DimensionCount
        {
            get { return dimensionData.Count; }
        }

        /// <inheritdoc/>
        public override int DataCount
        {
            get { return dataCount; }
        }

        /// <inheritdoc/>
        public override DimensionData this[int index]
        {
            get { return dimensionData[index]; }
        }

        /// <inheritdoc/>
        public override DimensionData this[string identifier]
        {
            get
            {
                foreach (DimensionData d in dimensionData)
                {
                    if (d.Identifier == identifier)
                    {
                        return d;
                    }
                }

                return null;
            }
        }

        private void Awake()
        {
            if (!IsLoaded)
                Load();
        }

        /// <inheritdoc/>
        public override void Load()
        {
            Load(Data);
        }

        /// <inheritdoc/>
        public override void LoadHeader()
        {
            if (Data != null)
            {
                LoadMetaData(Data.text.Split('\n'));
            }
        }

        /// <inheritdoc/>
        public override object GetValueApproximate(float normalisedValue, int identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;

            float normValue = UtilMath.NormaliseValue(normalisedValue, 0f, 1f, meta.Min, meta.Max);

            if (meta.Type == IATKDataType.String)
            {
                normValue = UtilMath.NormaliseValue(ValueClosestTo(this[identifier].Data, normalisedValue), 0f, 1f, meta.Min, meta.Max);
                return textualDimensionsList[this[identifier].Identifier][(int)normValue];  // textualDimensions[(int)normValue];
            }
            else return normValue;
        }

        /// <inheritdoc/>
        public override object GetValueApproximate(float normalisedValue, string identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;

            float normValue = UtilMath.NormaliseValue(normalisedValue, 0f, 1f, meta.Min, meta.Max);

            if (meta.Type == IATKDataType.String)
            {
                normValue = UtilMath.NormaliseValue(ValueClosestTo(this[identifier].Data, normalisedValue), 0f, 1f, meta.Min, meta.Max);
                return textualDimensionsList[identifier][(int)normValue]; // textualDimensions[(int)normValue];
            }
            else return normValue;
        }

        /// <inheritdoc/>
        public override object GetValuePrecise(float normalisedValue, int identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;

            float normValue = UtilMath.NormaliseValue(normalisedValue, 0f, 1f, meta.Min, meta.Max);

            if (meta.Type == IATKDataType.String)
            {
                return textualDimensionsList[this[identifier].Identifier][(int)normValue];
            }
            else return normValue;
        }

        /// <inheritdoc/>
        public override object GetValuePrecise(float normalisedValue, string identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;

            float normValue = UtilMath.NormaliseValue(normalisedValue, 0f, 1f, meta.Min, meta.Max);

            if (meta.Type == IATKDataType.String)
            {
                return textualDimensionsList[this[identifier].Identifier][(int)normValue];
            }
            else return normValue;
        }

        /// <inheritdoc/>
        public override int GetNumberOfCategories(int identifier)
        {
            return this[identifier].MetaData.CategoryCount;
        }

        /// <inheritdoc/>
        public override int GetNumberOfCategories(string identifier)
        {
            return this[identifier].MetaData.CategoryCount;
        }

        /// <summary>
        /// Loads the data from the given <see cref="TextAsset"/> file.
        /// </summary>
        /// <param name="data">A text asset of the data file to load.</param>
        private void Load(TextAsset data)
        {
            if (data == null)
                return;

            string[] lines = data.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Run checks on the data to ensure it will work
            if (!CheckData(data.name, lines))
                return;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            dimensionData = new List<DimensionData>();
            textualDimensionsList = new Dictionary<string, Dictionary<int, string>>();
            textualDimensionsListReverse = new Dictionary<string, Dictionary<string, int>>();

            if (LoadMetaData(lines))
            {
                // Now we populate a float array that represents our data
                float[,] dataArray = new float[lines.Length - 1, DimensionCount]; // ignore the first line of headers
                dataCount = dataArray.GetUpperBound(0) + 1;

                // Line reading. Start from 1 to ignore header
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(split);

                    // Dimension reading
                    for (int j = 0; j < values.Count(); j++)
                    {
                        string cleanedValue = CleanDataString(values[j]);

                        // Depending on the data type of this dimension, we treat the value differently
                        switch (dimensionData[j].MetaData.Type)
                            {
                                case IATKDataType.Bool:
                                    {
                                        bool result = false;
                                        bool.TryParse(cleanedValue, out result);
                                        dataArray[i - 1, j] = Convert.ToSingle(result);
                                        break;
                                    }
                                case IATKDataType.Date:
                                    {
                                        string[] valH = cleanedValue.Split('\\');
                                        if (valH.Length == 2)
                                            dataArray[i - 1, j] = float.Parse(valH[0]) * 60f + float.Parse(valH[1]);
                                        else if (valH.Length == 3)
                                            dataArray[i - 1, j] = float.Parse(valH[0]) * 3600f + float.Parse(valH[1]) * 60f + float.Parse(valH[2]);
                                        else dataArray[i - 1, j] = 0f;
                                        break;
                                    }
                                case IATKDataType.Time:
                                    {
                                        string[] valH = cleanedValue.Split(':');
                                        if (valH.Length == 2)
                                            dataArray[i - 1, j] = float.Parse(valH[0]) * 60f + float.Parse(valH[1]);
                                        else if (valH.Length == 3)
                                            dataArray[i - 1, j] = float.Parse(valH[0]) * 3600f + float.Parse(valH[1]) * 60f + float.Parse(valH[2]);
                                        else dataArray[i - 1, j] = 0f;
                                        break;
                                    }
                                case IATKDataType.Int:
                                    {
                                        int result = 0;
                                        int.TryParse(cleanedValue, out result);
                                        dataArray[i - 1, j] = (float)result;
                                        break;
                                    }
                                case IATKDataType.Float:
                                    {
                                        double result = 0.0f;
                                        double.TryParse(cleanedValue, out result);
                                        dataArray[i - 1, j] = (float)result;
                                        break;
                                    }
                                case IATKDataType.Graph:
                                    {
                                        char[] graphSeparator = new char[] { '|' };
                                        string[] edges = cleanedValue.Split(graphSeparator);
                                        List<int> localEdges = new List<int>();
                                        //read edges
                                        for (int ed=0;ed<edges.Length;ed++)
                                        {
                                            if(edges[ed]!="")
                                            localEdges.Add(int.Parse(edges[ed]));
                                        }
                                        GraphEdges.Add(i, localEdges);
                                        break;
                                    }
                                case IATKDataType.String:
                                    {
                                        // Check if we have a dictionary for this dimension
                                        if (textualDimensionsList.ContainsKey(dimensionData[j].Identifier))
                                        {
                                            // If we already do have a dictionary, get it for both the default and reverse directions
                                            Dictionary<string, int> dimensionDictionaryReverse = textualDimensionsListReverse[dimensionData[j].Identifier];
                                            Dictionary<int, string> dimensionDictionary = textualDimensionsList[dimensionData[j].Identifier];

                                            // If we already have an integer representation for the given string value, assign that integer to our data array
                                            if (dimensionDictionaryReverse.ContainsKey(cleanedValue))
                                            {
                                                int valueToEncode = dimensionDictionaryReverse[cleanedValue];
                                                dataArray[i - 1, j] = valueToEncode;
                                            }
                                            // If we do not yet have one, increment the integer to use from the last added element
                                            else
                                            {
                                                //increment from the last added element
                                                int lastEncodedValue = dimensionDictionaryReverse.Values.OrderBy(x => x).Last() + 1;
                                                dimensionDictionaryReverse.Add(cleanedValue, lastEncodedValue);
                                                dimensionDictionary.Add(lastEncodedValue, cleanedValue);
                                                textualDimensionsListReverse[dimensionData[j].Identifier] = dimensionDictionaryReverse;
                                                textualDimensionsList[dimensionData[j].Identifier] = dimensionDictionary;
                                                dataArray[i - 1, j] = lastEncodedValue;
                                            }
                                        }
                                        // If there isn't yet a dictionary for this dimension, create one and add the first value
                                        else
                                        {
                                            Dictionary<int, string> newEntry = new Dictionary<int, string>();
                                            Dictionary<string, int> newEntryReverse = new Dictionary<string, int>();
                                            newEntry.Add(0, cleanedValue);
                                            newEntryReverse.Add(cleanedValue, 0);
                                            textualDimensionsList.Add(dimensionData[j].Identifier, newEntry);
                                            textualDimensionsListReverse.Add(dimensionData[j].Identifier, newEntryReverse);
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        dataArray[i - 1, j] = 0f;
                                        break;
                                    }
                            }
                    }
                }

                // TODO: SORT MULTIPLE VALUES/CRITERIA

                // Populate our dimension data list with our parsed 2D float array
                for (int i = 0; i < DimensionCount; ++i)
                {
                    float[] normalisedColumn = NormaliseColumn(dataArray, i);
                    dimensionData[i].SetData(normalisedColumn, textualDimensionsList);
                }

                // Raise load event
                OnDataLoaded();

                stopwatch.Stop();
                Debug.Log("IATK.CSVDataSource: Data file " + data.name + " successfully loaded in " + (stopwatch.ElapsedMilliseconds / 1000f).ToString("F3") + " seconds.");
            }
        }

        /// <summary>
        /// Initialises the metadata for the data, including its string identifier and type. This function assumes that the header is the first element.
        /// </summary>
        /// <param name="lines">A string array representing lines from a data file.</param>
        /// <returns><c>true</c> if the headers and types were successfully loaded, otherwise <c>false</c>.</returns>
        private bool LoadMetaData(string[] lines)
        {
            if (lines.Length > 0)
            {
                string[] identifiers = lines[0].Split(split);

                // Create metadata
                DimensionData.Metadata[] metadata = new DimensionData.Metadata[identifiers.Count()];

                // Clean string identifiers for each dimension
                for (int i = 0; i < identifiers.Length; i++)
                {
                    string id = CleanDataString(identifiers[i]);
                    identifiers[i] = id;
                }

                // Check the types for each dimension
                if (lines.Length > 1)
                {
                    string[] typesToRead = lines[1].Split(split);

                    for (int i = 0; i < typesToRead.Length; i++)
                    {
                        metadata[i].Type = DataTypeExtension.inferFromString(CleanDataString(typesToRead[i]));
                    }
                }

                // Populate the dimension data list
                for (int i = 0; i < identifiers.Length; ++i)
                {
                    dimensionData.Add(new DimensionData(identifiers[i], i, metadata[i]));
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Performs simple checks to ensure the provided data is functional.
        /// </summary>
        /// <param name="name">The name of the data file.</param>
        /// <param name="lines">A string array representing lines from a data file.</param>
        /// <returns><c>true</c> if the data passess all checks, otherwise <c>false</c>.</returns>
        private bool CheckData(string name, string[] lines)
        {
            if (lines.Length == 0)
            {
                Debug.LogError("IATK.CSVDataSource: Data file " + name + " has no lines. The file may be corrupted or have an incorrect encoding.");
                return false;
            }
            if (lines.Length == 1)
            {
                Debug.LogError("IATK.CSVDataSource: Data file " + name + " has a header but no values.");
                return false;
            }
            if (lines[0].Split(split).Length != lines[1].Split(split).Length)
            {
                Debug.LogError("IATK.CSVDataSource: Data file " + name + " has an inconsistent number of headers and values in each row.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Cleans the given string.
        /// </summary>
        /// <param name="rawData">The string to clean.</param>
        /// <returns>The cleaned string.</returns>
        private string CleanDataString(string rawData)
        {
            return rawData.Replace("\r", string.Empty).Trim();
        }

        /// <summary>
        /// Normalises a given column from a 2D array of float values within the range 0..1. This function also sets some metadata values.
        /// </summary>
        /// <param name="dataArray">A 2D float array of data.</param>
        /// <param name="col">An integer index of the column to normalise.</param>
        /// <returns>A normalised float array in the range 0..1.</returns>
        private float[] NormaliseColumn(float[,] dataArray, int col)
        {
            float[] result = GetColumn(dataArray, col);
            float minValue = result.Min();
            float maxValue = result.Max();

            if (minValue == maxValue)
            {
                // where there are no distinct values, need the dimension to be distinct
                // otherwise lots of maths breaks with division by zero, etc.
                // this is the most elegant hack I could think of, but should be fixed properly in future
                minValue -= 1.0f;
                maxValue += 1.0f;
            }

            // Populate metadata values
            DimensionData.Metadata metadata = dimensionData[col].MetaData;
            metadata.Min = minValue;
            metadata.Max = maxValue;
            metadata.Categories = result.Distinct().Select(x => UtilMath.NormaliseValue(x, minValue, maxValue, 0.0f, 1.0f)).ToArray();
            metadata.CategoryCount = metadata.Categories.Count();
            metadata.BinCount = (int)(maxValue - minValue + 1);
            dimensionData[col].SetMetadata(metadata);

            for (int j = 0; j < result.Length; j++)
            {
                if (minValue < maxValue)
                {
                    result[j] = UtilMath.NormaliseValue(result[j], minValue, maxValue, 0f, 1f);
                }
                else
                {
                    // Avoid NaNs or nonsensical normalization
                    result[j] = 0;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a single column from a 2D array
        /// </summary>
        /// <param name="dataArray">A 2D float array of data.</param>
        /// <param name="col">An integer index of the column to get.</param>
        /// <returns>A float array of the requested column.</returns>
        private float[] GetColumn(float[,] dataArray, int col)
        {
            var colLength = dataArray.GetLength(0);
            var colVector = new float[colLength];

            for (var i = 0; i < colLength; i++)
            {
                colVector[i] = dataArray[i, col];
            }
            return colVector;
        }

        /// <summary>
        /// Returns the closest value item to target from a collection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private float ValueClosestTo(float[] collection, float target)
        {
            float closest_value = collection[0];
            float subtract_result = Math.Abs(closest_value - target);
            for (int i = 1; i < collection.Length; i++)
            {
                if (Math.Abs(collection[i] - target) < subtract_result)
                {
                    subtract_result = Math.Abs(collection[i] - target);
                    closest_value = collection[i];
                }
            }
            return closest_value;
        }
    }
}