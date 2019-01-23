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
	// It will be better to find only corner points
	// Also, it interesting to move this logic to job systems
	public class BakeUtility {
		static Vector2[] _lookupDirs = {
			Vector2.up, Vector2.down, Vector2.left, Vector2.right,
			Vector2.up   + Vector2.left, Vector2.up   + Vector2.right,
			Vector2.down + Vector2.left, Vector2.down + Vector2.right
		};
		
		static Vector3[] _raycastDirs = {
			Vector3.up, Vector3.down, Vector3.left, Vector3.right
		};

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
			var filledCellsResult = new ResultHolder<HashSet<Vector2>>();
			yield return FillRow(_origin, new HashSet<Vector2> {Vector2.zero}, filledCellsResult);
			var filledCells = filledCellsResult.Get();
			yield return FillRows(_origin, Vector3.up,   filledCells);
			yield return FillRows(_origin, Vector3.down, filledCells);
			SetupDelay();
			Undo.RecordObject(_root, "Disable original object");
			_root.SetActive(false);
		}

		IEnumerator FillRow(Vector3 origin, HashSet<Vector2> startQueue, ResultHolder<HashSet<Vector2>> result) {
			var queuedCells = new Queue<Vector2>(startQueue);
			var filledCells = new HashSet<Vector2>();
			var emptyCells  = new HashSet<Vector2>();
			while ( queuedCells.Count > 0 ) {
				var cell = queuedCells.Dequeue();
				var isOccupied = new ResultHolder<bool>();
				yield return IsPositionOccupiedBy(origin + new Vector3(cell.x, 0, cell.y), isOccupied);
				if ( isOccupied.Get() ) {
					filledCells.Add(cell);
					AddNearestCellsToQueue(cell, queuedCells, filledCells, emptyCells);
				} else {
					emptyCells.Add(cell);
				}
			}
			TryCreateRow(filledCells, origin);
			result.Set(filledCells);
		}

		void AddNearestCellsToQueue(Vector2 cell, Queue<Vector2> queue, HashSet<Vector2> filled, HashSet<Vector2> empty) {
			foreach ( var dir in _lookupDirs ) {
				AddCellToQueue(cell + dir, queue, filled, empty);
			}
		}

		void AddCellToQueue(Vector2 cell, Queue<Vector2> queue, HashSet<Vector2> filled, HashSet<Vector2> empty) {
			if ( filled.Contains(cell) || empty.Contains(cell) || queue.Contains(cell) ) {
				return;
			}
			queue.Enqueue(cell);
		}

		void TryCreateRow(HashSet<Vector2> cells, Vector3 origin) {
			if ( cells.Count == 0 ) {
				return;
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
		}

		IEnumerator FillRows(Vector3 origin, Vector3 offset, HashSet<Vector2> startQueue) {
			var acc    = origin + offset;
			var filled = startQueue;
			while ( filled.Count > 0 ) {
				var filledResult = new ResultHolder<HashSet<Vector2>>();
				yield return FillRow(acc, filled, filledResult);
				filled = filledResult.Get();
				acc    += offset;
			}
		}
		
		IEnumerator IsPositionOccupiedBy(Vector3 pos, ResultHolder<bool> result) {
			var isFailed = false;
			var distanceLimit = _raycastDistance;
			foreach ( var dir in _raycastDirs ) {
				var count = Physics.RaycastNonAlloc(pos + dir * _raycastDistance, -dir, _temp, _raycastDistance);
				if ( !IsWantedCollision(count, distanceLimit) ) {
					isFailed = true;
					break;
				}
				yield return null;
			}
			result.Set(!isFailed);
		}

		bool IsWantedCollision(int count, float distanceLimit) {
			for ( var i = 0; i < count; i++ ) {
				var hit = _temp[i];
				if ( hit.collider == _collider ) {
					return hit.distance < distanceLimit;
				}
			}
			return false;
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