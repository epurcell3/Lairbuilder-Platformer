using UnityEngine;
using System.Collections;

public class SlowAura : Aura {

	// Use this for initialization
	void Start () {
		ini (2.5f, new Color(25f/256f, 212f/256f, 181f/256f));
		makeCollider ();
		spriteMaker ();
		actor = GameObject.Find ("AI");
		
	}
	
	void OnTriggerEnter2D(Collider2D other){
		Vector2 sp = actor.GetComponent<MoveScript> ().speed;
		actor.GetComponent<MoveScript>().speed = new Vector2(sp.x/2.0f, sp.y/2.0f);
	}
	
	void OnTriggerExit2D(Collider2D other){
		Vector2 sp = actor.GetComponent<MoveScript> ().speed;
		actor.GetComponent<MoveScript>().speed = new Vector2(sp.x*2.0f, sp.y*2.0f);
	}
}
