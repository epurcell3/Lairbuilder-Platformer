using UnityEngine;
using System.Collections;

public class MineAura : Aura {

	// Use this for initialization
	void Start () {
		this.length = 1f;
		this.col = new Color (1f, 1f, 1f);
		this.mapHeight = GameObject.Find ("Wall").GetComponent<Tilemap>().size_y;
		
		BoxCollider2D c = gameObject.AddComponent<BoxCollider2D> ();
		c.center = new Vector2 (0.0f, 0.0f);
		c.size = new Vector2(1f, 1f);
		gameObject.layer = 0;
		c.isTrigger = false;
		//spriteMaker ();
		
	}
	//Null to prevent action
	void Update(){
	}

	void OnCollisionEnter2D(Collision2D other){
		if(other.gameObject.GetComponent<AIScript>() != null){
			other.gameObject.GetComponent<AIScript>().die();
			float x = this.basePoint.x;
			float y = this.basePoint.y;
			GameObject.Find("Wall").GetComponent<Tilemap>().EraseBlockNoResult(x,y);
		}
	}

}
