using UnityEngine;
using System.Collections;

public class Aura : MonoBehaviour {
	public float length;
	private float timeenter;
	private float timeexit;
	private int mapHeight;
	private float effectSize = 32f;
	private int frame = 0;
	private int totalFrame = 60;
	private string sp_file = "flareaura_3";
	public Color col;
	Vector3 basePoint;

	// Use this for initialization
	void Start () {
		ini (2.5f, Color.white);
		makeCollider ();
		spriteMaker ();
	}

	// Update is called once per frame
	void Update () {
		spriteMaker ();
		if(++frame >= totalFrame)
			frame = 0;
	}

	void ini(float l, Color c){
		this.length = l;
		this.col = c;
		gameObject.layer = 2;
		this.mapHeight = GameObject.Find ("Wall").GetComponent<Tilemap>().size_y;
	}

	void makeCollider(){
		CircleCollider2D c = gameObject.AddComponent<CircleCollider2D> ();
		c.center = new Vector2 (0.0f, 0.0f);
		c.radius = length;
		c.isTrigger = true;
	}


	void spriteMaker(){
		if(gameObject.GetComponent<SpriteRenderer>() == null)
			gameObject.AddComponent<SpriteRenderer> ();
		gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create (Resources.Load (sp_file) as Texture2D, new Rect ((float)(effectSize*frame), 0.0f, effectSize, effectSize), new Vector2 (0.5f, 0.5f), effectSize / length / 2.0f);
		gameObject.GetComponent<SpriteRenderer> ().color = this.col;
	}

	public void setBase(Vector3 v){
		this.basePoint = v;
	}

	public Vector3 getBase(){
		return this.basePoint;
	}
	
	void OnTriggerEnter2D(Collider2D other){
		timeenter = Time.time;
		Debug.Log ("In Aura at " + timeenter);
	}
	
	void OnTriggerExit2D(Collider2D other){
		timeexit = Time.time;
		Debug.Log ("Left Aura at "+timeexit+". Total of " + (timeexit-timeenter));
	}

	void OnTriggerStay2D(Collider2D other){
		Debug.Log ("Yep, still in.");
	}

}
