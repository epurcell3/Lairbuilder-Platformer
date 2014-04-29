using UnityEngine;
using System.Collections;

public class PlayerData : MonoBehaviour {

	int money;
	private readonly int BaseMoney = 10000;

	// Use this for initialization
	void Start () {
		money = BaseMoney;
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log (money);
		GameObject.Find ("GUI-Text CVal").GetComponent<GUIText> ().text = "$"+this.money.ToString ();
	}

	int getMoney(){
		return money;
	}

	public bool pay(int amount){
		if (money >= amount){
			money -= amount;
			return true;
		}
		return false;
	}
}
