using UnityEngine;
using System;
using System.Collections;


public delegate void EventHandler<ChangedEventArgs>(object sender, ChangedEventArgs e);

public class Tilemap_D {

	private int size_x;
	private int size_y;

	private Tile_D[,] map;

    public event EventHandler<ChangedEventArgs> Changed;

    protected virtual void OnChanged(int x, int y, int id)
    {
        EventHandler<ChangedEventArgs> handler = Changed;
        if (handler != null)
            handler(this, new ChangedEventArgs(x, y, id));
    }

	public Tilemap_D(int size_x, int size_y)
	{
		this.size_x = size_x;
		this.size_y = size_y;

        map = new Tile_D[size_x, size_y];
	}

    public Tilemap_D(int size_x, int size_y, int[] tile_ids)
    {
        this.size_x = size_x;
        this.size_y = size_y;

        map = new Tile_D[size_x, size_y];
        for (int y = 0; y < size_y; y++)
        {
            for (int x = 0; x < size_x; x++)
            {
                map[x, y] = new Tile_D(tile_ids[x + size_x * y]);
            }
        }
    }

    public Tile_D GetTileAt(int x, int y)
    {
        return map[x,y];
    }

    public void SetTileAt(int x, int y, int id)
    {
        map[x, y].ID = id;
        OnChanged(x, y, id);
    }
}

public class ChangedEventArgs : EventArgs
{
    private int _x, _y, _id;

    public ChangedEventArgs(int x, int y, int id)
    {
        _x = x;
        _y = y;
        _id = id;
    }

    public int X
    {
        get { return _x; }
    }

    public int Y
    {
        get { return _y; }
    }

    public int ID
    {
        get { return _id; }
    }
}