using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	public int tileId;
	public string dataString;
	protected int uses = 0;
	public int cmult;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	int costFunction(){
		return costFunction(uses+1);
	}

	protected virtual int costFunction(int n){
		return cmult * n * n;
	}

	public bool incUses(PlayerData p){
		if(p.pay(costFunction())){
			uses++;
			return true;
		}
		return false;
	}

	public bool decUses(PlayerData p){
		//Debug.Log (cmult);
		//Debug.Log (costFunction(uses));
		if(p.pay(-costFunction(uses))){
			uses--;
			return true;
		}
		return false;
	}


}
