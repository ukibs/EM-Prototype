using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public int squareSize = 5;

    private Transform playerTransform;

    private GameObject[] activeBlocks;
    private int centralBlock;
    private int halfMinusOne;

    // Start is called before the first frame update
    void Start()
    {
        //
        activeBlocks = new GameObject[squareSize * squareSize];
        centralBlock = (activeBlocks.Length - 1) / 2;
        halfMinusOne = (squareSize - 1) / 2;
        //
        playerTransform = FindObjectOfType<RobotControl>().transform;
        //
        for(int i = 0; i < squareSize; i++)
        {
            for (int j = 0; j < squareSize; j++)
            {
                Vector3 nextPosition = new Vector3(i * 200 - (200 * halfMinusOne), 0, j * 200 - (200 * halfMinusOne));
                activeBlocks[i * squareSize + j] = Instantiate(blockPrefab, nextPosition, Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckAndMoveBlocks();
    }

    //
    void CheckAndMoveBlocks()
    {
        // Prueba guarra
        // TODO: Manejar las cirbas con parámetro
        Vector3 playerOffsetFromCentralBlock = playerTransform.position - activeBlocks[centralBlock].transform.position;
        if (Mathf.Abs(playerOffsetFromCentralBlock.x) > 100 || Mathf.Abs(playerOffsetFromCentralBlock.z) > 100)
        {
            //
            //Vector3 playerCoordinateToBlocks = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
            Vector3 nextCenterForBlocks = activeBlocks[centralBlock].transform.position;
            //
            if (playerOffsetFromCentralBlock.x > 100) nextCenterForBlocks.x += 200;
            if (playerOffsetFromCentralBlock.x < -100) nextCenterForBlocks.x -= 200;
            if (playerOffsetFromCentralBlock.z > 100) nextCenterForBlocks.z += 200;
            if (playerOffsetFromCentralBlock.z < -100) nextCenterForBlocks.z -= 200;
            //
            //Debug.Log(nextCenterForBlocks + ", " + playerOffsetFromCentralBlock);
            //
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    Vector3 nextPosition = new Vector3(i * 200 - (200 * halfMinusOne) + nextCenterForBlocks.x, 0, 
                                                        j * 200 - (200 * halfMinusOne) + nextCenterForBlocks.z);
                    activeBlocks[i * squareSize + j].transform.position = nextPosition;
                }
            }
        }
    }
}
