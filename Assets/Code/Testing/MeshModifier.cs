using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshModifier : MonoBehaviour
{
    private Mesh objectMesh;

    private Vector3[] meshVertices;
    private Vector3[] meshNormals;
    private Vector3[] meshTriangles;

    private List<Vector3> groupedVertices;
    private List<Vector3> groupedNormals;
    private List<Vector3> groupedTriangles;

    private int[] ungroupedVertexIndices;

    // Start is called before the first frame update
    void Start()
    {

        GetGroupedVertices();

        TestUpperPartModifying();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GetGroupedVertices()
    {
        //
        objectMesh = GetComponent<MeshFilter>().mesh;
        meshVertices = objectMesh.vertices;
        ungroupedVertexIndices = new int[meshVertices.Length];
        meshNormals = objectMesh.normals;

        //
        groupedVertices = new List<Vector3>();
        groupedNormals = new List<Vector3>();

        //
        for (int i = 0; i < meshVertices.Length; i++)
        {
            //
            Debug.Log("Vertex: " + meshVertices[i]);
            //
            if (!groupedVertices.Contains(meshVertices[i]))
            {
                groupedVertices.Add(meshVertices[i]);
                groupedNormals.Add(meshNormals[i]);
                //Debug.Log("Vertex: " + meshVertices[i]);
                ungroupedVertexIndices[i] = groupedVertices.Count - 1;
            }
            else
            {
                int vertexIndex = groupedVertices.IndexOf(meshVertices[i]);
                groupedNormals[vertexIndex] += meshNormals[i];
                ungroupedVertexIndices[i] = vertexIndex;
            }
        }

        //
        for (int i = 0; i < groupedNormals.Count; i++)
        {
            groupedNormals[i] = groupedNormals[i].normalized;
            Debug.Log("Gouped normal: " + groupedNormals[i]);
        }
    } 

    void TestUpperPartModifying()
    {
        //
        for (int i = 0; i < meshVertices.Length; i++)
        {
            int groupedNormalIndex = ungroupedVertexIndices[i];
            Vector3 normalToUse = groupedNormals[groupedNormalIndex];
            if(normalToUse == Vector3.up)
                meshVertices[i] += normalToUse * UnityEngine.Random.Range(0.1f, 0.5f);
        }

        ////
        objectMesh.vertices = meshVertices;
        objectMesh.RecalculateBounds();
    }

    void TestKeyModifying()
    {
        //
        float verticalAxis = Input.GetAxisRaw("Vertical");
        //
        for (int i = 0; i < meshVertices.Length; i++)
        {
            int groupedNormalIndex = ungroupedVertexIndices[i];
            Vector3 normalToUse = groupedNormals[groupedNormalIndex];
            meshVertices[i] += normalToUse * Time.deltaTime * verticalAxis * 0.1f;
        }

        ////
        objectMesh.vertices = meshVertices;
        objectMesh.RecalculateBounds();
    }
}
