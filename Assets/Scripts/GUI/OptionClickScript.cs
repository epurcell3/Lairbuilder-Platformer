using UnityEngine;
using System.Collections;

public class OptionClickScript : MonoBehaviour {
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
		Tilemap t = (Tilemap)GameObject.Find("Wall").GetComponent("Tilemap");

		t.updateType (((Block)gameObject.GetComponent("Block")).tileId);
		//Debug.Log(t.tileType());
		//gameObject.guiTexture.color= colorPushed;
		((Highlighter)(GameObject.Find ("GUI").GetComponent ("Highlighter"))).updateCurrent(this);
	}

	public void setCur(bool t){
		isCurrent = t;
		if(!t)
			gameObject.guiTexture.color = originalColor;
		else
			gameObject.guiTexture.color = colorPushed;

	}

}
