using UnityEngine;
using System.Collections;

public class MoveScript : MonoBehaviour {
    public Vector2 speed = new Vector2(10,10);
	public int jumpCount = 0;
	public int totalJumpTime = 20;
	public int jumpTime = 0;
    private Vector2 movement;
	public int horizontal = 0;
	public int vertical = 0;
	private Rect box;
	
	// Update is called once per frame
	void Update () {

        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
		if(jumpCount > 0){
			if(jumpCount < 2 || !grounded)
			{

				if(jumpTime < (totalJumpTime * jumpCount)){
					jumpTime++;
					vertical = 1;
				}
				else{
					vertical = 0;
				}

			}
			else{
				jumpCount =0;
				jumpTime = 0;
				vertical = 0;
			}
		}
		//movement = new Vector2(speed.x * horizontal, speed.y * vertical);
        movement = new Vector2(speed.x * inputX, speed.y * inputY);
	}
	bool grounded(){

		if(rigidbody2D.velocity.y == 0){
			return true;
		}
		return false;
	}
    void FixedUpdate()
    {
		//box = new Rect(collider.bounds.min.x, collider.bounds.min.y, collider.bounds.size.x, collider.bounds.size.y);
        rigidbody2D.velocity = movement;
    }

	public void setSpeed(Vector2 newSpeed){
		speed = newSpeed;
	}
	public void right(){
		horizontal = 1;
	}
	public void left(){
		horizontal = -1;
	}
	public void stop(){
		horizontal = 0;
	}
	public void jump(){
		if (jumpCount == 0){
			jumpCount = 1;
		}
	}
	public void doubleJump(){
		if (jumpCount == 0){
			jumpCount = 1;
		}
		else if(jumpCount == 1){
			jumpCount = 2;
		}
	}
}
