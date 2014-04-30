using UnityEngine;
using System.Collections;

public class MoveScript : MonoBehaviour {
    public Vector2 speed = new Vector2(2,10);
	public int jumpCount = 0;
	public int totalJumpTime = 20;
	public int jumpTime = 0;
    private Vector2 movement;
	public int horizontal = 0;
	public int vertical = 0;
	private Rect box;
	public int sameMovesNoMovement = 0;
	private int jumplr = 0;
	private int mdir;
	private Vector3 npos;

	void Start(){
		npos = gameObject.transform.position;
	}

	// Update is called once per frame
	void Update () {
		if(this.gameObject.transform.position.x == npos.x && this.gameObject.transform.position.y == npos.y){
			sameMovesNoMovement++;
		}else{
			sameMovesNoMovement = 0;
			npos = gameObject.transform.position;
		}

		if(sameMovesNoMovement >= 100){
			Debug.Log ("Shunted");
			gameObject.transform.Translate(new Vector3(-.5f * (float)mdir, Random.Range(-0.3f, 0.3f), 0.0f));
			if(grounded())
				jump();
		}


        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
		if(jumpCount > 0){
			if(jumpTime < 2 || !grounded())
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

		movement = new Vector2(speed.x * horizontal, speed.y * vertical);
        //movement = new Vector2(speed.x * inputX, speed.y * inputY);
	}
	bool grounded(){

		if(rigidbody2D.velocity.y == 0 && Physics2D.Raycast(transform.position, new Vector2(0, -1), 1.1f).collider != null){
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
		Debug.Log("Right");
		horizontal = 1;
		mdir = 1;

		jumplr = 0;
	}
	public void left(){
		Debug.Log("Left");
		horizontal = -1;
		mdir = -1;

		jumplr = 0;
	}
	public void stop(){
		Debug.Log("Stop");
		horizontal = 0;
		vertical = 0;

	}
	public void jump(){
		Debug.Log("Jump");
		if (jumpCount == 0){
			jumpCount = 1;
		}
		if(horizontal ==0)
			jumplr ++;
		if(jumplr > 25)
			horizontal = Random.Range(0.0f, 1f) >= .5f ? 1:-1;
	}
	public void doubleJump(){
		Debug.Log("DoubleJump");
		if (jumpCount == 0){
			jumpCount = 1;
		}
		else if(jumpCount == 1){
			jumpCount = 2;
		}
	}
}
