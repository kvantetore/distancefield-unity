using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

    public Vector3 RotationSpeed = new Vector3(0, 180, 0);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.localEulerAngles += RotationSpeed * Time.fixedDeltaTime;
	}
}
