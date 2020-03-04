using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class RocketAgent : Agent
{
    Rigidbody rBody;
    Collider collider;

    #region PUBLIC VARIABLES

    public Transform droneObject;
    public float velocity; //check for speed
    public float upForce;
    [HideInInspector] public int directionForward;
    [HideInInspector] public int directionSwerve;
    // [HideInInspector] public int directionRight;
    [HideInInspector] public float tiltMovementSpeed;
    [HideInInspector] public float currentYRotation;

    #endregion

    #region PRIVATE VARIABLES

    private float movementForwardSpeed = 500.0f;
    private float tiltAmountForward = 0;
    private float tiltAmountSideways;
    private float tiltAmountVelocity;
    private float tiltVelocityForward;
    private float wantedYRotation;
    private float rotateAmountByKeys = 2.5f;
    private float rotationYVelocity;
    private float sideMovementAmount = 300.0f;
    private Vector3 velocityToSmoothDampZero;

    #endregion
    
    bool onPlane = false;

    public int minStepsOnPlane = 50;
    public float minAngleOnPlane = 15;
    public float randomForceFactor = 5f;
    public float randomTorqueFactor = 0.5f;
    public float speedThrehold = 10.0f; 
    private Vector3 speedBeforeCollision;
    private int stepsOnPlane = 0;

    

    void Start () 
    {
        rBody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    void FixedUpdate(){

        currentYRotation = Mathf.SmoothDamp(currentYRotation, wantedYRotation, ref rotationYVelocity, 0.25f);
        tiltAmountForward = Mathf.SmoothDamp(tiltAmountForward, 20 * directionForward, ref tiltVelocityForward, tiltMovementSpeed);
        tiltAmountSideways = Mathf.SmoothDamp(tiltAmountSideways, -20 * directionSwerve, ref tiltAmountVelocity, tiltMovementSpeed);
        rBody.AddRelativeForce(Vector3.up * upForce 
                               + Vector3.forward * directionForward * movementForwardSpeed
                               + Vector3.right * directionSwerve * sideMovementAmount);
        rBody.rotation = Quaternion.Euler(new Vector3(0, currentYRotation, 0));
        rBody.angularVelocity = new Vector3(0, 0, 0);
        droneObject.localRotation = Quaternion.Euler(new Vector3(tiltAmountForward, 0, tiltAmountSideways));
        rBody.velocity = new Vector3(Mathf.Lerp(rBody.velocity.x, 0, Time.deltaTime * 1.2f), 
                                     Mathf.Lerp(rBody.velocity.y, 0, Time.deltaTime * 3), 
                                     Mathf.Lerp(rBody.velocity.z, 0, Time.deltaTime * 1.2f));
        
        ClampingSpeedValues();
        GetVelocity();

        directionForward = 0;
        directionSwerve = 0;
        // directionRight = 0;
        upForce = 98.1f;
        tiltMovementSpeed = 0.3f;
    }

    void ClampingSpeedValues()
    {
        if (Mathf.Abs(directionForward) == 1 && Mathf.Abs(directionSwerve) == 1){
            rBody.velocity = Vector3.ClampMagnitude(rBody.velocity, Mathf.Lerp(rBody.velocity.magnitude, 10.0f, Time.deltaTime * 5f));
        }
        if (Mathf.Abs(directionForward) == 1 && directionSwerve == 0){
            rBody.velocity = Vector3.ClampMagnitude(rBody.velocity, Mathf.Lerp(rBody.velocity.magnitude, 10.0f, Time.deltaTime * 5f));
        }
        if (directionForward == 0 && Mathf.Abs(directionSwerve) == 1){
            rBody.velocity = Vector3.ClampMagnitude(rBody.velocity, Mathf.Lerp(rBody.velocity.magnitude, 5f, Time.deltaTime * 5f));
        }
        if (directionForward == 0 && Mathf.Abs(directionSwerve) == 0){
            rBody.velocity = Vector3.SmoothDamp(rBody.velocity, Vector3.zero, ref velocityToSmoothDampZero, 0.95f);
        }
    }

    public void GetVelocity()
    {
        velocity = rBody.velocity.magnitude;
    }


    void OnCollisionEnter(Collision collision)
    {
    	if (collision.collider.name == "Plane")
        {
        	// print(collision.relativeVelocity);  // 显示碰撞前的速度
        	speedBeforeCollision = collision.relativeVelocity;
        	onPlane = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
    	// 撞到平面且旋转角度没有过大，才算站到了平面上
    	if (collision.collider.name == "Plane" &&
    		Quaternion.Angle(transform.rotation, Quaternion.identity) < minAngleOnPlane
    		) // 火箭旋转的角度和向上的角度，角度小于15度
        {
	    	stepsOnPlane++;
	    } 
	    else {
	    	stepsOnPlane = 0;
	    }
    }

    void OnCollisionExit(Collision collision) // 如果没停够stepsOnPlane数的话就起飞了
    {
    	if (collision.collider.name == "Plane")
        {
        	onPlane = false; // onPlane重新设成false
        	stepsOnPlane = 0; // stepsOnPlane重新归零
        }
    }


    public override void AgentReset()
    {
    	onPlane = false;
    	stepsOnPlane = 0;

        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(0,0,0);
        tiltAmountForward = 0;
        tiltAmountSideways = 0;
        currentYRotation = 0;
        wantedYRotation = 0;
        transform.rotation = Quaternion.identity; // 重置角度
       	// transform.position = new Vector3(Random.Range(-2,2),
       	// 								 Random.Range(5,8), 
       	// 								 Random.Range(-2,2));
        transform.position = new Vector3(22.5f, 5, -10);
    }

    public override void CollectObservations() // RL中的observation/state
	{
	    AddVectorObs(transform.position);
	    // AddVectorObs(rBody.velocity);
	}

    
	public override void AgentAction(float[] vectorAction)
	{
    
        var movement = (int)vectorAction[0];

        switch (movement)
        {
            // case 0:
            //     upForce = 98.1f;
            //     // dirToGo = transform.up * upForce;
            //     // rBody.AddRelativeForce(Vector3.up * upForce);
            //     break;
            case 1: //upward
                upForce = 450;
                break;
            case 2: // downward
                upForce = -200;
                break;
            case 3: // forward
                directionForward = 1;
                tiltMovementSpeed = 0.1f;
                upForce = rBody.mass * 9.81001f;
                break;
            case 4: // backward
                directionForward = -1;
                tiltMovementSpeed = 0.1f;
                upForce = rBody.mass * 9.81001f;
                break;
            case 5: // left rotation
                wantedYRotation -= rotateAmountByKeys;
                // directionRight = -1;
                
                break;
            case 6: // right rotation
                wantedYRotation += rotateAmountByKeys;
                // directionRight = 1;
                
                break;
            case 7: // leftward
                directionSwerve = 1;
                tiltMovementSpeed = 0.1f;
                upForce = rBody.mass * 9.81001f;
                break;
            case 8: // rightward
                directionSwerve = -1;
                tiltMovementSpeed = 0.1f;
                upForce = rBody.mass * 9.81001f;
                break;
        }
        
        
		// // user's control signal
	 //    // Actions, size = 2
	 //    // Vector3 verticalSignal = Vector3.zero;
	 //    // verticalSignal.y = vectorAction[0]*20;
	 //    // 相较于上面那行代码，用下面的代码按上的时候，是沿着物体本身的向上方向，而不是单纯沿着y轴方向
	 //    Vector3 verticalSignal = transform.up*vectorAction[0]*20; 
	 //    // Vector3 horizontalSignal = Vector3.zero;
	 //    // horizontalSignal.y = vectorAction[1]*10;
	 //    // horizontalSignal.x = vectorAction[1]*20;
	 //    Vector3 horizontalSignal = transform.up*vectorAction[1]*10 + transform.right*vectorAction[1]*20;

	 //    rBody.AddForce(verticalSignal + horizontalSignal);
	 //    rBody.AddTorque(-transform.forward * vectorAction[1] * 1);  // 力矩，可以旋转，forward加负号就是backward，up是沿y轴旋转
	    //

	 //    // add random noisy force,类似于风
	 //    Vector3 randomForce = new Vector3(Random.Range(-randomForceFactor,randomForceFactor),
  //      									  Random.Range(-randomForceFactor,randomForceFactor), 
  //      									  Random.Range(-randomForceFactor,randomForceFactor));
		// Vector3 randomTorque = new Vector3(Random.Range(-randomTorqueFactor,randomTorqueFactor),
  //      									   Random.Range(-randomTorqueFactor,randomTorqueFactor), 
  //      									   Random.Range(-randomTorqueFactor,randomTorqueFactor));
  //       Vector3 upDown = new Vector3(0,9.8f,0);
  //       rBody.AddForce(upDown);
	 //    rBody.AddForce(randomForce);
	 //    rBody.AddTorque(randomTorque); 
	    

	    // 设置奖励，这里是一个step的奖励，所以不涉及到累加
	    if (onPlane){
	    	if (speedBeforeCollision.magnitude < speedThrehold){ // 如果落在平面上的速度不太快
	    		if (stepsOnPlane > minStepsOnPlane){ // 在平面上时要持续一段时间才算成功
	    			Done();
                    SetReward(10.0f);
		    		print("success");
	    		}
	    		else {
	    			SetReward(0.1f);
	    		}
		    	
	    	}
	    	else {  // 如果落在平面上的速度太快也算失败
	    		Done();
                SetReward(-2.0f);
	    		print("failed, too fast");
	    	}
	    	
	    } 

	    else if (transform.position.y < 0){  // 掉下平面
	    	Done();
            SetReward(-2.0f);
	    	print("failed, Fell off platform");
	    } 

	    else {
	    	SetReward(-0.01f);
	    } 

        float distanceToTarget = Vector3.Distance(transform.position, Vector3.zero);
        AddReward(-distanceToTarget/8);
	    //
	    
	}


    public override float[] Heuristic()
    {
        // var action = new float[];
        if (Input.GetKey(KeyCode.K))
        {
            return new float[] { 2 };
        }
        if (Input.GetKey(KeyCode.I))
        {
            return new float[] { 1 };
        }
        if (Input.GetKey(KeyCode.S))
        {
            return new float[] { 4 };
        }
        if (Input.GetKey(KeyCode.W))
        {
            return new float[] { 3 };
        }
        if (Input.GetKey(KeyCode.J))
        {
            return new float[] { 5 };
        }
        if (Input.GetKey(KeyCode.L))
        {
            return new float[] { 6 };
        }
        if (Input.GetKey(KeyCode.D))
        {
            return new float[] { 7 };
        }
        if (Input.GetKey(KeyCode.A))
        {
            return new float[] { 8 };
        }
        else 
        {
            return new float[] { 0 };
        }
        // return action;
    }
}
