using UnityEngine;
using System.Collections;

public class Mine : AuraBlock {

	// Use this for initialization
	void Start () {
		tileId = 5;
		dataString = "MineAura";
		cmult = 400;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	protected int costFunction(int n){
		return 250*n * n;
	}
}
