using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshModifier : MonoBehaviour
{
    //
    //esto es una prueba por germanSanchez

    public Vector2 pointsOffset = new Vector2(0, 0.3f);

    //
    private Mesh objectMesh;

    private Vector3[] meshVertices;
    private Vector3[] meshNormals;
    private Vector3[] meshTriangles;

    private List<Vector3> groupedVertices;
    private List<Vector3> groupedNormals;
    private List<Vector3> groupedTriangles;

    private int[] ungroupedVertexIndices;

    private List<int> upperVexterIndices;
    private List<int> upperBorderVertexIndices;

    private MeshCollider meshCollider;

    //
    private TerrainManager terrainManager;

    // Start is called before the first frame update
    void Start()
    {

        meshCollider = GetComponent<MeshCollider>();

        GetGroupedVertices();

        GetUpperVertexIndexes();

        TestUpperPartModifying();

        //
        terrainManager = FindObjectOfType<TerrainManager>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    //
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
            //Debug.Log("Vertex: " + meshVertices[i]);
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
            //Debug.Log("Gouped normal: " + groupedNormals[i]);
        }
    } 

    //
    void GetUpperVertexIndexes()
    {
        //
        upperVexterIndices = new List<int>();
        upperBorderVertexIndices = new List<int>();
        //
        for (int i = 0; i < meshVertices.Length; i++)
        {
            int groupedNormalIndex = ungroupedVertexIndices[i];
            Vector3 normalToUse = groupedNormals[groupedNormalIndex];
            if (normalToUse == Vector3.up)
            {
                //meshVertices[i] += normalToUse * UnityEngine.Random.Range(0.1f, 0.5f);
                upperVexterIndices.Add(i);
            }
            else if(normalToUse.y > 0)
            {
                //meshVertices[i] += normalToUse * 1;
                upperBorderVertexIndices.Add(i);
            }
        }

        ////
        //objectMesh.vertices = meshVertices;
        //objectMesh.RecalculateBounds();
    }

    //
    void TestUpperPartModifying()
    {
        //
        for (int i = 0; i < meshVertices.Length; i++)
        {
            //
            if (upperVexterIndices.Contains(i))
            {
                //float xzDistance = Mathf.Sqrt(Mathf.Pow(meshVertices[i].x,2) + Mathf.Pow(meshVertices[i].z, 2));
                //float operationToApply = Mathf.Cos(xzDistance);

                meshVertices[i] += Vector3.up * Random.Range(pointsOffset.x, pointsOffset.y);
            }
            //else if(upperBorderVertexIndices.Contains(i))
            //{
            //    meshVertices[i] += Vector3.up * Random.Range(0.5f, 1.5f);
            //}
        }

        objectMesh.vertices = meshVertices;
        objectMesh.RecalculateBounds();

        //
        meshCollider.sharedMesh = objectMesh;
    }

    //
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
