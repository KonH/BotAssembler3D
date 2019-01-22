using BotAssembler.Components;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace BotAssembler.Systems {
	public class TargetReachBarrier : BarrierSystem {}
	public class MovementTargetReachSystem : JobComponentSystem {
		TargetReachBarrier _barrier;

		struct RemoveTargetIfReachedJob : IJobProcessComponentDataWithEntity<Position, Scale, MovementTarget> {
			EntityCommandBuffer.Concurrent _commands;

			public RemoveTargetIfReachedJob(EntityCommandBuffer.Concurrent commands) {
				_commands = commands;
			}
			
			public void Execute(Entity entity, int index, ref Position position, ref Scale scale, ref MovementTarget target) {
				if ( target.Position >= 1.0f ) {
					position.Value = target.End;
					scale.Value = new float3(1, 1, 1);
					_commands.RemoveComponent<MovementTarget>(index, entity);
				}
			}
		}
		
		protected override void OnCreateManager() {
			_barrier = World.Active.GetOrCreateManager<TargetReachBarrier>();
		}
		
		protected override JobHandle OnUpdate(JobHandle inputDeps) {
			var commands = _barrier.CreateCommandBuffer().ToConcurrent();
			var handle = new RemoveTargetIfReachedJob(commands)
				.Schedule(this, inputDeps);
			_barrier.AddJobHandleForProducer(handle);
			return handle;
		}
	}
}