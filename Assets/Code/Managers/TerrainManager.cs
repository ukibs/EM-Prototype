using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public GameObject[] blockPrefabs;
    public int[] blockFrequencies;
    public int squareSize = 7;
    public float blockSize = 200; // TODO: Ponlo donde toque

    private Transform playerTransform;

    //private GameObject[] activeBlocks;
    private GameObject[,] activeBlocksMatrix;
    private int centralBlock;
    private int halfMinusOne;

    // Start is called before the first frame update
    void Start()
    {
        // De momento nada
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: QUe no haga el chequeo a cada frame
        //CheckAndMoveAllBlocks();

        //
        if (CheckIfPlayerOverCentralBlock())
        {
            MoveFarestBlocks(PlayerOffsetFromCentralBlockInUnits());
        }
    }

    //
    bool CheckIfPlayerOverCentralBlock()
    {
        Vector3 playerOffsetFromCentralBlock = playerTransform.position - activeBlocksMatrix[centralBlock, centralBlock].transform.position;
        return (Mathf.Abs(playerOffsetFromCentralBlock.x) > 100 || Mathf.Abs(playerOffsetFromCentralBlock.z) > 100);
    }

    //
    Vector2 PlayerOffsetFromCentralBlockInUnits()
    {
        Vector2 offsetInUnits = Vector2.zero;
        Vector3 playerOffsetFromCentralBlock = playerTransform.position - activeBlocksMatrix[centralBlock, centralBlock].transform.position;
        // Esto debería dar -1, 0, 1
        offsetInUnits.x = (int)(playerOffsetFromCentralBlock.x / 100);
        offsetInUnits.y = (int)(playerOffsetFromCentralBlock.z / 100);

        return offsetInUnits;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="levelInfo"></param>
    public void InitiateManager(LevelInfo levelInfo)
    {
        blockPrefabs = levelInfo.terrainPrefabs;
        blockFrequencies = levelInfo.terrainRatio;
        AllocateTerrain();
    }

    //
    void AllocateTerrain()
    {
        //
        //activeBlocks = new GameObject[squareSize * squareSize];
        activeBlocksMatrix = new GameObject[squareSize, squareSize];
        //centralBlock = (activeBlocks.Length - 1) / 2;
        halfMinusOne = (squareSize - 1) / 2;
        centralBlock = halfMinusOne;
        //
        playerTransform = FindObjectOfType<RobotControl>().transform;
        // Establecemos relación de frecuencias
        int totalSum = 0;
        for (int i = 0; i < blockFrequencies.Length; i++)
        {
            // TODO: Aplicar esto bien
            totalSum += blockFrequencies[i];
        }
        //
        for (int i = 0; i < squareSize; i++)
        {
            //
            for (int j = 0; j < squareSize; j++)
            {
                Vector3 nextPosition = new Vector3(i * 200 - (200 * halfMinusOne), 0, j * 200 - (200 * halfMinusOne));
                // TODO: Meter la variación acorde a la frecuencia
                // Que el central de todos sea el 1º, para asegurarnos de que el player no aparece dentro de un bloque
                GameObject prefabToUse;
                if (i == centralBlock && j == centralBlock)
                    prefabToUse = blockPrefabs[0];
                else
                {
                    prefabToUse = blockPrefabs[(int)UnityEngine.Random.Range(0, blockPrefabs.Length)];
                }

                //
                int yRotation = UnityEngine.Random.Range(0, 4) * 90;
                //Vector3 eulerToUse = 
                //Quaternion rotationToUse = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                // And put it
                activeBlocksMatrix[i, j] = Instantiate(prefabToUse, nextPosition, Quaternion.identity);
                activeBlocksMatrix[i, j].transform.eulerAngles = new Vector3(0, yRotation, 0);
            }
        }
        // Recordar que el orden es de - x a + x, y con y igual
    }

    //
    void MoveFarestBlocks(Vector2 playerOffsetInUnits)
    {
        // 
        GameObject[,] newActiveBlocksOrder = new GameObject[squareSize, squareSize];
        // Primero ver que bloques se quedan lejos con el offset
        // activeBlocks[i * squareSize + j]
        // ej: x = -1, los bloques de la derecha, los mandamos a la izquierda
        //     x = 1, 0 -> n-1
        //     x = -1, n-1 -> 0
        //     x = 0, nada 
        //Vector2 sidesToDisplace = -playerOffsetInUnits;

        //fsdf
        //
        int sideToGet;
        int sideToPut;
        //
        int start;
        int end;
        
        // Para el offset en Y
        if(playerOffsetInUnits.y != 0)
        {
            int displacementY = (int)playerOffsetInUnits.y;
            sideToGet = 0;
            sideToPut = 0;
            start = 0;
            end = squareSize;
            switch (displacementY)
            {
                case -1:
                    //     x = -1, n-1 -> 0
                    sideToGet = squareSize - 1;
                    sideToPut = 0;
                    start++;
                    break;
                case 1:
                    //     x = 1, 0 -> n-1
                    
                    sideToGet = 0;
                    sideToPut = squareSize - 1;
                    end--;
                    break;
            }
            // Colocamos el que hemos cabiado
            for (int i = 0; i < squareSize; i++)
            {
                //
                newActiveBlocksOrder[i, sideToPut] = activeBlocksMatrix[i, sideToGet];
                //
                Vector3 blockNewPosition = newActiveBlocksOrder[i, sideToPut].transform.position;
                blockNewPosition.z += 200 * squareSize * displacementY;
                newActiveBlocksOrder[i, sideToPut].transform.position = blockNewPosition;
            }
            // Y el resto
            // TODO: Decidimos cual es x y cual y para cambiarlo abajo
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = start; j < end; j++)
                {
                    newActiveBlocksOrder[i, j] = activeBlocksMatrix[i, j + displacementY];
                }
            }
        }

        // Para el offset en X
        if (playerOffsetInUnits.x != 0)
        {
            int displacementX = (int)playerOffsetInUnits.x;
            sideToGet = 0;
            sideToPut = 0;
            start = 0;
            end = squareSize;
            switch (displacementX)
            {
                case -1:
                    //     x = -1, n-1 -> 0
                    sideToGet = squareSize - 1;
                    sideToPut = 0;
                    start++;
                    break;
                case 1:
                    //     x = 1, 0 -> n-1

                    sideToGet = 0;
                    sideToPut = squareSize - 1;
                    end--;
                    break;
            }
            // Colocamos el que hemos cabiado
            for (int i = 0; i < squareSize; i++)
            {
                //
                newActiveBlocksOrder[sideToPut, i] = activeBlocksMatrix[sideToGet, i];
                //
                Vector3 blockNewPosition = newActiveBlocksOrder[sideToPut, i].transform.position;
                blockNewPosition.x += 200 * squareSize * displacementX;
                newActiveBlocksOrder[sideToPut, i].transform.position = blockNewPosition;
            }
            // Y el resto
            // TODO: Decidimos cual es x y cual y para cambiarlo abajo
            for (int i = start; i < end; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    newActiveBlocksOrder[i, j] = activeBlocksMatrix[i + displacementX, j];
                }
            }
        }

        //
        activeBlocksMatrix = newActiveBlocksOrder;
        // Y marcmos el nuevo bloque central
        //centralBlock = activeBlocksMatrix 
    }

    // TODO: Función para mandar todo de vuelta al centro cuando se aleje demasiado de éste
}
