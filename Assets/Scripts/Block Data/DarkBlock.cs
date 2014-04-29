﻿using UnityEngine;
using System.Collections;

public class DarkBlock : AuraBlock {

	// Use this for initialization
	void Start () {
		tileId = 4;
		dataString = "DarkAura";
		cmult = 50;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	protected int costFunction(int n){
		return 50*n * n;
	}
}