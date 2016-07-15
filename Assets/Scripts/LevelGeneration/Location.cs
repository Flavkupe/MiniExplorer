

using System;
using System.Collections.Generic;


[Serializable]
public abstract class Location 
{
	public string Path;
	public string Name;

    private LocationData locationData = new LocationData();
    public LocationData LocationData { get { return locationData; } }

    public Location()
    {
    }

    public Location(string path, string name)
    {
        this.Path = path;
        this.Name = name;
    }

    public abstract bool IsBackLocation { get; }
    public abstract string LocationKey { get; }
    public abstract Location Clone();    
}

[Serializable]
public class MainLocation : Location
{
    public override bool IsBackLocation { get { return false; } }

    public List<Location> subLocations = new List<Location>();

    public List<Location> SubLocations { get { return this.subLocations; } }

    public override Location Clone() 
    {
        MainLocation loc = new MainLocation(this.Path, this.Name);
        loc.subLocations = new List<Location>(this.subLocations);
        return loc;
    }

    public MainLocation()
        : base()
    {
    }

    public MainLocation(string path)
        : base(path, path)
    {
    }

    public MainLocation(string path, string name)
        : base(path, name)
    {
    }

    public override string LocationKey { get { return this.Path; } }
}

[Serializable]
public class BackLocation : Location
{
    public override bool IsBackLocation { get { return true; } }

    public override Location Clone()
    {
        return new BackLocation(this.Path, this.Name);
    }

    public BackLocation()
        : base()
    {
    }

    public BackLocation(string path, string name)
        : base(path, name)
    {
    }

    public override string LocationKey { get { return this.Path; } }
}

/// <summary>
/// A location that has a parent Location. The content
/// of the locations (text, images, etc) should be in here.
/// </summary>
[Serializable]
public class SubLocation : Location
{
    public override bool IsBackLocation { get { return false; } }

    public Location ParentLocation { get; set; }

    public override Location Clone()
    {
        SubLocation loc = new SubLocation();
        loc.Name = this.Name;
        loc.Path = this.Path;
        loc.ParentLocation = this.ParentLocation.Clone();
        return loc;
    }

    public SubLocation()
        : base()
    {
    }

    public SubLocation(Location parent, string name)
        : base(parent.Path, name)
    {
        this.ParentLocation = parent.Clone();
    }

    public override string LocationKey { get { return this.Path + "###" + this.Name; } }
}

