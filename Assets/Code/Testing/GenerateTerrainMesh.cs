using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrainMesh : MonoBehaviour
{
    private Mesh objectMesh;
    private MeshFilter meshFilter;

    private Vector3[] meshVertices;
    private Vector3[] meshNormals;
    private Vector3[] meshTriangles;

    private List<Vector3> groupedVertices;
    private List<Vector3> groupedNormals;
    private List<Vector3> groupedTriangles;

    // Start is called before the first frame update
    void Start()
    {
        //
        meshFilter = gameObject.AddComponent<MeshFilter>();
        objectMesh = meshFilter.mesh;
        //
        objectMesh.vertices = new Vector3[] {new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f),
                                            new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
                                            new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
                                            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f)};
        objectMesh.normals = new Vector3[] {new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f),
                                            new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
                                            new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
                                            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f)};
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
