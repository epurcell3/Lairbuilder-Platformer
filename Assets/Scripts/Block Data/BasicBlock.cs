using UnityEngine;
using System.Collections;

public class BasicBlock : Block {

	// Use this for initialization
	void Start () {
		tileId = 12;
		dataString = "Wall";
		cmult = 1;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	protected int costFunction(int n){
		return 1*n * n;
	}
}
