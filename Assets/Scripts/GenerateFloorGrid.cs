using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GenerateFloorGrid : MonoBehaviour
{

    [SerializeField] private int startAreaSize = 8;
    [SerializeField] private int dungeonAreaSize = 55;
    [SerializeField] private Material dungeonStartMat;
    [SerializeField] private Material dungeonRoomMat;
    [SerializeField] private Material dungeonPathMat;
    public Vector3 spawnPosition;


    void Awake()
    {

        GameObject[,] startFloorMap = new GameObject[startAreaSize,startAreaSize];
        Vector3[,] dungeonAreaGrid = new Vector3[dungeonAreaSize,dungeonAreaSize];

        // TODO: replace all uses of dungeonAreaGrid with dungeonFloorGrid instance of dungeonAreaGrid
        DungeonFloorGrid dungeonFloorGrid;

        foreach (Transform eachChild in transform) {
            if (eachChild.tag == "Floor"){
                // the gameobject of the tile i'll be placing down
                GameObject floorGameObject = eachChild.gameObject;

                dungeonFloorGrid = ScriptableObject.CreateInstance("DungeonFloorGrid") as DungeonFloorGrid;
                dungeonFloorGrid.init(dungeonAreaSize, dungeonAreaSize, floorGameObject, 8, 0.4f, dungeonStartMat, dungeonRoomMat, dungeonPathMat);
                dungeonFloorGrid.generateDungeon();
                this.spawnPosition = dungeonFloorGrid.spawnPosition;

                GameObject player = GameObject.FindGameObjectsWithTag("Player")[0];
                player.transform.position = this.spawnPosition;
                Destroy(eachChild.gameObject);
                break;
            }
        }

    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
