using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	public int tileId;
	public string dataString;
	private int uses;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	int costFunction(){
		return costFunction(uses);
	}

	int costFunction(int n){
		return n * n;
	}

	void incUses(PlayerData p){
		if(p.pay(costFunction())){
			uses++;
		}
	}

	void decUses(PlayerData p){
		if(p.pay(-costFunction(uses - 1))){
			uses--;
		}
	}

}
