using UnityEngine;
using System.Collections;

public class SlowClick : ToggleClickScript {
	bool setUp = false;
	
	// Update is called once per frame
	void Update () {
		if(this.isOn){
			if(!setUp){
				Vector2 sp = GameObject.Find ("AI").GetComponent<MoveScript> ().speed;
				GameObject.Find ("AI").GetComponent<MoveScript>().speed = new Vector2(sp.x/2.0f, sp.y/2.0f);
				this.setUp = true;
			}
		}else{
			if(this.setUp){
				Vector2 sp = GameObject.Find ("AI").GetComponent<MoveScript> ().speed;
				GameObject.Find ("AI").GetComponent<MoveScript>().speed = new Vector2(sp.x*2.0f, sp.y*2.0f);
				this.setUp = false;
			}
		}
		
	}
}
