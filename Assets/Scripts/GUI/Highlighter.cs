using UnityEngine;
using System.Collections;

public class Highlighter : MonoBehaviour {

	private OptionClickScript current;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void updateCurrent(OptionClickScript o){
		if (o.Equals (current)) {
			current = new OptionClickScript ();
			o.setCur (false);
		} else {
			if(current)
				current.setCur (false);
			current = o;
			o.setCur (true);
		}
	}
}
