using UnityEngine;
using System.Collections;

public class Tilemap_D {

	private int size_x;
	private int size_y;

	private Tile_D[,] map;

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
    }
}
