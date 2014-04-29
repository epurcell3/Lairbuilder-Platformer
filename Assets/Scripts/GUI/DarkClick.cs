using UnityEngine;
using System.Collections;

public class DarkClick : ToggleClickScript {
	bool setUp = false;
	
	// Update is called once per frame
	void Update () {
		if(this.isOn){
			if(!setUp){
				GameObject.Find ("AI").GetComponent<SLAM>().distance /= 2.0f;
				this.setUp = true;
			}
		}else{
			if(this.setUp){
				GameObject.Find ("AI").GetComponent<SLAM>().distance *= 2.0f;
				this.setUp = false;
			}
		}

	}
}
