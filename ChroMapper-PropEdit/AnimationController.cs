using System;
using System.Collections.Generic;
using UnityEngine;

using Beatmap.Enums;
using Beatmap.V2;
using SimpleJSON;

using ChroMapper_PropEdit.Components;
using ChroMapper_PropEdit.Utils;

namespace ChroMapper_PropEdit {

public class AnimationController {
	public readonly List<(string, string, Type)> components = new List<(string, string, Type)>() {
		("_animation._localRotation", "animation.localRotation", typeof(AnimateLocalRotation))
	};
	
	public void Init() {
		foreach (ObjectType type in Enum.GetValues(typeof(ObjectType))) {
			var collection = BeatmapObjectContainerCollection.GetCollectionForType(type);
			
			if (collection == null) {
				continue;
			}
			
			collection.ContainerSpawnedEvent += (bo) => {
				var v2 = (bo is V2Object);
				foreach (var c in components) {
					if (Data.GetNode(bo.CustomData, v2 ? c.Item1 : c.Item2) is JSONArray arr) {
						collection.LoadedContainers.TryGetValue(bo, out var container);
						if (container == null) {
							break;
						}
						(container.gameObject.AddComponent(c.Item3) as iAnimateComponent)!.PathAnimation(bo, arr);
					}
				}
			};
		}
	}
};

};
