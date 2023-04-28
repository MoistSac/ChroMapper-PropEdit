using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Enums;
using ChroMapper_PropEdit.Utils;
using SimpleJSON;

namespace ChroMapper_PropEdit.UserInterface {

public class SettingsController {
	public Window? window;
	public GameObject? panel;
	public GameObject? requirements_panel;
	public GameObject? settings_panel;
	public GameObject? current_panel;
	public ScrollBox? scrollbox;
	
	public Toggle? chroma_enable;
	public Toggle? noodle_enable;
	public Toggle? split_value;
	public UITextInput? gizmo_precision;
	
	public Dictionary<string, UIDropdown> requirements = new Dictionary<string, UIDropdown>();
	public Dictionary<string, Toggle> forced = new Dictionary<string, Toggle>();
	public Dictionary<string, Type> default_reqchecks = new Dictionary<string, Type>();
	public HashSet<RequirementCheck>? requirementsAndSuggestions;
	
	public void Init(MapEditorUI mapEditorUI) {
		var parent = mapEditorUI.MainUIGroup[5];
		
		window = Window.Create("Settings", "Settings", parent.transform, size: new Vector2(200, 80));
		window.onShow += OnResize;
		window.onResize += OnResize;
		
		{
			var button = UI.AddButton(window.title!, "Close", ToggleWindow);
			UI.AttachTransform(button.gameObject, pos: new Vector2(-35, -14), size: new Vector2(50, 30), anchor_min: new Vector2(1, 1), anchor_max: new Vector2(1, 1));
		}
		
		var window_content = UI.AddChild(window.gameObject, "Settings Window Content");
		UI.AttachTransform(window_content, new Vector2(-10, -40), new Vector2(0, -15), new Vector2(0, 0), new Vector2(1, 1));
		{
			var image = window_content.AddComponent<Image>();
			image.sprite = PersistentUI.Instance.Sprites.Background;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.1f, 0.1f, 0.1f, 1);
		}
		var scroll_area = UI.AddChild(window_content, "Scroll Area");
		UI.AttachTransform(scroll_area, new Vector2(0, -10), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1));
		scrollbox = scroll_area.AddComponent<ScrollBox>().Init(scroll_area.transform);
		panel = scrollbox.content;
		UI.AttachTransform(panel!, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1));
		
		
		UI.AddLabel(panel!.transform, "PropEdit", "PropEdit Settings", Vector2.zero);
		{
			var container = UI.AddField(panel, "Show Chroma");
			chroma_enable = UI.AddCheckbox(container, false, (v) => {
				Settings.Set(Settings.ShowChromaKey, v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		{
			var container = UI.AddField(panel, "Show Noodle Extensions");
			noodle_enable = UI.AddCheckbox(container, false, (v) => {
				Settings.Set(Settings.ShowNoodleKey, v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		{
			var container = UI.AddField(panel, "Split light values");
			split_value = UI.AddCheckbox(container, false, (v) => {
				Settings.Set(Settings.SplitValue, v);
				Plugin.main?.UpdateSelection(false);
			});
		}
		{
			var collapsible = UI.AddChild(panel, "Gizmos").AddComponent<Collapsible>().Init("Gizmos", true);
			{
				var container = UI.AddField(collapsible.panel!, "Gizmo Precision");
				gizmo_precision = UI.AddParsed<int>(container, 2, (v) => {
					Settings.Set(Settings.GizmoPrecision, v ?? 0);
				});
			}
		}
		
		UI.AddField(panel, "");
		UI.AddLabel(panel!.transform, "Map", "Map Settings", Vector2.zero);
		{
			var collapsible = UI.AddChild(panel, "Requirements").AddComponent<Collapsible>().Init("Requirements", true);
			requirements_panel = collapsible.panel;
			
			foreach (var rc in default_reqchecks) {
				AddReqField(rc.Key, false);
			}
			
			foreach (var req_status in (new Dictionary<string, RequirementCheck.RequirementType>() {{"_requirements", RequirementCheck.RequirementType.Requirement}, {"_suggestions", RequirementCheck.RequirementType.Suggestion}})) {
				if (BeatSaberSongContainer.Instance.DifficultyData.CustomData?[req_status.Key] is JSONArray reqs) {
					foreach (var req in reqs.Children) {
						var reqcheck = GetReqCheck(req);
						if (reqcheck == null) {
							RequirementCheck.RegisterRequirement(new CustomRequirement(req, req_status.Value));
							AddReqField(req, true);
						}
						else {
							if (reqcheck.IsRequiredOrSuggested(BeatSaberSongContainer.Instance.DifficultyData, BeatSaberSongContainer.Instance.Map) != req_status.Value) {
								// Triggers forced
								requirements[req].Dropdown.value = (int)req_status.Value;
							}
						}
					}
				}
			}
		}
		
		{
			var collapsible = UI.AddChild(panel, "Map Options").AddComponent<Collapsible>().Init("Map Options", true);
			{
				var c2 = UI.AddChild(collapsible.panel!, "_playerOptions").AddComponent<Collapsible>().Init("Player Options", true);
				prefix = "_playerOptions";
				current_panel = c2.panel;
				AddDropdown("Left Handed", "_leftHanded", MapSettings.OptionBool);
				AddParsed<float>("Player Height", "_playerHeight");
				AddDropdown("Automatic Player Height", "_automaticPlayerHeight", MapSettings.OptionBool);
				AddParsed<float>("Sfx Volume", "_sfxVolume");
				AddDropdown("Reduce Debris", "_reduceDebris", MapSettings.OptionBool);
				AddDropdown("No Hud", "_noTextsAndHuds", MapSettings.OptionBool);
				AddDropdown("Hide Miss Text", "_noFailEffects", MapSettings.OptionBool);
				AddDropdown("Advanced Hud", "_advancedHud", MapSettings.OptionBool);
				AddDropdown("Auto Restart", "_autoRestart", MapSettings.OptionBool);
				AddParsed<float>("Saber Trail Intensity", "_saberTrailIntensity");
				AddDropdown("Note Jump Duration Type", "_noteJumpDurationTypeSettings", MapSettings.JumpDurationTypes);
				AddParsed<float>("Fixed Note Jump Duration", "_noteJumpFixedDuration");
				AddParsed<float>("Note Jump Offset", "_noteJumpStartBeatOffset");
				AddDropdown("Hide Note Spawn Effect", "_hideNoteSpawnEffect", MapSettings.OptionBool);
				AddDropdown("Adaptive Sfx", "_adaptiveSfx", MapSettings.OptionBool);
				AddDropdown("Expert- Effects Filter", "_environmentEffectsFilterDefaultPreset", MapSettings.EffectsFilters);
				AddDropdown("Expert+ Effects Filter", "_environmentEffectsFilterExpertPlusPreset", MapSettings.EffectsFilters);
			}
			{
				var c2 = UI.AddChild(collapsible.panel!, "_modifiers").AddComponent<Collapsible>().Init("Modifiers", true);
				prefix = "_modifiers";
				current_panel = c2.panel;
				AddDropdown("Energy Type", "_energyType", MapSettings.EnergyTypes);
				AddDropdown("No Fail", "_noFailOn0Energy", MapSettings.OptionBool);
				AddDropdown("Instant Fail", "_instaFail", MapSettings.OptionBool);
				AddDropdown("Fail When Sabers Touch", "_failOnSaberClash", MapSettings.OptionBool);
				AddDropdown("Enabled Obstacle Types", "_enabledObstacleType", MapSettings.ObstacleTypes);
				AddDropdown("Fast Notes", "_fastNotes", MapSettings.OptionBool);
				AddDropdown("Strict Angles", "_strictAngles", MapSettings.OptionBool);
				AddDropdown("Disappearing Arrows", "_disappearingArrows", MapSettings.OptionBool);
				AddDropdown("Ghost Notes", "_ghostNotes", MapSettings.OptionBool);
				AddDropdown("No Bombs", "_noBombs", MapSettings.OptionBool);
				AddDropdown("Song Speed", "_songSpeed", MapSettings.SongSpeeds);
				AddDropdown("No Arrows", "_noArrows", MapSettings.OptionBool);
				AddDropdown("Pro Mode", "_proMode", MapSettings.OptionBool);
				AddDropdown("Zen Mode", "_zenMode", MapSettings.OptionBool);
				AddDropdown("Small Cubes", "_smallCubes", MapSettings.OptionBool);
			}
			{
				var c2 = UI.AddChild(collapsible.panel!, "_environments").AddComponent<Collapsible>().Init("Environments", true);
				prefix = "_environments";
				current_panel = c2.panel;
				AddDropdown("Override Environments", "_overrideEnvironments", MapSettings.OptionBool);
			}
			{
				var c2 = UI.AddChild(collapsible.panel!, "Colors").AddComponent<Collapsible>().Init("Colors", true);
				prefix = "_colors";
				current_panel = c2.panel;
				AddDropdown("Override Colors", "_overrideDefaultColors", MapSettings.OptionBool);
			}
			{
				var c2 = UI.AddChild(collapsible.panel!, "_graphics").AddComponent<Collapsible>().Init("Graphics", true);
				prefix = "_graphics";
				current_panel = c2.panel;
				AddParsed<int>("Mirror Quality", "_mirrorGraphicsSettings");
				AddParsed<int>("Bloom Post Process", "_mainEffectGraphicsSettings");
				AddParsed<int>("Smoke", "_smokeGraphicsSettings");
				AddDropdown("Burn Mark Trails", "_burnMarkTrailsEnabled", MapSettings.OptionBool);
				AddDropdown("Screen Displacement", "_screenDisplacementEffectsEnabled", MapSettings.OptionBool);
				AddParsed<int>("Max Shockwave Particles", "_maxShockwaveParticles");
			}
			{
				var c2 = UI.AddChild(collapsible.panel!, "_chroma").AddComponent<Collapsible>().Init("Chroma", true);
				prefix = "_chroma";
				current_panel = c2.panel;
				AddDropdown("Disable Chroma Events", "_disableChromaEvents", MapSettings.OptionBool);
				AddDropdown("Disable Environment Enhancements", "_disableEnvironmentEnhancements", MapSettings.OptionBool);
				AddDropdown("Disable Note Coloring", "_disableNoteColoring", MapSettings.OptionBool);
				AddDropdown("Force Zen Mode Walls", "_forceZenModeWalls", MapSettings.OptionBool);
			}
		}
		
		
		Refresh();
	}
	
	private void AddDropdown<T>(string name, string path, Map<T?> options) {
		path = $"_settings.{prefix}.{path}";
		var container = UI.AddField(current_panel!, name);
		var node = Data.GetNode(BeatSaberSongContainer.Instance.DifficultyData.CustomData, path);
		var value = (node == null)
			? default(T)!
			: Data.CreateConvertFunc<JSONNode, T>()(node);
		UI.AddDropdown<T>(container, value, (v) => {
			if (v == null) {
				Data.RemoveNode(BeatSaberSongContainer.Instance.DifficultyData.CustomData, path);
			}
			else {
				Data.SetNode(BeatSaberSongContainer.Instance.DifficultyData.CustomData, path, Data.CreateConvertFunc<T, SimpleJSON.JSONNode>()(v));
			}
		}, options, true);
	}
	
	private UITextInput AddParsed<T>(string name, string path) where T : struct {
		path = $"_settings.{prefix}.{path}";
		var container = UI.AddField(current_panel!, name);
		var node = Data.GetNode(BeatSaberSongContainer.Instance.DifficultyData.CustomData, path);
		T? value = (node == null)
			? null
			: Data.CreateConvertFunc<JSONNode, T>()(node);
		return UI.AddParsed<T>(container, value, (v) => {
			if (v == null) {
				Data.RemoveNode(BeatSaberSongContainer.Instance.DifficultyData.CustomData, path);
			}
			else {
				Data.SetNode(BeatSaberSongContainer.Instance.DifficultyData.CustomData, path, Data.CreateConvertFunc<T, SimpleJSON.JSONNode>()((T)v));
			}
		});
	}
	
	private string prefix = "";
	
	private void AddReqField(string name, bool force) {
		var container = UI.AddField(requirements_panel!, name);
		requirements[name] = UI.AddDropdown(container, 0, (v) => {
			SetForced(name, true);
		}, MapSettings.RequirementStatus);
		var container2 = UI.AddField(requirements_panel!, "Override");
		forced[name] = UI.AddCheckbox(container2, force, (v) => {
			// Can't un-force a custom requirement
			if (!default_reqchecks.ContainsKey(name)) {
				v = true;
			}
			SetForced(name, v);
		});
	}
	
	public void Disable() {
		// Restore any requirement checks that were yoted
		requirementsAndSuggestions!.Clear();
		RequirementCheck.RegisterRequirement(new ChromaReq());
		RequirementCheck.RegisterRequirement(new LegacyChromaReq());
		RequirementCheck.RegisterRequirement(new MappingExtensionsReq());
		RequirementCheck.RegisterRequirement(new NoodleExtensionsReq());
		RequirementCheck.RegisterRequirement(new CinemaReq());
		RequirementCheck.RegisterRequirement(new SoundExtensionsReq());
	}
	
	private void SetForced(string name, bool force) {
		requirementsAndSuggestions!.Remove(GetReqCheck(name)!);
		RequirementCheck.RegisterRequirement(force
			? (new CustomRequirement(name, (RequirementCheck.RequirementType)requirements[name].Dropdown.value))
			: ((RequirementCheck)Activator.CreateInstance(default_reqchecks[name])));
		forced[name].isOn = force;
		Refresh();
	}
	
	private RequirementCheck? GetReqCheck(string name) {
		return requirementsAndSuggestions.FirstOrDefault((r) => r.Name == name);
	}
	
	public SettingsController() {
		// Break into ChroMapper's house and grab the requirement check list via reflection
		var req_type = typeof(RequirementCheck);
		var ras = req_type.GetField("requirementsAndSuggestions", BindingFlags.Static | BindingFlags.NonPublic);
		requirementsAndSuggestions = (HashSet<RequirementCheck>)ras.GetValue(null);
		foreach (var rc in requirementsAndSuggestions) {
			default_reqchecks[rc.Name] = rc.GetType();
		}
	}
	
	public void Refresh() {
		chroma_enable!.isOn = Settings.Get(Settings.ShowChromaKey, true);
		noodle_enable!.isOn = Settings.Get(Settings.ShowNoodleKey, true);
		split_value!.isOn = Settings.Get(Settings.SplitValue, true);
		gizmo_precision!.InputField.text = Settings.Get(Settings.GizmoPrecision, 2);
		foreach (var r in requirements) {
			r.Value.Dropdown.SetValueWithoutNotify((int)(GetReqCheck(r.Key)!.IsRequiredOrSuggested(BeatSaberSongContainer.Instance.DifficultyData, BeatSaberSongContainer.Instance.Map)));
		}
	}
	
	private void OnResize() {
		var layout = panel!.GetComponent<LayoutElement>();
		layout!.minHeight = window!.GetComponent<RectTransform>().sizeDelta.y - 40 - 15;
	}
	
	public void ToggleWindow() {
		Refresh();
		window!.Toggle();
	}
}

}
