using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AISuperScript : MonoBehaviour {
	public Vector2 speed = new Vector2(10,10);
	
	private Vector2 movement;
	public enum Status
	{
		Idle,
		Suspicious,  
		Alert
	}
	public Status status;
	public List<Material> materials;
	Tilemap currentMap;
	Vector3[] newVertices;
	Vector2[] newUV;
	int[] newTriangles;
	Mesh mesh;
	MeshRenderer meshRenderer;
	FOV2DEyes eyes;
	List<RaycastHit> hits;
	int i;
	int v;
	int[,] map;
	Vector2 goalTile;
	void Start() 
	{		
		mesh = GetComponent<MeshFilter>().mesh;
		meshRenderer = GetComponent<MeshRenderer>();
		eyes = gameObject.GetComponent<FOV2DEyes>();
		map = new int[64,20];
		initializeMap();
		meshRenderer.material = materials[0];
	}
	// Update is called once per frame
	void Update () {
		float inputX = Input.GetAxis("Horizontal");
		float inputY = Input.GetAxis("Vertical");
		Vector2 goalPos = currentMap.getPosOfTile(goalTile);
		float xval = this.gameObject.transform.position.x - goalPos.x;
		xval = xval / Mathf.Abs(xval);
		float yval = this.gameObject.transform.position.y - goalPos.y;
		yval = yval / Mathf.Abs(yval);
		
		movement = new Vector2(speed.x * xval, speed.y * yval);
	}
	
	void FixedUpdate()
	{
		rigidbody2D.velocity = movement;
	}
	void LateUpdate() 
	{
		UpdateMesh();
		
		UpdateMeshMaterial();
	}
	void initializeMap(){
		for(int x = 0; x< 64; x++){
			for( int y = 0; y < 20; y++){
				map[x,y] = 0;
			}
		}
	}
	void UpdateMesh()
	{
		hits = eyes.hits;
		
		if (hits == null || hits.Count == 0)
			return;
		
		if (mesh.vertices.Length != hits.Count + 1)
		{
			mesh.Clear();
			newVertices = new Vector3[hits.Count + 1];
			newTriangles = new int[(hits.Count - 1) * 3];
			
			i = 0;
			v = 1;
			while (i < newTriangles.Length)
			{
				if ((i % 3) == 0)
				{
					newTriangles[i] = 0;
					newTriangles[i + 1] = v;
					newTriangles[i + 2] = v + 1;
					v++;
				}
				i++;
			}
		}

		for( int hitNum = 0; hitNum < hits.Count; hitNum++){
			RaycastHit hit = hits.indexOf(hitNum);
			if(hit.transform){
				string hitTag = hit.transform.tag; 
				Vector2 tile = currentMap.GetTileForPos(hit.point);
				Vector2[] tilesBetween = currentMap.getTilesBetweenPoints(this.gameObject.transform.position, hit.point);
				foreach(Vector2 cTile in tilesBetween){
					map[cTile.x, cTile.y] = 1;
				}
				map[tile.x, tile.y] = tagMap(tag);
			}

		}
		newVertices[0] = Vector3.zero;
		for (i = 1; i <= hits.Count; i++)
		{
			newVertices[i] = transform.InverseTransformPoint(hits[i-1].point);
		}
		
		newUV = new Vector2[newVertices.Length];
		i = 0;
		while (i < newUV.Length) {
			newUV[i] = new Vector2(newVertices[i].x, newVertices[i].z);
			i++;
		}
		
		mesh.vertices = newVertices;
		mesh.triangles = newTriangles;
		mesh.uv = newUV;
		
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
	int tagMap(string tag){
		return 0;
	// todo when tags are 
	}
	void UpdateMeshMaterial()
	{	
		for (i = 0; i < materials.Count; i++)
		{
			if (i == (int) status && meshRenderer.material != materials[i])
			{
				meshRenderer.material = materials[i];
			}
		}
	}
}
