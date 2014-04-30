using UnityEngine;
using System.Collections;

public class SimpleExploreScript {

	public MoveScript mover;
	private int moveCount;
	private Rigidbody2D movingBody;
	private bool right;
	private int jumptimer;
	public SimpleExploreScript(MoveScript mover, Rigidbody2D body){
		this.mover = mover;
		movingBody = body;
		moveCount = 0;
		right = true;
		jumptimer = 0;
	}
	public void move(){
		if (moveCount == 0) {
						mover.right ();
		} 
		else {
			if( right){
				if(movingBody.velocity.x <= 0&& jumptimer == 0){
					right = false;
					mover.left();
				}
				else{
					mover.right();
				}
			}
			else{
				if(movingBody.velocity.x >= 0 && jumptimer == 0){
					right = true;
					mover.right();
				}
				else{
					mover.left();
				}
			}
		}
		jumptimer++;
		if (jumptimer == 50) {
			mover.jump();
			jumptimer = 0;
		}
		moveCount++;
	}

}
