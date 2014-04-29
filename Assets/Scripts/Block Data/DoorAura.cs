using UnityEngine;
using System.Collections;

public class DoorAura : Aura {

	// Use this for initialization
	void Start () {
		ini (1f, new Color(53f/256f, 222f/256f, 107f/256f));
		gameObject.layer = 0;
		
		BoxCollider2D c = gameObject.AddComponent<BoxCollider2D> ();
		c.center = new Vector2 (0.0f, 0.0f);
		c.size = new Vector2(1f, 1f);
		c.isTrigger = false;
		spriteMaker ();
		
	}

	void OnCollisionEnter2D(Collision2D other){
		if(other.gameObject.GetComponent<AIScript>() != null){
			other.gameObject.GetComponent<AIScript>().win();
		}
	}
}
