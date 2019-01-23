using System.Collections.Generic;
using System.Linq;
using BotAssembler.Components;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BotAssembler.Editor {
	public class BakeUtility {
		static Vector2[] _lookupDirs = {
			Vector2.up, Vector2.down, Vector2.left, Vector2.right,
			Vector2.up   + Vector2.left, Vector2.up   + Vector2.right,
			Vector2.down + Vector2.left, Vector2.down + Vector2.right
		};

		Collider[]                    _temp      = new Collider[16];
		List<CompositionRowComponent> _instances = new List<CompositionRowComponent>();

		GameObject _root;
		Transform  _trans;
		Vector3    _origin;

		public BakeUtility(GameObject root) {
			_root      = root;
			_trans     = _root.transform;
			_origin    = _root.GetComponent<Collider>().bounds.center;
		}

		public void Bake() {
			var filledCells = FillRow(_origin, new HashSet<Vector2> {Vector2.zero});
			FillRows(_origin, Vector3.up,   filledCells);
			FillRows(_origin, Vector3.down, filledCells);
			SetupDelay();
			_root.SetActive(false);
		}
		
		HashSet<Vector2> FillRow(Vector3 origin, HashSet<Vector2> startQueue) {
			var queuedCells = new Queue<Vector2>(startQueue);
			var filledCells = new HashSet<Vector2>();
			var emptyCells  = new HashSet<Vector2>();
			while ( queuedCells.Count > 0 ) {
				var cell = queuedCells.Dequeue();
				if ( IsPositionOccupiedBy(origin + new Vector3(cell.x, 0, cell.y)) ) {
					filledCells.Add(cell);
					AddNearestCellsToQueue(cell, queuedCells, filledCells, emptyCells);
				} else {
					emptyCells.Add(cell);
				}
			}
			TryCreateRow(filledCells, origin);
			return filledCells;
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

		void FillRows(Vector3 origin, Vector3 offset, HashSet<Vector2> startQueue) {
			var acc    = origin + offset;
			var filled = startQueue;
			while ( filled.Count > 0 ) {
				filled =  FillRow(acc, filled);
				acc    += offset;
			}
		}

		bool IsPositionOccupiedBy(Vector3 pos) {
			var count = Physics.OverlapBoxNonAlloc(pos, Vector3.one, _temp);
			for ( var i = 0; i < count; i++ ) {
				var result = _temp[i];
				if ( result.transform == _trans ) {
					return true;
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
				compositionRow.Delay = i * 1.5f;
				instance.Value = compositionRow;
			}
		}

		static int CompareByPositionY(CompositionRowComponent x, CompositionRowComponent y) {
			return x.transform.position.y.CompareTo(y.transform.position.y);
		}
	}
}