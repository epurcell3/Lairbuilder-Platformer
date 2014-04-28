using UnityEngine;
using System.Collections;

public class Aura : MonoBehaviour {
	private float length;
	private int pointc;
	private Color projectColor;
	private float timeenter;
	private float timeexit;
	private int mapHeight;

	// Use this for initialization
	void Start () {
		mapHeight = GameObject.Find ("Timemap").GetComponent<Tilemap>().size_y;
		pointc = 60;
		length = 2.0f;
		gameObject.layer = 2;
		CircleCollider2D c = gameObject.AddComponent<CircleCollider2D> ();
		c.center = new Vector2(0.5f, mapHeight - 0.5f - (2 * this.transform.position.y));
		c.radius = length;
		c.isTrigger = true;
	}
	
	// Update is called once per frame
	void Update () {
	
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

	}
}
