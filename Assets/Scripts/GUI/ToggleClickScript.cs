using UnityEngine;
using System.Collections;

public class ToggleClickScript : MonoBehaviour {
	public Color colorOver = new Color(.9f,.9f,.9f); 
	
	public Color colorPushed  = new Color(0.75f,0.75f,0.75f);  
	private Color originalColor = Color.gray;
	
	protected bool isOn;

	// Use this for initialization
	void Start () { 
		isOn = false;
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	void OnMouseEnter()    
	{
		if(!isOn)
			gameObject.guiTexture.color= colorOver;
	}   
	
	void OnMouseExit()
	{
		if(!isOn)
			gameObject.guiTexture.color= originalColor;
	}
	
	void OnMouseUpAsButton()
	{
		//Debug.Log(t.tileType());
		//gameObject.guiTexture.color= colorPushed;
		(GameObject.Find ("GUI").GetComponent<Multilighter>()).updateCurrent(this);
	}
	
	public void onoff(bool t){
		isOn = t;
		if(!t)
			gameObject.guiTexture.color = originalColor;
		else
			gameObject.guiTexture.color = colorPushed;
		
	}

}
