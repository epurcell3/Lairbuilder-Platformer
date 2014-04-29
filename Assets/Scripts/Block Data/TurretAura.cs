﻿using UnityEngine;
using System.Collections;

public class TurretAura : Aura {

	private float killtime;

	// Use this for initialization
	void Start () {
		sp_file = "turret";
		totalFrame = 1;
		effectSize = 128f;
		ini (2.5f, new Color(1f,1f,1f));//213f/256f, 102f/256f, 14f/256f));
		makeCollider ();
		spriteMaker ();
		actor = GameObject.Find ("AI");
		killtime = 2f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerStay2D(Collider2D other){
		float tnow = Time.time;
		if(timeenter+killtime < tnow && other.gameObject.GetComponent<AIScript>() != null){
			other.gameObject.GetComponent<AIScript>().die();
			timeenter = tnow;
		}
	}

}
