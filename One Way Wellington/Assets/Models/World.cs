﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The world contains all information of the ship tiles 
// TODO: Rename to 'ship' 
[System.Serializable]
public class World
{
    private int width;
    private int height;

    private TileOWW[,] tiles;

    public World(int width, int height)
    {

        // Tile array
        tiles = new TileOWW[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j] = new TileOWW(i, j, "Empty");
            }
        }
    }

    public TileOWW GetTileAt(int x, int y)
    {
        if (0 <= x && x < 100 && 0 <= y && y < 100)
        {
            return tiles[x, y];
        }
        else return null;
    }

    public TileOWW[,] GetAllTiles()
    {
        return tiles;
    }

    public TileOWW GetRandomHullTile(bool avoidJobs = false)
    {
        // If no empty hull tile exists 
        if (BuildModeController.Instance.emptyHullTiles.Count == 0)
        {
            Debug.LogError("Couldn't find any empty hull tiles!");
            return null;
        }

        if (avoidJobs)
        {
            // Collect all job-less hull tiles into a list  
            List<TileOWW> joblessTiles = new List<TileOWW>();
            foreach (TileOWW tileJobCheck in BuildModeController.Instance.emptyHullTiles)
            {
                if (tileJobCheck.currentJobType == null)
                {
                    joblessTiles.Add(tileJobCheck);
                }
            }

            // If no job-less hull tiles exist
            if (joblessTiles.Count == 0)
            {
                Debug.LogError("No empty jobless hull tile exists!");
                return null;
            }

            // Pick random empty job-less hull tile 
            return joblessTiles[Random.Range(0, joblessTiles.Count)];
            
        }
        else
        {     
            // Pick random empty hull tile 
            return BuildModeController.Instance.emptyHullTiles[Random.Range(0, BuildModeController.Instance.emptyHullTiles.Count)];
        }
    }
}
