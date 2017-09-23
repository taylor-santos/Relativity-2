using UnityEngine;
using System.Collections;

public class Relativity_ClockController : MonoBehaviour {
	public GameObject SecondHand;
	public GameObject MinuteHand;
	public GameObject TextMeshObject;
	public double time;
	public bool pauseAtTime = false;
	public double pauseTime;
	private Relativity_Rigidbody rb;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Relativity_Rigidbody>();
		time = rb.Proper_Time;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (time < pauseTime && rb.Proper_Time >= pauseTime && pauseAtTime)
		{
			rb.Observer.GetComponent<Relativity_Observer>().FreezeTime = true;
		}
		time = rb.Proper_Time;
		double sec = time/60 % 1;
		double min = time/(60*12);
		if (SecondHand != null)
			SecondHand.transform.localEulerAngles = new Vector3(0,0,(float)sec*360f);
		if (MinuteHand != null)
			MinuteHand.transform.localEulerAngles = new Vector3(0,0,(float)min*360f);
		if (TextMeshObject != null)
			TextMeshObject.GetComponent<TextMesh>().text = ((float)time).ToString("F2") + "s";

	}
}
