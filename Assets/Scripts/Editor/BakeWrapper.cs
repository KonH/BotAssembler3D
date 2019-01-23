using System.Collections.Generic;
using System.Linq;
using BotAssembler.Components;
using Unity.EditorCoroutines.Editor;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BotAssembler.Editor {
	public static class BakeWrapper {
		static BakeUtility _utility = null;

		[MenuItem("GameObject/Wrap", false, 0)]
		public static void Bake() {
			var go = Selection.activeGameObject;
			if ( !go ) {
				return;
			}
			_utility = new BakeUtility(go);
			EditorCoroutineUtility.StartCoroutineOwnerless(_utility.Bake());
		}
	}
}