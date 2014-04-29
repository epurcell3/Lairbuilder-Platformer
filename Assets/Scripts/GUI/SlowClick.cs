using UnityEngine;
using System.Collections;

public class SlowClick : ToggleClickScript {
	bool setUp = false;
	int cost = 7;
	
	// Update is called once per frame
	void Update () {
		if(this.isOn){
			if(!setUp){
				Vector2 sp = GameObject.Find ("AI").GetComponent<MoveScript> ().speed;
				GameObject.Find ("AI").GetComponent<MoveScript>().speed = new Vector2(sp.x*.8f, sp.y*.8f);
				this.setUp = true;
			}
			if(!GameObject.Find ("Player").GetComponent<PlayerData> ().pay (cost))
				(GameObject.Find ("GUI").GetComponent<Multilighter>()).updateCurrent(this);
		}else{
			if(this.setUp){
				Vector2 sp = GameObject.Find ("AI").GetComponent<MoveScript> ().speed;
				GameObject.Find ("AI").GetComponent<MoveScript>().speed = new Vector2(sp.x/.8f, sp.y/.8f);
				this.setUp = false;
			}
		}
		
	}
}
