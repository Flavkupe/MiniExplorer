using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LocationData
{
    public void Clear()
    {
        this.locationText.Clear();
        this.imagePaths.Clear();
    }

    private List<string> locationText = new List<string>();
    public List<string> LocationText
    {
        get { return locationText; }
    }

    private List<ImagePathData> imagePaths = new List<ImagePathData>();
    public List<ImagePathData> ImagePaths
    {
        get { return imagePaths; }
    }
}

public class ImagePathData
{
    public string DisplayName { get; set; }
    public string Path { get; set; }
    public ImagePathData(string display, string path)
    {
        this.DisplayName = display;
        this.Path = path;
    }
}

