using BotAssembler.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BotAssembler.Systems {
	public class BotSpawnSystem : ComponentSystem {
		ComponentGroup _rows;
		
		protected override void OnCreateManager() {
			_rows = GetComponentGroup(typeof(Position), typeof(RuntimeRow));
		}

		protected override void OnUpdate() {
			using ( var rows = _rows.ToEntityArray(Allocator.TempJob) ) {
				foreach ( var row in rows ) {
					var rowData = EntityManager.GetSharedComponentData<RuntimeRow>(row);
					if ( rowData.Positions.Count == 0 ) {
						rowData.Positions.Dispose();
						EntityManager.DestroyEntity(row);
						return;
					}
					if ( rowData.Timer < rowData.Interval ) {
						rowData.Timer += Time.deltaTime;
					} else {
						rowData.Timer = 0;
						var pos2D = rowData.Positions.Dequeue();
						var pos = EntityManager.GetComponentData<Position>(row).Value;
						pos.x += pos2D.x;
						pos.z += pos2D.y;
						var instance = EntityManager.Instantiate(rowData.Prefab);
						EntityManager.AddComponentData(instance, new MovementTarget() { Set = true, Value = pos });
						var startPos = GetStartPos(pos, rowData.Distance);
						EntityManager.SetComponentData(instance, new Position() { Value = startPos });
					}
					EntityManager.SetSharedComponentData(row, rowData);
				}
			}
		}

		float3 GetStartPos(float3 pos, float distance) {
			if ( math.abs(pos.x) > math.abs(pos.z) ) {
				return (pos.x >= 0) ? pos + new float3(distance, 0, 0) : pos - new float3(distance, 0, 0);
			}
			return (pos.z >= 0) ? pos + new float3(0, 0, distance) : pos - new float3(0, 0, distance);
		}
	}
}