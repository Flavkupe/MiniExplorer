using UnityEngine;
using System.Collections;
using Assets.Scripts.LevelGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Text;

public class SceneLoader : MonoBehaviour 
{
    public AreaMapDrawer Minimap = null;
    public PlayerControl Player = null;

    public string InitialLocation = "C:\\test";

    public LevelGenerationMode Mode = LevelGenerationMode.File;

    private AreaGenerationReadyEventArgs delayedAreaLoadArgs = null;

    public Room[] RoomPrefabs;

    void Awake() 
    {
        Player.transform.gameObject.SetActive(false);
        StageManager.SetLevelGenMode(this.Mode);
        StageManager.SceneLoader = this;

        if (StageManager.CurrentLocation == null)
        {
            StageManager.CurrentLocation = new MainLocation(this.InitialLocation);
        }

        if (StageManager.LevelGenerator.NeedsAreaGenPreparation)
        {
            StageManager.LevelGenerator.OnAreaGenReady += LevelGenerator_OnAreaGenReady;
        }
    }

    void Start()
    {
        Location current = StageManager.CurrentLocation;        

        if (StageManager.LevelGenerator.NeedsAreaGenPreparation)
        {
            StartCoroutine(StageManager.LevelGenerator.PrepareAreaGeneration(StageManager.CurrentLocation, this));
        }
        else 
        {
            this.GenerateLevel(current);
        }
    }

    void LevelGenerator_OnAreaGenReady(object sender, AreaGenerationReadyEventArgs e)
    {
        this.delayedAreaLoadArgs = e;        
    }

    private void GenerateLevel(Location currentLocation)
    {
        Area area = Instantiate(ResourceManager.GetEmptyAreaPrefab()) as Area;
        area.name = currentLocation.Name ?? currentLocation.Path;
        area.DisplayName = area.name;

        RoomGrid grid = StageManager.GetAreaRoomGridOrNull(currentLocation);
        if (grid == null)
        {
            // Generate new map if none exists            
            grid = StageManager.LevelGenerator.GenerateRoomGrid(currentLocation);
        }

        area.RoomGrid = grid;
        StageManager.CurrentArea = area;
        Queue<LevelImage> levelImages = StageManager.LevelGenerator.GetLevelImages(currentLocation).ToQueue();
        List<Room> instances = new List<Room>();
        foreach (RoomData roomData in grid.Rooms)
        {
            Room model = ResourceManager.GetRoomByPrefabID(area.Theme, roomData.PrefabID);
            Room roomInstance = Instantiate(model) as Room;
            roomInstance.transform.parent = area.transform;
            roomInstance.transform.position = roomData.WorldCoords;

            int locationCount = 0;
            foreach (Door door in roomInstance.Doors)
            {
                if (locationCount >= roomData.Locations.Count)
                {
                    door.gameObject.SetActive(false);
                }
                else
                {
                    Location currentLoc = roomData.Locations[locationCount];
                    door.SetLocation(currentLoc);
                    door.SetName(currentLoc.Name);
                    if (StageManager.PreviousLocation != null && currentLoc.LocationKey == StageManager.PreviousLocation.LocationKey)
                    {
                        this.Player.TeleportToLocation(door.transform.position);
                    }

                    locationCount++;
                }
            }

            int entityCount = 0;
            foreach (SpawnPoint spawn in roomInstance.SpawnPoints)
            {
                if (entityCount < roomData.SpawnPoints.Count)
                {
                    SpawnPointData spawnData = roomData.SpawnPoints[entityCount];
                    if (!string.IsNullOrEmpty(spawnData.Entity))
                    {                        
                        Enemy enemyModel = ResourceManager.GetRandomEnemyOfType(spawnData.EnemyTypes);

                        if (enemyModel != null)
                        {
                            Enemy enemyInstance = Instantiate(enemyModel) as Enemy;
                            enemyInstance.name = spawnData.Entity;
                            spawn.Data = spawnData;
                            enemyInstance.transform.position = spawn.transform.position;
                            entityCount++;
                        }
                    }
                }

                spawn.gameObject.SetActive(false);
            }
            
            
            foreach (RoomImageFrame frame in roomInstance.RoomImageFrames)
            {
                if (!frame.IsUsed)
                {
                    if (levelImages.Count == 0)
                    {
                        frame.gameObject.SetActive(false);
                    }
                    else
                    {
                        LevelImage image = levelImages.Dequeue();
                        frame.SetLevelImage(image);                     
                    }
                }
            }
            

            foreach (RoomConnectorData connectorData in roomData.Connectors)
            {
                foreach (RoomConnector instanceConnector in roomInstance.Connectors)
                {
                    if (connectorData.IsSamePrefab(instanceConnector))
                    {
                        instanceConnector.gameObject.SetActive(!connectorData.Used);
                        break;
                    }
                }
            }            

            instances.Add(roomInstance);
        }

        area.Rooms = instances.ToArray();
        StageManager.KnownAreaMap[currentLocation.LocationKey] = area.RoomGrid;

        if (this.Minimap != null)
        {
            this.Minimap.RefreshMinimap();
        }

        this.Player.transform.gameObject.SetActive(true);
    }
        
    void Update () 
    {
        if (this.delayedAreaLoadArgs != null)
        {
            try
            {
                this.GenerateLevel(this.delayedAreaLoadArgs.AreaLocation);
            }
            finally
            {
                this.delayedAreaLoadArgs = null;
            }
            
        }
    }
}
