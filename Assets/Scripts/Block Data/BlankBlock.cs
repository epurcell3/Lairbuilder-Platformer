using UnityEngine;
using System.Collections;

public class BlankBlock : Block {

	//public static Block[] list = new Block[16];

	// Use this for initialization
	void Start () {
		tileId = 0;
		dataString = "Blank";
		/*list [0] = new BlankBlock ();
		list [2] = new TurretBlock ();
		list [3] = new SlowBlock ();
		list [4] = new DarkBlock ();
		list [5] = new Mine ();
		list [11] = new SuperBlock ();
		list [12] = new BasicBlock ();

		/*
		 * 
		list [0] = BlankBlock.cmult;
		list [2] = TurretBlock.cmult;
		list [3] = SlowBlock.cmult;
		list [4] = DarkBlock.cmult;
		list [5] = Mine.cmult;
		list [11] = SuperBlock.cmult;
		list [12] = BasicBlock.cmult;*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
