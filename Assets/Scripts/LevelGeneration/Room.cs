
using System;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour, IHasName
{   	
	private NameTag nametag;

    public int Width { get { return this.Data.Width; } }
    public int Height { get { return this.Data.Height; } }

    public RoomData Data = new RoomData();

	public Door[] Doors = new Door[] {};

    public SpawnPoint[] SpawnPoints = new SpawnPoint[] { }; 

    public RoomConnector[] Connectors = new RoomConnector[] { };

    public RoomImageFrame[] RoomImageFrames = new RoomImageFrame[] { };


    void Awake()
    {        
    }

	void Start() 
	{
		this.nametag = this.GetComponentInChildren<NameTag>();
	}

	void Update() {

	}

	public void SetName(string name) 
	{ 
		this.Data.DisplayName = name; 
		if (this.nametag != null) 
		{
			this.nametag.RefreshName();
		}

		foreach (Door door in this.Doors) 
		{
			door.SetName(name);
		}
	}

	public string GetName() { return this.Data.DisplayName; }

    public RoomData ToRoomData()
    {
        RoomData data = this.Data.Clone(false);
        foreach (Door door in this.Doors) 
        {
            data.Doors.Add(door.ToDoorData()); 
        }

        foreach (RoomConnector connector in this.Connectors)
        {
            data.Connectors.Add(connector.ToRoomConnectorData()); 
        }

        foreach (SpawnPoint spawn in this.SpawnPoints)
        {
            data.SpawnPoints.Add(spawn.ToSpawnPointData());
        }

        data.PrefabID = this.name;
        return data;
    }

    public void PopulateParts()
    {
        this.Doors = this.transform.GetComponentsInChildren<Door>(true);
        this.Connectors = this.transform.GetComponentsInChildren<RoomConnector>(true);
        this.SpawnPoints = this.transform.GetComponentsInChildren<SpawnPoint>(true);
        this.RoomImageFrames = this.transform.GetComponentsInChildren<RoomImageFrame>(true);
    }
}

[Serializable]
public class RoomData : IMatchesPrefab
{    
    public int Width;
    public int Height;
    public string DisplayName;

    public string PrefabID { get; set; }

    private List<Location> locations = new List<Location>();
    private List<RoomConnectorData> connectors = new List<RoomConnectorData>();
    private List<DoorData> doors = new List<DoorData>();
    private List<SpawnPointData> spawnPoints = new List<SpawnPointData>();      

    public RoomData() 
    {    
    }

    public List<DoorData> Doors
    {
        get { return doors; }
    }

    public List<Location> Locations
    {
        get { return locations; }
    }

    public List<RoomConnectorData> Connectors
    {
        get { return connectors; }
    }

    public List<SpawnPointData> SpawnPoints
    {
        get { return spawnPoints; }
    }
    
    public Vector3 WorldCoords { get; set; }
    public Vector2 GridCoords { get; set; }

    public RoomData Clone(bool deepCopy = true)
    {
        RoomData data = new RoomData();
        data.WorldCoords = this.WorldCoords;
        data.GridCoords = this.GridCoords;
        data.Width = this.Width;
        data.Height = this.Height;
        data.PrefabID = this.PrefabID;
        data.doors = new List<DoorData>();
        data.connectors = new List<RoomConnectorData>();
        data.locations = new List<Location>();
        data.spawnPoints = new List<SpawnPointData>();

        if (deepCopy)
        {
            foreach (DoorData door in this.Doors)
            {
                data.Doors.Add(door.Clone());
            }

            foreach (Location location in this.Locations)
            {
                data.Locations.Add(location.Clone());
            }

            foreach (RoomConnectorData connector in this.Connectors)
            {
                data.Connectors.Add(connector.Clone());
            }

            foreach (SpawnPointData spawn in this.SpawnPoints)
            {
                data.SpawnPoints.Add(spawn.Clone());
            }
        }

        return data;
    }
}
