using System.Collections.Generic;
using System;
using UnityEngine;

public class Plane {
	public Vector4d Normal;
	public double Distance;
	public Plane() {
		Normal = new Vector4d {
			t = 1
		};
		Distance = 0;
	}
	public Plane(Vector4d _Normal, double _Distance) {
		Normal = _Normal;
		Distance = _Distance;
	}
}
[Serializable]
public class Vector3d {
	public double x, y, z;

	public double Magnitude() { return Math.Sqrt(x* x + y* y + z* z); }
	public double SqrMagnitude() { return x * x + y * y + z * z; }
	public double this[int index] {
		get {
			switch (index) {
				case 0: return x;
				case 1: return y;
				case 2: return z;
				default: throw new ArgumentOutOfRangeException();
			}
		}
		set {
			switch (index) {
				case 0: x = value; break;
				case 1: y = value; break;
				case 2: z = value; break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}

	public Vector3d() {
		 x = y = z = 0;
	}
	public Vector3d(double x_, double y_, double z_) {
		x = x_;
		y = y_;
		z = z_;
	}
	public Vector3d(Vector3d other) {
		x = other.x;
		y = other.y;
		z = other.z;
	}
	public Vector3d Normalized() {
		double magnitude = Magnitude();
		return new Vector3d(
			x / magnitude,
			y / magnitude,
			z / magnitude
		);
	}
	public static double operator *(Vector3d lhs, Vector3d rhs) {
		return
			lhs.x * rhs.x +
			lhs.y * rhs.y +
			lhs.z * lhs.z;
	}
	public static Vector3d operator +(Vector3d lhs, Vector3d rhs) {
		return new Vector3d {
			x = lhs.x + rhs.x,
			y = lhs.y + rhs.y,
			z = lhs.z + rhs.z
		};
	}
	public static Vector3d operator *(double d, Vector3d v) {
		return new Vector3d {
			x = d * v.x,
			y = d * v.y,
			z = d * v.z
		};
	}
	public static Vector3d operator *(Vector3d v, double d) {
		return d * v;
	}
	public static Vector3d operator /(Vector3d v, double d) {
		return new Vector3d {
			x = v.x / d,
			y = v.y / d,
			z = v.z / d
		};
	}
	public static Vector3d operator -(Vector3d v) {
		return new Vector3d {
			x = -v.x,
			y = -v.y,
			z = -v.z
		};
	}
}
[Serializable]
public class Vector4d {
	public double t, x, y, z;

	public double this[int index] {
		get {
			switch(index) {
				case 0: return t;
				case 1: return x;
				case 2: return y;
				case 3: return z;
				default: throw new ArgumentOutOfRangeException();
			}
		}
		set {
			switch (index) {
				case 0: t = value; break;
				case 1: x = value; break;
				case 2: y = value; break;
				case 3: z = value; break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}

	public double Magnitude() { return Math.Sqrt(t * t + x * x + y * y + z * z); }
	public double SqrMagnitude() { return t * t + x * x + y * y + z * z; }

	public Vector4d() {
		t = x = y = z = 0;
	}
	public Vector4d(double t_, double x_, double y_, double z_) {
		t = t_;
		x = x_;
		y = y_;
		z = z_;
	}
	public Vector4d(Vector4d other) {
		t = other.t;
		x = other.x;
		y = other.y;
		z = other.z;
	}
	public Vector4d Normalized() {
		double magnitude = Magnitude();
		return new Vector4d(
			t / magnitude,
			x / magnitude,
			y / magnitude,
			z / magnitude
		);
	}
	public static double operator* (Vector4d lhs, Vector4d rhs) {
		return
			lhs.t * rhs.t +
			lhs.x * rhs.x +
			lhs.y * rhs.y +
			lhs.z * lhs.z;
	}
	public static Vector4d operator+ (Vector4d lhs, Vector4d rhs) {
		return new Vector4d {
			t = lhs.t + rhs.t,
			x = lhs.x + rhs.x,
			y = lhs.y + rhs.y,
			z = lhs.z + rhs.z
		};
	}
	public static Vector4d operator* (double d, Vector4d v) {
		return new Vector4d {
			t = d * v.t,
			x = d * v.x,
			y = d * v.y,
			z = d * v.z
		};
	}
	public static Vector4d operator *(Vector4d v, double d) {
		return d * v;
	}
	public static Vector4d operator- (Vector4d v) {
		return new Vector4d {
			t = -v.t,
			x = -v.x,
			y = -v.y,
			z = -v.z
		};
	}
}

public class Matrix4d {
	public double
		m00, m10, m20, m30,
		m01, m11, m21, m31,
		m02, m12, m22, m32,
		m03, m13, m23, m33;

	public Matrix4d() { }
	public Matrix4d(Vector4d column0, Vector4d column1, Vector4d column2, Vector4d column3) {
		m00 = column0.t; m01 = column1.t; m02 = column2.t; m03 = column3.t;
		m10 = column0.x; m11 = column1.x; m12 = column2.x; m13 = column3.x;
		m20 = column0.y; m21 = column1.y; m22 = column2.y; m23 = column3.y;
		m30 = column0.z; m31 = column1.z; m32 = column2.z; m33 = column3.z;
	}
	public double this[int row, int column] {
		get {
			return this[row + column * 4];
		}
		set {
			this[row + column * 4] = value;
		}
	}
	public double this[int index] {
		get {
			switch (index) {
				case 0: return m00;
				case 1: return m10;
				case 2: return m20;
				case 3: return m30;
				case 4: return m01;
				case 5: return m11;
				case 6: return m21;
				case 7: return m31;
				case 8: return m02;
				case 9: return m12;
				case 10: return m22;
				case 11: return m32;
				case 12: return m03;
				case 13: return m13;
				case 14: return m23;
				case 15: return m33;
				default:
					throw new IndexOutOfRangeException("Invalid matrix index!");
			}
		}
		set {
			switch (index) {
				case 0: m00 = value; break;
				case 1: m10 = value; break;
				case 2: m20 = value; break;
				case 3: m30 = value; break;
				case 4: m01 = value; break;
				case 5: m11 = value; break;
				case 6: m21 = value; break;
				case 7: m31 = value; break;
				case 8: m02 = value; break;
				case 9: m12 = value; break;
				case 10: m22 = value; break;
				case 11: m32 = value; break;
				case 12: m03 = value; break;
				case 13: m13 = value; break;
				case 14: m23 = value; break;
				case 15: m33 = value; break;

				default:
					throw new IndexOutOfRangeException("Invalid matrix index!");
			}
		}
	}
	public static Matrix4d operator *(Matrix4d lhs, Matrix4d rhs) {
		Matrix4d res = new Matrix4d {
			m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30,
			m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31,
			m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32,
			m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33,

			m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30,
			m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31,
			m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32,
			m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33,

			m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30,
			m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31,
			m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32,
			m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33,

			m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30,
			m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31,
			m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32,
			m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33
		};

		return res;
	}
	public static Vector4d operator *(Matrix4d lhs, Vector4d vector) {
		Vector4d res = new Vector4d {
			t = lhs.m00 * vector.t + lhs.m01 * vector.x + lhs.m02 * vector.y + lhs.m03 * vector.z,
			x = lhs.m10 * vector.t + lhs.m11 * vector.x + lhs.m12 * vector.y + lhs.m13 * vector.z,
			y = lhs.m20 * vector.t + lhs.m21 * vector.x + lhs.m22 * vector.y + lhs.m23 * vector.z,
			z = lhs.m30 * vector.t + lhs.m31 * vector.x + lhs.m32 * vector.y + lhs.m33 * vector.z
		};
		return res;
	}

	public Matrix4d Inverse() {
		Matrix4d result = new Matrix4d();
		result[0] = this[5] * this[10] * this[15] -
			this[5] * this[11] * this[14] -
			this[9] * this[6] * this[15] +
			this[9] * this[7] * this[14] +
			this[13] * this[6] * this[11] -
			this[13] * this[7] * this[10];

		result[4] = -this[4] * this[10] * this[15] +
			this[4] * this[11] * this[14] +
			this[8] * this[6] * this[15] -
			this[8] * this[7] * this[14] -
			this[12] * this[6] * this[11] +
			this[12] * this[7] * this[10];

		result[8] = this[4] * this[9] * this[15] -
			this[4] * this[11] * this[13] -
			this[8] * this[5] * this[15] +
			this[8] * this[7] * this[13] +
			this[12] * this[5] * this[11] -
			this[12] * this[7] * this[9];

		result[12] = -this[4] * this[9] * this[14] +
			this[4] * this[10] * this[13] +
			this[8] * this[5] * this[14] -
			this[8] * this[6] * this[13] -
			this[12] * this[5] * this[10] +
			this[12] * this[6] * this[9];

		result[1] = -this[1] * this[10] * this[15] +
			this[1] * this[11] * this[14] +
			this[9] * this[2] * this[15] -
			this[9] * this[3] * this[14] -
			this[13] * this[2] * this[11] +
			this[13] * this[3] * this[10];

		result[5] = this[0] * this[10] * this[15] -
			this[0] * this[11] * this[14] -
			this[8] * this[2] * this[15] +
			this[8] * this[3] * this[14] +
			this[12] * this[2] * this[11] -
			this[12] * this[3] * this[10];

		result[9] = -this[0] * this[9] * this[15] +
			this[0] * this[11] * this[13] +
			this[8] * this[1] * this[15] -
			this[8] * this[3] * this[13] -
			this[12] * this[1] * this[11] +
			this[12] * this[3] * this[9];

		result[13] = this[0] * this[9] * this[14] -
			this[0] * this[10] * this[13] -
			this[8] * this[1] * this[14] +
			this[8] * this[2] * this[13] +
			this[12] * this[1] * this[10] -
			this[12] * this[2] * this[9];

		result[2] = this[1] * this[6] * this[15] -
			this[1] * this[7] * this[14] -
			this[5] * this[2] * this[15] +
			this[5] * this[3] * this[14] +
			this[13] * this[2] * this[7] -
			this[13] * this[3] * this[6];

		result[6] = -this[0] * this[6] * this[15] +
			this[0] * this[7] * this[14] +
			this[4] * this[2] * this[15] -
			this[4] * this[3] * this[14] -
			this[12] * this[2] * this[7] +
			this[12] * this[3] * this[6];

		result[10] = this[0] * this[5] * this[15] -
			this[0] * this[7] * this[13] -
			this[4] * this[1] * this[15] +
			this[4] * this[3] * this[13] +
			this[12] * this[1] * this[7] -
			this[12] * this[3] * this[5];

		result[14] = -this[0] * this[5] * this[14] +
			this[0] * this[6] * this[13] +
			this[4] * this[1] * this[14] -
			this[4] * this[2] * this[13] -
			this[12] * this[1] * this[6] +
			this[12] * this[2] * this[5];

		result[3] = -this[1] * this[6] * this[11] +
			this[1] * this[7] * this[10] +
			this[5] * this[2] * this[11] -
			this[5] * this[3] * this[10] -
			this[9] * this[2] * this[7] +
			this[9] * this[3] * this[6];

		result[7] = this[0] * this[6] * this[11] -
			this[0] * this[7] * this[10] -
			this[4] * this[2] * this[11] +
			this[4] * this[3] * this[10] +
			this[8] * this[2] * this[7] -
			this[8] * this[3] * this[6];

		result[11] = -this[0] * this[5] * this[11] +
			this[0] * this[7] * this[9] +
			this[4] * this[1] * this[11] -
			this[4] * this[3] * this[9] -
			this[8] * this[1] * this[7] +
			this[8] * this[3] * this[5];

		result[15] = this[0] * this[5] * this[10] -
			this[0] * this[6] * this[9] -
			this[4] * this[1] * this[10] +
			this[4] * this[2] * this[9] +
			this[8] * this[1] * this[6] -
			this[8] * this[2] * this[5];

		double det = this[0] * result[0] + this[1] * result[4] + this[2] * result[8] + this[3] * result[12];

		if (det == 0)
			return new Matrix4d();

		det = 1.0 / det;

		for (int i = 0; i < 16; i++)
			result[i] *= det;

		return result;
	}
	public Matrix4d Transpose() {
		Matrix4d result = new Matrix4d {
			m00 = m00,
			m01 = m10,
			m02 = m20,
			m03 = m30,
			m10 = m01,
			m11 = m11,
			m12 = m21,
			m13 = m31,
			m20 = m02,
			m21 = m12,
			m22 = m22,
			m23 = m32,
			m30 = m03,
			m31 = m13,
			m32 = m23,
			m33 = m33
		};
		return result;
	}

	public static Matrix4d Identity() {
		return new Matrix4d() {
			m00 = 1,
			m11 = 1,
			m22 = 1,
			m33 = 1
		};
	}
}

public class Matrix5d {
	public Matrix4d Transformation;
	public Vector4d Translation;
	public Matrix5d() {
		Transformation = Matrix4d.Identity();
		Translation = new Vector4d();
	}
	public Matrix5d(Matrix4d _Transformation, Vector4d _Translation) {
		Transformation = _Transformation;
		Translation = _Translation;
	}
	public Matrix5d(Matrix4d _Transformation) {
		Transformation = _Transformation;
		Translation = new Vector4d();
	}
	public Matrix5d(Matrix5d other) {
		Transformation = other.Transformation;
		Translation = other.Translation;
	}

	public Matrix5d Inverse() {
		Matrix4d inv = Transformation.Inverse();
		return new Matrix5d(inv, -(inv * Translation));
	}
	public Plane InvTransposePlaneMultiplication(Plane p) {
		Vector4d O = this * (p.Normal.Normalized() * p.Distance);
		Vector4d N = (Transformation.Inverse().Transpose() * p.Normal).Normalized();
		double d = O * N;
		return new Plane(N, d);
	}
	public static Matrix5d operator *(Matrix5d lhs, Matrix5d rhs) {
		Matrix5d ret = new Matrix5d();
		ret.Transformation = lhs.Transformation * rhs.Transformation;
		ret.Translation = lhs.Translation + lhs.Transformation * rhs.Translation;
		return ret;
	}
	public static Vector4d operator *(Matrix5d lhs, Vector4d rhs) {
		return lhs.Translation + lhs.Transformation * rhs;
	}
}

public class Relativity_Controller : MonoBehaviour {
	public GameObject Observer;
	public Vector3d InitialVelocity; //Velocity relative to coordinate frame
	public double ProperTimeOffset;
	public List<Vector3d> ProperAccelerations;
	public List<double> AccelerationDurations;
	public List<Vector3d> AccelerationOffsets;
	public Vector4d CurrentEvent;
	public double ProperTime;
	public Vector3d Velocity;

	private Relativity_Observer observerScript;
	private GameObject plane;

	void Start() {
		observerScript = Observer.GetComponent<Relativity_Observer>();
		plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.name = name + " Simultaneity Plane";
	}

	void FixedUpdate() {
		double observerTime = observerScript.CoordinateTime;
		CurrentEvent = GetState(observerTime);
	}

	Vector4d GetState(double observerTime) {
		Vector4d localObserverEvent = new Vector4d();
		// To transform from observer to coordinate, translate observer's event by observer.position, then apply the lorentz transformation on the resulting event.
		// Therefore, coordinateToObserverBoost = boost (observer.velocity) to observer frame, then translate by -observer position.
		// Then observerToCoordinateBoost = coordinateToObserverBoost.Inverse();
		Vector3d observerVelocity = new Vector3d((double)observerScript.velocity.x, (double)observerScript.velocity.y, (double)observerScript.velocity.z);
		Matrix5d observerToCoordinateBoost = new Matrix5d(LorentzBoost(-observerVelocity)); // coordinateToObserverBoost.Inverse();
		double calculatedTime = 0;
		if (observerTime > 0) {
			for (int i = 0; i < observerScript.accelerations.Count; i++) {
				Vector3d A = new Vector3d((double)observerScript.accelerations[i].x, (double)observerScript.accelerations[i].y, (double)observerScript.accelerations[i].z);
				double ASqr = A.SqrMagnitude();
				double T = observerScript.durations[i];
				bool lastAccel = false;
				if (observerTime < GetTemporalComponent(localObserverEvent) + T) {
					T = observerTime - GetTemporalComponent(localObserverEvent);
					lastAccel = true;
				}
				localObserverEvent = CombineTemporalAndSpatial(T, new Vector3d());
				double aSqr = Math.Pow(A.x, 2) + Math.Pow(A.y, 2) + Math.Pow(A.z, 2);
				double a = Math.Sqrt(aSqr);
				double t = Math.Sinh(T * a) / a;
				Vector3d velocity = A * t / Math.Sqrt(1 + t * t * ASqr);
				Vector3d displacement = A * (Math.Sqrt(1 + t * t * aSqr) - 1) / aSqr;
				Matrix5d M = new Matrix5d(LorentzBoost(velocity), CombineTemporalAndSpatial(-t, displacement));
				Matrix5d MInv = M.Inverse();
				observerToCoordinateBoost *= MInv;
				calculatedTime += T;
				if (lastAccel) break;
			}
		}
		localObserverEvent = CombineTemporalAndSpatial(observerTime - calculatedTime, new Vector3d());
		Plane simulPlane = new Plane {
			Distance = observerTime - calculatedTime
		};
		Vector4d coordinateObserverEvent = observerToCoordinateBoost * localObserverEvent;
		Vector3d coe = GetSpatialComponent(coordinateObserverEvent);
		coe.z += GetTemporalComponent(coordinateObserverEvent);
		Debug.DrawRay(new Vector3((float)coe.x, (float)coe.y, (float)coe.z), Vector3.up, Color.white, 1000);
		simulPlane = observerToCoordinateBoost.InvTransposePlaneMultiplication(simulPlane);
		Vector3d normal = GetSpatialComponent(simulPlane.Normal);
		normal.z += GetTemporalComponent(simulPlane.Normal);
		plane.transform.up = -new Vector3((float)normal.x, (float)normal.y, (float)normal.z);
		Vector3d pos = normal.Normalized() * simulPlane.Distance;
		plane.transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);
		float di = 10;
		for (float i = -10; i < 10; i += di) {
			Vector4d e1 = observerToCoordinateBoost * CombineTemporalAndSpatial(i + observerTime - calculatedTime, new Vector3d());
			Vector4d e2 = observerToCoordinateBoost * CombineTemporalAndSpatial(i + di + observerTime - calculatedTime, new Vector3d());
			Vector3d v1 = GetSpatialComponent(e1);
			v1.z += GetTemporalComponent(e1);
			Vector3d v2 = GetSpatialComponent(e2);
			v2.z += GetTemporalComponent(e2);
			Debug.DrawLine(new Vector3((float)v1.x, (float)v1.y, (float)v1.z), new Vector3((float)v2.x, (float)v2.y, (float)v2.z));
		}
		return new Vector4d();
	}

	Vector3d VelocityToProper(Vector3d v) {
		return v / Math.Sqrt(1.0 - v.SqrMagnitude());
	}

	Vector3d ProperToVelocity(Vector3d v) {
		return v / Math.Sqrt(1.0 + v.SqrMagnitude());
	}

	Vector3d AddVelocity(Vector3d v, Vector3d u) {
		//Einstein Velocity Addition
		if (v.SqrMagnitude() == 0)
			return u;
		if (u.SqrMagnitude() == 0)
			return v;
		double gamma = Lorentz(v);
		return 1.0 / (1 + u * v) * (v + u * InvLorentz(v) + gamma / (1f + gamma) * (u * v) * v);
	}

	Vector3d AddProperVelocity(Vector3d v, Vector3d u) {
		double Bu = 1 / Math.Sqrt(1.0 + u.SqrMagnitude());
		double Bv = 1 / Math.Sqrt(1.0 + v.SqrMagnitude());
		return v + u + (Bv / (1 + Bv) * (v * u) + (1 - Bu) / Bu) * v;
	}

	double Lorentz(Vector3d v) {
		//Lorentz Factor
		return 1.0 / Math.Sqrt(1.0 - v.SqrMagnitude());
	}

	double InvLorentz(Vector3d v) {
		//Reciprocal Lorentz Factor
		return Math.Sqrt(1 - v.SqrMagnitude());
	}

	Matrix4d LorentzBoost(Vector3d v) {
		//Computes the Lorentz Boost for a given 3-velocity
		double βSqr = v.SqrMagnitude();
		if (βSqr == 0f) {
			return Matrix4d.Identity();
		}
		double βx = v.x;
		double βy = v.y;
		double βz = v.z;
		double gamma = Lorentz(v);
		Matrix4d boost = new Matrix4d(
			new Vector4d(gamma, -gamma * βx, -gamma * βy, -gamma * βz),
			new Vector4d(-βx * gamma, (gamma - 1) * (βx * βx) / (βSqr) + 1, (gamma - 1) * (βx * βy) / (βSqr), (gamma - 1) * (βx * βz) / (βSqr)),
			new Vector4d(-βy * gamma, (gamma - 1) * (βy * βx) / (βSqr), (gamma - 1) * (βy * βy) / (βSqr) + 1, (gamma - 1) * (βy * βz) / (βSqr)),
			new Vector4d(-βz * gamma, (gamma - 1) * (βz * βx) / (βSqr), (gamma - 1) * (βz * βy) / (βSqr), (gamma - 1) * (βz * βz) / (βSqr) + 1)
		);
		return boost;
	}

	Vector4d CombineTemporalAndSpatial(double t, Vector3d p) {
		return new Vector4d(t, p.x, p.y, p.z);
	}

	double GetTemporalComponent(Vector4d v) {
		return v.t;
	}

	Vector3d GetSpatialComponent(Vector4d v) {
		return new Vector3d(v.x, v.y, v.z);
	}
}