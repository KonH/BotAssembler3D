using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BotAssembler.Components;
using BotAssembler.Utils;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BotAssembler.Editor {
	public class BakeUtility {
		const float _raycastDistance = 10000;

		RaycastHit[]                  _temp      = new RaycastHit[16];
		List<CompositionRowComponent> _instances = new List<CompositionRowComponent>();

		GameObject _root;
		Collider   _collider;
		Vector3    _origin;

		public BakeUtility(GameObject root) {
			_root     = root;
			_collider = _root.GetComponent<Collider>();
			_origin   = _collider.bounds.center;
		}

		public IEnumerator Bake() {
			yield return FillRow(_origin);
			yield return FillRows(_origin, Vector3.up);
			yield return FillRows(_origin, Vector3.down);
			SetupDelay();
			Undo.RecordObject(_root, "Disable original object");
			_root.SetActive(false);
		}

		IEnumerator FillRow(Vector3 origin, ResultHolder<bool> result = null) {
			var points = new HashSet<Vector2>();
			yield return FillContactsPointsAtSide(origin, Vector3.forward, points);
			yield return FillContactsPointsAtSide(origin, Vector3.back, points);
			yield return FillContactsPointsAtSide(origin, Vector3.left, points);
			yield return FillContactsPointsAtSide(origin, Vector3.right, points);
			var createResult = TryCreateRow(points, origin);
			if ( result != null ) {
				result.Value = createResult;
			}
		}

		bool TryAddContactPoints(Vector3 origin, Vector3 direction, HashSet<Vector2> points) {
			var count = Physics.RaycastNonAlloc(origin - direction * _raycastDistance, direction, _temp, _raycastDistance * 2);
			var result = false;
			for ( var i = 0; i < count; i++ ) {
				var hit = _temp[i];
				if ( hit.collider == _collider ) {
					var point = hit.point;
					points.Add(new Vector2(point.x, point.z));
					result = true;
				}
			}
			
			return result;
		}

		IEnumerator FillContactsPointsAtSide(Vector3 origin, Vector3 direction, HashSet<Vector2> points) {
			if ( TryAddContactPoints(origin, Vector3.forward, points) ) {
				yield return null;
				var leftShift = new Vector3(-direction.z, 0, direction.x);
				yield return FillContactsPointsBeside(origin, direction, leftShift, points);
				var rightShift = new Vector3(direction.z, 0, direction.x);
				yield return FillContactsPointsBeside(origin, direction, rightShift, points);
			}
		}
		
		IEnumerator FillContactsPointsBeside(Vector3 origin, Vector3 direction, Vector3 offset, HashSet<Vector2> points) {
			var acc = origin;
			var maxFails = 10;
			var fails = 0;
			bool found;
			do {
				acc += offset;
				found = TryAddContactPoints(acc, direction, points);
				if ( !found && (fails < maxFails) ) {
					fails++;
					found = true;
				}
				yield return null;
			} while ( found );
		}

		bool TryCreateRow(HashSet<Vector2> cells, Vector3 origin) {
			if ( cells.Count == 0 ) {
				return false;
			}
			var rowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Row.prefab");
			var instance  = (GameObject)PrefabUtility.InstantiatePrefab(rowPrefab, SceneManager.GetActiveScene());
			var name = string.Format("Row({0})", origin.y);
			Undo.RegisterCreatedObjectUndo(instance, "Create " + name);
			Undo.RecordObject(instance, "Setup " + name);
			instance.name               = name;
			instance.transform.position = origin;
			var compositionRow = instance.GetComponent<CompositionRowComponent>();
			compositionRow.Value.Positions.AddRange(cells.Select(vector => new float2(vector.x, vector.y)));
			_instances.Add(compositionRow);
			return true;
		}

		IEnumerator FillRows(Vector3 origin, Vector3 offset) {
			var acc = origin;
			bool filled;
			do {
				acc += offset;
				var filledResult = new ResultHolder<bool>();
				yield return FillRow(acc, filledResult);
				filled = filledResult.Value;
			} while ( filled );
		}

		void SetupDelay() {
			_instances.Sort(CompareByPositionY);
			for ( var i = 0; i < _instances.Count; i++ ) {
				var instance = _instances[i];
				Undo.RecordObject(instance, "Setup delay for " + instance.gameObject.name);
				var compositionRow = instance.Value;
				compositionRow.Delay = i * 0.5f;
				instance.Value = compositionRow;
			}
		}

		static int CompareByPositionY(CompositionRowComponent x, CompositionRowComponent y) {
			return x.transform.position.y.CompareTo(y.transform.position.y);
		}
	}
}