using UnityEngine;
using System.Collections;

public class AIScript : MonoBehaviour {
	private MoveScript mover;
	private SLAM slam;
	public bool goalBased;
	private Vector2 goalState;
	private Rigidbody2D body;
	public SimpleExploreScript explorer;
	public int AITime = 0;
	private SearchScript searcher;
	private bool started = false;
	private float timer;
	// Use this for initialization
	void Start () {
		mover = gameObject.GetComponent<MoveScript> ();

		slam = gameObject.GetComponent<SLAM>();
		body = gameObject.GetComponent<Rigidbody2D> ();
		goalBased = false;

		if (mover != null) {
			mover.setSpeed (new Vector2 (6, 14));
			explorer = new SimpleExploreScript (mover, body); 
			searcher = new SearchScript(body, slam, mover);
		}

		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown("space")){
			started = true;
			timer = Time.time;
			GameObject.Find("GUI").GetComponent<Highlighter>().off();
			GameObject.Find("GUI").GetComponent<Multilighter>().on();
		}
		if(started){

			if (mover == null ||AITime ==0) {
				mover = gameObject.GetComponent<MoveScript> ();
				mover.setSpeed (new Vector2 (6, 14));
				explorer = new SimpleExploreScript (mover, body); 	
			}
			if (! goalBased) {
				goalBased = slam.exploredEnoughToSlam();
				if(AITime >350){
					goalBased = true;
				}

			}
			if (goalBased) {
				if(searcher == null || searcher.atFinalGoal){
					pickGoalState();
					searcher = new SearchScript(body, slam, mover);
					searcher.setGoalState(goalState);
				}
				else{
					searcher.move();
				}
					
			} 
			else {
				AITime++;
				if(AITime < 325)
					explorer.move ();
				else if(AITime < 350){
					mover.stop();
				}

			}
		}
	
	}
	bool goalAttained(){
		if (slam.Position == goalState) {
			return true;
		}
		return false;

	}
	void pickGoalState(){
		SLAM.Cell[,] grid = slam.OccupancyGrid;
		int maxValue = int.MinValue;
		Vector2 maxGridPos = new Vector2(0,0);
		if(grid != null){
			for (int i = 0; i < grid.GetLength(0);i++){
				for(int j = 0; j < grid.GetLength(1); j++){
					int tempValue = int.MinValue;
					if(grid[i,j].Occupant == SLAM.Occupant.DOOR){
						goalState = new Vector2(i,j);
						return;
					}
					else if(grid[i,j].Occupant == SLAM.Occupant.OPEN){
						tempValue = evalueatePos(new Vector2(i,j), grid);
					}
					else if(grid[i,j].Occupant == SLAM.Occupant.DANGER || grid[i,j].Occupant == SLAM.Occupant.AURA){
						tempValue = evalueatePos(new Vector2(i,j), grid) / 25;
					}
					if(tempValue > maxValue){
						maxValue = tempValue;
						maxGridPos = new Vector2(i,j);
					}
					
				}
			}
			goalState = maxGridPos;
		}
	}
	
	public void die(){
		searcher = null;
		Debug.Log ("Died at " + Time.time);
	}

	public void win(){
		Debug.Log ("Won after " + (Time.time - timer) + " seconds.");
	}

	private int evalueatePos(Vector2 pos, SLAM.Cell[,] grid){

		if(grid[(int)pos.x,(int)pos.y].Occupant == SLAM.Occupant.DOOR){
			return int.MaxValue;
		}
		if(pos.x == 0 || pos.y == 0){
			return 0 - manhattanDist(slam.Position , pos);
		}
		int totalValue = 0;
		if(grid[(int)pos.x, (int)pos.y - 1].Occupant == SLAM.Occupant.WALL){
			totalValue += 1000;
		}
		if(grid[(int)pos.x - 1, (int)pos.y].Occupant == SLAM.Occupant.UNEXPLORED ||
		   grid[(int)pos.x + 1, (int)pos.y].Occupant == SLAM.Occupant.UNEXPLORED ||
		   grid[(int)pos.x , (int)pos.y + 1].Occupant == SLAM.Occupant.UNEXPLORED ||
		   grid[(int)pos.x - 1, (int)pos.y + 1].Occupant == SLAM.Occupant.UNEXPLORED ||
		   grid[(int)pos.x + 1, (int)pos.y +1].Occupant == SLAM.Occupant.UNEXPLORED){
			totalValue += 10000;
		}
		totalValue -= manhattanDist(slam.Position, pos);
		return totalValue;
	}
	private int manhattanDist(Vector2 start, Vector2 dest){
		return (int)Mathf.Abs((int)start.x - (int)dest.x) + Mathf.Abs((int)start.y - (int)dest.y);
	}
}
