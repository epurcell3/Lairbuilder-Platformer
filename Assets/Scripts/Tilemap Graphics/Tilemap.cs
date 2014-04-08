using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Tilemap : MonoBehaviour {

    //public stuff for the Inspector
    public int size_x = 40; //tiles wide
    public int size_y = 24; //tiles high

    public int tile_resolution = 16; //pixels across, assume square

    public int solid_threshold = 1; //where tiles become solid;

    public Texture2D tileset; //texture of the tiles

    public string tile_ids; //list of tile ides

    //private stuff for actual work
    private Tilemap_D _map;
    private int[] _converted_tile_ids;

    private int _n_rows, _n_cols;

	// Use this for initialization
	void Start () {
        string[] tmp = tile_ids.Split(',');
        _converted_tile_ids = new int[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            _converted_tile_ids[i] = Convert.ToInt32(tmp[i]);
        }
        _map = new Tilemap_D(size_x, size_y, _converted_tile_ids);
        _n_rows = tileset.height / tile_resolution;
        _n_cols = tileset.width / tile_resolution;
        //BuildMesh();
	}

    public void Generate()
    {
        string[] tmp = tile_ids.Split(',');
        _converted_tile_ids = new int[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            _converted_tile_ids[i] = Convert.ToInt32(tmp[i]);
        }
        _map = new Tilemap_D(size_x, size_y, _converted_tile_ids);
        _n_rows = tileset.height / tile_resolution;
        _n_cols = tileset.width / tile_resolution;
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

                int h = (size_y - 1) * tile_resolution;

                vertices[4 * (x + y * size_x) + 0] = new Vector3(x * tile_resolution                  , h - y * tile_resolution                  , 0);
                vertices[4 * (x + y * size_x) + 1] = new Vector3(x * tile_resolution + tile_resolution, h - y * tile_resolution                  , 0);
                vertices[4 * (x + y * size_x) + 2] = new Vector3(x * tile_resolution                  , h - y * tile_resolution + tile_resolution, 0);
                vertices[4 * (x + y * size_x) + 3] = new Vector3(x * tile_resolution + tile_resolution, h - y * tile_resolution + tile_resolution, 0);
                normals[4 * (x + y * size_x) + 0] = Vector3.up;
                normals[4 * (x + y * size_x) + 1] = Vector3.up;
                normals[4 * (x + y * size_x) + 2] = Vector3.up;
                normals[4 * (x + y * size_x) + 3] = Vector3.up;
                uv[4 * (x + y * size_x) + 0] = new Vector2((float)(tile_x) / _n_cols    , 1 - (float)(tile_y) / _n_rows);
                uv[4 * (x + y * size_x) + 1] = new Vector2((float)(tile_x + 1) / _n_cols, 1 - (float)(tile_y) / _n_rows);
                uv[4 * (x + y * size_x) + 2] = new Vector2((float)(tile_x) / _n_cols    , 1 - (float)(tile_y + 1) / _n_rows);
                uv[4 * (x + y * size_x) + 3] = new Vector2((float)(tile_x + 1) / _n_cols, 1 - (float)(tile_y + 1) / _n_rows);
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
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        MeshCollider mesh_collider = GetComponent<MeshCollider>();

        mesh_filter.mesh = mesh;
        mesh_collider.sharedMesh = mesh;
    }
}
