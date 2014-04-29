using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
//[RequireComponent(typeof(MeshCollider))]
public class Tilemap : MonoBehaviour {
    
    //public stuff for the Inspector
    public enum InputMethod { MANUAL, FILE };

	public List<GameObject> auras = new List<GameObject> ();

    public InputMethod input_method = InputMethod.FILE;

    public int size_x = 40; //tiles wide
    public int size_y = 24; //tiles high

	public int tile_resolution = 32;//16; //pixels across, assume square

    public float tile_size = 1.0f;

	public int solid_threshold = 10; //where tiles become solid;

    public Texture2D tileset; //texture of the tiles

    public string tile_ids; //list of tile ides

    public string tiled_filepath;

    //private stuff for actual work
    private Tilemap_D _map;
    private int[] _converted_tile_ids;

	private int _n_rows, _n_cols;

	// Storing where the map is in screenspace is a big deal... Just have to figure out
	private Vector2 location;
	private int b_type = -1;
	private string blstr = "blank";
	private int acount;

	private Vector2 lastP = new Vector2 (-1f, -1f);


    //Event handler for setting a new tile
    private void MapChanged(object sender, ChangedEventArgs e)
    {
        int x = e.X;
        int y = e.Y;
        int id = e.ID;

        int tile_x = _map.GetTileAt(x, y).ID % _n_cols;
        int tile_y = _map.GetTileAt(x, y).ID / _n_cols;
        int h = (int)((size_y - 1) * tile_size);

        MeshFilter mesh_filter = GetComponent<MeshFilter>();

        Vector2[] uv = mesh_filter.sharedMesh.uv;

        uv[4 * (x + y * size_x) + 0] = new Vector2((float)(tile_x) / _n_cols, 1 - (float)(tile_y + 1) / _n_rows);
        uv[4 * (x + y * size_x) + 1] = new Vector2((float)(tile_x + 1) / _n_cols, 1 - (float)(tile_y + 1) / _n_rows);
        uv[4 * (x + y * size_x) + 2] = new Vector2((float)(tile_x) / _n_cols, 1 - (float)(tile_y) / _n_rows);
        uv[4 * (x + y * size_x) + 3] = new Vector2((float)(tile_x + 1) / _n_cols, 1 - (float)(tile_y) / _n_rows);

        mesh_filter.sharedMesh.uv = uv;

        var findTileCollider = from collider in GetComponents<BoxCollider2D>()
                               where collider.center.x == (x * tile_size * 2 + tile_size) / 2 &&
                                     collider.center.y == (2 * (h - y * tile_size) + tile_size) / 2
                               select collider;
        bool found = false;

        foreach (BoxCollider2D collider in findTileCollider)
        {
        }

        if (!_map.GetTileAt(x, y).Solid)
        {
            foreach (BoxCollider2D collider in findTileCollider)
            {
                DestroyImmediate(collider);
            }
        }
        else
        {
            foreach (BoxCollider2D collider in findTileCollider)
            {
                found = true;
            }
            if (!found)
            {
                BoxCollider2D collider = this.gameObject.AddComponent<BoxCollider2D>();
                collider.center = new Vector2((x * tile_size * 2 + tile_size) / 2, (2 * (h - y * tile_size) + tile_size) / 2);
                collider.size = new Vector2(tile_size, tile_size);
                collider.name = "Wall";
            }
        }

    }


	// Use this for initialization
	void Start () {
        //_map = new Tilemap_D(size_x, size_y);
		this.Generate (); //There's not enough set variables at the start, no point in running Generate to do wasted effort.
		Camera.main.enabled = true;
	}

    public void Generate()
    {
        if (input_method == InputMethod.FILE)
        {
            using (StreamReader sr = File.OpenText(@tiled_filepath))
            {
                JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(sr));
                size_x = Convert.ToInt32(o["width"]);
                size_y = Convert.ToInt32(o["height"]);
                tile_resolution = Convert.ToInt32(o["tileheight"]);
                try
                {
                    solid_threshold = Convert.ToInt32(o["tilesets"][0]["properties"]["solid_threshold"]);
					//solid_threshold = 10;
                } catch {
                 solid_threshold = 1;    
                }
                //tileset = new Texture2D(size_x * tile_resolution, size_y * tile_resolution);
                //string path = (string)o["tilesets"][0]["image"];
                //using (StreamReader imgr = new StreamReader(File.Open(path, FileMode.Open)))
                //{
                //    tileset.LoadImage(imgr.r
                //}
                JArray data = (JArray)o["layers"][0]["data"];
                tile_ids = data.ToString();
                tile_ids = tile_ids.Replace("\n", "");
                tile_ids = tile_ids.Substring(1, tile_ids.Length - 2); //remove brackets
            }
        }
        Tile_D.SOLID_THRESHOLD = solid_threshold;
        string[] tmp = tile_ids.Split(',');
        _converted_tile_ids = new int[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            _converted_tile_ids[i] = Convert.ToInt32(tmp[i]);
        }
        _map = new Tilemap_D(size_x, size_y, _converted_tile_ids);
        _map.Changed += new EventHandler<ChangedEventArgs>(MapChanged); //set the event handler
        _n_rows = tileset.height / tile_resolution;
        _n_cols = tileset.width / tile_resolution;
        while(GetComponents<BoxCollider2D>().Length > 0)
            DestroyImmediate(GetComponent<BoxCollider2D>());
        BuildMesh();
	}
	
	private int aexists (int x, int y){
		for(int i = 0; i < auras.Count; i++)
			if(auras[i].GetComponent<Aura>().getBase().x == x && auras[i].GetComponent<Aura>().getBase().y == y)
				return i;
		return -1;
	}

    public void PlaceBlock(int x, int y, int id)
    {
		if(_map.GetTileAt(x,y).ID == 11 || (this.lastP.x == x && this.lastP.y == y))
			return;
		this.lastP = new Vector2 ((float)x, (float)y);
        _map.SetTileAt(x, y, id);
		int i = aexists (x, y);
		//Debug.Log (id);
		if(aexists(x,y) != -1){
			GameObject g = auras[i];
			auras.Remove(auras[i]);
			DestroyImmediate(g);
		}
		if((id <= 10) && (id > 1) && (aexists(x,y) == -1)){
			auras.Add(new GameObject("Aura" + ++this.acount));
			int aid = auras.Count - 1;
			auras[aid].AddComponent(blstr);
			auras[aid].transform.Translate(x,y,10);
			auras[aid].GetComponent<Aura>().setBase(auras[aid].transform.position);
			Vector3 cen = auras[aid].transform.position;
			auras[aid].transform.position = new Vector3(cen.x+.5f, (float)this.size_y - (float)cen.y - .5f,1.0f);
		}
    }

    // Breaks the vector down to indices and calls the other function
    // returns the center position of the tile
    public Vector2 getPosOfTile(Vector2 tile)
    {
        int x = (int)tile.x;
        int y = (int)tile.y;

        return getPosOfTile(x, y);
    }

    // Takes in x and y indices for where the tile is on the map and returns the 
    // center location of the tile.
    public Vector2 getPosOfTile(int x, int y)
    {
        return new Vector2(x * tile_size + tile_size / 2, (size_y - y) * tile_size - tile_size / 2);
    }

    // Returns a vector of the x, y coords of the tile containing some position pos.
    public Vector2 GetTileForPos(Vector2 pos)
    {
        int x = (int)Math.Floor(pos.x / tile_size);
        int y = size_y - (int)Math.Floor(pos.y / tile_size) - 1;

        return new Vector2(x, y);
    }

    public Vector2[] getTilesBetweenPoints(Vector2 start, Vector2 end)
    {
        List<Vector2> tiles = new List<Vector2>();

        Vector2 u = new Vector2(end.x - start.x, end.y - start.y);
        u = new Vector2(u.x / u.magnitude, u.y / u.magnitude);
        //traverse the ray...
        for (float x = start.x, y = start.y; x < end.x && y < end.y; x += u.x / 2, y += u.y / 2)
        {
            Vector2 tile = GetTileForPos(new Vector2(x, y));
            if (!tiles.Contains(tile))
                tiles.Add(tile);
        }
        return tiles.ToArray();
    }

	void Update(){
		Vector3 mouseTile = mouseToTile();

		//Check for collision first
		Ray checker = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit2D r = Physics2D.GetRayIntersection(checker);
		bool collided = false;
		if (r != null){
			if(r.collider is BoxCollider2D){
				collided = true;
			}
		}
		
		if (Input.GetMouseButton (0) && mouseTile.z == 0.0) {
			//Draw a block. This is hardcoded for now.
			/*if(!collided && b_type != 0 && b_type != -1)
				this.PlaceBlock ((int)(mouseTile.x), (int)(mouseTile.y), b_type);
			else if(b_type == 0)
			//Erase a block.
				this.PlaceBlock ((int)(mouseTile.x), (int)(mouseTile.y), b_type);*/
			if(b_type != -1)
				this.PlaceBlock((int)(mouseTile.x), (int)(mouseTile.y), b_type);
		}
	}


    void BuildMesh()
    {
        int num_tiles = size_x * size_y;
        int num_triangles = 2 * num_tiles;

        int vsize_x = 2 * size_x;
        int vsize_y = 2 * size_y;
        int num_verts = vsize_x * vsize_y;

        Vector3[] vertices = new Vector3[num_verts];
        Vector3[] normals = new Vector3[num_verts];
        Vector2[] uv = new Vector2[num_verts];

        int[] triangles = new int[num_triangles * 3];

        for (int y = 0; y < size_y; y++)
        {
            for (int x = 0; x < size_x; x++)
            {
                int tile_x = _map.GetTileAt(x, y).ID % _n_cols;
                int tile_y = _map.GetTileAt(x, y).ID / _n_cols;

                int h = (int)((size_y - 1) * tile_size);

                vertices[4 * (x + y * size_x) + 0] = new Vector3(x * tile_size, h - y * tile_size, 0);
                vertices[4 * (x + y * size_x) + 1] = new Vector3(x * tile_size + tile_size, h - y * tile_size, 0);
                vertices[4 * (x + y * size_x) + 2] = new Vector3(x * tile_size, h - y * tile_size + tile_size, 0);
                vertices[4 * (x + y * size_x) + 3] = new Vector3(x * tile_size + tile_size, h - y * tile_size + tile_size, 0);
                normals[4 * (x + y * size_x) + 0] = Vector3.up;
                normals[4 * (x + y * size_x) + 1] = Vector3.up;
                normals[4 * (x + y * size_x) + 2] = Vector3.up;
                normals[4 * (x + y * size_x) + 3] = Vector3.up;
                uv[4 * (x + y * size_x) + 0] = new Vector2((float)(tile_x) / _n_cols, 1 - (float)(tile_y + 1) / _n_rows);
                uv[4 * (x + y * size_x) + 1] = new Vector2((float)(tile_x + 1) / _n_cols, 1 - (float)(tile_y + 1) / _n_rows);
                uv[4 * (x + y * size_x) + 2] = new Vector2((float)(tile_x) / _n_cols, 1 - (float)(tile_y) / _n_rows);
                uv[4 * (x + y * size_x) + 3] = new Vector2((float)(tile_x + 1) / _n_cols, 1 - (float)(tile_y) / _n_rows);
                //Debug.Log(uv[4 * (x + y * size_x) + 0] + "," + uv[4 * (x + y * size_x) + 1] + "," + uv[4 * (x + y * size_x) + 2] + "," + uv[4 * (x + y * size_x) + 3]);

                //triangle shit
                int square_index = x + y * size_x;
                int triangle_offset = square_index * 6;
                triangles[triangle_offset + 0] = 4 * (x + y * size_x) + 0;
                triangles[triangle_offset + 1] = 4 * (x + y * size_x) + 2;
                triangles[triangle_offset + 2] = 4 * (x + y * size_x) + 3;

                triangles[triangle_offset + 3] = 4 * (x + y * size_x) + 0;
                triangles[triangle_offset + 4] = 4 * (x + y * size_x) + 3;
                triangles[triangle_offset + 5] = 4 * (x + y * size_x) + 1;

                //Add a box collider if the tile is solid
                if (_map.GetTileAt(x, y).Solid)
                {
                    BoxCollider2D collider = this.gameObject.AddComponent<BoxCollider2D>();
                    collider.center = new Vector2((x * tile_size * 2 + tile_size) / 2, (2 * (h - y * tile_size) + tile_size) / 2);
                    collider.size = new Vector2(tile_size, tile_size);
                    collider.name = "Wall";
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        mesh_filter.mesh = mesh;
    }
    
    private Vector2 v3ToTile(Vector3 v){
		Vector3 vec = Camera.main.ScreenToWorldPoint (v);
		return new Vector2((float)(int)vec.x, (float)(int)(size_y-(vec.y)));
    }
    

	//Turns mouse position into a tile.
	//Z-value is if it is within the tilemap (0 if so, -1 if not).
    private Vector3 mouseToTile()
    {
		Vector2 v = v3ToTile (Input.mousePosition);
		float valid = 0.0f;
		if(!(v.x < this.size_x && v.x >= 0.0 && v.y < this.size_y && v.y >= 0.0))
			valid = -1.0f;
		//Debug.Log (v.x + " " + v.y + " " + valid);
        return new Vector3(v.x, v.y, valid);
    }

	public void updateType(int a, string s){
		//Debug.Log ("UPD" + a);
		if (this.b_type == a) {
			this.b_type = -1;
			this.blstr = "";
			this.lastP = new Vector2(-1f, -1f);
		} else {
			this.lastP = new Vector2(-1f, -1f);
			this.b_type = a;
			this.blstr = s;
		}
	}

	public int tileType(){
		return this.b_type;
	}
}
