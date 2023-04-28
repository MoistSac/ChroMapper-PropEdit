using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;

using SimpleJSON;

using  ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class PositionGizmo : MonoBehaviour {
	public event Action<Axis>? onDragBegin;
	public event Action<Vector3, Axis>? onDragMove;
	public event Action? onDragEnd;
	
	public List<DragData> queued = new List<DragData>();
	
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
	
	public PositionGizmo() {
		SelectionController.SelectionChangedEvent += () =>  UpdateSelection();
		BeatmapActionContainer.ActionCreatedEvent += (_) => UpdateSelection();
		BeatmapActionContainer.ActionUndoEvent += (_) =>    UpdateSelection();
		BeatmapActionContainer.ActionRedoEvent += (_) =>    UpdateSelection();
	}
	
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
	
	public void OnEnable() {
		var f = queued.First();
		
		var selected_area = new Bounds(f.con.gameObject.transform.position, Vector3.zero);
		
		foreach (var d in queued) {
			selected_area.Encapsulate(d.con.gameObject.transform.position);
			if (d.o is BaseObstacle) {
				selected_area.Encapsulate(d.con.gameObject.transform.position + d.con.gameObject.transform.localScale);
			}
		}
		
		var atsc = UnityEngine.Object.FindObjectOfType<AudioTimeSyncController>();
		transform.parent = f.con.gameObject.transform.parent;
		transform.position = selected_area.center;
		// Window snapping be silly
		transform.rotation = Quaternion.Euler(0, f.con.gameObject.transform.rotation.eulerAngles.y, 0);
		//position_gizmo.ball_visible = (editing.Count() > 1);
	}
	
	void UpdateSelection() {
		QueueEditing();
		if (queued.Count() == 0) {
			gameObject.SetActive(false);
		}
		else {
			OnEnable();
		}
	}
	
	protected virtual void QueueEditing() {
		queued = SelectionController.SelectedObjects
			.Where(o => o is BaseGrid)
			.Select(it => (BaseGrid)it)
			.Select(o => {
				var collection = BeatmapObjectContainerCollection.GetCollectionForType(o.ObjectType);
				
				collection.LoadedContainers.TryGetValue(o, out var container);
				
				return new DragData(
					o,
					container
				);
			}).ToList();
	}
	
	protected virtual void OnDragBegin(Axis axis) {
		onDragBegin?.Invoke(axis);
	}
	
	protected virtual void OnDragMove(Vector3 delta, Axis axis) {
		onDragMove?.Invoke(delta, axis);
	}
	
	protected virtual void OnDragEnd() {
		onDragEnd?.Invoke();
	}
	
	public struct DragData {
		public BaseGrid o;
		public readonly BaseGrid old;
		public ObjectContainer con;
		public DragData(BaseGrid o, ObjectContainer con) {
			this.o = o;
			this.old = BeatmapFactory.Clone(o);
			this.con = con;
			this.con.Dragging = true;
		}
	}
}

public class TranslationGizmo : PositionGizmo {
	public Vector3 start;
	
	public TranslationGizmo() {
		handle_shape = PrimitiveType.Cylinder;
		
		onDragBegin += OnDragBegin;
		onDragMove  += OnDragMove;
		onDragEnd   += OnDragEnd;
	}
	
	new void OnDragBegin(Axis _) {
		start = transform.localPosition;
	}
	
	new void OnDragMove(Vector3 delta, Axis axis) {
		var precision = Settings.Get(Settings.GizmoPrecision, 0);
		foreach (var d in queued) {
			var pos = new Vector3(
				d.old.CustomCoordinate?.AsArray?[0] ?? d.o.PosX - 2,
				d.old.CustomCoordinate?.AsArray?[1] ?? d.o.PosY,
				d.old.Time * EditorScaleController.EditorScale);
			pos += delta;
			if (precision != 0) {
				pos.x = (float)Math.Round(pos.x * precision) / precision;
				pos.y = (float)Math.Round(pos.y * precision) / precision;
				pos.z = (float)Math.Round(pos.z / EditorScaleController.EditorScale * precision) / precision * EditorScaleController.EditorScale;
			}
			d.o.CustomCoordinate = new Vector2(pos.x, pos.y);
			d.o.Time = pos.z / EditorScaleController.EditorScale;
			d.con?.UpdateGridPosition();
		}
		transform.localPosition = start + delta;
	}
	
	new void OnDragEnd() {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var d in queued) {
			d.con.Dragging = false;
			beatmapActions.Add(new BeatmapObjectModifiedAction(d.o, d.o, d.old, $"Moved a {d.o.ObjectType} with gizmo.", true));
		}
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Moved ({queued.Count()}) objects with gizmo."),
			true);
		OnEnable();
	}
}

public class ScaleGizmo : PositionGizmo {
	public Vector3 start;
	
	public ScaleGizmo() {
		handle_shape = PrimitiveType.Cube;
		
		onDragBegin += OnDragBegin;
		onDragMove  += OnDragMove;
		onDragEnd   += OnDragEnd;
	}
	
	new void OnDragBegin(Axis axis) {
		start = handles[axis].gameObject.transform.localPosition;
	}
	
	new void OnDragMove(Vector3 delta, Axis axis) {
		var precision = Settings.Get(Settings.GizmoPrecision, 0);
		foreach (var d in queued) {
			if (d.o is BaseObstacle o) {
				var scale_orig = new Vector3(
					((BaseObstacle)d.old).CustomSize?.AsArray?[0] ?? o.Width,
					((BaseObstacle)d.old).CustomSize?.AsArray?[1] ?? o.Height,
					((BaseObstacle)d.old).CustomSize?.AsArray?[2] ?? o.Duration * EditorScaleController.EditorScale);
				var pos = new Vector3(
					d.old.CustomCoordinate?.AsArray?[0] ?? d.o.PosX - 2,
					d.old.CustomCoordinate?.AsArray?[1] ?? d.o.PosY,
					d.old.Time * EditorScaleController.EditorScale);
				var scale = scale_orig + Vector3.Scale(scale_orig, delta);
				if (precision != 0) {
					scale.x = (float)Math.Round(scale.x * precision) / precision;
					scale.y = (float)Math.Round(scale.y * precision) / precision;
					scale.z = (float)Math.Round(scale.z * precision) / precision;
				}
				pos -= (scale - scale_orig) / 2;
				o.CustomSize = new JSONArray();
				o.CustomSize[0] = scale.x;
				o.CustomSize[1] = scale.y;
				o.CustomSize[2] = scale.z;
				d.o.CustomCoordinate = new Vector2(pos.x, pos.y);
				d.con?.UpdateGridPosition();
			}
		}
		handles[axis].gameObject.transform.localPosition = start + delta;
	}
	
	new void OnDragEnd() {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var d in queued) {
			d.con.Dragging = false;
			beatmapActions.Add(new BeatmapObjectModifiedAction(d.o, d.o, d.old, $"Scaled a {d.o.ObjectType} with gizmo.", true));
		}
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Scaled ({queued.Count()}) objects with gizmo."),
			true);
		
		foreach (var handle in handles) {
			handle.Value.gameObject.transform.localPosition = AxisMovement.vectors[handle.Key];
		}
		OnEnable();
	}
	
	new void QueueEditing() {
		queued = SelectionController.SelectedObjects
			.Where(o => o is BaseObstacle)
			.Select(it => (BaseObstacle)it)
			.Select(o => {
				var collection = BeatmapObjectContainerCollection.GetCollectionForType(o.ObjectType);
				
				collection.LoadedContainers.TryGetValue(o, out var container);
				
				return new DragData(
					o,
					container
				);
			}).ToList();
	}
}

}
