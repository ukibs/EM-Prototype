using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public GameObject[] blockPrefabs;
    public int[] blockFrequencies;
    public int[] minBlockAmounts;
    public int[] maxBlockAmounts;
    public int squareSize = 7;
    public float blockSize = 200; // TODO: Ponlo donde toque
    public float maxDistanceFromCenter = 2000;

    private Transform playerTransform;

    //private GameObject[] activeBlocks;
    private GameObject[,] activeBlocksMatrix;
    private int centralBlock;
    private int halfMinusOne;
    private int[] currentBlockAmounts;
    private Waypoint[] allWaypoints;
    private Waypoint nearestWaypointToPlayer;
    //private DestructibleTerrain[] destructibleTerrains;

    //
    public Waypoint[] AllWaypoints { get { return allWaypoints; } }

    //
    private float maxTestMultiMoveTime = 1;
    private float currentTestMultiMoveTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        // 
        // GetNearestWaypointToPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: QUe no haga el chequeo a cada frame
        //CheckAndMoveAllBlocks();
        //
        currentTestMultiMoveTime += Time.deltaTime;
        //
        if (CheckIfPlayerOverCentralBlock()
            && currentTestMultiMoveTime >= maxTestMultiMoveTime)
        {
            MoveFarestBlocks(PlayerOffsetFromCentralBlockInUnits());
            // De momento lo hacemos aqui
            // Pero seria bueno chequearlo aparte
            // GetNearestWaypointToPlayer();
            currentTestMultiMoveTime = 0;
        }

        // Si el player se ha alejado lo suficiente del centro...
        if(playerTransform.position.magnitude > maxDistanceFromCenter)
        {
            RealocateEverything();
            Debug.Log("Realocating everything");
        }
    }

    //
    private void OnDrawGizmos()
    {
        // Vamos a pintar con un degradado para ver que coño pasa
        Color currentPieceColor = new Color(1, 0, 1, 1f);
        // Para chequear posibles repes en x,y
        float baseHeightModifier = 5;
        float heightModifier = 0;
        for(int i = 0; i < squareSize; i++)
        {
            //
            //currentPieceColor.r -= 1 / squareSize;
            currentPieceColor.r -= 0.1f;
            //
            for (int j = 0; j < squareSize; j++)
            {
                //
                heightModifier += baseHeightModifier;
                //
                //currentPieceColor.b -= 1 / squareSize;
                currentPieceColor.b -= 0.1f;
                //
                if(i == centralBlock && j == centralBlock)
                    Gizmos.color = new Color(0, 1, 0, 1f);
                else
                    Gizmos.color = currentPieceColor;
                //
                Gizmos.DrawCube(activeBlocksMatrix[i, j].transform.position + (Vector3.up * (50 + heightModifier)), 
                    new Vector3(100, 10, 100));
            }
        }
    }

    //
    void GetNearestWaypointToPlayer()
    {
        // De momento vamos a pasar por todos los waypoints
        // Mas adelante filtraremos los del bloque central
        float nearestDistance = Mathf.Infinity;
        for(int i = 0; i < allWaypoints.Length; i++)
        {
            Vector3 distanceToPlayer = playerTransform.position - allWaypoints[i].transform.position;
            if(distanceToPlayer.magnitude < nearestDistance)
            {
                nearestDistance = distanceToPlayer.magnitude;
                nearestWaypointToPlayer = allWaypoints[i];
            }
        }
    }

    //
    public Waypoint GetNearestWaypointTo(Transform transformToCheck)
    {
        Waypoint nearestWaypoint = null;
        float nearestDistance = Mathf.Infinity;
        for (int i = 0; i < allWaypoints.Length; i++)
        {
            Vector3 distanceToPlayer = playerTransform.position - allWaypoints[i].transform.position;
            if (distanceToPlayer.magnitude < nearestDistance)
            {
                nearestDistance = distanceToPlayer.magnitude;
                nearestWaypoint = allWaypoints[i];
            }
        }
        return nearestWaypoint;
    }

    // TODO: Ya lo haremos algún día
    public Vector3[] GetPathToPlayer(Transform playerSeeker)
    {
        Vector3[] pathToPlayer = null;
        Waypoint playerSeekerWaypoint = GetNearestWaypointTo(playerSeeker);
        //nearestWaypointToPlayer

        return pathToPlayer;
    }

    //
    bool CheckIfPlayerOverCentralBlock()
    {
        //
        //Debug.Log("fdsfsd: " + activeBlocksMatrix[centralBlock, centralBlock].transform.position);
        //
        Vector3 playerOffsetFromCentralBlock = playerTransform.position - activeBlocksMatrix[centralBlock, centralBlock].transform.position;
        // TODO: Hacerlo trabajar con variable de tamaño bloque
        bool notOverCentralBlock = (Mathf.Abs(playerOffsetFromCentralBlock.x) > 100 || Mathf.Abs(playerOffsetFromCentralBlock.z) > 100);
        //
        //if(notOverCentralBlock)
        //    Debug.Log("Central block offset: " + playerOffsetFromCentralBlock.x + ", " + playerOffsetFromCentralBlock.z);
        //
        return notOverCentralBlock;
    }

    //
    Vector2 PlayerOffsetFromCentralBlockInUnits()
    {
        Vector2 offsetInUnits = Vector2.zero;
        Vector3 playerOffsetFromCentralBlock = playerTransform.position - activeBlocksMatrix[centralBlock, centralBlock].transform.position;
        // Esto debería dar -1, 0, 1
        // Y cifras mayores (en caso de teleport) también
        // TODO: Manejar eso
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
        minBlockAmounts = levelInfo.terrainMin;
        maxBlockAmounts = levelInfo.terrainMax;
        AllocateTerrain();
    }

    //
    void AssignNeighbours()
    {
        //
        for (int i = 0; i < allWaypoints.Length; i++)
            allWaypoints[i].GetNeighbours();
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
        //
        currentBlockAmounts = new int[blockFrequencies.Length];
        // Establecemos relación de frecuencias
        int[] freqSuccesion = new int[blockFrequencies.Length];
        int accumulated = 0;
        for (int i = 0; i < blockFrequencies.Length; i++)
        {
            // TODO: Aplicar esto bien
            freqSuccesion[i] = blockFrequencies[i] + accumulated;
            accumulated += blockFrequencies[i];
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
                    //
                    int decideIndex = UnityEngine.Random.Range(0, accumulated);
                    //
                    for(int k = 0; k < freqSuccesion.Length; k++)
                    {
                        if(decideIndex <= freqSuccesion[k])
                        {
                            decideIndex = k;
                            break;
                        }
                    }
                    //
                    //prefabToUse = blockPrefabs[(int)UnityEngine.Random.Range(0, blockPrefabs.Length)];
                    // Chequeo de máximo
                    // 0 como parámetros de control si tiene max 0 se ignora
                    if(currentBlockAmounts[decideIndex] >= maxBlockAmounts[decideIndex] &&
                        maxBlockAmounts[decideIndex] > 0)
                    {
                        j--;
                        continue;
                    }
                    //
                    prefabToUse = blockPrefabs[decideIndex];
                    currentBlockAmounts[decideIndex]++;
                }

                //
                int yRotation = UnityEngine.Random.Range(0, 4) * 90;
                //Vector3 eulerToUse = 
                //Quaternion rotationToUse = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                // And put it
                activeBlocksMatrix[i, j] = Instantiate(prefabToUse, nextPosition, Quaternion.identity);
                activeBlocksMatrix[i, j].transform.eulerAngles = new Vector3(0, yRotation, 0);
                // Desrotamos el suelo
                Transform floor = activeBlocksMatrix[i, j].transform.GetChild(0);
                floor.eulerAngles = new Vector3(0, 0, 0);
            }
        }
        // Recordar que el orden es de - x a + x, y con y igual
        //
        for(int i = 0; i < currentBlockAmounts.Length; i++)
        {
            if(currentBlockAmounts[i] < minBlockAmounts[i])
            {
                //TODO: Aplicarlo bien
                Debug.Log("Minimo incumplido");
            }
        }
        //
        allWaypoints = FindObjectsOfType<Waypoint>();
        //Debug.Log("Total waypoints: " + allWaypoints.Length);
        AssignNeighbours();
    }

    // TODO: Que pueda trabajar con más de uno a la vez
    void MoveFarestBlocks(Vector2 playerOffsetInUnits)
    {
        //
        //Debug.Log("Player offset in units: " + playerOffsetInUnits);
        // 
        GameObject[,] newActiveBlocksOrder = new GameObject[squareSize, squareSize];
        // Primero ver que bloques se quedan lejos con el offset
        // activeBlocks[i * squareSize + j]
        // ej: x = -1, los bloques de la derecha, los mandamos a la izquierda
        //     x = 1, 0 -> n-1
        //     x = -1, n-1 -> 0
        //     x = 0, nada 
        
        //
        int sideToGet;
        int sideToPut;
        //
        int start;
        int end;

        // Para el offset en Y -------------------------------------------------------------------------------------------------------
        if (playerOffsetInUnits.y != 0)
        {

            //
            int displacementY = (int)playerOffsetInUnits.y;
            int displaceMentYSign = (int)Mathf.Sign(displacementY);
            int absoluteDisplaceMentY = Mathf.Abs(displacementY);
            // 
            sideToGet = 0;
            sideToPut = 0;
            start = 0;
            end = squareSize;
            // Inicamos los parametros para moverlos en una dirección o en otra
            switch (displaceMentYSign)
            {
                case -1:
                    //     x = -1, n-1 -> 0
                    sideToGet = squareSize - 1;
                    sideToPut = 0;
                    start = 1;
                    break;
                case 1:
                    //     x = 1, 0 -> n-1

                    sideToGet = 0;
                    sideToPut = squareSize - 1;
                    end = squareSize - 1;
                    break;
            }
            // Tantas veces como de el displacement
            //for (int h = 0; h < absoluteDisplaceMentY; h++)
            //{
                //
                //Debug.Log("Iterations: " + h + ", " + Mathf.Abs(displacementY));
                // Mandamos los que han quedado atrás al principio (solo una fila)
                for (int i = 0; i < squareSize; i++)
                {
                    // Los recolocamos en la matriz
                    newActiveBlocksOrder[i, sideToPut] = activeBlocksMatrix[i, sideToGet];

                    // Y en el terreno
                    Vector3 blockNewPosition = newActiveBlocksOrder[i, sideToPut].transform.position;
                    blockNewPosition.z += blockSize * squareSize * displaceMentYSign;
                    newActiveBlocksOrder[i, sideToPut].transform.position = blockNewPosition;

                    // Reseteamos los elementos destruidos
                    DestructibleTerrain[] destructibleElements = newActiveBlocksOrder[i, sideToPut].GetComponentsInChildren<DestructibleTerrain>();
                    for (int j = 0; j < destructibleElements.Length; j++)
                        destructibleElements[j].Restore();
                }
                // Y el resto
                // El ancho (todo menos la fila)
                for (int i = 0; i < squareSize; i++)
                {
                    // Y el largo (dependiendo de por donde lo hemos cogido)
                    for (int j = start; j < end; j++)
                    {
                        // TODO: Si vuelve a pasar mirar aqui
                        newActiveBlocksOrder[i, j] = activeBlocksMatrix[i, j + displaceMentYSign];
                    }
                }
                // Lo reasignamos en cada pasada
                activeBlocksMatrix = newActiveBlocksOrder;
            //}
        }

        // Para el offset en X ------------------------------------------------------------------------------------------------------------
        if (playerOffsetInUnits.x != 0)
        {
            //
            int displacementX = (int)playerOffsetInUnits.x;
            int displacementXSign = (int)Mathf.Sign(displacementX);
            int absoluteDisplaceMentX = Mathf.Abs(displacementX);
            //
            sideToGet = 0;
            sideToPut = 0;
            start = 0;
            end = squareSize;
            switch (displacementXSign)
            {
                case -1:
                    //     x = -1, n-1 -> 0
                    sideToGet = squareSize - 1;
                    sideToPut = 0;
                    start = 1;
                    break;
                case 1:
                    //     x = 1, 0 -> n-1

                    sideToGet = 0;
                    sideToPut = squareSize - 1;
                    end = squareSize - 1;
                    break;
            }
            //
            //for (int h = 0; h < Mathf.Abs(displacementX); h++)
            //{
                
                // Colocamos el que hemos cabiado
                for (int i = 0; i < squareSize; i++)
                {
                    //
                    newActiveBlocksOrder[sideToPut, i] = activeBlocksMatrix[sideToGet, i];
                    //
                    Vector3 blockNewPosition = newActiveBlocksOrder[sideToPut, i].transform.position;
                    blockNewPosition.x += 200 * squareSize * displacementXSign;
                    newActiveBlocksOrder[sideToPut, i].transform.position = blockNewPosition;
                    // TODO: Añadir chequeo de terreno destruido para que lo resetee
                    DestructibleTerrain[] destructibleElements = newActiveBlocksOrder[sideToPut, i].GetComponentsInChildren<DestructibleTerrain>();
                    for (int j = 0; j < destructibleElements.Length; j++)
                        destructibleElements[j].Restore();
                }
                // Y el resto
                // TODO: Decidimos cual es x y cual y para cambiarlo abajo
                for (int i = start; i < end; i++)
                {
                    for (int j = 0; j < squareSize; j++)
                    {
                        // TODO: Aquí da el fallo cuando intentamos mover varios
                        newActiveBlocksOrder[i, j] = activeBlocksMatrix[i + displacementXSign, j];
                    }
                }
                //
                activeBlocksMatrix = newActiveBlocksOrder;
            //}
        }

        //
        //activeBlocksMatrix = newActiveBlocksOrder;
        // Y marcmos el nuevo bloque central
        //centralBlock = activeBlocksMatrix 
        //
        AssignNeighbours();
    }

    // Función para mandar todo de vuelta al centro cuando se aleje demasiado de éste
    void RealocateEverything()
    {
        //
        Vector3 directionToMove = -playerTransform.position;
        directionToMove.y = 0;
        //
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
        //
        for(int i = 0; i < allGameObjects.Length; i++)
        {
            // No movemos hijos para evitar descalabros
            if(allGameObjects[i].transform.parent == null)
                allGameObjects[i].transform.position += directionToMove;
        }
    }
}
