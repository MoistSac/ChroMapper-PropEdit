using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;

using  ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class PositionGizmo : MonoBehaviour {
	public event Action? onDragBegin;
	public event Action<Vector3, Axis>? onDragMove;
	public event Action? onDragEnd;
	
	public Dictionary<Axis, AxisMovement> handles = new Dictionary<Axis, AxisMovement>();
	
	public PrimitiveType handle_shape = PrimitiveType.Cube;
	
	public bool ball_visible {
		get { return _ball; }
		set {
			_ball = value;
			ball?.SetActive(value);
		}
	}
	private bool _ball = true;
	
	public GameObject? ball;
	
	public void Start() {
		ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		ball.SetActive(_ball);
		ball.transform.parent = this.gameObject.transform;
		ball.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		ball.transform.localPosition = new Vector3();
		AddAxis(Axis.X, new Vector3(0, 0, 90));
		AddAxis(Axis.Y, new Vector3(0, 0, 0));
		AddAxis(Axis.Z, new Vector3(90, 0, 0));
	}
	
	private void AddAxis(Axis axis, Vector3 rot) {
		GameObject handle = GameObject.CreatePrimitive(handle_shape);
		handle.transform.parent = this.gameObject.transform;
		handle.transform.localPosition = AxisMovement.vectors[axis];
		handle.transform.localEulerAngles = rot;
		handle.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
		var am = handle.AddComponent<AxisMovement>();
		am.axis = axis;
		am.onDragBegin += OnDragBegin;
		am.onDragMove += OnDragMove;
		am.onDragEnd += OnDragEnd;
		handles[axis] = am;
	}
	
	void OnDragBegin() {
		onDragBegin?.Invoke();
	}
	
	void OnDragMove(Vector3 delta, Axis axis) {
		onDragMove?.Invoke(delta, axis);
	}
	
	void OnDragEnd() {
		onDragEnd?.Invoke();
	}
	
	
}

}
