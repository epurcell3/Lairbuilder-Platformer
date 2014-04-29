using UnityEngine;
using System.Collections;

public class BlkData : MonoBehaviour {

	public Block[] blk;

	// Use this for initialization
	void Start () {
		blk = new Block[16];
		blk [0] = GameObject.Find ("GUI-Button Blank").GetComponent<BlankBlock> ();
		blk [2] = GameObject.Find ("GUI-Button Turret").GetComponent<TurretBlock> ();
		blk [3] = GameObject.Find ("GUI-Button Slow").GetComponent<SlowBlock> ();
		blk [4] = GameObject.Find ("GUI-Button Dark").GetComponent<DarkBlock> ();
		blk [5] = GameObject.Find ("GUI-Button Mine").GetComponent<Mine> ();
		//blk [11] = gameObject.GetComponent<SuperBlock> ();
		blk [12] = GameObject.Find ("GUI-Button Base").GetComponent<BasicBlock> ();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
