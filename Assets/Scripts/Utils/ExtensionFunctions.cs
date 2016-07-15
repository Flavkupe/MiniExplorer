using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class ExtensionFunctions
{
    private static System.Random rand = new System.Random(); 

    public static T GetRandom<T>(this IEnumerable<T> list)
    {
        List<T> items = list.ToList();

        if (items.Count == 0) 
        {
            return default(T);
        }                      

        int index = rand.Next(0, items.Count);
        return items[index];           
    }

    public static Queue<T> ToQueue<T>(this IEnumerable<T> list) 
    {
        Queue<T> queue = new Queue<T>();
        foreach (T item in list)
        {
            queue.Enqueue(item);
        }

        return queue;
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }    

    public static Vector2 SetX(this Vector2 vector, float x)
    {
        return new Vector2(x, vector.y);
    }

    public static Vector2 SetY(this Vector2 vector, float y)
    {
        return new Vector2(vector.x, y);
    }

    public static Vector3 SetX(this Vector3 vector, float x)
    {
        return new Vector3(x, vector.y, vector.z);
    }

    public static Vector3 SetY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, y, vector.z);
    }

    public static Vector3 SetZ(this Vector3 vector, float z)
    {
        return new Vector3(vector.x, vector.y, z);
    }

    public static R GetValueOrDefault<T, R>(this IDictionary<T, R> dict, T key)
    {
        if (dict.ContainsKey(key))
        {
            return dict[key];
        }

        return default(R);
    }

    public static bool IsSamePrefab(this IMatchesPrefab model, MonoBehaviour prefab)
    {
        if (model != null && prefab != null &&
            string.Equals(model.PrefabID, prefab.name, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public static void SetPixel(this Texture2D texture, int x, int y, Color color, int radius) 
    {
        for (int i = -radius; i <= radius; ++i)
        {
            for (int j = -radius; j <= radius; ++j)
            {
                texture.SetPixel((int)x + i, (int)y + j, color);
            }
        }
    }
}
