using UnityEngine;
using System.Collections;

public class Relativity_ClockController : MonoBehaviour {
	public GameObject SecondHand;
	public GameObject MinuteHand;
	public GameObject TextMeshObject;
	public double time;
	public bool pauseAtTime = false;
	public double pauseTime;
	//private Relativity_Rigidbody rb;
	private Relativity_Controller RC;
	private Vector3 offset;
	// Use this for initialization
	void Start () {
		RC = GetComponent<Relativity_Controller>();
		time = RC.ProperTime;
		if (TextMeshObject != null)
			offset = TextMeshObject.transform.position - transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (time < pauseTime && RC.ProperTime >= pauseTime && pauseAtTime)
		{
			RC.Observer.GetComponent<Relativity_Observer>().FreezeTime = true;
		}
		time = RC.ProperTime;
		double sec = time/60 % 1;
		double min = time/(60*12);
		if (SecondHand != null)
			SecondHand.transform.localEulerAngles = new Vector3(0,0,(float)sec*360f);
		if (MinuteHand != null)
			MinuteHand.transform.localEulerAngles = new Vector3(0,0,(float)min*360f);
		if (TextMeshObject != null){
			TextMeshObject.GetComponent<TextMesh>().text = ((float)time).ToString("F2") + "s";
			TextMeshObject.transform.position = new Vector3(RC.CurrentEvent.y, RC.CurrentEvent.z, RC.CurrentEvent.w) + offset;
		}

	}
}
