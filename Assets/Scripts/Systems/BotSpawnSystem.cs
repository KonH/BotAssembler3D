using BotAssembler.Components;
using Unity.Collections;
using Unity.Entities;
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
						var pos2d = rowData.Positions.Dequeue();
						var pos = EntityManager.GetComponentData<Position>(row).Value;
						pos.x += pos2d.x;
						pos.z += pos2d.y;
						var instance = EntityManager.Instantiate(rowData.Prefab);
						EntityManager.SetComponentData(instance, new Position { Value = pos });
					}
					EntityManager.SetSharedComponentData(row, rowData);
				}
			}
		}
	}
}