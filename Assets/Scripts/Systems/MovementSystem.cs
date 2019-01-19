using BotAssembler.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BotAssembler.Systems {
	[UpdateAfter(typeof(RandomMovementTargetSystem))]
	public class MovementSystem : JobComponentSystem {
		[BurstCompile]
		struct MovementJob : IJobProcessComponentData<Position, MovementSpeed, MovementTarget> {
			[ReadOnly] float _dt;

			public MovementJob(float dt) {
				_dt = dt;
			}
			
			public void Execute(ref Position position, [ReadOnly] ref MovementSpeed speed, [ReadOnly] ref MovementTarget target) {
				if ( !target.Set ) {
					return;
				}
				var direction = math.normalize(target.Value - position.Value);
				position.Value += direction * speed.Value * _dt;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps) {
			return new MovementJob(Time.deltaTime)
				.Schedule(this, inputDeps);
		}
	}
}