using UnityEngine;
using System.Collections;

public class DarkAura : Aura {
	
	void Start () {
		ini (2.5f, new Color(113f/256f, 76f/256f, 91f/256f));
		makeCollider ();
		spriteMaker ();
		actor = GameObject.Find ("AI");
	}
	
	void OnTriggerEnter2D(Collider2D other){
		actor.GetComponent<SLAM>().distance /= 2.0f;
	}
	
	void OnTriggerExit2D(Collider2D other){
		actor.GetComponent<SLAM>().distance *= 2.0f;
	}
}
