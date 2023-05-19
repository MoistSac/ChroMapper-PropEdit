using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

using SimpleJSON;

namespace ChroMapper_PropEdit.Components {

public class UpdateChecker : MonoBehaviour {
	public readonly string VERSIONS_URL = "https://raw.githubusercontent.com/MoistSac/ChroMapper-PropEdit/versions/versions.json";
	
	public void Awake() {
		StartCoroutine(CheckVersion());
	}
	
	public IEnumerator CheckVersion() {
		var channel = (global::Settings.Instance.ReleaseChannel == 1) ? "dev" : "stable";
		
		using (var request = UnityWebRequest.Get(VERSIONS_URL)) {
			yield return request.SendWebRequest();
			
			var json = JSON.Parse(request.downloadHandler.text);
			
			if (json == null) {
				yield break;
			}
			
			var current = typeof(UpdateChecker).Assembly.GetName().Version;
			var latest = new Version(json[channel]["latest"]);
			
			if (latest > current) {
				Debug.LogError($"New PropEdit version {latest} available, check Discord for link.");
			}
		}
	}
}

}
