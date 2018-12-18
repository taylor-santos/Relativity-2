using System.Collections.Generic;
using UnityEngine;

public class Plane {
	public Vector4 Normal;
	public float Distance;
	public Plane() {
		Normal = Vector4.zero;
		Normal.x = 1;
		Distance = 0;
	}
	public Plane(Vector4 _Normal, float _Distance) {
		Normal = _Normal;
		Distance = _Distance;
	}
}

public class Matrix {
	public Matrix4x4 Transformation;
	public Vector4 Translation;
	public Matrix() {
		Transformation = Matrix4x4.identity;
		Translation = Vector3.zero;
	}
	public Matrix(Matrix4x4 _Transformation, Vector4 _Translation) {
		Transformation = _Transformation;
		Translation = _Translation;
	}
	public Matrix(Matrix4x4 _Transformation) {
		Transformation = _Transformation;
		Translation = Vector4.zero;
	}
	public Matrix Inverse() {
		Matrix4x4 inv = Transformation.inverse;
		return new Matrix(inv, -(inv * Translation));
	}
	public Plane InvTransposePlaneMultiplication(Plane p) {
		Vector4 O = this * (p.Normal * p.Distance);
		Vector4 N = (Transformation.inverse.transpose * p.Normal).normalized;
		float d = Vector4.Dot(O, N);
		return new Plane(N, d);
	}
	public static Matrix operator *(Matrix lhs, Matrix rhs) {
		Matrix ret = new Matrix();
		ret.Transformation = lhs.Transformation * rhs.Transformation;
		ret.Translation.x = lhs.Translation.x + rhs.Translation.x * lhs.Transformation[0, 0] + rhs.Translation.y * lhs.Transformation[0, 1] + rhs.Translation.z * lhs.Transformation[0, 2] + rhs.Translation.w * lhs.Transformation[0, 3];
		ret.Translation.y = lhs.Translation.y + rhs.Translation.x * lhs.Transformation[1, 0] + rhs.Translation.y * lhs.Transformation[1, 1] + rhs.Translation.z * lhs.Transformation[1, 2] + rhs.Translation.w * lhs.Transformation[1, 3];
		ret.Translation.z = lhs.Translation.z + rhs.Translation.x * lhs.Transformation[2, 0] + rhs.Translation.y * lhs.Transformation[2, 1] + rhs.Translation.z * lhs.Transformation[2, 2] + rhs.Translation.w * lhs.Transformation[2, 3];
		ret.Translation.w = lhs.Translation.w + rhs.Translation.x * lhs.Transformation[3, 0] + rhs.Translation.y * lhs.Transformation[3, 1] + rhs.Translation.z * lhs.Transformation[3, 2] + rhs.Translation.w * lhs.Transformation[3, 3];
		return ret;
	}
	public static Vector4 operator *(Matrix lhs, Vector4 rhs) {
		return lhs.Translation + lhs.Transformation * rhs;
	}
}

public class Relativity_Controller : MonoBehaviour {
	public GameObject Observer;
	public Vector3 InitialVelocity; //Velocity relative to coordinate frame
	public float ProperTimeOffset;
	public List<Vector3> ProperAccelerations;
	public List<float> AccelerationDurations;
	public List<Vector3> AccelerationOffsets;
	public Vector4 CurrentEvent;
	public float ProperTime;
	public Vector3 Velocity;

	private Relativity_Observer observerScript;
	private GameObject plane;

	void Start() {
		observerScript = Observer.GetComponent<Relativity_Observer>();
		plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.name = name + " Simultaneity Plane";
	}

	void FixedUpdate() {
		float observerTime = observerScript.CoordinateTime;
		CurrentEvent = GetState(observerTime);
		float di = 0.01f;
		Vector3 A = observerScript.accelerations[0];
		for (float i = 0; i+di < Sinh(observerScript.durations[0] * A.magnitude)/A.magnitude; i += di) {
			Vector3 d1 = A * (-1 + Mathf.Sqrt(1 + i * i * A.sqrMagnitude)) / A.sqrMagnitude;
			Vector3 d2 = A * (-1 + Mathf.Sqrt(1 + (i+di) * (i+di) * A.sqrMagnitude)) / A.sqrMagnitude;
			d1.z += i;
			d2.z += i + di;
			Debug.DrawLine(d1, d2);
		}
	}

	Vector4 GetState(float observerTime) {
		Vector4 localObserverEvent = Vector4.zero;
		Matrix coordinateToObserverBoost = new Matrix(LorentzBoost(observerScript.velocity), CombineTemporalAndSpatial(0, -Observer.transform.position));
		Matrix observerToCoordinateBoost = coordinateToObserverBoost.Inverse();
		if (observerTime > 0) {
			for (int i = 0; i < observerScript.accelerations.Count; i++) {
				Vector3 A = observerScript.accelerations[i];
				float T = observerScript.durations[i];
				if (observerTime < GetTemporalComponent(localObserverEvent) + T) {
					T = observerTime - GetTemporalComponent(localObserverEvent);
					float t = Sinh(T * A.magnitude) / A.magnitude;
					Debug.Log("t = " + t);
					Debug.Log("T = " + T);
					Vector3 velocity = A * t / Mathf.Sqrt(1 + t * t * A.sqrMagnitude);
					float gamma = Mathf.Sqrt(1 + t * t * A.sqrMagnitude);
					Debug.Log("y = " + gamma);
					Vector3 displacement = A * (Mathf.Sqrt(1 + t * t * A.sqrMagnitude) - 1) / A.sqrMagnitude;
					Debug.Log("d = " + displacement.x);
					Vector3 x = A * Mathf.Sqrt(1 + t * t * A.sqrMagnitude) / A.sqrMagnitude;
					Debug.Log("ds2 = " + (-t * t + x.x * x.x)); // = (1/A)^2
					Matrix M = new Matrix(LorentzBoost(velocity), CombineTemporalAndSpatial(-t + T, displacement));
					coordinateToObserverBoost *= M;
					observerToCoordinateBoost = coordinateToObserverBoost.Inverse();	
				}
			}
		}
		localObserverEvent = CombineTemporalAndSpatial(observerTime, Vector3.zero);
		Plane simulPlane = new Plane {
			Distance = observerTime
		};

		Vector4 coordinateObserverEvent = observerToCoordinateBoost * localObserverEvent;
		Debug.Log(coordinateObserverEvent.x + ", " + coordinateObserverEvent.y + ", " + coordinateObserverEvent.z + ", " + coordinateObserverEvent.w);
		Debug.Log(coordinateToObserverBoost * coordinateObserverEvent);
		simulPlane = observerToCoordinateBoost.InvTransposePlaneMultiplication(simulPlane);
		Vector3 normal = GetSpatialComponent(simulPlane.Normal);
		normal.z += GetTemporalComponent(simulPlane.Normal);
		float di = 10;
		for (float i = -10; i < 10; i += di) {
			Vector4 e1 = observerToCoordinateBoost * CombineTemporalAndSpatial(i + observerTime, Vector3.zero);
			Vector4 e2 = observerToCoordinateBoost * CombineTemporalAndSpatial(i + di + observerTime, Vector3.zero);
			Vector3 v1 = GetSpatialComponent(e1);
			v1.z += GetTemporalComponent(e1);
			Vector3 v2 = GetSpatialComponent(e2);
			v2.z += GetTemporalComponent(e2);
			Debug.DrawLine(v1, v2, Random.ColorHSV());
		}
		plane.transform.up = -normal;
		plane.transform.position = normal.normalized * simulPlane.Distance;
		return Vector4.zero;
	}

	Vector3 VelocityToProper(Vector3 v) {
		return v / Mathf.Sqrt(1f - v.sqrMagnitude);
	}

	Vector3 ProperToVelocity(Vector3 v) {
		return v / Mathf.Sqrt(1f + v.sqrMagnitude);
	}

	Vector3 AddVelocity(Vector3 v, Vector3 u) {
		//Einstein Velocity Addition
		if (v.sqrMagnitude == 0)
			return u;
		if (u.sqrMagnitude == 0)
			return v;
		float gamma = Lorentz(v);
		return 1f / (1 + Vector3.Dot(u, v)) * (v + u * InvLorentz(v) + gamma / (1f + gamma) * Vector3.Dot(u, v) * v);
	}

	Vector3 AddProperVelocity(Vector3 v, Vector3 u) {
		float Bu = 1 / Mathf.Sqrt(1f + u.sqrMagnitude);
		float Bv = 1 / Mathf.Sqrt(1f + v.sqrMagnitude);
		return v + u + (Bv / (1 + Bv) * Vector3.Dot(v, u) + (1 - Bu) / Bu) * v;
	}

	float Lorentz(Vector3 v) {
		//Lorentz Factor
		return 1f / Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float InvLorentz(Vector3 v) {
		//Reciprocal Lorentz Factor
		return Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float Sinh(float x) {
		return (Mathf.Pow(Mathf.Exp(1), x) - Mathf.Pow(Mathf.Exp(1), -x)) / 2;
	}

	float Cosh(float x) {
		return (Mathf.Pow(Mathf.Exp(1), x) + Mathf.Pow(Mathf.Exp(1), -x)) / 2;
	}

	float Tanh(float x) {
		return Sinh(x) / Cosh(x);
	}

	float ASinh(float x) {
		return Mathf.Log(x + Mathf.Sqrt(1 + Mathf.Pow(x, 2)));
	}

	float ACosh(float x) {
		return Mathf.Log(x + Mathf.Sqrt(Mathf.Pow(x, 2) - 1));
	}

	float ATanh(float x) {
		return (Mathf.Log(1 + x) - Mathf.Log(1 - x)) / 2;
	}

	Matrix4x4 LorentzBoost(Vector3 v) {
		//Computes the Lorentz Boost for a given 3-velocity
		float βSqr = v.sqrMagnitude;
		if (βSqr == 0f) {
			return Matrix4x4.identity;
		}
		float βx = v.x;
		float βy = v.y;
		float βz = v.z;
		float gamma = Lorentz(v);
		Matrix4x4 boost = new Matrix4x4(
			new Vector4(gamma, -gamma * βx, -gamma * βy, -gamma * βz),
			new Vector4(-βx * gamma, (gamma - 1) * (βx * βx) / (βSqr) + 1, (gamma - 1) * (βx * βy) / (βSqr), (gamma - 1) * (βx * βz) / (βSqr)),
			new Vector4(-βy * gamma, (gamma - 1) * (βy * βx) / (βSqr), (gamma - 1) * (βy * βy) / (βSqr) + 1, (gamma - 1) * (βy * βz) / (βSqr)),
			new Vector4(-βz * gamma, (gamma - 1) * (βz * βx) / (βSqr), (gamma - 1) * (βz * βy) / (βSqr), (gamma - 1) * (βz * βz) / (βSqr) + 1)
		);
		return boost;
	}

	Vector4 CombineTemporalAndSpatial(float t, Vector3 p) {
		return new Vector4(t, p.x, p.y, p.z);
	}

	float GetTemporalComponent(Vector4 v) {
		return v.x;
	}

	Vector3 GetSpatialComponent(Vector4 v) {
		return new Vector3(v.y, v.z, v.w);
	}

	Vector3 AddTimeToZAxis(Vector4 e, float t) {
		//Add new z value to vector v's z-axis
		Vector3 v = GetSpatialComponent(e);
		v.z += GetTemporalComponent(e) + t;
		return v;
	}
}