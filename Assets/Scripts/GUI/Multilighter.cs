using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Multilighter : MonoBehaviour {
		
		private List<ToggleClickScript> onButtons;
		private bool enableButton;
		
		// Use this for initialization
		void Start () {
			onButtons = new List<ToggleClickScript>();
			off ();
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}
		
		public void updateCurrent(ToggleClickScript o){
			if(enableButton){
				if (onButtons.Contains(o)) {
					onButtons.Remove(o);
					o.onoff(false);
				Debug.Log ("R1");
				} else {
					onButtons.Add(o);
					o.onoff(true);
				Debug.Log ("A1");
				}
			}
		}
		
		public void off(){
			if(onButtons.Count != 0)
				for(int i = onButtons.Count - 1; i >= 0; i--)
					this.updateCurrent (onButtons[i]);
			Vector3 v = GameObject.Find ("GUI-Disable Hacks").transform.position;
			v.z = 14;
			GameObject.Find ("GUI-Disable Hacks").transform.position = v;
			this.enableButton = false;
		}
		
		public void on(){
			Vector3 v = GameObject.Find ("GUI-Disable Hacks").transform.position;
			v.z = 0;
			GameObject.Find ("GUI-Disable Hacks").transform.position = v;
			this.enableButton = true;
		}
}
