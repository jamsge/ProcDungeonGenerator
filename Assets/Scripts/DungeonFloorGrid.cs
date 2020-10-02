using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class DungeonFloorGrid : ScriptableObject 
{
    private Vector3[,] dungeonAreaGrid;
    private GameObject[,] dungeonAreaTiles;
    private GameObject floorGameObject;
    private List<Tree> trees;
    private List<Room> rooms;
    private int minRoomSize;
    private float minRoomFactor;
    private int roomCreationPercentage;
    private int dungeonAreaSizeX;
    private int dungeonAreaSizeZ;
    private float offset;

    private Material dungeonStartMat;
    private Material dungeonRoomMat;
    private Material dungeonPathMat;
    public Vector3 spawnPosition;

    public void init(int dungeonAreaSizeX, int dungeonAreaSizeZ, GameObject floor, int minRoomSize, float minRoomFactor, Material dungeonStartMat,
    Material dungeonRoomMat, Material dungeonPathMat){
        this.dungeonAreaSizeX = dungeonAreaSizeX;
        this.dungeonAreaSizeZ = dungeonAreaSizeZ;
        dungeonAreaGrid = new Vector3[dungeonAreaSizeX, dungeonAreaSizeZ];
        dungeonAreaTiles = new GameObject[dungeonAreaSizeX, dungeonAreaSizeZ];
        floorGameObject = floor;
        trees = new List<Tree>();
        rooms = new List<Room>();
        this.minRoomSize = minRoomSize;
        this.minRoomFactor = minRoomFactor;
        this.roomCreationPercentage = 85;
        this.dungeonStartMat = dungeonStartMat;
        this.dungeonRoomMat = dungeonRoomMat;
        this.dungeonPathMat = dungeonPathMat;

        this.offset = 2; 
        Vector3 currentPosition = new Vector3();
        for (int i = 0; i < dungeonAreaSizeZ; i++){
            currentPosition.z = 0;
            for (int j = 0; j < dungeonAreaSizeX; j++){
                this.setGridElementPos(i, j, currentPosition);
                currentPosition.z += offset;
            }
            currentPosition.x += offset;
        }
    }
    
    public void changeTileColor(Material material){
        floorGameObject.GetComponent<Renderer>().material = material;
    }

    // x and z are in grid coordinates, not world position coordinates
    public void createTile(int x, int z) {
        floorGameObject.name = x + " " + z;
        if (x >= dungeonAreaSizeX || x < 0 || z >= dungeonAreaSizeZ || z < 0){
            Vector3 pos = gridToWorldPosition(x, z);
            Instantiate(floorGameObject, pos, Quaternion.identity);
        } else {
            dungeonAreaTiles[x,z] = Instantiate(floorGameObject, dungeonAreaGrid[x,z], Quaternion.identity);
        }
    }

    public GameObject getTile(int x, int z){
        if (x >= dungeonAreaSizeX || z >= dungeonAreaSizeZ || x < 0 || z < 0){
            return null;
        }
        return dungeonAreaTiles[x,z];
    }

    public Vector3 gridToWorldPosition(int x, int z){
        return new Vector3(x * offset, 0, z * offset);
    }


    int iterationCount = 0;
    public void generateDungeon(){
        void startTree(){
            trees.Add(new Tree(1, 1, dungeonAreaGrid.GetLength(0) - 2, dungeonAreaGrid.GetLength(1) - 2));
            createLeaf(0);
        }
        void createLeaf(int parentIndex, int overrideNextParent = -1){
            iterationCount++;

            int x = trees[parentIndex].x;
            int z = trees[parentIndex].z;
            int w = trees[parentIndex].w;
            int h = trees[parentIndex].h;

            trees[parentIndex].centerX = x + w/2;
            trees[parentIndex].centerZ = z + h/2;

            bool canSplit = false;

            char splitType = Random.Range(0,2) == 0 ? 'v' : 'h';
            if (minRoomFactor * w < minRoomSize) splitType = 'h';
            if (minRoomFactor * h < minRoomSize) splitType = 'v';

            Tree leaf1 = null;
            Tree leaf2 = null;

            if (splitType == 'v'){
                int roomSize = (int)(minRoomFactor * (float)w);
                if (roomSize >= minRoomSize){
                    // don't know if end range should be inclusive or exclusive
                    int w1 = Random.Range(roomSize, w-roomSize);
                    int w2 = w - w1;
                    leaf1 = new Tree(x, z, w1, h, 'v', true);
                    leaf2 = new Tree(x + w1, z, w2, h, 'v', true);
                    canSplit = true;
                } 
            } else {
                int roomSize = (int)(minRoomFactor * (float)h);
                if (roomSize >= minRoomSize){
                    // don't know if end range should be inclusive or exclusive
                    int h1 = Random.Range(roomSize, h-roomSize);
                    int h2 = h - h1;
                    leaf1 = new Tree(x, z, w, h1, 'h', true);
                    leaf2 = new Tree(x, z + h1, w, h2, 'h', true);
                    canSplit = true;
                } 
            }

            if (canSplit){
                leaf1.parentIndex = parentIndex;
                trees.Add(leaf1);
                trees[parentIndex].l = trees.Count - 1;
                
                leaf2.parentIndex = parentIndex;
                trees.Add(leaf2);
                trees[parentIndex].r = trees.Count - 1;
                createLeaf(trees[parentIndex].l);
                createLeaf(trees[parentIndex].r);
            } else {
                trees[parentIndex].hasChildren = false;
            }

            // aaaaaaaaaaaaaaaaaaaaa nightmarenightmarenightmarenightmarenightmarenightmarenightmare
            // Debug.Log("this is iteration #" + iterationCount);
            // Debug.Log(canSplit);
            // Debug.Log(trees[parentIndex].l);
            // Debug.Log(trees[parentIndex].r);
            // Debug.Log(parentIndex);
            // Debug.Log(trees.Count);
        }

        void createRooms(){
            changeTileColor(dungeonRoomMat);
            for (int i = 0; i < trees.Count; i++){
                Tree leaf = trees[i];
                if (leaf.hasChildren)
                    continue;
                
                if (Random.Range(1,101) <= roomCreationPercentage){
                    Room room = new Room();
                    room.correspondingLeafID = i;
                    room.w = Random.Range(minRoomSize, leaf.w) - 1;
                    room.h = Random.Range(minRoomSize, leaf.h) - 1;
                    room.x = leaf.x + (leaf.w-room.w)/2 + 1;
                    room.z = leaf.z + (leaf.h-room.h)/2 + 1;
                    room.split = leaf.split;

                    room.centerX = room.x + room.w/2;
                    room.centerZ = room.z + room.h/2;

                    rooms.Add(room);
                }
            }

            for (int i = 0; i < rooms.Count; i++){
                var room = rooms[i];
                for (int x = room.x; x < room.x + room.w; x++){
                    for (int z = room.z; z < room.z + room.h; z++){
                        createTile(x, z);
                    }
                }
            }
        }

        void joinRooms(){
            changeTileColor(dungeonPathMat);
            for (int i = 0; i < trees.Count; i++){
                int a = trees[i].l;
                int b = trees[i].r;
                // Debug.Log("iteration #" + i);
                // Debug.Log(a);
                // Debug.Log(b);
                if (a > 0 && b > 0)
                connectRooms(trees[a], trees[b]);
            }
        }

        void connectRooms(Tree leaf1, Tree leaf2){
            int x = System.Math.Min(leaf1.centerX, leaf2.centerX);
            int z = System.Math.Min(leaf1.centerZ, leaf2.centerZ);
            int w = 1;
            int h = 1;

            if (leaf1.split == 'h'){
                x -= w/2 + 1;
                h = System.Math.Abs(leaf1.centerZ - leaf2.centerZ);
            } else {
                z -= h/2 + 1;
                w = System.Math.Abs(leaf1.centerX - leaf2.centerX);
            }

            if (x < 0)
                x = 0;
            if (z < 0)
                z = 0;

            for (int i = x; i < x+w; i++){
                for (int j = z; j < z+h; j++){

                    // this will throw an error i think
                    if (this.getTile(i,j) == null){
                        this.createTile(i,j);
                    }
                }
            }
        }

        void clearDeadEnds(){
            bool done = false;
            int iteration = 0;
            while (!done){
                iteration++;
                done = true;
                for (int i = 0; i < dungeonAreaSizeX; i++){
                    for (int j = 0; j < dungeonAreaSizeZ; j++){
                        if (getTile(i,j) == null){
                            // Debug.Log("no tile found for " + i + " " + j);
                            continue;
                        } 

                        int blankCount = checkNearby(i,j);
                        if (blankCount == 3){
                            Destroy(dungeonAreaTiles[i,j]);
                            dungeonAreaTiles[i,j] = null;
                            done = false;
                        }
                    }
                }
                
                // If this loops more than 1000 times, then something is definitely wrong and I don't want unity to freeze
                if (iteration > 1000){
                    break;
                }
            }
        }

        int checkNearby(int x, int z){
            int count = 0;
            if (getTile(x,z-1) == null) count++;
            if (getTile(x,z+1) == null) count++;  
            if (getTile(x+1,z) == null) count++;
            if (getTile(x-1,z) == null) count++;
            return count;
        }

        void createOuterStartRoom(){
            changeTileColor(dungeonStartMat);
            List<Room> firstRoomCandidates = new List<Room>();
            int edgeCheckThickness = minRoomSize + 2;
            foreach (Room room in rooms){
                for (int x = 0; x < dungeonAreaSizeX; x++){
                    for (int z = 0; z < dungeonAreaSizeZ; z++){
                        if (x < edgeCheckThickness || x > dungeonAreaSizeX - edgeCheckThickness
                        || z < edgeCheckThickness || z > dungeonAreaSizeX - edgeCheckThickness){
                            if (room.centerX == x && room.centerZ == z){
                                firstRoomCandidates.Add(room);
                            }
                        }
                    }
                }
            }

            Room firstRoom = firstRoomCandidates[Random.Range(0,firstRoomCandidates.Count)];
            int firstRoomCenterX = firstRoom.centerX;
            int firstRoomCenterZ = firstRoom.centerZ;
            bool xEdgePositive = false;
            bool xEdgeNegative = false;
            bool zEdgePositive = false;
            bool zEdgeNegative = false;

            if (firstRoomCenterX < edgeCheckThickness)
                xEdgeNegative = true;
            if (firstRoomCenterX > dungeonAreaSizeX - edgeCheckThickness)
                xEdgePositive = true;
            if (firstRoomCenterZ < edgeCheckThickness)
                zEdgeNegative = true;
            if (firstRoomCenterZ > dungeonAreaSizeZ - edgeCheckThickness)
                zEdgePositive = true;
            Debug.Log(firstRoomCenterX + " " + firstRoomCenterZ);
            Debug.Log("xEdgeNegative " + xEdgeNegative);
            Debug.Log("xEdgePositive " + xEdgePositive);
            Debug.Log("zEdgeNegative " + zEdgeNegative);
            Debug.Log("zEdgePositive " + zEdgePositive);
            
            if (xEdgePositive && zEdgePositive){
                if (Random.Range(0,2) == 0)
                    xEdgePositive = false;
                else {
                    zEdgePositive = false;
                }
            }
            if (xEdgePositive && zEdgeNegative){
                if (Random.Range(0,2) == 0)
                    xEdgePositive = false;
                else {
                    zEdgeNegative = false;
                }
            }
            if (xEdgeNegative && zEdgeNegative){
                if (Random.Range(0,2) == 0)
                    xEdgeNegative = false;
                else {
                    zEdgeNegative = false;
                }
            }
            if (xEdgeNegative && zEdgePositive){
                if (Random.Range(0,2) == 0)
                    xEdgeNegative = false;
                else {
                    zEdgePositive = false;
                }
            }

            int startTileX = 0;
            int startTileZ = 0;
            for (int i = 0; i < minRoomSize * 2; i++){
                if (zEdgePositive)
                    if (getTile(firstRoomCenterX, firstRoomCenterZ + i) == null){
                        startTileX = firstRoomCenterX;
                        startTileZ = firstRoomCenterZ + i;
                        break;
                    }
                if (xEdgePositive)
                    if (getTile(firstRoomCenterX + i, firstRoomCenterZ) == null){
                        startTileX = firstRoomCenterX + i;
                        startTileZ = firstRoomCenterZ;
                        break;
                    }
                if (zEdgeNegative)
                    if (getTile(firstRoomCenterX, firstRoomCenterZ - i) == null){
                        startTileX = firstRoomCenterX;
                        startTileZ = firstRoomCenterZ - i;
                        break;
                    }
                if (xEdgeNegative)
                    if (getTile(firstRoomCenterX - i, firstRoomCenterZ) == null){
                        startTileX = firstRoomCenterX - i;
                        startTileZ = firstRoomCenterZ;
                        break;
                    }
            }
            
            int startRoomPathLength = minRoomSize/2;
            int endTileX = -1;
            int endTileZ = -1;
            for (int i = 0; i < startRoomPathLength; i++){
                if (xEdgePositive)
                    createTile(startTileX + i, startTileZ);
                if (xEdgeNegative)
                    createTile(startTileX - i, startTileZ);
                if (zEdgePositive)
                    createTile(startTileX, startTileZ + i);
                if (zEdgeNegative)
                    createTile(startTileX, startTileZ - i);

                if (i == startRoomPathLength - 1){
                    if (xEdgePositive){
                        endTileX = startTileX + i;
                        endTileZ = startTileZ;
                    }
                    if (xEdgeNegative){
                        endTileX = startTileX - i;
                        endTileZ = startTileZ;
                    }
                    if (zEdgePositive){
                        endTileX = startTileX;
                        endTileZ = startTileZ + i;
                    }
                    if (zEdgeNegative){
                        endTileX = startTileX;
                        endTileZ = startTileZ - i;
                    }
                    Debug.Log(endTileX);
                    Debug.Log(endTileZ);
                }
            }

            int startRoomSizeX = Random.Range(minRoomSize, (int)(minRoomSize * 1.5f)) - 1;
            int startRoomSizeZ = Random.Range(minRoomSize, (int)(minRoomSize * 1.5f)) - 1;
            int currentPosX = -1;
            int currentPosZ = -1;
            Debug.Log(endTileX);
            Debug.Log(endTileZ);
            for (int i = 0; i < startRoomSizeX; i++){
                for (int j = 0; j < startRoomSizeZ; j++){
                    if (xEdgePositive){
                        currentPosX = endTileX + startRoomSizeX - i;
                        currentPosZ = endTileZ - startRoomSizeZ/2 + j;
                        createTile(currentPosX,currentPosZ);
                    } else if (zEdgePositive){
                        currentPosX = endTileX - startRoomSizeX/2 + i;
                        currentPosZ = endTileZ + startRoomSizeZ - j;
                        createTile(currentPosX,currentPosZ);
                    } else if (xEdgeNegative){
                        currentPosX = endTileX - startRoomSizeX + i;
                        currentPosZ = endTileZ - startRoomSizeZ/2 + j;
                        createTile(currentPosX,currentPosZ);
                    } else if (zEdgeNegative){
                        currentPosX = endTileX - startRoomSizeX/2 + i;
                        currentPosZ = endTileZ - startRoomSizeZ + j;
                        createTile(currentPosX,currentPosZ);
                    }
                    if (i == startRoomSizeX/2 && j == startRoomSizeZ/2){
                        spawnPosition = gridToWorldPosition(currentPosX, currentPosZ);
                    }
                }
            }
            Debug.Log(spawnPosition);
        }

        startTree();
        createRooms();
        joinRooms();
        clearDeadEnds();
        createOuterStartRoom();
    }

    public void setGridElementPos(int xGrid, int zGrid, int x, int z){
        dungeonAreaGrid[xGrid,zGrid] = new Vector3(x, 0, z);
    }

    public void setGridElementPos(int xGrid, int zGrid, Vector3 position){
        // Debug.Log(xGrid + " " + zGrid);
        // Debug.Log(position);
        dungeonAreaGrid[xGrid, zGrid] = position;
    }
}