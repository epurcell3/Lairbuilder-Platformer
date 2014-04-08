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
    public enum Input { MANUAL, FILE };

    public Input input_method = Input.MANUAL;

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

	// Use this for initialization
	void Start () {

	}

    public void Generate()
    {
        if (input_method == Input.FILE)
        {
            Debug.Log(tiled_filepath);
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
                Debug.Log(tile_ids);
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
        _n_rows = tileset.height / tile_resolution;
        _n_cols = tileset.width / tile_resolution;
        Debug.Log(GetComponents<BoxCollider2D>().Length);
        while(GetComponents<BoxCollider2D>().Length > 0)
            DestroyImmediate(GetComponent<BoxCollider2D>());
        Debug.Log(GetComponents<BoxCollider2D>().Length);
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
                    Debug.Log(collider);
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
        //MeshCollider mesh_collider = GetComponent<MeshCollider>();

        mesh_filter.mesh = mesh;
        //mesh_collider.sharedMesh = mesh;
    }
}
