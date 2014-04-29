using UnityEngine;
using System.Collections;

public class EndText : MonoBehaviour {

	private float t;
	private int diescore;

	// Use this for initialization
	void Start () {
		diescore = 300;
	
	}
	
	// Update is called once per frame
	void Update () {
			
	}

	public void setTime(){
		this.t = Time.time;
		int score = (((int)(60.0f * (t - GameObject.Find ("AI").GetComponent<AIScript> ().timer))) + (diescore * GameObject.Find ("AI").GetComponent<AIScript> ().deaths));
		gameObject.GetComponent<GUIText>().text = "You scored "+ score +" points.";
	}
}
