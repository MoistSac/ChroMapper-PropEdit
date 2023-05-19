using System;
using System.Collections.Generic;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Enums;
using SimpleJSON;

namespace ChroMapper_PropEdit.Components {

public class AnimateLocalRotation : MonoBehaviour, iAnimateComponent {
	public SortedSet<PointDefinition> points = new SortedSet<PointDefinition>();
	public AudioTimeSyncController? atsc;
	
	
	
	public iAnimateComponent PathAnimation(BaseObject bo, JSONArray points) {
		atsc = UnityEngine.Object.FindObjectOfType<AudioTimeSyncController>();
		
		// HJD, copied from rynan4818/ChroMapper-HalfJumpDurationMark
		// TODO: object-specific bpn/njs/offset
		var njs = BeatSaberSongContainer.Instance.DifficultyData.NoteJumpMovementSpeed;
		var offset = BeatSaberSongContainer.Instance.DifficultyData.NoteJumpStartBeatOffset;
		var bpm = BeatmapObjectContainerCollection.GetCollectionForType<BPMChangeGridContainer>(ObjectType.BpmChange)
			.FindLastBpm(bo.SongBpmTime)
			.Bpm;
		var _hjd = 4f;
		var num = 60 / bpm;
		while (njs * _hjd * num > 17.999f)
			_hjd /= 2;
		_hjd += offset;
		if (_hjd < 0.25f) _hjd = 0.25f;
		
		// TODO: Combine points from various sources
		foreach (var ai in points) {
			var data = ai.Value.AsArray;
			var scale = (_hjd * 2) + ((bo is BaseObstacle obs) ? obs.Duration : 0);
			
			var pitch = data[0];
			var yaw   = data[1];
			var roll  = data[2];
			// probably wrong
			var time = bo.JsonTime + (data[3] - 0.5f) * scale;
			var easing = data.Count > 4 ? data[4] : null;
			var spline = data.Count > 5 ? data[5] : null;
			
			//Debug.Log($"Point {roll} at t={time}");
			
			this.points.Add(new PointDefinition(pitch, yaw, roll, time, easing, spline));
		}
		return this;
	}
	
	void Update() {
		transform.localEulerAngles = RotationAtTime(atsc!.CurrentBeat);
	}
	
	Func<float, float> easing = Easing.Linear;
	PointDefinition? previous;
	PointDefinition? next;
	
	Vector3 RotationAtTime(float time) {
		previous = null;
		foreach (var point in points) {
			next = point;
			if (time > point.time) {
				previous = point;
				//Debug.Log($"{time} {point.time} {rot}");
			}
			else {
				break;
			}
		}
		if (previous == null) {
			return Vector3.zero;
		}
		if (previous == next) {
			return next.rotation;
		}
		
		var dt = (time - previous.time) * (next.time - previous.time);
		var eased = easing(dt);
		
		return new Vector3(
			previous.rotation.x + (next.rotation.x - previous.rotation.x) * eased,
			previous.rotation.y + (next.rotation.y - previous.rotation.y) * eased,
			previous.rotation.z + (next.rotation.z - previous.rotation.z) * eased
		);
	}
	
	public class PointDefinition : IComparable<PointDefinition> {
		public Vector3 rotation;
		public float time;
		public string? easing;
		public string? spline;
		
		public PointDefinition(float p, float y, float r, float t, string? e, string? s) {
			rotation = new Vector3(p, y, r);
			time = t;
			easing = e;
			spline = s;
		}
		
		public int CompareTo(PointDefinition other) {
			return this.time.CompareTo(other.time);
		}
	};
};

}
