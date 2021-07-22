using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewIATK
{
    [ExecuteInEditMode]
    public class View : MonoBehaviour
    {
        public MeshTopology MeshTopology { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public int[] Indices { get; private set; }
        public Color[] Colours { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Vector4[] UVs { get; private set; }
        public Vector3[] AnimationUVs { get; private set; }
        public Material Material { get; private set; }

        private MeshFilter viewMeshFilter;
        private MeshRenderer viewMeshRenderer;
        private Mesh viewMesh;

        public void CreateView(Vector3[] vertices, int[] indices, Color[] colours, Vector3[] normals, Vector4[] uvs, MeshTopology meshTopology, Material material)
        {
            // Store variables
            MeshTopology = meshTopology;
            Vertices = vertices;
            Indices = indices;
            Colours = colours;
            Normals = normals;
            UVs = uvs;
            Material = material;

            // Create mesh
            viewMesh = new Mesh();
            viewMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            viewMesh.MarkDynamic();
            viewMesh.SetVertices(Vertices);
            viewMesh.SetIndices(Indices, MeshTopology, 0);
            viewMesh.SetColors(Colours);
            viewMesh.SetNormals(Normals);
            viewMesh.SetUVs(0, UVs);

            // Assign mesh
            if (viewMeshFilter == null)
                viewMeshFilter = gameObject.AddComponent<MeshFilter>();
            if (viewMeshRenderer == null)
                viewMeshRenderer = gameObject.AddComponent<MeshRenderer>();

            viewMeshFilter.mesh = viewMesh;
            viewMeshRenderer.material = material;

            viewMesh.RecalculateBounds();
        }

        public void SetVertices(Vector3[] vertices)
        {
            Vertices = vertices;
            viewMesh.SetVertices(Vertices);
            viewMesh.RecalculateBounds();
        }

        public void SetVertices(float[] values, IATKDimension dimension)
        {
            for (int i = 0; i < values.Length; i++)
                Vertices[i][(int)dimension] = values[i];
            viewMesh.SetVertices(Vertices);
            viewMesh.RecalculateBounds();
        }

        public void SetIndices(int[] indices, MeshTopology meshTopology)
        {
            Indices = indices;
            MeshTopology = meshTopology;
            viewMesh.SetIndices(Indices, MeshTopology, 0);
        }

        public void SetUVs(Vector4[] uvs)
        {
            UVs = uvs;
            viewMesh.SetUVs(0, UVs);
        }

        public void SetUVs(float[] values, IATKDimension dimension)
        {
            for (int i = 0; i < values.Length; i++)
                UVs[i][(int)dimension] = values[i];

            viewMesh.SetUVs(0, UVs);
        }

        public void SetUVs(float value, IATKDimension dimension)
        {
            for (int i = 0; i < UVs.Length; i++)
                UVs[i][(int)dimension] = value;

            viewMesh.SetUVs(0, UVs);
        }

        public void SetColour(Color colour)
        {
            for (int i = 0; i < Colours.Length; i++)
                Colours[i] = colour;

            viewMesh.SetColors(Colours);
        }

        public void SetColours(Color[] colours)
        {
            Colours = colours;
            viewMesh.SetColors(Colours);
        }

        public Vector3[] GetVertices()
        {
            return Vertices;
        }

        public float[] GetVertices(int dimension)
        {
            if (dimension < 0 && 2 < dimension)
                return null;

            float[] values = new float[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
                values[i] = Vertices[i][dimension];

            return values;
        }

        public int[] GetIndices()
        {
            return Indices;
        }

        public Vector4[] GetUVs()
        {
            return UVs;
        }

        public float[] GetUVs(int dimension)
        {
            if (dimension < 0 && 3 < dimension)
                return null;

            float[] values = new float[UVs.Length];
            for (int i = 0; i < UVs.Length; i++)
                values[i] = UVs[i][dimension];

            return values;
        }

        public Color[] GetColours()
        {
            return Colours;
        }

        public void SetIntProperty(string property, int value)
        {
            Material.SetInt(property, value);
        }

        public void SetFloatProperty(string property, float value)
        {
            Material.SetFloat(property, value);
        }

        public void SetVectorProperty(string property, Vector4 value)
        {
            Material.SetVector(property, value);
        }

        public void SetColourProperty(string property, Color value)
        {
            Material.SetColor(property, value);
        }

        public int GetIntProperty(string property)
        {
            return Material.GetInt(property);
        }

        public float GetFloatProperty(string property)
        {
            return Material.GetFloat(property);
        }

        public Vector4 GetVectorProperty(string property)
        {
            return Material.GetVector(property);
        }

        public Color GetColorProperty(string property)
        {
            return Material.GetColor(property);
        }
    }
}