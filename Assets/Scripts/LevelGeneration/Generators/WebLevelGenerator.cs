using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class WebLevelGenerator : BaseLevelGenerator
{
    private static string activeMarkup = null;

    private List<LevelImage> images = new List<LevelImage>();

    public WebLevelGenerator()
    {
    }

    public override List<Location> GetBranchLocations(Location location)
    {
        MatchCollection doorMatches = Regex.Matches(activeMarkup, @"<a[^>]*href=\""([^\\""]+)\""[^>]*>([^<]+)</a>");                

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(activeMarkup);

        HtmlNode contentNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='mw-content-text']");
        HtmlNodeCollection subCategories = contentNode.SelectNodes("h2 | p | div/div/a/img");

        List<SubLocation> sublocations = new List<SubLocation>();
        SubLocation subLocation = new SubLocation();

        location.LocationData.Clear();

        foreach (HtmlNode node in subCategories)
        {
            if (node.Name == "h2")
            {
                // Create sublocation for h2 header
                if (subLocation != null)
                {
                    sublocations.Add(subLocation);
                }
                
                string title = node.SelectSingleNode("span[@class='mw-headline']").InnerText;
                subLocation = new SubLocation(location, title);                
            }
            else if (node.Name == "p")
            {
                string text = node.InnerText;
                // Store data from sublocation
                if (subLocation == null)
                {
                    // If no header seen yet, it's for the main article
                    location.LocationData.LocationText.Add(text);
                    continue;
                }
                else
                {                    
                    subLocation.LocationData.LocationText.Add(text);
                }
            }
            else if (node.Name == "img")
            {
                string imageUrl = node.GetAttributeValue("src", "");
                string imageCaption = node.GetAttributeValue("alt", "");
                subLocation.LocationData.ImagePaths.Add(new ImagePathData(imageCaption, imageUrl));
            }
        }

        List<Location> matchLocs = new List<Location>();
        Uri currentUri = new Uri(location.Path);
        
        int count = 0;
        foreach (Match match in doorMatches)
        {
            count++;
            
            // skip first and stop after max
            if (count == 1) { continue; }
            if (count > StageManager.MaxRoomDoors) 
            { 
                break; 
            }

            if (match.Groups.Count < 3) { continue; }

            string cleanString = match.Groups[1].Value;
            string nameString = match.Groups[2].Value;
            string path = "http://" + currentUri.Host + "/" + cleanString.TrimStart('/');
            Location loc = new MainLocation(path, nameString);
            matchLocs.Add(loc);
        }

        return matchLocs;
    }

    private IEnumerator ProcessImages(Location location)
    {
        // TODO

        if (activeMarkup == null)
        {
            yield return null;
        }

        Uri currentUri = new Uri(location.Path);
        images.Clear();
        int count = 0;
        MatchCollection imageMatches = Regex.Matches(activeMarkup, @"<img[^>]*src=\""([^\\""]+)\""[^>]*>");
        foreach (Match match in imageMatches)
        {
            if (count == 1) { continue; }
            if (match.Groups.Count < 2) { continue; }

            string cleanString = match.Groups[1].Value;

            LevelImage imageData = new LevelImage() { Name = cleanString };
            
            string url;
            if (cleanString.StartsWith("//")) 
            {
                url = "http://" + cleanString.TrimStart('/');
            }
            else
            {
                url = "http://" + currentUri.Host + "/" + cleanString.TrimStart('/');
            }
            

            WWW www = new WWW(url);
            yield return www;

            if (www.error != null)
            {
                continue;
            }

            if (www.texture != null)
            {
                imageData.Texture2D = new Texture2D(www.texture.width, www.texture.height);

                www.LoadImageIntoTexture(imageData.Texture2D);

                images.Add(imageData);
            }
        }

        this.CallOnAreaGenReady(new AreaGenerationReadyEventArgs() { AreaLocation = StageManager.CurrentLocation });
    }

    public override Location GetBackLocation(Location currentLcation)
    {
        return null;
    }

    public override List<string> GetLevelEntities(Location location)
    {
        return new List<string>();
    }

    public override bool CanLoadLocation(Location location)
    {
        return true;
    }

    public override IEnumerator PrepareAreaGeneration(Location location, MonoBehaviour caller)
    {
        // TODO: this one is special 
        WWW www = new WWW(location.Path);
        yield return www;        

        activeMarkup = www.text;

        yield return caller.StartCoroutine(this.ProcessImages(location));        
    }

    public override bool NeedsAreaGenPreparation { get { return true; } }

    public override List<LevelImage> GetLevelImages(Location location)
    {
        return this.images;
    }

    public override AreaTheme GetAreaTheme(Location location)
    {
        // TODO
        return AreaTheme.Circuit;
    }
}

