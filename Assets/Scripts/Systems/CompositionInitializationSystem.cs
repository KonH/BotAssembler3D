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
					var positions = rowData.Positions;
					positions.Sort(CompareByDistance);
					var queue = new NativeQueue<float2>(Allocator.Persistent);
					foreach ( var pos in positions ) {
						queue.Enqueue(pos);
					}
					var maxPos = positions[positions.Count - 1];
					var maxLength = 10 + math.max(math.abs(maxPos.x), math.abs(maxPos.y));
					var runtimeRow = new RuntimeRow {
						Prefab    = rowData.Prefab,
						Delay     = rowData.Delay,
						Interval  = rowData.Interval,
						Positions = queue,
						Distance  = maxLength
					};
					EntityManager.AddSharedComponentData(row, runtimeRow);
					EntityManager.RemoveComponent<CompositionRow>(row);
				}
			}
		}

		int CompareByDistance(float2 x, float2 y) {
			return math.length(x).CompareTo(math.length(y));
		}
	}
}