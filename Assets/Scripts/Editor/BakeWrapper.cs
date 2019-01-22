using System.Collections.Generic;
using System.Linq;
using BotAssembler.Components;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BakeWrapper {
	static Collider[] _temp = new Collider[16];

	static Vector2[] _dirs = {
		Vector2.up, Vector2.down, Vector2.left, Vector2.right,
		Vector2.up + Vector2.left, Vector2.up + Vector2.right,
		Vector2.down + Vector2.left, Vector2.down + Vector2.right
	};
	
	[MenuItem("GameObject/Wrap", false, 0)]
	public static void Bake() {
		var go = Selection.activeGameObject;
		if ( !go ) {
			return;
		}
		var trans = go.transform;
		var origin = trans.position;
		var filledCells = FillRow(origin, new HashSet<Vector2> { Vector2.zero }, trans);
		FillRows(origin, Vector3.up, filledCells, trans);
		FillRows(origin, Vector3.down, filledCells, trans);
		go.SetActive(false);
	}

	static HashSet<Vector2> FillRow(Vector3 origin, HashSet<Vector2> startQueue, Transform trans) {
		var queuedCells = new Queue<Vector2>(startQueue);
		var filledCells = new HashSet<Vector2>();
		var emptyCells = new HashSet<Vector2>();
		while ( queuedCells.Count > 0 ) {
			var cell = queuedCells.Dequeue();
			if ( IsPositionOccupiedBy(origin + new Vector3(cell.x, 0, cell.y), trans) ) {
				filledCells.Add(cell);
				AddNearestCellsToQueue(cell, queuedCells, filledCells, emptyCells);
			} else {
				emptyCells.Add(cell);
			}
		}
		TryCreateRow(filledCells, origin);
		return filledCells;
	}

	static void AddNearestCellsToQueue(Vector2 cell, Queue<Vector2> queue, HashSet<Vector2> filled, HashSet<Vector2> empty) {
		foreach ( var dir in _dirs ) {
			AddCellToQueue(cell + dir, queue, filled, empty);
		}
    }
	
	static void AddCellToQueue(Vector2 cell, Queue<Vector2> queue, HashSet<Vector2> filled, HashSet<Vector2> empty) {
		if ( filled.Contains(cell) || empty.Contains(cell) || queue.Contains(cell) ) {
			return;
		}
		queue.Enqueue(cell);
	}
	
	static void TryCreateRow(HashSet<Vector2> cells, Vector3 origin) {
		if ( cells.Count == 0 ) {
			return;
		}
		var rowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Row.prefab");
		var instance = (GameObject)PrefabUtility.InstantiatePrefab(rowPrefab, SceneManager.GetActiveScene());
		Undo.RegisterCreatedObjectUndo(instance, "Create row");
		Undo.RecordObject(instance, "Setup row");
		instance.name = string.Format("Row({0})", origin.y);
		instance.transform.position = origin;
		var compositionRow = instance.GetComponent<CompositionRowComponent>();
		compositionRow.Value.Positions.AddRange(cells.Select(vector => new float2(vector.x, vector.y)));
	}
	
	static void FillRows(Vector3 origin, Vector3 offset, HashSet<Vector2> startQueue, Transform trans) {
		var acc = origin + offset;
		var filled = startQueue;
		while ( filled.Count > 0 ) {
			filled = FillRow(acc, filled, trans);
			acc += offset;
		}
	}
	
	static bool IsPositionOccupiedBy(Vector3 pos, Transform trans) {
		var count = Physics.OverlapBoxNonAlloc(pos, Vector3.one, _temp);
		for ( var i = 0; i < count; i++ ) {
			var result = _temp[i];
			if ( result.transform == trans ) {
				return true;
			}
		}
		return false;
	}
}
