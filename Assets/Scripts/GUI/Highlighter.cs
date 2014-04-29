using UnityEngine;
using System.Collections;

public class Highlighter : MonoBehaviour {

	private OptionClickScript current;
	private bool enableButton;

	// Use this for initialization
	void Start () {
		on ();
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void updateCurrent(OptionClickScript o){
		if(enableButton){
			if (o.Equals (current)) {
				current = null;//new OptionClickScript ();
				o.setCur (false);
			} else {
				if(current)
					current.setCur (false);
				current = o;
				o.setCur (true);
			}
		}
	}

	public void off(){
		if(current != null)
			this.updateCurrent (current);
		Vector3 v = GameObject.Find ("GUI-Disable Tools").transform.position;
		v.z = 14;
		GameObject.Find ("GUI-Disable Tools").transform.position = v;
		this.enableButton = false;
	}

	public void on(){
		Vector3 v = GameObject.Find ("GUI-Disable Tools").transform.position;
		v.z = 0;
		Debug.Log (v);
		GameObject.Find ("GUI-Disable Tools").transform.position = v;
		this.enableButton = true;
	}
}
