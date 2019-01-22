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
					if ( rowData.Delay > 0 ) {
						rowData.Delay -= Time.deltaTime;
						EntityManager.SetSharedComponentData(row, rowData);
						continue;
					}
					if ( rowData.Positions.Count == 0 ) {
						rowData.Positions.Dispose();
						EntityManager.DestroyEntity(row);
						continue;
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
						var startPos = GetStartPos(pos, rowData.Distance);
						var speedPerUnit = EntityManager.GetComponentData<MovementSpeed>(instance).Value;
						var actualSpeed = speedPerUnit / math.distance(startPos, pos);
						EntityManager.SetComponentData(instance, new Position() { Value = startPos });
						EntityManager.SetComponentData(instance, new MovementSpeed { Value = actualSpeed });
						EntityManager.AddComponentData(instance, new MovementTarget() { Start = startPos, End = pos });
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