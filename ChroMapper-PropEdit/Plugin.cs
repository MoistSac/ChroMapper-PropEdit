using UnityEngine;
using UnityEngine.SceneManagement;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit {

[Plugin("PropEdit")]
public class Plugin {
	public static MainWindow? main;
	public static SettingsWindow? settings;
	
	[Init]
	private void Init() {
		SceneManager.sceneLoaded += SceneLoaded;
		main = new MainWindow();
		settings = new SettingsWindow();
	}
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex == 1) {
			UI.AddChild(scene.GetRootGameObjects()[0], "PropEdit update check").AddComponent<ChroMapper_PropEdit.Components.UpdateChecker>();
		}
		if (scene.buildIndex == 3) {
			// Map Edit
			var mapEditorUI = Object.FindObjectOfType<MapEditorUI>();
			main?.Init(mapEditorUI);
			settings?.Init(mapEditorUI);
		}
		else {
			main?.Disable();
		}
	}
	
	[Exit]
	private void Exit() {
		
	}
}

}
