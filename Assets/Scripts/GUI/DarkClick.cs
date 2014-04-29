using UnityEngine;
using System.Collections;

public class DarkClick : ToggleClickScript {
	bool setUp = false;
	int cost = 7;
	
	// Update is called once per frame
	void Update () {
		if(this.isOn){
			if(!setUp){
				GameObject.Find ("AI").GetComponent<SLAM>().distance /= 2.0f;
				this.setUp = true;
			}
			if(!GameObject.Find ("Player").GetComponent<PlayerData> ().pay (cost))
				(GameObject.Find ("GUI").GetComponent<Multilighter>()).updateCurrent(this);
		}else{
			if(this.setUp){
				GameObject.Find ("AI").GetComponent<SLAM>().distance *= 2.0f;
				this.setUp = false;
			}
		}

	}
}
