using BotAssembler.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace BotAssembler.Systems {	
	public class MovementTargetReachSystem : JobComponentSystem {
		[BurstCompile]
		struct RemoveTargetIfReachedJob : IJobProcessComponentDataWithEntity<Position, MovementTarget> {
			public void Execute(Entity entity, int index, ref Position position, ref MovementTarget target) {
				if ( target.Set && (math.distance(position.Value, target.Value) < 0.01f) ) {
					position.Value = target.Value;
					target.Set = false;
				}
			}
		}
		
		protected override JobHandle OnUpdate(JobHandle inputDeps) {
			return new RemoveTargetIfReachedJob()
				.Schedule(this, inputDeps);
		}
	}
}