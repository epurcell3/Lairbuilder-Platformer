using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SearchScript : MonoBehaviour {
	public Rigidbody2D rigid;
	public SLAM slam;
	public MoveScript mover;
	public ArrayList moveList;
	public Vector2 goalState;
	// Use this for initialization
	public SearchScript(Rigidbody2D rigidbody, SLAM slammer, MoveScript move){	
		rigid = rigidbody;
		slam = slammer;
		mover = move;
		moveList = new ArrayList();

	}
	public void setGoalState(Vector2 goal){
		goalState = goal;
	}
	private void doAStar(){
		SLAM.Cell[,] occupancy = slam.OccupancyGrid;
	}



}
