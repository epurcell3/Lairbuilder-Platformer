using UnityEngine;
using System.Collections;
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
    public enum Inpu { MANUAL, FILE };

    public Inpu input_method = Inpu.FILE;

    public int size_x = 40; //tiles wide
    public int size_y = 24; //tiles high

    public int tile_resolution = 16; //pixels across, assume square

    public float tile_size = 1.0f;

    public int solid_threshold = 1; //where tiles become solid;

    public Texture2D tileset; //texture of the tiles

    public string tile_ids; //list of tile ides

    public string tiled_filepath;

    //private stuff for actual work
    private Tilemap_D _map;
    private int[] _converted_tile_ids;

	private int _n_rows, _n_cols;

	// Storing where the map is in screenspace is a big deal... Just have to figure out
	private Vector2 location;


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
            }
        }

    }


	// Use this for initialization
	void Start () {
		this.Generate ();
	}

    public void Generate()
    {
        if (input_method == Inpu.FILE)
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
                int tile_x = _map.GetTileAt(x,y).ID % _n_cols;
                int tile_y = _map.GetTileAt(x,y).ID / _n_cols;

                int h = (int)((size_y - 1) * tile_size);

                vertices[4 * (x + y * size_x) + 0] = new Vector3(x * tile_size                  , h - y * tile_size                 , 0);
                vertices[4 * (x + y * size_x) + 1] = new Vector3(x * tile_size + tile_size      , h - y * tile_size                 , 0);
                vertices[4 * (x + y * size_x) + 2] = new Vector3(x * tile_size                  , h - y * tile_size + tile_size     , 0);
                vertices[4 * (x + y * size_x) + 3] = new Vector3(x * tile_size + tile_size      , h - y * tile_size + tile_size     , 0);
                normals[4 * (x + y * size_x) + 0] = Vector3.up;
                normals[4 * (x + y * size_x) + 1] = Vector3.up;
                normals[4 * (x + y * size_x) + 2] = Vector3.up;
                normals[4 * (x + y * size_x) + 3] = Vector3.up;
                uv[4 * (x + y * size_x) + 0] = new Vector2((float)(tile_x) / _n_cols    , 1 - (float)(tile_y+1) / _n_rows);
                uv[4 * (x + y * size_x) + 1] = new Vector2((float)(tile_x + 1) / _n_cols, 1 - (float)(tile_y+1) / _n_rows);
                uv[4 * (x + y * size_x) + 2] = new Vector2((float)(tile_x) / _n_cols    , 1 - (float)(tile_y) / _n_rows);
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

    public void PlaceBlock(int x, int y, int id)
    {
        _map.SetTileAt(x, y, id);
    }

	void Update(){
		Vector2 mouseTile = mouseToTile();

		//Check for collision first
		Ray checker = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit2D r = Physics2D.GetRayIntersection(checker);
		bool collided = false;
		if (r != null){
			if(r.collider is BoxCollider2D){
				collided = true;
			}
		}
		
		if (Input.GetMouseButton (0)) {
			//Draw a block. This is hardcoded for now.
			if(!collided)
				this.PlaceBlock ((int)(mouseTile.x), (int)(mouseTile.y), 35);
		} else if (Input.GetMouseButton (1)) {
			//Erase a block.
			if(collided)
				this.PlaceBlock ((int)(mouseTile.x), (int)(mouseTile.y), 1);
		}
	}

	//Turns mouse position into a tile.
	private Vector2 mouseToTile(){
		Vector3 mouse = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		return new Vector2((float)(int)mouse.x, (float)(int)(size_y-(mouse.y)));
	}
}
