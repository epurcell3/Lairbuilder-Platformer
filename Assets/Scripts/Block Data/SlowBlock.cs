using UnityEngine;
using System.Collections;

public class SlowBlock : AuraBlock {

	// Use this for initialization
	void Start () {
		tileId = 3;
		dataString = "SlowAura";
		cmult = 50;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	protected int costFunction(int n){
		return 50*n * n;
	}
}
