using UnityEngine;
using System.Collections;

public class Tile_D {
	private int id;
    public static int SOLID_THRESHOLD = 11;

	public Tile_D(int id)
	{
		this.id = id;
	}

	public int ID {
		get { return id; }
		set { this.id = value; }
	}

    public bool Solid {
        get { return id >= SOLID_THRESHOLD; }
    }
}
