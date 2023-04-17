using UnityEngine;
using UnityEngine.SceneManagement;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit {

[Plugin("PropEdit")]
public class Plugin {
	public static MainWindow? main;
	public static SettingsController? settings;
	public static GizmoController? gizmos;
	
	[Init]
	private void Init() {
		SceneManager.sceneLoaded += SceneLoaded;
		main = new MainWindow();
		settings = new SettingsController();
		gizmos = new GizmoController();
	}
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex == 3) {
			// Map Edit
			var mapEditorUI = Object.FindObjectOfType<MapEditorUI>();
			main?.Init(mapEditorUI);
			settings?.Init(mapEditorUI);
			gizmos?.Init();
		}
		else {
			main?.Disable();
			gizmos?.Disable();
		}
	}
	
	[Exit]
	private void Exit() {
		
	}
}

}
