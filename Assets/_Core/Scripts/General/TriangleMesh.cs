using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TriangleMesh : MonoBehaviour
{
    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;

    private void Awake()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
    }

    private void Start()
    {
        MakeMeshData();
        CreateMesh();
    }

    private void MakeMeshData()
    {
        // create an array of vertices
        _vertices = new [] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0) };
        // create an array of integers
        _triangles = new [] { 0, 1, 2 };
    }

    private void CreateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
    }
}