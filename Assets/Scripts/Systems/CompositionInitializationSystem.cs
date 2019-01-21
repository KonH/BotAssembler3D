using BotAssembler.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace BotAssembler.Systems {
	public class CompositionInitializationSystem : ComponentSystem {
		ComponentGroup _rows;

		protected override void OnCreateManager() {
			_rows = GetComponentGroup(typeof(Position), typeof(CompositionRow));
		}

		protected override void OnUpdate() {
			using ( var rows = _rows.ToEntityArray(Allocator.TempJob) ) {
				foreach ( var row in rows ) {
					var rowData = EntityManager.GetSharedComponentData<CompositionRow>(row);
					var queue = new NativeQueue<float2>(Allocator.Persistent);
					foreach ( var pos in rowData.Positions ) {
						queue.Enqueue(pos);
					}
					var runtimeRow = new RuntimeRow {
						Prefab    = rowData.Prefab,
						Interval  = 0.25f,
						Positions = queue
					};
					EntityManager.AddSharedComponentData(row, runtimeRow);
					EntityManager.RemoveComponent<CompositionRow>(row);
				}
			}
		}
	}
}