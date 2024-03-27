using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using static SwingsPerSecond;
using TMPro;
using UnityEngine;

namespace ChroMapper_PropEdit.UserInterface {
	public class DictionaryFormatter : IFormatProvider, ICustomFormatter {
		private Dictionary<string, string> _dict;
		public DictionaryFormatter(Dictionary<string, string> dict) {
			_dict = dict;
		}

		public string Format(string? format, object? arg, IFormatProvider? formatProvider) {
			var key = arg?.ToString() ?? String.Empty;
			return _dict[key];
		}

		public object? GetFormat(Type? formatType) {
			if (formatType == typeof(ICustomFormatter))
				return this;
			else
				return null;
		}
	}

	public class TooltipStrings {
		private Dictionary<string, string> objects = new Dictionary<string, string>() {
			{ "N", "Note" },
			{ "CN", "CustomNote" },
			{ "A", "Arc" },
			{ "AH", "Arc Head" },
			{ "AT", "Arc Tail" },
			{ "C", "Chain" },
			{ "CH", "Chain Head" },
			{ "CT", "Chain Tail" },
			{ "O", "Obstacle" },
			{ "E", "Event" },
			{ "CE", "CustomEvent" },
			{ "BC", "BpmChange" },
			{ "Obj", "Object"},
			{ "G", "Gradient"},
			{ "SG", "Start Gradient"},
			{ "EG", "End Gradient"},
		};

		public enum Tooltip {
			Beat,
			X,
			Y,
			Type,
			CutDirection,
			AngleOffset,
			Color,
			DisableSpawnEffect,
			DisableDebris,
			SpawnEffect,
			NJS,
			SpawnOffset,
			Coordinates,
			Rotation,
			LocalRotation,
			Fake,
			Flip,
			DisableGravity,
			DisableLook,
			NoBadcutDirection,
			NoBadcutSpeed,
			NoBadcutColor,
			Track,
			Interactable,
			Uninteractable,
			Link,
			Multiplier,
			TailMultiplier,
			Slices,
			Squish,
			Duration,
			Width,
			Height,
			Size,
			EventColor,
			EventAction,
			LegacyEventType,
			Brightness,
			LightID,
			Easing,
			LerpType,
			V2Gradient,
			V2Duration,
		
			LaserSpeed,
			LockRotation,
			LaserDirection,
			PreciseSpeed,


			RingFilter,
			RingRotation,
			RingStep,
			RingPropagation,
			RingSpeed,
			RingDirection,

			RingV2CounterSpin,
			RingV2Reset,


			RingZoomSpeed,
			RingZoomStep,

			BoostColorSet,
			LaneRotation,

			BPMChange,

			TrackDuration,
			TrackEasing,
			TrackProperty,
			TrackRepeat,

			AnimateTrackPosition,
			AnimateTrackRotation,
			AnimateTrackLocalRotation,
			AnimateTrackScale,
			AnimateTrackDissolve,
			AnimateTrackDissolveArrow,
			AnimateTrackInteractable,
			AnimateTrackTime,

			AssignTrackParentParent,
			AssignTrackChildren,
			AssignTrackKeepPosition,


			V2AssignFogTrackAttenuation,
			V2AssignFogTrackOffset,
			V2AssignFogTrackStartY,
			V2AssignFogTrackHeight,
			
			AssignPathAnimationDefinitePosition,


			AssignPlayerToTrackTrack,
			AssignPlayerToTrackTarget,

			//There is another track called AnimateComponent, https://github.com/Aeroluna/Heck/wiki/Environment/#Components
			//https://github.com/Aeroluna/Heck/wiki/Animation#AnimateComponent
			AnimateComponentILightWithId,
			AnimateComponentLightID,
			AnimateComponentType,
			AnimateComponentBloomFogEnvironment,
			AnimateComponentTubeBloomPrePassLight,
			AnimateComponentTubeBloomPrePassLightColorAlphaMultiplier,
			AnimateComponentTubeBloomPrePassLightBloomFogIntensityMultiplier,


		}

		private Dictionary<Tooltip, string> name = new Dictionary<Tooltip, string>() {
			
			//Generic Values
			{ Tooltip.Beat, "A specific point in time, as determined by the BPM of the song, when this {0} should reach the player"},
			{ Tooltip.X, "The horizontal row where the {0} should reside on the grid. The indices run from 0 to 3, with 0 being the left-most lane" },
			{ Tooltip.Y, "The vertical column where the {0} should reside on the grid. The indices run from 0 to 2, with 0 being the bottom-most lane"},
			{ Tooltip.Type, "Indicates which saber should be able to successfully cut the {0}." },
			{ Tooltip.CutDirection, "Indicates the direction the player should swing to successfully cut the {0}." },
			{ Tooltip.AngleOffset, "A value (in degrees) which applies a counter-clockwise rotational offset to the {0}'s cut direction."},

			//Chroma
			{ Tooltip.Color, "Changes the color and opacity of the {0}. Is displayed in Hex colors"},
			{ Tooltip.DisableSpawnEffect, "Disables the light effect, that occurs when the {0} spawns"},
			{ Tooltip.DisableDebris, "Disables the debris, that occurs when the {0} spawns"},

			//Noodle Extensions
			{ Tooltip.SpawnEffect, "Toggles the light spawn effect, that occurs when the {0} spawns"},
			{ Tooltip.NJS, "Changes the note jump speed (NJS) of the {0}"},
			{ Tooltip.SpawnOffset, "Changes the jump distance (JD) of the {0}"},
			{ Tooltip.Coordinates, "Allows to set the coordinates [x,y,z] of the {0}. Keep in mind, that the center [0,0,0] is different from vanilla coordinates"},
			{ Tooltip.Rotation, "Allows to set the global rotation [x,y,z]/[yaw, pitch, roll] of the {0}. [0,0,0] is always the rotation, that faces the player"},
			{ Tooltip.LocalRotation, "Allows to set the local rotation [x,y,z]/[yaw, pitch, roll] of the {0}. This won't affect the direction it spawns from or the path it takes"},
			{ Tooltip.Fake, "If activated, the {0} will not count towards your score or combo, meaning, if you miss it, it won't have any effect"},
			{ Tooltip.Uninteractable, "If activated, the {0} cannot be interacted with / cut through"},
			{ Tooltip.Flip, "Allows you to change how the {0} spawns. [flip line index, flip jump] Flip line index is the initial x the note will spawn at and flip jump is how high (or low) the note will jump up (or down) when flipping to its true position. Base game behaviour will set one note's flip jump to -1 and the other to 1."},
			{ Tooltip.DisableGravity, "If disabled, the {0} will no longer do their animation where they float up"}, //needs change
			{ Tooltip.DisableLook, "If disabled, the {0} will no longer try to face the player as it comes closer to them."},
			{ Tooltip.NoBadcutDirection, "If activated, the {0} cannot be badcut from a wrong direction, meaning it will go straight through"},
			{ Tooltip.NoBadcutSpeed, "If activated, the {0} cannot be badcut with insufficient/slow speed, meaning it will go straight through"},
			{ Tooltip.NoBadcutColor, "If activated, the {0} cannot be badcut with the wrong saber, meaning it will go straight through"},
			{ Tooltip.Track, "Groups the {0} together with different objects, that have the same track name/string"},
			{ Tooltip.Link, "When cut, all {0}'s that share the same link string will also be cut"},

			//V2 Noodle
			{ Tooltip.Interactable, "If deactivated, the {0} cannot be interacted with / cut through"},

			//Arcs
			{ Tooltip.Multiplier, "A value that controls the magnitude of the curve approaching the {0} respectively. \n If the Cut Direction is set to \"Any\"(8), this value is ignored." },

			//Chains
			{ Tooltip.Slices, "An integer value which represents the number of segments in the {0}. The head counts as a segment" },
			{ Tooltip.Squish, "An integer value which represents the proportion of how much of the path from Head (x, y) to Tail(tx, ty) is used by the {0}. This does not alter the shape of the path." },

			//Obstacles
			{ Tooltip.Duration, "A value (in beats) that determines how long the {0} extends for." },
			{ Tooltip.Width, "An integer value which represents how many columns the {0} should take up on the grid."},
			{ Tooltip.Height, "An integer value which represents how many rows the {0} should take up on the grid. The range of acceptable values runs from 1 to 5."},
			{ Tooltip.Size, "The width, height and length of the {0}[w, h, l]. [1, 1, 1] will be perfectly square. Each number is fully optional."},
			
			//Events
			{ Tooltip.EventColor, "Changes the vanilla color of the {0}. Allows to be set to the saber colors, white and off"},
			{ Tooltip.EventAction, "Determines how the {0} should behave"},
			{ Tooltip.LegacyEventType, "Changes the color and behaviour of the {0}"},
			{ Tooltip.Brightness, "Used to control the brightness of the {0}. A value of 0 will turn the light off."},
			{ Tooltip.LightID, "Causes the {0} to only affect the specified ID. Can be an array."},
			{ Tooltip.Easing, "The easing effect that the {0} should use. Check out \"easings.net\" for visualization examples"},
			{ Tooltip.LerpType, "Determines, in what way the color should transition. RGB: changing every value. HSV: primarly changing its hue"},
			{ Tooltip.V2Gradient, "If activated, allows to set a color gradient"},

			//Laser Speeds
			{Tooltip.LaserSpeed, "Sets the speed, the lasers should rotate." },
			{Tooltip.LockRotation, "If activated, The {0} will not reset the previous set laser rotation" },
			{Tooltip.LaserDirection, "Sets the spin direction. CCW -> spins to the left | CW -> spins to the right" }, //DirectionMerge
			{Tooltip.PreciseSpeed, "Sets the speed the lasers should rotate. Allows for decimal values" },

			//Ring Rotation
			{Tooltip.RingFilter, "Causes the {0} to only affect rings with a listed name (e.g. SmallTrackLaneRings, BigTrackLaneRings)" },
			{Tooltip.RingRotation, "Dictates how far the first ring will spin. Allows for decimal values."},
			{Tooltip.RingStep, "Dictates how much rotation is added between each ring. Allows for decimal values."},
			{Tooltip.RingPropagation, "Dictates the rate at which rings behind the first one have physics applied to them. High value makes all rings move simultaneously, low value gives them significant delay. Allows for decimal values." },
			{Tooltip.RingSpeed, "Dictates the speed multiplier of the rings. Allows for decimal values." },
			{Tooltip.RingDirection, "Sets the spin direction. CCW -> spins to the left | CW -> spins to the right" }, //DirectionMerge

			{Tooltip.RingV2CounterSpin, "Causes the smaller ring to spin in the opposite direction" },
			{Tooltip.RingV2Reset, "When activated, resets the rings" },

			//Ring Zoom
			{Tooltip.RingZoomSpeed, "Dictates how quickly it will move to its new position. Allows for decimal values" },
			{Tooltip.RingZoomStep, "Dictates how much position offset is added between each ring. Allows for decimal values" },

			//Boost Color
			{Tooltip.BoostColorSet, "Allows to change the vanilla colors to a different set, specified by the map" },

			//Lane Rotation (90/360 Maps)
			{Tooltip.LaneRotation, "Changes the Rotation of the Beatmap to a different one." },

			//BPM Changes
			{Tooltip.BPMChange, "Alters the BPM to the defined value at the indicated beat." },

			//Custom Events
			{Tooltip.TrackDuration, "The length of the {0} in beats (defaults to 0)" },
			{Tooltip.TrackEasing, "An easing for the animation to follow (defaults to easeLinear)." },
			{Tooltip.TrackProperty, "The property you want to animate." },
			{Tooltip.TrackRepeat, "How many times to repeat this {0} (defaults to 0)" },

			//AnimateTrack
			{Tooltip.AnimateTrackPosition, "Describes the position offset of an object. It will continue any normal movement and have this stacked on top of it." },
			{Tooltip.AnimateTrackLocalRotation, "describes the local rotation offset of an object. This means it is rotated with itself as the origin. Uses euler values. Do note that the note spawn effect will be rotated accordlingly. Notes attempting to look towards the player may look strange, you can disable their look with disableNoteLook." },
			{Tooltip.AnimateTrackRotation, "Describes the world rotation offset of an object. This means it is rotated with the world as the origin. Uses euler values. Think of 360 mode." },
			{Tooltip.AnimateTrackScale, "Decribes the scale of an object. This will be based off their initial size. A scale of 1 is equal to normal size, anything under is smaller, over is larger." },
			{Tooltip.AnimateTrackDissolve, "Controls the dissolve effect on both notes and walls. It's the effect that happens when things go away upon failing a song. Keep in mind that notes and the arrows on notes have seperate dissolve properties, see dissolveArrow. \n Note: How this looks will depend on the player's graphics settings." },
			{Tooltip.AnimateTrackDissolveArrow, "Controls the dissolve effect on the arrows of notes. Similar to the look of the disappearing notes modifier. Has no effect on obstacles" },
			{Tooltip.AnimateTrackInteractable, "Controls whether or not the player can interact with the note/obstacle." },
			{Tooltip.AnimateTrackTime, "lets you control what point in the note's \"lifespan\" it is at a given time. Lifespan examples: 0->note spawns at its earliest point \n 0.25->note is halfway by the player \n 0.5->note is on the platform the player stands \n 0.75-> note about to go \n 1-> Note gone.  It is worth noting that every object on one track will get the same time values when animating this property. This means they would suddenly appear to all be at the same point. It is recommended for every object to have its own track when using _time." },
			
			//AssignTrackParent
			{Tooltip.AssignTrackParentParent, "The track you want to animate." },
			{Tooltip.AssignTrackChildren, "An array of tracks that get inherited by the ParentTrack." },
			{Tooltip.AssignTrackKeepPosition, "If activated, the parent-relative position, scale and rotation are modified such that the object keeps the same world space position, rotation and scale as before." },

			//AssingFogTrack (outdated but used in AnimateComponent)
			{Tooltip.V2AssignFogTrackAttenuation, "controls the fog density. Is calculated logarithmic" },
			{Tooltip.V2AssignFogTrackOffset, "controls the \"offset\" property of fog." },
			{Tooltip.V2AssignFogTrackStartY, "startY is starting Y of the gradient thing." },
			{Tooltip.V2AssignFogTrackHeight, "controls the height is the gradient length of the dissolving plane fog." },

			//AssignPlayerToTrack
			{Tooltip.AssignPlayerToTrackTrack, "The track you wish to assign the player to." },
			{Tooltip.AssignPlayerToTrackTarget, "(optional) The specific player object you wish to target. Available targets are \"Root\", \"Head\", \"LeftHand\", and \"RightHand\"." },


			{Tooltip.AssignPathAnimationDefinitePosition, "Describes the definite position of an object. Will completely overwrite the object's default movement. However, this does take into account lineIndex/lineLayer and world rotation." },


			//AnimateComponent
			{Tooltip.AnimateComponentILightWithId, "Activates the lightIdInput" }, //Unsure
			{Tooltip.AnimateComponentLightID, "Which ID to assign. For use with the lightID tag for lighting events (Not animateable)." },
			{Tooltip.AnimateComponentType, "Which event type to active on. (Not animateable)" }, //unsure, note type or if its obstacle or note?
			{Tooltip.AnimateComponentBloomFogEnvironment, "Activates the BloomFogEnvironment Input. Will always be found on the [0]Environment object" },
			{Tooltip.AnimateComponentTubeBloomPrePassLight, "Activates the TubeBloomPrePassLight input." },
			{Tooltip.AnimateComponentTubeBloomPrePassLightColorAlphaMultiplier, "Changes the multiplier for the color ." }, //Unsure
			{Tooltip.AnimateComponentTubeBloomPrePassLightBloomFogIntensityMultiplier, "Changes the multiplier for the fog intensity" },



			};
		public string GetTooltip(string BSobject, Tooltip property, string foreachString = "", string foreachKey = "") {

			//Normal case
			if (foreachString == "") {
				Console.WriteLine(string.Format(name[property], objects[BSobject]));
				return string.Format(name[property], objects[BSobject]);
			}

			// for foreach cases
			Tooltip tooltipEnum;
			string enumString = foreachKey + foreachString.Replace(" ", "");
			if (Enum.TryParse(enumString, true, out tooltipEnum)) {
				Console.WriteLine(string.Format(name[tooltipEnum], objects[BSobject]));
				return string.Format(name[tooltipEnum], objects[BSobject]);
			}
			else {
				// Handle the case where the string does not match any enum value
				Console.WriteLine($"The string '{enumString}' does not match any Tooltip enum value.");
				return string.Empty; // or return a default value
			}
		}
	}
}
