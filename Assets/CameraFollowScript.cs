using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{

	private Transform rbody;
	void Awake(){
		rbody = GameObject.FindGameObjectWithTag("Player").transform;
	}

	private Vector3 velocityCameraFollow;
	public Vector3 behindPosition = new Vector3(0, 10, -8);
	public float angle;
	void FixedUpdate(){
		transform.position = Vector3.SmoothDamp(transform.position, rbody.transform.TransformPoint(behindPosition) + Vector3.up, ref velocityCameraFollow, 0.1f);
		transform.rotation = Quaternion.Euler(new Vector3(angle, rbody.GetComponent<RocketAgent>().currentYRotation, 0));
	}

}
