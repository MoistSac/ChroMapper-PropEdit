using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;
using Beatmap.Shared;

using SimpleJSON;

using ChroMapper_PropEdit.Components;

namespace ChroMapper_PropEdit {

public class GizmoController {
	public PositionGizmo? position_gizmo;
	public PositionGizmo? scale_gizmo;
	
	public InputAction translate_keybind;
	public InputAction expand_keybind;
	
	public List<BaseGrid> editing = new List<BaseGrid>();
	public List<DragData> queued = new List<DragData>();
	
	//public Bounds? selected_area;
	
	public GizmoController() {
		var map = CMInputCallbackInstaller.InputInstance.asset.actionMaps
			.Where(x => x.name == "Node Editor")
			.FirstOrDefault();
		map.Disable();
		translate_keybind = map.AddAction("Translate Gizmo", type: InputActionType.Button);
		translate_keybind.AddCompositeBinding("ButtonWithOneModifier")
			.With("Modifier", "<Keyboard>/ctrl")
			.With("Button", "<Keyboard>/t");
		translate_keybind.performed += ToggleTranslate;
		translate_keybind.Disable();
		expand_keybind = map.AddAction("Expand Gizmo", type: InputActionType.Button);
		expand_keybind.AddCompositeBinding("ButtonWithOneModifier")
			.With("Modifier", "<Keyboard>/ctrl")
			.With("Button", "<Keyboard>/e");
		expand_keybind.performed += ToggleExpand;
		expand_keybind.Disable();
		map.Enable();
	}
	
	public void Init() {
		var position_gizmo_go = new GameObject("Position Gizmo");
		position_gizmo_go.SetActive(false);
		position_gizmo = position_gizmo_go.AddComponent<PositionGizmo>();
		position_gizmo.handle_shape = PrimitiveType.Cylinder;
		
		position_gizmo.onDragBegin += QueueEditing;
		position_gizmo.onDragMove  += OnTranslateMove;
		position_gizmo.onDragEnd   += OnTranslateEnd;
		
		var scale_gizmo_go = new GameObject("Scale Gizmo");
		scale_gizmo_go.SetActive(false);
		scale_gizmo = scale_gizmo_go.AddComponent<PositionGizmo>();
		scale_gizmo.handle_shape = PrimitiveType.Cube;
		
		scale_gizmo.onDragBegin += QueueEditing;
		scale_gizmo.onDragMove  += OnScaleMove;
		scale_gizmo.onDragEnd   += OnScaleEnd;
		
		SelectionController.SelectionChangedEvent += () =>  UpdateSelection();
		BeatmapActionContainer.ActionCreatedEvent += (_) => UpdateSelection();
		BeatmapActionContainer.ActionUndoEvent += (_) =>    UpdateSelection();
		BeatmapActionContainer.ActionRedoEvent += (_) =>    UpdateSelection();
		
		translate_keybind.Enable();
	}
	
	public void Disable() {
		translate_keybind.Disable();
	}
	
	public void UpdateSelection() {
		editing = SelectionController.SelectedObjects.Where(o => o is BaseGrid).Select(it => (BaseGrid)it).ToList();
		if (editing.Count() == 0) {
			position_gizmo?.gameObject.SetActive(false);
			scale_gizmo?.gameObject.SetActive(false);
		}
		QueueEditing();
	}
	
	void ToggleTranslate(InputAction.CallbackContext _) {
		if (!Input.GetKey(KeyCode.LeftCtrl) && !Input.GetKey(KeyCode.RightCtrl)) {
			return;
		}
		if (!position_gizmo!.gameObject.activeSelf && editing.Count() > 0) {
			ShowGizmo(position_gizmo);
		}
		else {
			position_gizmo.gameObject.SetActive(false);
		}
	}
	void ToggleExpand(InputAction.CallbackContext _) {
		if (!Input.GetKey(KeyCode.LeftCtrl) && !Input.GetKey(KeyCode.RightCtrl)) {
			return;
		}
		if (!scale_gizmo!.gameObject.activeSelf && editing.Count() > 0) {
			ShowGizmo(scale_gizmo);
		}
		else {
			scale_gizmo.gameObject.SetActive(false);
		}
	}
	
	void ShowGizmo(PositionGizmo gizmo) {
		gizmo.gameObject.SetActive(true);
		var f = queued.First();
		
		var selected_area = new Bounds(f.con.gameObject.transform.position, Vector3.zero);
		
		foreach (var d in queued) {
			selected_area.Encapsulate(d.con.gameObject.transform.position);
		}
		
		var atsc = UnityEngine.Object.FindObjectOfType<AudioTimeSyncController>();
		gizmo.gameObject.transform.parent = f.con.gameObject.transform.parent;
		gizmo.gameObject.transform.position = selected_area.center;
		// Window snapping be silly
		gizmo.gameObject.transform.rotation = Quaternion.Euler(0, f.con.gameObject.transform.rotation.eulerAngles.y, 0);
		//position_gizmo.ball_visible = (editing.Count() > 1);
	}
	
	void QueueEditing() {
		queued = editing.Select(o => {
			var collection = BeatmapObjectContainerCollection.GetCollectionForType(o.ObjectType);
			
			collection.LoadedContainers.TryGetValue(o, out var container);
			
			return new DragData(
				o,
				container
			);
		}).ToList();
	}
	
	void OnTranslateMove(Vector3 delta, Axis axis) {
		foreach (var d in queued) {
			var pos = new Vector3(
				d.o.CustomCoordinate?.AsArray?[0] ?? d.o.PosX - 2,
				d.o.CustomCoordinate?.AsArray?[1] ?? d.o.PosY,
				d.o.Time * EditorScaleController.EditorScale);
			pos += delta;
			d.o.CustomCoordinate = new Vector2(pos.x, pos.y);
			d.o.Time = pos.z / EditorScaleController.EditorScale;
			d.con?.UpdateGridPosition();
		}
		position_gizmo!.gameObject.transform.localPosition += delta;
	}
	void OnTranslateEnd() {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var d in queued) {
			d.con.Dragging = false;
			beatmapActions.Add(new BeatmapObjectModifiedAction(d.o, d.o, d.old, $"Moved a {d.o.ObjectType} with gizmo.", true));
		}
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Moved ({queued.Count()}) objects with gizmo."),
			true);
	}
	
	void OnScaleMove(Vector3 delta, Axis axis) {
		foreach (var d in queued) {
			if (d.o is BaseObstacle o) {
				var scale = new Vector3(
					o.CustomSize?.AsArray?[0] ?? o.Width,
					o.CustomSize?.AsArray?[1] ?? o.Height,
					o.CustomSize?.AsArray?[2] ?? o.Duration * 100 * BeatSaberSongContainer.Instance.DifficultyData.NoteJumpMovementSpeed / BeatSaberSongContainer.Instance.Song.BeatsPerMinute);
				var scale_orig = new Vector3(
					((BaseObstacle)d.old).CustomSize?.AsArray?[0] ?? o.Width,
					((BaseObstacle)d.old).CustomSize?.AsArray?[1] ?? o.Height,
					((BaseObstacle)d.old).CustomSize?.AsArray?[2] ?? o.Duration * 100 * BeatSaberSongContainer.Instance.DifficultyData.NoteJumpMovementSpeed / BeatSaberSongContainer.Instance.Song.BeatsPerMinute);
				// 100 = 60 * 5 / 3, which is the conversion scale for some reason
				scale += Vector3.Scale(scale_orig, delta);
				o.CustomSize = new JSONArray();
				o.CustomSize[0] = scale.x;
				o.CustomSize[1] = scale.y;
				o.CustomSize[2] = scale.z;
				d.con?.UpdateGridPosition();
			}
		}
		scale_gizmo!.handles[axis].gameObject.transform.localPosition += delta;
	}
	void OnScaleEnd() {
		var beatmapActions = new List<BeatmapObjectModifiedAction>();
		foreach (var d in queued) {
			d.con.Dragging = false;
			beatmapActions.Add(new BeatmapObjectModifiedAction(d.o, d.o, d.old, $"Scaled a {d.o.ObjectType} with gizmo.", true));
		}
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(beatmapActions, true, false, $"Scaled ({queued.Count()}) objects with gizmo."),
			true);
		
		foreach (var handle in scale_gizmo!.handles) {
			handle.Value.gameObject.transform.localPosition = AxisMovement.vectors[handle.Key];
		}
	}
	
	public struct DragData {
		public BaseGrid o;
		public readonly BaseObject old;
		public ObjectContainer con;
		public DragData(BaseGrid o, ObjectContainer con) {
			this.o = o;
			this.old = BeatmapFactory.Clone(o);
			this.con = con;
			this.con.Dragging = true;
		}
	}
	
	
}

}
