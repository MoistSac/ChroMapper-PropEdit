// Vaguely based on PaulMapper

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Beatmap.Enums;

namespace ChroMapper_PropEdit.Components {

public class AxisMovement : MonoBehaviour
{
	// onMove(delta)
	public event Action<Axis>? onDragBegin;
	public event Action<Vector3, Axis>? onDragMove;
	public event Action? onDragEnd;
	
	public Axis axis;
	
	private Plane _plane;
	
	private Vector3 _prev;
	private Vector3 _axisv;
	
	public AxisMovement() { }
	
	public void OnEnable() { }
	
	public void OnDisable() {
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(AxisMovement), ActionMapsDisabled);
	}
	
	void OnMouseEnter() {
		CMInputCallbackInstaller.DisableActionMaps(typeof(AxisMovement), ActionMapsDisabled);
	}
	void OnMouseExit() {
		CMInputCallbackInstaller.ClearDisabledActionMaps(typeof(AxisMovement), ActionMapsDisabled);
	}
	
	void OnMouseDown() {
		// Get a plane along axis line and facing the camera
		var cd = (Camera.main.transform.position - this.gameObject.transform.position);
		// The axis part of the normal needs to be 0
		_axisv = this.gameObject.transform.parent.rotation * vectors[axis];
		var normal = cd - Vector3.Project(cd, _axisv);
		_plane = new Plane(normal, this.gameObject.transform.position);
		if (axis_pos() is Vector3 pos) {
			_prev = pos;
			onDragBegin?.Invoke(axis);
		}
	}
	
	void OnMouseDrag() {
		if (axis_pos() is Vector3 pos) {
			onDragMove?.Invoke(pos - _prev, axis);
		}
	}
	
	void OnMouseUp() {
		onDragEnd?.Invoke();
	}
	
	private Vector3? axis_pos() {
		var ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
		float depth = 0;
		if (_plane.Raycast(ray, out depth)) {
			var plane_pos = ray.GetPoint(depth);
			// Get only axis movement, and convert to local
			return Quaternion.Inverse(this.gameObject.transform.parent.rotation) * Vector3.Project(plane_pos, _axisv);
		}
		return null;
	}
	
	private readonly System.Type[] ActionMapsDisabled = {
		typeof(CMInput.IPlacementControllersActions)
	};
	
	public static readonly Dictionary<Axis, Vector3> vectors = new Dictionary<Axis, Vector3>() {
		{Axis.X, Vector3.right},
		{Axis.Y, Vector3.up},
		{Axis.Z, Vector3.forward}
	};
}

}
