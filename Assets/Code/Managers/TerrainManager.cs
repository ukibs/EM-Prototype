using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public TerrainElementSettings[] terrainElementsSettings;
    //public GameObject[] blockPrefabs;
    //public int[] blockFrequencies;
    //public int[] minBlockAmounts;
    //public int[] maxBlockAmounts;
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
    private float maxTestMultiMoveTime = 0.1f;
    private float currentTestMultiMoveTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        // 
        GetNearestWaypointToPlayer();
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
            GetNearestWaypointToPlayer();
            currentTestMultiMoveTime = 0;
        }

        // Si el player se ha alejado lo suficiente del centro...
        Vector3 playerOffsetFromCenter = playerTransform.position;
        playerOffsetFromCenter.y = 0;
        // Obviamos y (al menos de momento)
        if (playerOffsetFromCenter.magnitude > maxDistanceFromCenter)
        {
            //
            PlayerReference.playerIntegrity.TranslationAllowed = true;
            //
            RealocateEverything();
            Debug.Log("Realocating everything");
        }
    }

    //
    private void OnDrawGizmos()
    {
        //
        DrawTerrainOrder();
        //
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(nearestWaypointToPlayer.transform.position, 10);
    }

    void DrawTerrainOrder()
    {
        // Vamos a pintar con un degradado para ver que coño pasa
        Color currentPieceColor = new Color(1, 0, 1, 1f);
        // Para chequear posibles repes en x,y
        float baseHeightModifier = 5;
        float heightModifier = 0;
        for (int i = 0; i < squareSize; i++)
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
                if (i == centralBlock && j == centralBlock)
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
            Vector3 distanceToTransform = transformToCheck.position - allWaypoints[i].transform.position;
            if (distanceToTransform.magnitude < nearestDistance)
            {
                nearestDistance = distanceToTransform.magnitude;
                nearestWaypoint = allWaypoints[i];
            }
        }
        return nearestWaypoint;
    }

    // TODO: Ya lo haremos algún día
    public List<Waypoint> GetPathToPlayer(Transform playerSeeker)
    {
        //
        int maxIterations = 50;
        int currentIterations = 0;

        // Este iremos rellenando con los nodos buenos
        //List<Waypoint> pathToPlayer = new List<Waypoint>(10);
        Waypoint playerSeekerWaypoint = GetNearestWaypointTo(playerSeeker);
        //El nearestWaypointToPlayer ya lo tenemos

        // A chequear
        List<Waypoint> openSet = new List<Waypoint>();
        // Ya chequeados
        List<Waypoint> closedSet = new List<Waypoint>();
        
        Waypoint nextNodeToCheck = null;
        // Empezando por el mas cercano al enemigo
        // Lo metemos al open set
        openSet.Add(playerSeekerWaypoint);
        

        // Empezamos a iterar con el open set
        while (openSet.Count > 0 && nextNodeToCheck != nearestWaypointToPlayer && currentIterations < maxIterations)
        {
            // Cogemos el primero de la lista (previamente ordenada)
            nextNodeToCheck = openSet[0];

            // Lo sacamos de la abierta y lo metemos en la cerrada
            openSet.Remove(nextNodeToCheck);
            closedSet.Add(nextNodeToCheck);

            // Mientras no sea el destino...
            if (nextNodeToCheck != nearestWaypointToPlayer)
            {
                
                // Recorremos los vecinos del nodo a chequear
                for (int i = 0; i < nextNodeToCheck.CurrentNeighbors.Count; i++)
                {
                    // Que no esté en ninguno ed los dos
                    if (!openSet.Contains(nextNodeToCheck.CurrentNeighbors[i]) && 
                        !closedSet.Contains(nextNodeToCheck.CurrentNeighbors[i]))
                    {
                        // Recordar costes: f, h y g
                        nextNodeToCheck.CurrentNeighbors[i].hCost = Heuristic(nextNodeToCheck.CurrentNeighbors[i]);
                        nextNodeToCheck.CurrentNeighbors[i].fCost = nextNodeToCheck.fCost + nextNodeToCheck.DistancesToNeighbors[i];
                        nextNodeToCheck.CurrentNeighbors[i].pathParent = nextNodeToCheck;
                        openSet.Add(nextNodeToCheck.CurrentNeighbors[i]);
                    }
                }
                // Y los ordenamos de menor coste a mayor
                //openSet.Sort((x, y) => y.GCost.CompareTo(x.GCost));
                openSet.Sort((x, y) => x.GCost.CompareTo(y.GCost));
            }
            // Y cuando lo tenemos...
            else
            {
                closedSet = RetracePath(playerSeekerWaypoint, nearestWaypointToPlayer);
                break;
            }

            //
            currentIterations++;
        }
        
        //
        return closedSet;
    }

    /// <summary>
    /// Rehacemos el camino desde el final para devolverlo limpio
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <returns></returns>
    private List<Waypoint> RetracePath(Waypoint startPoint, Waypoint endPoint)
    {
        //
        List<Waypoint> retracedPath = new List<Waypoint>(10);
        Waypoint currentNode = endPoint;
        //
        while(currentNode != startPoint)
        {
            retracedPath.Insert(0, currentNode);
            currentNode = currentNode.pathParent;
        }

        retracedPath.Insert(0, startPoint);

        //
        return retracedPath;
    }
    
    //
    private float Heuristic(Waypoint waypointToCheck)
    {
        return (waypointToCheck.transform.position - nearestWaypointToPlayer.transform.position).magnitude;
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
        terrainElementsSettings = levelInfo.terrainElementsSettings;
        //
        //Debug.Log(terrainElementsSettings);
        //blockPrefabs = levelInfo.terrainPrefabs;
        //blockFrequencies = levelInfo.terrainRatio;
        //minBlockAmounts = levelInfo.terrainMin;
        //maxBlockAmounts = levelInfo.terrainMax;
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
        currentBlockAmounts = new int[terrainElementsSettings.Length];
        // Establecemos relación de frecuencias
        int[] freqSuccesion = new int[terrainElementsSettings.Length];
        int accumulated = 0;
        for (int i = 0; i < terrainElementsSettings.Length; i++)
        {
            // TODO: Aplicar esto bien
            freqSuccesion[i] = terrainElementsSettings[i].terrainRatio + accumulated;
            accumulated += terrainElementsSettings[i].terrainRatio;
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
                    prefabToUse = terrainElementsSettings[0].terrainPrefab;
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
                    if(currentBlockAmounts[decideIndex] >= terrainElementsSettings[decideIndex].terrainMax &&
                        terrainElementsSettings[decideIndex].terrainMax > 0)
                    {
                        j--;
                        continue;
                    }
                    //
                    prefabToUse = terrainElementsSettings[decideIndex].terrainPrefab;
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
            if(currentBlockAmounts[i] < terrainElementsSettings[i].terrainMin)
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
            //for (int h = 0; h < absoluteDisplaceMentX; h++)
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
        //CheckTerrainAlignment();
        //
        AssignNeighbours();
    }

    // Ojo que tira mal
    void CheckTerrainAlignment()
    {
        //

        //
        for (int i = 0; i < squareSize; i++)
        {
            for (int j = 0; i < squareSize; i++)
            {
                Vector3 blockIdealPosition = activeBlocksMatrix[centralBlock, centralBlock].transform.position
                    + new Vector3(blockSize * (centralBlock - i), 0, blockSize * (centralBlock - j));
                // De momento a la brava
                activeBlocksMatrix[i, j].transform.position = blockIdealPosition;
            }
        }
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
