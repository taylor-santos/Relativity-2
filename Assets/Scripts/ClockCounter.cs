using UnityEngine;
using System.Collections;

public class ClockCounter : MonoBehaviour {
	public GameObject SecondHand;
	public GameObject MinuteHand;
	public int Count;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float sec = (float)(Count%12);
		float min = (float)(Count)/12;
		if (SecondHand != null)
			SecondHand.transform.localEulerAngles = new Vector3(0,0,((float)sec/12)*360);
		if (MinuteHand != null)
			MinuteHand.transform.localEulerAngles = new Vector3(0,0,(float)(min/12)*360);
	}
}
