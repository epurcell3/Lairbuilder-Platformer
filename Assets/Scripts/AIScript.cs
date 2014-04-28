﻿using UnityEngine;
using System.Collections;

public class AIScript : MonoBehaviour {
	private MoveScript mover;
	private SLAM slam;
	public bool goalBased;
	private Vector2 goalState;
	private Rigidbody2D body;
	private SimpleExploreScript explorer;
	private int AITime = 0;
	private SearchScript;
	// Use this for initialization
	void Start () {
		mover = gameObject.GetComponent<MoveScript> ();

		slam = gameObject.GetComponent<SLAM>();
		body = gameObject.GetComponent<Rigidbody2D> ();
		goalBased = false;

		if (mover != null) {
			Debug.Log("Mover Not Null");
			mover.setSpeed (new Vector2 (6, 14));
			explorer = new SimpleExploreScript (mover, body); 
			SearchScript = new SearchScript(body, slam, mover);
		}

		
	}
	
	// Update is called once per frame
	void Update () {
		if (mover == null) {
			Debug.Log("Mover Null");
			mover = gameObject.GetComponent<MoveScript> ();
			mover.setSpeed (new Vector2 (6, 14));
			explorer = new SimpleExploreScript (mover, body); 	
		}
		if (! goalBased) {
			goalBased = slam.exploredEnoughToSlam();
			if(AITime >300){
				goalBased = true;
			}

		}
		if (goalBased) {
				
				} 
		else {
			AITime++;
			explorer.move ();

		}

	
	}
	bool goalAttained(){
		if (slam.Position == goalState) {
			return true;
		}
		return false;

	}
}