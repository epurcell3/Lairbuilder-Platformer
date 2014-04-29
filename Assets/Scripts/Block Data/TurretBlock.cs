using UnityEngine;
using System.Collections;

public class TurretBlock : AuraBlock {

	// Use this for initialization
	void Start () {
		tileId = 2;
		dataString = "TurretAura";
		cmult = 250;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	protected int costFunction(int n){
		return 250*n * n;
	}
}
