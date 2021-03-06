﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SearchScript  {
	public Rigidbody2D rigid;
	public SLAM slam;
	public MoveScript mover;
	public ArrayList moveList;
	public Vector2 goalState;
	public bool atFinalGoal;
	public int maxMoveTime = 50;
	public int moveTime = 0;
	public int jumpEstimation = 6;
	public int rightLeftVal = 2;
	public bool unExploredGoal = true;
	// Use this for initialization
	public SearchScript(Rigidbody2D rigidbody, SLAM slammer, MoveScript move){	
		rigid = rigidbody;
		slam = slammer;
		mover = move;
		moveList = new ArrayList();
		atFinalGoal = false;

	}
	public void setGoalState(Vector2 goal){
		goalState = goal;
		atFinalGoal = false;
		moveList = doAStar();
		return;
	}
	public void move(){

		if(moveList == null || moveList.Count == 0)
		{
			mover.stop();
			atFinalGoal = true;
		}
		else if(unExploredGoal && slam.OccupantGrid[(int)goalState.x, (int)goalState.y].Occupant != SLAM.Occupant.UNEXPLORED){
			mover.stop();
			atFinalGoal = true;
		}
		else{
			AStarNode current = (AStarNode)moveList[0];
			moveTime ++;
			if(slam.Position == current.position ||
			   (current.thisMove == Move.LEFT && slam.Position.x <= current.position.x) ||
			   (current.thisMove == Move.RIGHT && slam.Position.x >= current.position.x) ||
			   (current.thisMove == Move.JUMP && slam.Position.y >= current.position.y) ||
			   (current.thisMove == Move.JUMP && slam.Position.y >= current.position.y) ||
			   current.thisMove == Move.STOPPED){
				moveTime = 0;
				if(moveList.Count == 1){
					mover.stop();
				}
				else if(current.thisMove == Move.JUMP  || current.thisMove == Move.DOUBLEJUMP){
					mover.stop();
					makeMove(((AStarNode)moveList[1]).thisMove);
				}
				else{
					makeMove(((AStarNode)moveList[1]).thisMove);
				}
				moveList.RemoveAt(0);
			}

			else{
				if(moveTime >= maxMoveTime){
					moveList = new ArrayList();
					moveList = doAStar();
				}
				else{
					makeMove(current.thisMove);
				}
			}
		}
		
	}
	private void makeMove(Move aMove){
		if(aMove == Move.RIGHT){
			mover.right();
		}
		else if(aMove == Move.LEFT){
			mover.left();
		}
		else if(aMove == Move.JUMP){
			mover.jump();
		}
		else if(aMove == Move.DOUBLEJUMP){
			mover.doubleJump();
		}
		else{
			mover.stop();
		}
	}
	private ArrayList doAStar(){
		moveTime = 0;

		OccupancyGrid occupancy = slam.OccupantGrid;

		PriorityQueue<AStarNode> aStarQueue = new PriorityQueue<AStarNode>();
		Vector2 currentPos = slam.Position;
		int loopCap = 0;

		aStarQueue.enQueue(heuristic(Move.STOPPED, 0, occupancy, slam.Position), new AStarNode( slam.Position, Move.STOPPED, new ArrayList()));
		while(aStarQueue.notEmpty()){
			loopCap++;
			if( aStarQueue.priorityPeek() == int.MaxValue){
				AStarNode final = aStarQueue.deQueue();
				ArrayList path = final.path;
				path.Add(final);
				return path;
			}
			else if(aStarQueue.priorityPeek() == int.MinValue){
				return null;
			}
			else if(loopCap >= 1000){
				AStarNode final = aStarQueue.deQueue();
				ArrayList path = final.path;
				path.Add(final);
				return path;
			}
			AStarNode current = aStarQueue.deQueue();

			AStarNode rightNode = new AStarNode(new Vector2(current.position.x + rightLeftVal, current.position.y), Move.RIGHT, (ArrayList)current.path.Clone());
			rightNode.path.Add(current);
			aStarQueue.enQueue (heuristic(Move.RIGHT, rightNode.path.Count, occupancy, rightNode.position), rightNode);
			AStarNode leftNode = new AStarNode(new Vector2(current.position.x -rightLeftVal, current.position.y), Move.LEFT, (ArrayList)current.path.Clone());
			leftNode.path.Add(current);
			aStarQueue.enQueue (heuristic(Move.LEFT, leftNode.path.Count, occupancy, leftNode.position), leftNode);
			if(!wallAbove(current.position)){
				if(current.thisMove == Move.JUMP){
					AStarNode doubleJumpNode = new AStarNode(new Vector2(current.position.x, current.position.y + jumpEstimation), Move.DOUBLEJUMP, (ArrayList)current.path.Clone());
					doubleJumpNode.path.Add(current);
					aStarQueue.enQueue(heuristic(Move.DOUBLEJUMP, doubleJumpNode.path.Count, occupancy, doubleJumpNode.position), doubleJumpNode);
				}
				else if( current.thisMove != Move.DOUBLEJUMP){
					AStarNode jumpNode = new AStarNode(new Vector2(current.position.x, current.position.y + jumpEstimation), Move.JUMP, (ArrayList)current.path.Clone());
					jumpNode.path.Add(current);
					aStarQueue.enQueue(heuristic(Move.JUMP, jumpNode.path.Count, occupancy, jumpNode.position), jumpNode);
				}
			}

		}
		return null;
	}
	private int heuristic(Move lastMove, int pathsize, OccupancyGrid occupancy, Vector2 opos){

		int value = 100 - pathsize;
		Vector2 pos = new Vector2 (opos.x - 0.5f, opos.y + 1f);
		int goalDist = manhattanDist(pos, goalState);

		if((int)pos.x < 0 ||
		        (int)pos.x >= occupancy.GetLength(0) ||
		        (int)pos.y < 0 ||
		        (int)pos.y >= occupancy.GetLength(1)){
			value = int.MinValue;
		}
		else if (inRangeOfGoal(pos)|| occupancy[(int)pos.x, (int)pos.y].Occupant == SLAM.Occupant.DOOR){
			value = int.MaxValue;
		}

	//	else if(occupancy[(int)pos.x, (int)pos.y].Occupant == SLAM.Occupant.WALL || occupancy[(int)pos.x, (int)pos.y].Occupant == SLAM.Occupant.OUTTER_WALL){
	//		value = int.MinValue;
	//	}
		else if(occupancy[(int)pos.x, (int)pos.y].Occupant == SLAM.Occupant.OPEN){
			value -= goalDist;
		}
		else if(occupancy[(int)pos.x, (int)pos.y].Occupant == SLAM.Occupant.DANGER || occupancy[(int)pos.x, (int)pos.y].Occupant == SLAM.Occupant.AURA){
			value -= (100 + goalDist);
		}
		else if(occupancy[(int)pos.x, (int)pos.y].Occupant == SLAM.Occupant.UNEXPLORED){
			value -= (50 + goalDist);
		}
		if(value > int.MinValue && lastMove != Move.JUMP && floating(pos, occupancy)){
			value -= 1000;
		}
		return value;
	}
	private bool inRangeOfGoal(Vector2 pos){
		if ((Mathf.Abs (pos.x - goalState.x) + Mathf.Abs (pos.y - goalState.y)) <= 3f) {
			return true;
		}
		return false;
	}
	private bool wallAbove(Vector2 pos){

		for(int i = 1; i < jumpEstimation; i++){
			if( (int)pos.y + i < slam.OccupantGrid.GetLength(1)){
				if(slam.OccupantGrid[(int)pos.x, (int)pos.y + i].Occupant == SLAM.Occupant.WALL ||
				   slam.OccupantGrid[(int)pos.x, (int)pos.y + i].Occupant == SLAM.Occupant.OUTTER_WALL){
					return true;
				}
			}
		}
		return false;

	}
	private bool floating( Vector2 pos, OccupancyGrid occupancy){

		if((int)pos.x < 1 ||
		        (int)pos.x >= occupancy.GetLength(0) - 1 ||
		        (int)pos.y < 1 ||
		        (int)pos.y >= occupancy.GetLength(1) -1){
			return false;
		}

		if(occupancy[(int)pos.x + rightLeftVal, (int)pos.y].Occupant != SLAM.Occupant.OPEN &&
		   occupancy[(int)pos.x + rightLeftVal, (int)pos.y].Occupant != SLAM.Occupant.UNEXPLORED && 
		   occupancy[(int)pos.x + rightLeftVal, (int)pos.y].Occupant != SLAM.Occupant.AURA &&
		   occupancy[(int)pos.x + rightLeftVal, (int)pos.y].Occupant != SLAM.Occupant.DANGER){
			return false;
		}

		if(occupancy[(int)pos.x - rightLeftVal, (int)pos.y].Occupant != SLAM.Occupant.OPEN &&
		   occupancy[(int)pos.x - rightLeftVal, (int)pos.y].Occupant != SLAM.Occupant.UNEXPLORED && 
		   occupancy[(int)pos.x - rightLeftVal, (int)pos.y].Occupant != SLAM.Occupant.AURA &&
		   occupancy[(int)pos.x - rightLeftVal, (int)pos.y].Occupant != SLAM.Occupant.DANGER){
			return false;
		}

		if(occupancy[(int)pos.x , (int)pos.y + 1].Occupant != SLAM.Occupant.OPEN &&
		   occupancy[(int)pos.x , (int)pos.y + 1].Occupant != SLAM.Occupant.UNEXPLORED && 
		   occupancy[(int)pos.x , (int)pos.y + 1].Occupant != SLAM.Occupant.AURA &&
		   occupancy[(int)pos.x , (int)pos.y + 1].Occupant != SLAM.Occupant.DANGER){
			return false;
		}

		if(occupancy[(int)pos.x + rightLeftVal , (int)pos.y + 1].Occupant != SLAM.Occupant.OPEN &&
		   occupancy[(int)pos.x + rightLeftVal, (int)pos.y + 1].Occupant != SLAM.Occupant.UNEXPLORED && 
		   occupancy[(int)pos.x + rightLeftVal, (int)pos.y + 1].Occupant != SLAM.Occupant.AURA &&
		   occupancy[(int)pos.x + rightLeftVal, (int)pos.y + 1].Occupant != SLAM.Occupant.DANGER){
			return false;
		}

		if(occupancy[(int)pos.x - rightLeftVal, (int)pos.y + 1].Occupant != SLAM.Occupant.OPEN &&
		   occupancy[(int)pos.x - rightLeftVal, (int)pos.y + 1].Occupant != SLAM.Occupant.UNEXPLORED && 
		   occupancy[(int)pos.x - rightLeftVal, (int)pos.y + 1].Occupant != SLAM.Occupant.AURA &&
		   occupancy[(int)pos.x - rightLeftVal, (int)pos.y + 1].Occupant != SLAM.Occupant.DANGER){
			return false;
		}

		if(occupancy[(int)pos.x , (int)pos.y - 1].Occupant != SLAM.Occupant.OPEN &&
		   occupancy[(int)pos.x , (int)pos.y - 1].Occupant != SLAM.Occupant.UNEXPLORED && 
		   occupancy[(int)pos.x , (int)pos.y - 1].Occupant != SLAM.Occupant.AURA &&
		   occupancy[(int)pos.x , (int)pos.y - 1].Occupant != SLAM.Occupant.DANGER){
			return false;
		}

		if(occupancy[(int)pos.x + rightLeftVal, (int)pos.y - 1].Occupant != SLAM.Occupant.OPEN &&
		   occupancy[(int)pos.x + rightLeftVal, (int)pos.y - 1].Occupant != SLAM.Occupant.UNEXPLORED && 
		   occupancy[(int)pos.x + rightLeftVal, (int)pos.y - 1].Occupant != SLAM.Occupant.AURA &&
		   occupancy[(int)pos.x + rightLeftVal, (int)pos.y - 1].Occupant != SLAM.Occupant.DANGER){
			return false;
		}

		if(occupancy[(int)pos.x - rightLeftVal, (int)pos.y - 1].Occupant != SLAM.Occupant.OPEN &&
		   occupancy[(int)pos.x - rightLeftVal, (int)pos.y - 1].Occupant != SLAM.Occupant.UNEXPLORED && 
		   occupancy[(int)pos.x - rightLeftVal, (int)pos.y - 1].Occupant != SLAM.Occupant.AURA &&
		   occupancy[(int)pos.x - rightLeftVal, (int)pos.y - 1].Occupant != SLAM.Occupant.DANGER){
			return false;
		}
		return true;
		
	}
	private int manhattanDist(Vector2 start, Vector2 dest){
		return (int)Mathf.Abs((int)start.x - (int)dest.x) + Mathf.Abs((int)start.y - (int)dest.y);
	}
	public enum Move
	{
		STOPPED = -1, RIGHT = 0, LEFT = 1, JUMP = 2, DOUBLEJUMP = 3 
	}
	private class AStarNode{
		public Vector2 position;
		public Move thisMove;
		public ArrayList path;

		public AStarNode(Vector2 pos, Move move, ArrayList pathTo){
			position = pos;
			thisMove = move;
			path = pathTo;
		}

	}
	private class PriorityQueue<T>{
		ArrayList data;
		ArrayList priorities;
		public PriorityQueue(){
			data = new ArrayList();
			priorities = new ArrayList();
		}
		public void enQueue(int priority, T value){
			if(priorities.Count == 0){
				data.Add(value);
				priorities.Add(priority);
				return;
			}
			else{
				for(int i = 0; i< priorities.Count; i++){
					if(priority > (int)priorities[i]){
						data.Insert(i, value);
						priorities.Insert(i,priority);
						return;
					}
				}
				data.Add(value);
				priorities.Add(priority);
				return;
			}
		}
		public T peek(){
			return (T)data[0];
		}
		public T deQueue(){
			T value = (T)data[0];
			data.RemoveAt(0);
			priorities.RemoveAt(0);
			return value;
		}
		public int priorityPeek(){
			return (int)priorities[0];
		}
		public bool notEmpty(){
			if(priorities.Count > 0){
				return true;
			}
			return false;
		}
	}




}
