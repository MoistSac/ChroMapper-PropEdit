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

using ChroMapper_PropEdit.Components;

namespace ChroMapper_PropEdit {

public class GizmoController {
	public TranslationGizmo? position_gizmo;
	public ScaleGizmo? scale_gizmo;
	
	public InputAction translate_keybind;
	public InputAction expand_keybind;
	
	public List<BaseGrid> editing = new List<BaseGrid>();
	
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
		position_gizmo = position_gizmo_go.AddComponent<TranslationGizmo>();
		
		var scale_gizmo_go = new GameObject("Scale Gizmo");
		scale_gizmo_go.SetActive(false);
		scale_gizmo = scale_gizmo_go.AddComponent<ScaleGizmo>();
		
		SelectionController.SelectionChangedEvent += () =>  UpdateSelection();
		BeatmapActionContainer.ActionCreatedEvent += (_) => UpdateSelection();
		BeatmapActionContainer.ActionUndoEvent += (_) =>    UpdateSelection();
		BeatmapActionContainer.ActionRedoEvent += (_) =>    UpdateSelection();
		
		translate_keybind.Enable();
		
		editing = new List<BaseGrid>();
	}
	
	public void Disable() {
		translate_keybind.Disable();
	}
	
	public void UpdateSelection() {
		editing = SelectionController.SelectedObjects.Where(o => o is BaseGrid).Select(it => (BaseGrid)it).ToList();
	}
	
	void ToggleTranslate(InputAction.CallbackContext _) {
		if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) {
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
		if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) {
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
	}
}

}
