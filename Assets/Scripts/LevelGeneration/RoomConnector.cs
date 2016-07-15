using UnityEngine;
using System.Collections;
using System;

public class RoomConnector : MonoBehaviour 
{    
    public Room ParentRoom = null;

    public RoomConnectorData Data = new RoomConnectorData();

    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    public string PrefabID { get { return this.Data.PrefabID; } }

    public ConnectorType Type { get { return this.Data.Type; } }

    public ConnectorPosition Position { get { return this.Data.Position; } }    

    public RoomConnectorData ToRoomConnectorData()
    {
        RoomConnectorData data = this.Data.Clone();
        data.PrefabID = this.name;
        data.RelativeGridCoords = this.GetRelativeGridCoords();
        return data;
    }

    /// <summary>
    /// Gets the grid location of this connector relative to the room and step size,
    /// where the top left is 0, 0 and the bottom right is 
    /// ((roomWidth/StageManager.StepSize)-1), ((roomHeight/StageManager.StepSize)-1)
    /// </summary>
    /// <returns></returns>
    public Vector2 GetRelativeGridCoords()
    {
        // Shift location such that room's bottom-left is at 0,0, rather than center at 0,0
        int locX = (int)this.transform.localPosition.x + (this.ParentRoom.Width / 2);
        int locY = (int)this.transform.localPosition.y + (this.ParentRoom.Height / 2);
        locX = locX / StageManager.StepSize;
        locY = locY / StageManager.StepSize;
        return new Vector2(locX, locY);
    }

    public bool IsMatchingConnector(RoomConnectorData other)
    {
        return this.Data.IsMatchingConnector(other);
    }

    public bool IsMatchingConnector(RoomConnector other)
    {
        return this.Data.IsMatchingConnector(other.Data);
    }
}

[Serializable]
public class RoomConnectorData : IMatchesPrefab 
{
    public string PrefabID { get; set; }

    public bool Used { get; set; }

    public ConnectorType Type;

    public ConnectorPosition Position;

    public Vector2 RelativeGridCoords { get; set; }

    public RoomConnectorData Clone()
    {
        RoomConnectorData data = this.MemberwiseClone() as RoomConnectorData;
        data.PrefabID = this.PrefabID;
        return data;
    }

    public bool IsMatchingConnector(RoomConnectorData other)
    {
        if (this.Type == other.Type)
        {
            if (this.Position == ConnectorPosition.Bottom && other.Position == ConnectorPosition.Top ||
                this.Position == ConnectorPosition.Top && other.Position == ConnectorPosition.Bottom ||
                this.Position == ConnectorPosition.Right && other.Position == ConnectorPosition.Left ||
                this.Position == ConnectorPosition.Left && other.Position == ConnectorPosition.Right)
            {
                return true;
            }
        }

        return false;
    }
}

public enum ConnectorType 
{
    OneByThreeEnd,
    ThreeByOneEnd
}

public enum ConnectorPosition
{
    Top,
    Bottom,
    Left, 
    Right
}