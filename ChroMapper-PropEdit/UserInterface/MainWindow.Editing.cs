using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using Beatmap.V2;
using Beatmap.V2.Customs;
using Beatmap.V3;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.Utils;

using Convert = System.Convert;
using System.Security;
using UnityEngine.Assertions.Must;
using static ChroMapper_PropEdit.UserInterface.TooltipStrings;

namespace ChroMapper_PropEdit.UserInterface {

public partial class MainWindow {
	public readonly string CHROMA_NAME = "Chroma";
	public readonly string NOODLE_NAME = "Noodle Extensions";
		public TooltipStrings tooltip = new TooltipStrings();


		public void UpdateSelection(bool real) { lock(this) {
		scrollbox!.TargetScroll = real ? 1f : scrollbox!.scrollbar!.value;
		
		foreach (Transform child in panel!.transform) {
			GameObject.Destroy(child.gameObject);
		}
		
		editing = SelectionController.SelectedObjects.Select(it => it).ToList();
		
		if (SelectionController.HasSelectedObjects() && editing.Count() > 0) {
			window!.SetTitle($"{SelectionController.SelectedObjects.Count} Items selected");
			
			if (editing.GroupBy(o => o.ObjectType).Count() > 1) {
				UI.AddLabel(panel!, "Unsupported", "Multi-Type Unsupported!", Vector2.zero);
				return;
			}
			
			var o = editing.First();
			var type = o.ObjectType;
			var v2 = (o is V2Object);
			var animation_branch = (System.Type.GetType("Beatmap.Animations.ObjectAnimator, Main, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") is System.Type);
			
			current_panel = panel;
			
			AddParsed("Beat", Data.GetSet<float>("JsonTime"), true, tooltip.GetTooltip("Obj", TooltipStrings.Tooltip.Beat));
			
			switch (type) {
				case ObjectType.Note:
					var note = (o as BaseNote)!;
					AddParsed("X", Data.GetSet<int>("PosX"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.X));
					AddParsed("Y", Data.GetSet<int>("PosY"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Y));
					AddDropdown<int?>("Type", Data.GetSet<int>("Type"), Notes.NoteTypes, false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Type));
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections, false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.CutDirection));
					if (!v2) {
						AddParsed("Angle Offset", Data.GetSet<int>("AngleOffset"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.AngleOffset));
					}
					AddLine("");
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Color));
						if (o is V2Note) {
							AddCheckbox("Disable Spawn Effect", Data.CustomGetSet<bool?>("_disableSpawnEffect"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.DisableSpawnEffect));
						}
						else {
							AddCheckbox("Spawn Effect", Data.CustomGetSet<bool?>("spawnEffect"), true, tooltip.GetTooltip("N", TooltipStrings.Tooltip.SpawnEffect));
							AddCheckbox("Disable Debris", Data.CustomGetSet<bool?>("disableDebris"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.DisableSpawnEffect));
						}
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false,tooltip.GetTooltip("N", TooltipStrings.Tooltip.NJS));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.SpawnOffset));
						AddTextbox("Coordinates", Data.CustomGetSetRaw(note.CustomKeyCoordinate), true, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Coordinates));
						AddTextbox("Rotation", Data.CustomGetSetRaw(note.CustomKeyWorldRotation), true, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Rotation));
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(note.CustomKeyLocalRotation), true, tooltip.GetTooltip("N", TooltipStrings.Tooltip.LocalRotation));
						if (o is V2Note) {
							AddParsed("Exact Angle", Data.CustomGetSet<float?>("_cutDirection"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.CutDirection));
							AddCheckbox("Fake", Data.CustomGetSet<bool?>("_fake"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Fake));
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Interactable));
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Flip));
						}
						else {
							if (animation_branch) {
								AddCheckbox("Fake", Data.GetSet<bool>("CustomFake"), null, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Fake));
								AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Uninteractable));
							}
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Beat)); //subject to change
							AddCheckbox("Disable Look", Data.CustomGetSet<bool?>("disableNoteLook"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.DisableLook));
							AddCheckbox("No Badcut Direction", Data.CustomGetSet<bool?>("disableBadCutDirection"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.NoBadcutDirection));
							AddCheckbox("No Badcut Speed", Data.CustomGetSet<bool?>("disableBadCutSpeed"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.NoBadcutSpeed)); //unsure
							AddCheckbox("No Badcut Color", Data.CustomGetSet<bool?>("disableBadCutSaberType"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.NoBadcutColor));
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Flip));
							AddTextbox("Link", Data.CustomGetSet<string>("link"), false, tooltip.GetTooltip("N", TooltipStrings.Tooltip.Link));
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true,tooltip.GetTooltip("N", TooltipStrings.Tooltip.Track)); //prob. needs more info
						AddAnimation(v2);
						current_panel = panel;
					}
					
					break;
				case ObjectType.CustomNote:
					AddLine("Wow, a custom note! How did you do this?");
					break;

				case ObjectType.Arc: {
					AddParsed("Head X", Data.GetSet<int>("PosX"), false, tooltip.GetTooltip("AH", TooltipStrings.Tooltip.X));
					AddParsed("Head Y", Data.GetSet<int>("PosY"), false, tooltip.GetTooltip("AH", TooltipStrings.Tooltip.Y));
					AddDropdown<int?>("Color", Data.GetSet<int>("Color"), Notes.ArcColors, false,tooltip.GetTooltip("A", TooltipStrings.Tooltip.Type));
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections, false, tooltip.GetTooltip("AH", TooltipStrings.Tooltip.CutDirection));
					AddParsed("Head Multiplier", Data.GetSet<float>("HeadControlPointLengthMultiplier"), false, tooltip.GetTooltip("AH", TooltipStrings.Tooltip.Multiplier));
					AddParsed("Tail Beat", Data.GetSet<float>("TailJsonTime"), false, tooltip.GetTooltip("AT", TooltipStrings.Tooltip.Beat)); 
					AddParsed("Tail X", Data.GetSet<int>("TailPosX"), false, tooltip.GetTooltip("AT", TooltipStrings.Tooltip.X));
					AddParsed("Tail Y", Data.GetSet<int>("TailPosY"), false, tooltip.GetTooltip("AT", TooltipStrings.Tooltip.Y));
					AddDropdown<int?>("Tail Direction", Data.GetSet<int>("TailCutDirection"), Notes.CutDirections, false, tooltip.GetTooltip("AT", TooltipStrings.Tooltip.CutDirection));
					AddParsed("Tail Multiplier", Data.GetSet<float>("TailControlPointLengthMultiplier"), false, tooltip.GetTooltip("AT", TooltipStrings.Tooltip.Multiplier));
					AddLine("");
					
					var s = (o as BaseSlider)!;
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip("A", TooltipStrings.Tooltip.Color));
						current_panel = panel;
					}
					
								if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
									AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false, tooltip.GetTooltip("A", TooltipStrings.Tooltip.NJS));
									AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, tooltip.GetTooltip("A", TooltipStrings.Tooltip.SpawnOffset));
									AddTextbox("Head Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true, tooltip.GetTooltip("AH", TooltipStrings.Tooltip.Coordinates));
									AddTextbox("Tail Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true, tooltip.GetTooltip("AT", TooltipStrings.Tooltip.Coordinates));
									AddTextbox("Rotation", Data.CustomGetSetRaw(s.CustomKeyWorldRotation), true, tooltip.GetTooltip("A", TooltipStrings.Tooltip.Rotation));
									AddTextbox("Local Rotation", Data.CustomGetSetRaw(s.CustomKeyLocalRotation), true, tooltip.GetTooltip("A", TooltipStrings.Tooltip.LocalRotation));
						if (v2) {
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true, tooltip.GetTooltip("A", TooltipStrings.Tooltip.Interactable));
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true, tooltip.GetTooltip("A", TooltipStrings.Tooltip.Flip)); //not sure if this works
						}
						else {
							AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false, tooltip.GetTooltip("A", TooltipStrings.Tooltip.Uninteractable));
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false, tooltip.GetTooltip("A", TooltipStrings.Tooltip.DisableGravity));
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true, tooltip.GetTooltip("A", TooltipStrings.Tooltip.Flip));
							AddTextbox("Link", Data.CustomGetSet<string>("link"), false, tooltip.GetTooltip("A", TooltipStrings.Tooltip.Link));
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true, tooltip.GetTooltip("A", TooltipStrings.Tooltip.Track));
						AddAnimation(v2);
						current_panel = panel;
					}
					
				}	break;
				case ObjectType.Chain: {
					AddParsed("Head X", Data.GetSet<int>("PosX"), false, tooltip.GetTooltip("CH", TooltipStrings.Tooltip.X));
					AddParsed("Head Y", Data.GetSet<int>("PosY"), false, tooltip.GetTooltip("CH", TooltipStrings.Tooltip.Y));
					AddDropdown<int?>("Color", Data.GetSet<int>("Color"), Notes.ArcColors, false, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Color));
					AddDropdown<int?>("Direction", Data.GetSet<int>("CutDirection"), Notes.CutDirections, false, tooltip.GetTooltip("C", TooltipStrings.Tooltip.CutDirection));
					AddParsed("Slices", Data.GetSet<int>("SliceCount"), false, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Slices));
					AddParsed("Squish", Data.GetSet<float>("Squish"), false, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Squish));
					AddParsed("Tail X", Data.GetSet<int>("TailPosX"), false, tooltip.GetTooltip("CT", TooltipStrings.Tooltip.X));
					AddParsed("Tail Y", Data.GetSet<int>("TailPosY"), false, tooltip.GetTooltip("CT", TooltipStrings.Tooltip.Y));
					AddLine("");
					
					var s = (o as BaseSlider)!;
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Color));
						current_panel = panel;
					}

					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false, tooltip.GetTooltip("C", TooltipStrings.Tooltip.NJS));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, tooltip.GetTooltip("C", TooltipStrings.Tooltip.SpawnOffset));
						AddTextbox("Head Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true, tooltip.GetTooltip("CH", TooltipStrings.Tooltip.Coordinates));
						AddTextbox("Tail Coordinates", Data.CustomGetSetRaw(s.CustomKeyCoordinate), true, tooltip.GetTooltip("CT", TooltipStrings.Tooltip.Coordinates));
						AddTextbox("Rotation", Data.CustomGetSetRaw(s.CustomKeyWorldRotation), true, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Rotation));
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(s.CustomKeyLocalRotation), true, tooltip.GetTooltip("C", TooltipStrings.Tooltip.LocalRotation));
									if (animation_branch) {
							AddCheckbox("Fake", Data.GetSet<bool>("CustomFake"), null, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Fake));
							AddCheckbox(v2 ? "Interactable" : "Uninteractable", Data.CustomGetSet<bool?>(v2 ? "_interactable" : "uninteractable"), v2, tooltip.GetTooltip("C", (v2 ? TooltipStrings.Tooltip.Interactable : TooltipStrings.Tooltip.Uninteractable)));
						}
						if (v2) {
							AddTextbox("Flip", Data.CustomGetSetRaw("_flip"), true, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Flip));
						}
						else {
							AddCheckbox("Disable Gravity", Data.CustomGetSet<bool?>("disableNoteGravity"), false, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Flip));
							AddTextbox("Flip", Data.CustomGetSetRaw("flip"), true, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Flip));
							AddTextbox("Link", Data.CustomGetSet<string>("link"), false, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Link));
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true, tooltip.GetTooltip("C", TooltipStrings.Tooltip.Track));
						AddAnimation(v2);
						current_panel = panel;
					}
					
				}	break;
				case ObjectType.Obstacle:
					var ob = (o as BaseObstacle)!;
					AddParsed("Duration", Data.GetSet<float>("Duration"), false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Duration));
					if (o is V2Obstacle) {
						AddParsed("X", Data.GetSet<int>("PosX"), false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.X));
						AddParsed("Width", Data.GetSet<int>("Width"), false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Width));
						AddDropdown<int?>("Height", Data.GetSet<int>("Type"), Obstacles.WallHeights, false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Width));
					}
					else {
						AddParsed("X (Left)", Data.GetSet<int>("PosX"), false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.X));
						AddParsed("Y (Bottom)", Data.GetSet<int>("PosY"), false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Y));
						AddParsed("Width", Data.GetSet<int>("Width"), false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Width));
						AddParsed("Height", Data.GetSet<int>("Height"), false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Height));
					}
					AddLine("");
					
					if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
						current_panel = collapsible.panel;
						AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Color));
						current_panel = panel;
					}
					
					if (Settings.Get(Settings.ShowNoodleKey)?.AsBool ?? false) {
						var collapsible = Collapsible.Create(panel, NOODLE_NAME, "Noodle Extensions", true);
						current_panel = collapsible.panel;
						AddParsed("NJS", Data.CustomGetSet<float?>(v2 ? "_noteJumpMovementSpeed" : "noteJumpMovementSpeed"), false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.NJS));
						AddParsed("Spawn Offset", Data.CustomGetSet<float?>(v2 ? "_noteJumpStartBeatOffset" : "noteJumpStartBeatOffset"), false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.SpawnOffset));
						AddTextbox("Coordinates", Data.CustomGetSetRaw(ob.CustomKeyCoordinate), true, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Coordinates));
						AddTextbox("Rotation", Data.CustomGetSetRaw(ob.CustomKeyWorldRotation), true, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Rotation));
						AddTextbox("Local Rotation", Data.CustomGetSetRaw(ob.CustomKeyLocalRotation), true, tooltip.GetTooltip("O", TooltipStrings.Tooltip.LocalRotation));
						AddTextbox("Size", Data.CustomGetSetRaw(ob.CustomKeySize), true, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Size));
						if (animation_branch) {
							AddCheckbox("Fake", Data.GetSet<bool>("CustomFake"), null, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Fake));
						}
						if (o is V2Obstacle) {
							AddCheckbox("Interactable", Data.CustomGetSet<bool?>("_interactable"), true, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Interactable));
						}
						else {
							AddCheckbox("Uninteractable", Data.CustomGetSet<bool?>("uninteractable"), false, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Uninteractable)); // not sure if this means that it will screw up your score
						}
						AddTextbox("Track", Data.CustomGetSetRaw(o.CustomKeyTrack), true, tooltip.GetTooltip("O", TooltipStrings.Tooltip.Track));
						AddAnimation(o is V2Obstacle);
						current_panel = panel;
					}
					
					break;
				case ObjectType.Event: {
					var env = BeatSaberSongContainer.Instance.Song.EnvironmentName;
					var events = editing.Select(o => (BaseEvent)o);
					var f = events.First();
					// Light
					if (events.Where(e => e.IsLightEvent(env)).Count() == editing.Count()) {
						if (Settings.Get(Settings.SplitValue, true)!.AsBool) {
							AddDropdown<int?>("Color", Data.GetSetSplitValue(0b1100), Events.LightColors, false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.EventColor));
							AddDropdown<int?>("Action", Data.GetSetSplitValue(0b0011), Events.LightActions, false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.EventAction));
						}
						else {
							AddDropdown<int?>("Value", Data.GetSet<int>("Value"), Events.LightValues, false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.LegacyEventType));
						}
						AddParsed("Brightness", Data.GetSet<float>("FloatValue"), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.Brightness));
						AddLine("");
						
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddTextbox("LightID", Data.CustomGetSetRaw(f.CustomKeyLightID), true, tooltip.GetTooltip("E", TooltipStrings.Tooltip.LightID));
							AddColor("Color", o.CustomKeyColor, tooltip.GetTooltip("E", TooltipStrings.Tooltip.Color));
							if (events.Where(e => e.IsTransition).Count() == editing.Count()) {
								AddDropdown<string>("Easing",    Data.CustomGetSet<string>(f.CustomKeyEasing), Events.Easings, true, tooltip.GetTooltip("E", TooltipStrings.Tooltip.Easing));
								AddDropdown<string>("Lerp Type", Data.CustomGetSet<string>(f.CustomKeyLerpType), Events.LerpTypes, true, tooltip.GetTooltip("E", TooltipStrings.Tooltip.LerpType)); //Unsure
							}
							if (o is V2Event e) {
								AddCheckbox("V2 Gradient", Data.GetSetGradient(), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.V2Gradient));
								if (e.CustomLightGradient != null) {
									AddParsed("Duration",     Data.CustomGetSet<float?>($"{e.CustomKeyLightGradient}._duration"), false, tooltip.GetTooltip("G", TooltipStrings.Tooltip.Duration));
									AddColor("Start Color", $"{e.CustomKeyLightGradient}._startColor", tooltip.GetTooltip("SG", TooltipStrings.Tooltip.Color));
									AddColor("End Color", $"{e.CustomKeyLightGradient}._endColor", tooltip.GetTooltip("EG", TooltipStrings.Tooltip.Color));
									AddDropdown<string>("Easing",    Data.CustomGetSet<string>($"{e.CustomKeyLightGradient}._easing"), Events.Easings, false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.Easing));
								}
							}
							current_panel = panel;
						}
					}
					// Laser Speeds
					if (events.Where(e => e.IsLaserRotationEvent(env)).Count() == editing.Count()) {
						AddParsed("Speed", Data.GetSet<int>("Value"), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.LaserSpeed));
						AddLine("");
						
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddCheckbox("Lock Rotation", Data.CustomGetSet<bool?> (f.CustomKeyLockRotation), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.LockRotation));
							AddDropdown<int?>("Direction",     Data.CustomGetSet<int?>  (f.CustomKeyDirection), Events.LaserDirection, true, tooltip.GetTooltip("E", TooltipStrings.Tooltip.LaserDirection));
							AddParsed("Precise Speed",   Data.CustomGetSet<float?>(f.CustomKeyPreciseSpeed), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.PreciseSpeed));
							current_panel = panel;
						}
					}
					// Ring Rotation
					if (events.Where(e => e.Type == (int)EventTypeValue.RingRotation).Count() == editing.Count()) {
						AddLine("");
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddTextbox("Filter",     Data.CustomGetSet<string>(f.CustomKeyNameFilter), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.RingFilter));
							if (o is V2Event) {
								AddCheckbox("Reset", Data.CustomGetSet<bool?>("_reset"), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.RingV2Reset));
							}
							AddParsed("Rotation",    Data.CustomGetSet<float?>(f.CustomKeyLaneRotation), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.RingRotation));
							AddParsed("Step",        Data.CustomGetSet<float?>(f.CustomKeyStep), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.RingStep));
							AddParsed("Propagation", Data.CustomGetSet<float?>(f.CustomKeyProp), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.RingPropagation));
							AddParsed("Speed",       Data.CustomGetSet<float?>(f.CustomKeySpeed), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.RingSpeed));
							AddDropdown<int?>("Direction", Data.CustomGetSet<int?>  (f.CustomKeyDirection), Events.RingDirection, true, tooltip.GetTooltip("E", TooltipStrings.Tooltip.RingDirection));
							if (o is V2Event) {
								AddCheckbox("Counter Spin", Data.CustomGetSet<bool?>("_counterSpin"), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.RingV2CounterSpin));
							}
							current_panel = panel;
						}
					}
					// Ring Zoom
					if (events.Where(e => e.Type == (int)EventTypeValue.RingZoom).Count() == editing.Count()) {
						AddLine("");
						if (Settings.Get(Settings.ShowChromaKey)?.AsBool ?? false) {
							var collapsible = Collapsible.Create(panel, CHROMA_NAME, "Chroma", true);
							current_panel = collapsible.panel;
							AddParsed("Step",  Data.CustomGetSet<float?>(f.CustomKeyStep), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.RingZoomStep));
							AddParsed("Speed", Data.CustomGetSet<float?>(f.CustomKeySpeed), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.RingZoomSpeed));
							current_panel = panel;
						}
					}
					// Boost Color
					if (events.Where(e => e.IsColorBoostEvent()).Count() == editing.Count()) {
						AddDropdown<int?>("Color Set", Data.GetSet<int>("Value"), Events.BoostSets, false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.BoostColorSet));
					}
					// Lane Rotations
					if (events.Where(e => e.IsLaneRotationEvent()).Count() == editing.Count()) {
						AddDropdown<int?>("Rotation", Data.GetSet<int>("Value"), Events.LaneRotaions, false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.LaneRotation));
					}
				}	break;
				case ObjectType.CustomEvent: {
					var events = editing.Select(o => (BaseCustomEvent)o);
					var f = events.First();
					
					if (events.Where(e => e.Type == "AnimateTrack").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.Track));
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackRepeat));
						}
						AddLine("");
						AddTextbox("Color", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_color" : "color"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.Color));
						foreach (var property in Events.NoodleProperties) {
							AddTextbox(property.Key, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", property.Value[v2 ? 0 : 1]), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.AnimateTrackDissolve, property.Key, "AnimateTrack"));
						}
						AddTextbox("Time", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_time" : "time"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.AnimateTrackTime));
					}

					if (events.Where(e => e.Type == "AssignPathAnimation").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.Track));
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackRepeat));
						}
						AddLine("");
						AddTextbox("Color", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_color" : "color"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.Color));
						foreach (var property in Events.NoodleProperties) {
							AddTextbox(property.Key, Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", property.Value[v2 ? 0 : 1]), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.Track, property.Key, "AnimateTrack"));
						}
						AddTextbox("Definite Position", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_definitePosition" : "definitePosition"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.AssignPathAnimationDefinitePosition));
					}

					if (events.Where(e => e.Type == "AssignTrackParent").Count() == editing.Count()) {
						AddTextbox("Parent", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_parentTrack" : "parentTrack"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.AssignTrackParentParent));
						AddTextbox("Children", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_childrenTracks" : "childrenTracks"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.AssignTrackChildren));
						AddCheckbox("Keep Position", Data.JSONGetSet<bool?>(typeof(BaseCustomEvent), "Data", v2 ? "_worldPositionStays" : "worldPositionStays"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.AssignTrackKeepPosition));
					}

					if (events.Where(e => e.Type == "AssignPlayerToTrack").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.AssignPlayerToTrackTrack));
						AddTextbox("Target", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_target" : "target"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.AssignPlayerToTrackTarget));
					}

					if (events.Where(e => e.Type == "AssignFogTrack").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.Track));
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackRepeat));
						}
						
						AddParsed("Attenuation", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_attenuation"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.V2AssignFogTrackAttenuation));
						AddParsed("Offset", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_offset"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.V2AssignFogTrackOffset));
						AddParsed("Start Y", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_startY"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.V2AssignFogTrackStartY));
						AddParsed("Height", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", "_height"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.V2AssignFogTrackHeight));
					}

					if (events.Where(e => e.Type == "AnimateComponent").Count() == editing.Count()) {
						AddTextbox("Track", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", v2 ? "_track" : "track"), true, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.Track));
						AddParsed("Duration", Data.JSONGetSet<float?>(typeof(BaseCustomEvent), "Data", v2 ? "_duration" : "duration"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackDuration));
						AddDropdown<string>("Easing", Data.JSONGetSet<string>(typeof(BaseCustomEvent), "Data", v2 ? "_easing" : "easing"), Events.Easings, false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackEasing));
						if (!v2) {
							AddParsed("Repeat", Data.JSONGetSet<int?>(typeof(BaseCustomEvent), "Data", "repeat"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.TrackRepeat));
						}
						//it seems these are only normal json inputs. might have to change the tooltip then.
						AddTextbox("Environment Fog", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "BloomFogEnvironment"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.AnimateComponentBloomFogEnvironment));
						AddTextbox("Tube Bloom Light", Data.JSONGetSetRaw(typeof(BaseCustomEvent), "Data", "TubeBloomPrePassLight"), false, tooltip.GetTooltip("CE", TooltipStrings.Tooltip.AnimateComponentTubeBloomPrePassLight));
					}
					
				}	break;
				case ObjectType.BpmChange:
					AddParsed("BPM", Data.GetSet<float>("Bpm"), false, tooltip.GetTooltip("E", TooltipStrings.Tooltip.BPMChange));
					break;
			}
		}
		else {
			window!.SetTitle("No items selected");
		}
	}}

	private void AddAnimation(bool v2) {
		var CustomKeyAnimation = v2 ? "_animation" : "animation";
		AddCheckbox("Animation", Data.GetSetAnimation(v2), false);
		if (editing.Where(o => o.CustomData?.HasKey(CustomKeyAnimation) ?? false).Count() == editing.Count()) {
			AddCheckbox("  Color", Data.CustomGetSetNode(CustomKeyAnimation+"."+ (v2 ? "_color" : "color"), "[[0,0,0,0,0], [1,1,1,1,0.49]],"), false, tooltip.GetTooltip("Obj", TooltipStrings.Tooltip.Color));
				foreach (var property in Events.NoodleProperties) {
				AddCheckbox("  "+property.Key, Data.CustomGetSetNode(CustomKeyAnimation+"."+ property.Value[v2 ? 0 : 1], property.Value[2]), false, tooltip.GetTooltip("Obj", TooltipStrings.Tooltip.AnimateTrackLocalRotation, property.Key, "AnimateTrack"));
			}
			AddCheckbox("  Definite Position", Data.CustomGetSetNode(CustomKeyAnimation+"."+ (v2 ? "_definitePosition" : "definitePosition"), "[[0,0,0,0], [0,0,0,0.49]]"), true, tooltip.GetTooltip("Obj", TooltipStrings.Tooltip.AssignPathAnimationDefinitePosition));
			AddLine("");
		}
	}
	
	private void AddColor(string label, string key, string tooltip = "") {
		if (Settings.Get(Settings.ColorHex, true)) {
			AddTextbox(label, Data.CustomGetSetColor(key), false, tooltip);
		}
		else {
			AddTextbox(label, Data.CustomGetSetRaw(key), true, tooltip);
		}
	}
}

}
