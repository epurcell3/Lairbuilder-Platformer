using UnityEngine;
using System.Collections;

public class PlayerData {

	int money;
	private readonly int BaseMoney = 10500;

	// Use this for initialization
	void Start () {
		money = BaseMoney;
	}
	
	// Update is called once per frame
	void Update () {
	
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
