using UnityEngine;
using System.Collections;

public class SpecialButtonScript : MonoBehaviour {
	public Color colorOver = new Color(.9f,.9f,.9f); 
	
	public Color colorPushed  = new Color(0.75f,0.75f,0.75f);  
	private Color originalColor = Color.gray;
	
	private bool isCurrent;
	
	// Use this for initialization
	void Start () { 
		isCurrent = false;
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnMouseEnter()    
	{
		if(!isCurrent)
			gameObject.guiTexture.color= colorOver;
	}   
	
	void OnMouseExit()
	{
		if(!isCurrent)
			gameObject.guiTexture.color= originalColor;
	}
	
	void OnMouseUpAsButton()
	{
		Debug.Log ("Quitting");
		Application.Quit ();
	}

}
