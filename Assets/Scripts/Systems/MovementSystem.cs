using BotAssembler.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BotAssembler.Systems {
	public class MovementSystem : JobComponentSystem {
		[BurstCompile]
		struct MovementJob : IJobProcessComponentData<Position, Scale, MovementSpeed, MovementTarget> {
			[ReadOnly] float _dt;

			public MovementJob(float dt) {
				_dt = dt;
			}
			
			public void Execute(ref Position position, ref Scale scale, [ReadOnly] ref MovementSpeed speed, ref MovementTarget target) {
				position.Value = math.lerp(target.Start, target.End, target.Position);
				scale.Value = math.lerp(float3.zero, new float3(1, 1, 1), target.Position);
				target.Position += speed.Value * _dt;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps) {
			return new MovementJob(Time.deltaTime)
				.Schedule(this, inputDeps);
		}
	}
}