using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using System.IO;

public class RocketAgent : Agent
{
    Rigidbody rBody;
    Collider colli;

    #region PUBLIC VARIABLES

    public Transform droneObject;
    public GameObject goal;
    public GameObject tag;
    public GameObject tag2;
    public GameObject pathPoint;
    public GameObject oneOb;  // test the distance moved in one step
    GameObject cloneObj;  // cloned obstacles
    GameObject cloneObj2;  // cloned obstacles
    GameObject clonePath;
    List<GameObject> cloneObjList = new List<GameObject>();  // list for storing cloned obstacles
    List<GameObject> cloneObjList2 = new List<GameObject>();  // list for storing cloned obstacles
    List<GameObject> clonePathList = new List<GameObject>();
    public Transform dynamicObstacle1;
    public Transform dynamicObstacle2;
    public float velocity; //check for speed
    public float sphereRadius;
    public float maxLaserDistance;
    public float safeDistance;
    public float dynamicSpeed1;
    public float dynamicSpeed2;
    public float timeScaleNumber;
    public float oneObPosition;
    [HideInInspector] public float upForce;
    [HideInInspector] public float directionForward;
    [HideInInspector] public float directionSwerve;
    [HideInInspector] public float directionUpDown;
    // [HideInInspector] public int directionRight;
    [HideInInspector] public float tiltMovementSpeed;
    [HideInInspector] public float currentYRotation;
    // [HideInInspector] public float currentXRotation;

    #endregion

    #region PRIVATE VARIABLES

    private float movementForwardSpeed = 500.0f;
    private float tiltAmountForward = 0;
    private float tiltAmountSideways;
    private float tiltAmountVelocity;
    private float tiltVelocityForward;
    private float wantedYRotation;
    // private float wantedXRotation;
    private float rotateAmountByKeys = 1.5f;
    private float rotationYVelocity;
    // private float rotationXVelocity;
    private float sideMovementAmount = 300.0f;
    private float upMovementAmount = 500.0f;
    private Vector3 velocityToSmoothDampZero;
    private float tempY;
    private float tempZ;
    private float tempX;
    private float[] distanceToObstacle = new float[13];
    private float minDistanceToObstacle;
    private float angle;
    private float distanceToTarget;
    bool runIntoObstacle = false;
    // bool findObstacle = false;
    bool flag;

    bool rayR90 = false;
    bool rayL90 = false;
    bool rayR60 = false;
    bool rayL60 = false;
    bool rayR30 = false;
    bool rayL30 = false;
    bool rayF = false;
    bool rayUp30 = false;
    bool rayDown30 = false;
    bool rayUp60 = false;
    bool rayDown60 = false;
    bool rayUp90 = false;
    bool rayDown90 = false;

    private int position_num;
    private string[] positions = new string[10000];  // not less than max_steps+1 (the extra 1 is for storing the target position)
    private string dataPath = "E:/123.csv";  // Used to store the drone's pos, speed, and the target pos, this file must already exist in the folder
    private int idx;

    #endregion

    void Start () 
    {
        rBody = GetComponent<Rigidbody>();
        colli = GetComponent<Collider>();
        Time.timeScale = timeScaleNumber;
        Random.InitState(1);
    }

    void Update()
    {  
    }

    void FixedUpdate()
    {
        currentYRotation = Mathf.SmoothDamp(currentYRotation, wantedYRotation, ref rotationYVelocity, 0.25f);
        // currentXRotation = Mathf.SmoothDamp(currentXRotation, wantedXRotation, ref rotationXVelocity, 0.10f);
        tiltAmountForward = Mathf.SmoothDamp(tiltAmountForward, 20 * directionForward, ref tiltVelocityForward, tiltMovementSpeed);
        tiltAmountSideways = Mathf.SmoothDamp(tiltAmountSideways, -20 * directionSwerve, ref tiltAmountVelocity, tiltMovementSpeed);
        rBody.AddRelativeForce(Vector3.up * (upForce + directionUpDown * upMovementAmount) 
                               + Vector3.forward * directionForward * movementForwardSpeed
                               + Vector3.right * directionSwerve * sideMovementAmount);
        rBody.rotation = Quaternion.Euler(new Vector3(0, currentYRotation, 0));
        rBody.angularVelocity = new Vector3(0, 0, 0);
        droneObject.localRotation = Quaternion.Euler(new Vector3(tiltAmountForward, 0, tiltAmountSideways));
        rBody.velocity = new Vector3(Mathf.Lerp(rBody.velocity.x, 0, Time.deltaTime * 1.2f), 
                                     Mathf.Lerp(rBody.velocity.y, 0, Time.deltaTime * 3), 
                                     Mathf.Lerp(rBody.velocity.z, 0, Time.deltaTime * 1.2f));
        
        // ClampingSpeedValues();
        GetVelocity();
        LimitHeight();
        SetLaser();
        DynamicOb();
        // DynamicTar();
        DrawPath();

        directionForward = 0;
        directionSwerve = 0;
        directionUpDown = 0;
        // directionRight = 0;
        upForce = 98.1f;
        tiltMovementSpeed = 0.3f;
        oneObPosition = oneOb.transform.position.x;
    }

    void DrawPath()
    {
        clonePath=Instantiate(pathPoint, pathPoint.transform.position = transform.position, transform.rotation);
        clonePathList.Add(clonePath);
    }

    void DynamicTar()
    {
    	if (idx < 1){
    		if (Vector3.Distance(transform.position, goal.transform.position) < 250){
    			goal.transform.position = new Vector3(Random.Range(-190,190), Random.Range(30,90), Random.Range(160,190));
    			idx += 1;
    		}
    	}
    }

    void DynamicOb()
    {
    	# region DYNAMIC OBSTACLES

        foreach (var cloneObj in cloneObjList)
        {
            cloneObj.transform.position += Vector3.right * dynamicSpeed1 * Time.deltaTime;
            if (cloneObj.transform.position.x >= 185.0f || cloneObj.transform.position.x <= -185.0f){
                dynamicSpeed1 = -dynamicSpeed1;
            }
        }

        foreach (var cloneObj2 in cloneObjList2)
        {
            cloneObj2.transform.position += Vector3.right * dynamicSpeed2 * Time.deltaTime;
            if (cloneObj2.transform.position.x >= 185.0f || cloneObj2.transform.position.x <= -185.0f){
                dynamicSpeed2 = -dynamicSpeed2;
            }
        }

        dynamicObstacle1.position += Vector3.right * dynamicSpeed1 * Time.deltaTime;
        // reach the boundary of the area
        if (dynamicObstacle1.position.x >= 185.0f || dynamicObstacle1.position.x <= -185.0f){
            // reverse movement
            dynamicSpeed1 = -dynamicSpeed1;
        }
        dynamicObstacle2.position += Vector3.right * dynamicSpeed2 * Time.deltaTime;
        // reach the boundary of the area
        if (dynamicObstacle2.position.x >= 185.0f || dynamicObstacle2.position.x <= -185.0f){
            // reverse movement
            dynamicSpeed2 = -dynamicSpeed2;
        }

        # endregion
    }

    void SetLaser()
    {
    	RaycastHit hit0; // hit0-hit6 are ranging lasers on the horizontal plane
    	RaycastHit hit1;
    	RaycastHit hit2;
    	RaycastHit hit3;
    	RaycastHit hit4;
    	RaycastHit hit5;
    	RaycastHit hit6;
    	RaycastHit hit7; // hit7-hit12 are ranging lasers on the vertical plane
    	RaycastHit hit8; 
    	RaycastHit hit9; 
    	RaycastHit hit10; 
    	RaycastHit hit11; 
    	RaycastHit hit12; 

    	for (int i = 0; i < 13; i++)
        {
            distanceToObstacle[i] = maxLaserDistance;
        }
        minDistanceToObstacle = maxLaserDistance;

    	if (Physics.SphereCast(transform.position, sphereRadius, -transform.right, out hit0, maxLaserDistance)&&hit0.collider.tag == "Obstacles"){
            distanceToObstacle[0] = (transform.position-hit0.point).magnitude;
            Debug.DrawRay(transform.position, (-transform.right)*distanceToObstacle[0], Color.red);
            rayL90 = true;
        }
        else {
        	rayL90 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, (-Mathf.Sqrt(3)*transform.right + transform.forward)/2, out hit1, maxLaserDistance)&&hit1.collider.tag == "Obstacles"){
            distanceToObstacle[1] = (transform.position-hit1.point).magnitude;
            Debug.DrawRay(transform.position, ((-Mathf.Sqrt(3)*transform.right + transform.forward)/2)*distanceToObstacle[1], Color.red);
            rayL60 = true;
        } 
        else {
        	rayL60 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, (-transform.right + Mathf.Sqrt(3)*transform.forward)/2, out hit2, maxLaserDistance)&&hit2.collider.tag == "Obstacles"){
            distanceToObstacle[2] = (transform.position-hit2.point).magnitude;
            Debug.DrawRay(transform.position, ((-transform.right + Mathf.Sqrt(3)*transform.forward)/2)*distanceToObstacle[2], Color.red);
            rayL30 = true;
        }
        else {
        	rayL30 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, transform.forward, out hit3, maxLaserDistance)&&hit3.collider.tag == "Obstacles"){
            distanceToObstacle[3] = (transform.position-hit3.point).magnitude;
            Debug.DrawRay(transform.position, transform.forward*distanceToObstacle[3], Color.red);
            rayF = true;
        }
        else {
        	rayF = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, (transform.right + Mathf.Sqrt(3)*transform.forward)/2, out hit4, maxLaserDistance)&&hit4.collider.tag == "Obstacles"){
            distanceToObstacle[4] = (transform.position-hit4.point).magnitude;
            Debug.DrawRay(transform.position, ((transform.right + Mathf.Sqrt(3)*transform.forward)/2)*distanceToObstacle[4], Color.red);
            rayR30 = true;
        }
        else {
        	rayR30 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, (Mathf.Sqrt(3)*transform.right + transform.forward)/2, out hit5, maxLaserDistance)&&hit5.collider.tag == "Obstacles"){
            distanceToObstacle[5] = (transform.position-hit5.point).magnitude;
            Debug.DrawRay(transform.position, ((Mathf.Sqrt(3)*transform.right + transform.forward)/2)*distanceToObstacle[5], Color.red);
            rayR60 = true;
        }
        else {
        	rayR60 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, transform.right, out hit6, maxLaserDistance)&&hit6.collider.tag == "Obstacles"){
            distanceToObstacle[6] = (transform.position-hit6.point).magnitude;
            Debug.DrawRay(transform.position, transform.right*distanceToObstacle[6], Color.red);
            rayR90 = true;
        }
        else {
        	rayR90 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, (Mathf.Sqrt(3)*transform.forward + transform.up)/2, out hit7, maxLaserDistance)&&hit7.collider.tag == "Obstacles"){
            distanceToObstacle[7] = (transform.position-hit7.point).magnitude;
            Debug.DrawRay(transform.position, ((Mathf.Sqrt(3)*transform.forward + transform.up)/2)*distanceToObstacle[7], Color.red);
            rayUp30 = true;
        }
        else {
        	rayUp30 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, (Mathf.Sqrt(3)*transform.forward - transform.up)/2, out hit8, maxLaserDistance)&&hit8.collider.tag == "Obstacles"){
            distanceToObstacle[8] = (transform.position-hit8.point).magnitude;
            Debug.DrawRay(transform.position, ((Mathf.Sqrt(3)*transform.forward - transform.up)/2)*distanceToObstacle[8], Color.red);
            rayDown30 = true;
        }
        else {
        	rayDown30 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, (transform.forward + Mathf.Sqrt(3)*transform.up)/2, out hit9, maxLaserDistance)&&hit9.collider.tag == "Obstacles"){
            distanceToObstacle[9] = (transform.position-hit9.point).magnitude;
            Debug.DrawRay(transform.position, ((transform.forward + Mathf.Sqrt(3)*transform.up)/2)*distanceToObstacle[9], Color.red);
            rayUp60 = true; //60
        } 
        else {
        	rayUp60 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, (transform.forward - Mathf.Sqrt(3)*transform.up)/2, out hit10, maxLaserDistance)&&hit10.collider.tag == "Obstacles"){
            distanceToObstacle[10] = (transform.position-hit10.point).magnitude;
            Debug.DrawRay(transform.position, ((transform.forward - Mathf.Sqrt(3)*transform.up)/2)*distanceToObstacle[10], Color.red);
            rayDown60 = true;
        }
        else {
        	rayDown60 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, transform.up, out hit11, maxLaserDistance)&&hit11.collider.tag == "Obstacles"){
            distanceToObstacle[11] = (transform.position-hit11.point).magnitude;
            Debug.DrawRay(transform.position, transform.up*distanceToObstacle[11], Color.red);
            rayUp90 = true; //90
        }
        else {
        	rayUp90 = false;
        }

        if (Physics.SphereCast(transform.position, sphereRadius, -transform.up, out hit12, maxLaserDistance)&&hit12.collider.tag == "Obstacles"){
            distanceToObstacle[12] = (transform.position-hit12.point).magnitude;
            Debug.DrawRay(transform.position, -transform.up*distanceToObstacle[12], Color.red);
            rayDown90 = true;
        }
        else {
        	rayDown90 = false;
        }

        for (int i = 0; i < 13; i++)
        {
            if (distanceToObstacle[i] < minDistanceToObstacle){
                minDistanceToObstacle = distanceToObstacle[i];
            }
        }
    }

    void LimitHeight()
    {
        tempY = Mathf.Clamp(transform.position.y, 20.0f, 100.0f);
        tempZ = Mathf.Clamp(transform.position.z, -195f, 195f);
        tempX = Mathf.Clamp(transform.position.x, -195f, 195f);
        transform.position = new Vector3(tempX, tempY, tempZ);
    }

    # region WRITE DRONE'S POSITIONS INTO CSV FILE

    private StreamWriter Write(string path)
    {
        if (path == null)
            return null;
        if (!File.Exists(path))
            File.CreateText(path);
        return new StreamWriter(path);
    }

    public void WriteCsv(string[] strs, string path)
    {
        // // clear
        // Stream fs = File.Open(path, FileMode.Open,FileAccess.ReadWrite, FileShare.Read);
        // fs.SetLength(0);
        // fs.Close();
        // write
        StreamWriter stream = Write(path);
        for (int i = 0; i < 10000; i++)
        {
            if (strs[i] != null)
                stream.WriteLine($"{(i + 1).ToString()},{strs[i]}");
        }
        stream.Close();
        stream.Dispose();
    }

    # endregion

    public void GetVelocity()
    {
        velocity = rBody.velocity.magnitude;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacles")){
            runIntoObstacle = true;
        }
    }

    void OnCollisionExit(Collision collision) 
    {
        if (collision.gameObject.CompareTag("Obstacles")){
            runIntoObstacle = false;
        }
    }


    public override void OnEpisodeBegin()
    {
        runIntoObstacle = false;
        // findObstacle = false;

		rayR90 = false;
		rayL90 = false;
		rayR60 = false;
		rayL60 = false;
		rayR30 = false;
		rayL30 = false;
		rayF = false;
		rayUp30 = false;
		rayDown30 = false;
		rayUp60 = false;
		rayDown60 = false;
		rayUp90 = false;
		rayDown90 = false;

        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(0,0,0);
        tiltAmountForward = 0;
        tiltAmountSideways = 0;
        currentYRotation = 0;
        // currentXRotation = 0;
        wantedYRotation = 0;
        // wantedXRotation = 0;
        transform.rotation = Quaternion.identity;  // reset angle
        transform.position = new Vector3(Random.Range(-190,190), Random.Range(20,100), Random.Range(-190,-160));
        goal.transform.position = new Vector3(Random.Range(-190,190), Random.Range(30,90), Random.Range(160,190));
        dynamicObstacle1.position = new Vector3(0, 0, 0);
        dynamicObstacle2.position = new Vector3(0, 0, 0);
        positions[0] = goal.transform.position.x.ToString()+","+goal.transform.position.z.ToString();
        position_num = 1;  // the 0th row of positions stores the target location, the UAV location is stored starting from the 1st row
        idx = 0;
        
        // clear the path
        foreach (var clonePath in clonePathList)
        {
            DestroyImmediate(clonePath);
        }
        clonePathList.Clear();

        // generate random obstacles
        foreach (var cloneObj in cloneObjList)
        {
        	DestroyImmediate(cloneObj);
        }
        cloneObjList.Clear();
        int i = 0;
        while (i < 5)
        {
        	flag = true;
        	cloneObj=Instantiate(tag, tag.transform.position = 
        		new Vector3(Random.Range(-185,185), Random.Range(0,130) ,Random.Range(-110,110)), transform.rotation=Quaternion.Euler(0,0,0));
        	for (int j = 0; j < i; j++)
        	{
        		if (Vector3.Distance(cloneObj.transform.position, cloneObjList[j].transform.position) < 20.0f)
        		{
        			DestroyImmediate(cloneObj);
        			flag = false;
        			break;
        		}
        	}
        	if (flag)
        	{
        		cloneObjList.Add(cloneObj);
        		i++;
        	}
        }

        foreach (var cloneObj2 in cloneObjList2)
        {
        	DestroyImmediate(cloneObj2);
        }
        cloneObjList2.Clear();
        int num = 0;
        while (num < 5)
        {
        	flag = true;
        	cloneObj2=Instantiate(tag2, tag2.transform.position =
        		new Vector3(Random.Range(-185,185),Random.Range(0,130),Random.Range(-110,110)), transform.rotation=Quaternion.Euler(0,0,0));
        	for (int j = 0; j < num; j++)
        	{
        		if (Mathf.Abs(cloneObj2.transform.position.z - cloneObjList2[j].transform.position.z) < 20.0f)
        		{
        			DestroyImmediate(cloneObj2);
        			flag = false;
        			break;
        		}
        	}
        	if (flag)
        	{
        		cloneObjList2.Add(cloneObj2);
        		num++;
        	}
        }
        // print(cloneObjList2.Count());
    }

    public override void CollectObservations(VectorSensor sensor)
	{
	    sensor.AddObservation(rayL90);
	    sensor.AddObservation(distanceToObstacle[0] / maxLaserDistance);
	    sensor.AddObservation(rayL60);
	    sensor.AddObservation(distanceToObstacle[1] / maxLaserDistance);
	    sensor.AddObservation(rayL30);
	    sensor.AddObservation(distanceToObstacle[2] / maxLaserDistance);
	    sensor.AddObservation(rayF);
	    sensor.AddObservation(distanceToObstacle[3] / maxLaserDistance);
	    sensor.AddObservation(rayR30);
	    sensor.AddObservation(distanceToObstacle[4] / maxLaserDistance);
	    sensor.AddObservation(rayR60);
	    sensor.AddObservation(distanceToObstacle[5] / maxLaserDistance);
	    sensor.AddObservation(rayR90);
	    sensor.AddObservation(distanceToObstacle[6] / maxLaserDistance);

	    sensor.AddObservation(rayUp90);
	    sensor.AddObservation(distanceToObstacle[11] / maxLaserDistance);
	    sensor.AddObservation(rayUp60);
	    sensor.AddObservation(distanceToObstacle[9] / maxLaserDistance);
	    sensor.AddObservation(rayUp30);
	    sensor.AddObservation(distanceToObstacle[7] / maxLaserDistance);
	    sensor.AddObservation(rayF);
	    sensor.AddObservation(distanceToObstacle[3] / maxLaserDistance);
	    sensor.AddObservation(rayDown30);
	    sensor.AddObservation(distanceToObstacle[8] / maxLaserDistance);
	    sensor.AddObservation(rayDown60);
	    sensor.AddObservation(distanceToObstacle[10] / maxLaserDistance);
	    sensor.AddObservation(rayDown90);
	    sensor.AddObservation(distanceToObstacle[12] / maxLaserDistance);


	    sensor.AddObservation((goal.transform.position.x-transform.position.x) / 390); 
        sensor.AddObservation((goal.transform.position.z-transform.position.z) / 390);
        sensor.AddObservation((goal.transform.position.y-transform.position.y) / 50);

        sensor.AddObservation(rBody.velocity.x / 48);
        sensor.AddObservation(rBody.velocity.z / 48);
        sensor.AddObservation(rBody.velocity.y / 48);

        Vector3 targetDir = goal.transform.position - transform.position;
        angle = Vector3.Angle(targetDir, transform.forward) / 180.0f;
        sensor.AddObservation(angle);

        distanceToTarget = Vector3.Distance(transform.position, goal.transform.position);
        sensor.AddObservation(distanceToTarget / 554); // Sqrt(390^2+390^2+50^2)

        // sensor.AddObservation(findObstacle ? 1 : 0);
	}

    
	public override void OnActionReceived(float[] vectorAction)
	{
        // action
        directionForward = vectorAction[0];
        directionSwerve = vectorAction[1];
        wantedYRotation -= vectorAction[2] * rotateAmountByKeys;
        directionUpDown = vectorAction[3];
        // wantedXRotation -= vectorAction[4] * 1.0f;

        // save drone's positions
        positions[position_num] = transform.position.x.ToString()+","
                                  + transform.position.z.ToString()+","
                                  + rBody.velocity.x.ToString()+","
                                  + rBody.velocity.z.ToString();
        position_num += 1;

	    // reward
        if (distanceToTarget < 10){   
            WriteCsv(positions, dataPath);
            SetReward(50.0f);
            EndEpisode();
            print("success");
        }
        
        SetReward(-0.2f);  // step penalty
        AddReward(-distanceToTarget / 554); // Sqrt(390^2+390^2+50^2)
        AddReward(-0.5f * angle);

        if (minDistanceToObstacle < safeDistance){
            AddReward(-1.0f * (safeDistance - minDistanceToObstacle));
        }
        if (runIntoObstacle){  // collide with obstalces or walls
            AddReward(-50.0f);
            EndEpisode();
            print("collision");
        }
	}

    public override float[] Heuristic()
    {
    	var action = new float[4];

        action[0] = Input.GetAxis("Vertical");
        action[1] = Input.GetAxis("Horizontal");
        if (Input.GetKey(KeyCode.J))
        {
            action[2] = 1f;
        }
        if (Input.GetKey(KeyCode.L))
        {
            action[2] = -1f;
        }

        if (Input.GetKey(KeyCode.I))
        {
        	action[3] = 1f;
        }
        if (Input.GetKey(KeyCode.K))
        {
        	action[3] = -1f;
        }
        // if (Input.GetKey(KeyCode.Y))
        // {
        // 	action[4] = 1f;
        // }
        // if (Input.GetKey(KeyCode.H))
        // {
        // 	action[4] = -1f;
        // }
        return action;

    }
}
