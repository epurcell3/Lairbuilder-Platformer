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
	public float timer;
	public int deaths = 0;
	private Vector3 start;
	// Use this for initialization
	void Start () {
		start = gameObject.transform.position;

		mover = gameObject.GetComponent<MoveScript> ();

		slam = gameObject.GetComponent<SLAM>();
		body = gameObject.GetComponent<Rigidbody2D> ();
		goalBased = false;

		if (mover != null) {
			mover.setSpeed (new Vector2 (9, 15));
			explorer = new SimpleExploreScript (mover, body); 
			searcher = new SearchScript(body, slam, mover);
		}

		new GameObject("AITEST");
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
				mover.setSpeed (new Vector2 (9, 15));
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
					GameObject g = GameObject.Find("AITEST");
					if(g.GetComponent<SpriteRenderer>() == null)
						g.AddComponent<SpriteRenderer> ();
					gameObject.GetComponent<SpriteRenderer> ().color =  Color.yellow;
					gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create (Resources.Load ("flareaura_3") as Texture2D, new Rect ((float)(32f*1), 0.0f, 32f, 32f), new Vector2 (0.5f, 0.5f), 32f / 2.5f / 2.0f);
					g.transform.position = new Vector3(goalState.x/2f, goalState.y/2f, 25f);
					if(slam.OccupantGrid[(int)goalState.x, (int) goalState.y].Occupant == SLAM.Occupant.UNEXPLORED){
						searcher.unExploredGoal = true;
					}
					else{
						searcher.unExploredGoal = false;
					}
					Debug.Log ("final Goal state: " + goalState.x + " , " + goalState.y + " ");
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
		OccupancyGrid grid = slam.OccupantGrid;
		int maxValue = int.MinValue;
		Vector2 maxGridPos = new Vector2(0,0);
		if(grid != null){
			Debug.Log("Grid dimentions: " + grid.GetLength(0) + " , " + grid.GetLength(1));
			for (int i = 0; i < grid.GetLength(0);i++){
				for(int j = 0; j < grid.GetLength(1); j++){

				//	Debug.Log ("tested: " + i + " , " + j +" ");
					grid = slam.OccupantGrid;
					int tempValue = int.MinValue;
					if(grid[i,j].Occupant == SLAM.Occupant.DOOR){
						goalState = new Vector2(i,j);
						return;
					}
					else if(grid[i,j].Occupant == SLAM.Occupant.UNEXPLORED){
						tempValue = 1000 - manhattanDist(new Vector2(i, j), slam.Position);
					}
					//else if(grid[i,j].Occupant == SLAM.Occupant.OPEN){
				//		tempValue = evalueatePos(new Vector2(i,j), grid);
				//	}
				//	else if(grid[i,j].Occupant == SLAM.Occupant.DANGER || grid[i,j].Occupant == SLAM.Occupant.AURA){
				//		tempValue = evalueatePos(new Vector2(i,j), grid) / 25;
				//	}
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
		GameObject.Find ("AI").transform.position = start;
		Debug.Log ("Died at " + Time.time);
		deaths++;
	}

	public void win(){
		started = false;
		Vector3 baseV = new Vector3 (0.0f, 0.0f, 25f);
		GameObject.Find ("GUI-End Back").transform.position =baseV;
		baseV.z = 30f;
		GameObject.Find ("GUI-End Button").transform.position =baseV;
		GameObject.Find ("GUI-End Text").transform.position =baseV;
		GameObject.Find ("GUI-End Text").GetComponent<EndText> ().setTime ();
		Debug.Log ("Won after " + (Time.time - timer) + " seconds.");
	}

	private int evalueatePos(Vector2 pos, OccupancyGrid grid){

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
