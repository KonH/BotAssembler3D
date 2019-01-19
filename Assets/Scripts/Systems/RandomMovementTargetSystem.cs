using BotAssembler.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace BotAssembler.Systems {
	public class RandomMovementTargetSystem : JobComponentSystem {
		[BurstCompile]
		struct RandomMovementTargetJob : IJobProcessComponentDataWithEntity<Position, MovementTarget> {
			Random _random;

			public RandomMovementTargetJob(Random random) {
				_random = random;
			}
			
			public void Execute(Entity entity, int index, ref Position position, ref MovementTarget target) {
				if ( target.Set ) {
					return;
				}
				var dir = GetRandomDir(position.Value);
				var targetValue = position.Value + dir;
				target.Value = targetValue;
				target.Set = true;
			}
			
			float3 GetRandomDir(float3 position) {
				var x = 0;
				var y = 0;
				var z = 0;
				var axis = _random.NextInt(3);
				switch ( axis ) {
					case 0: x = GetRandomValue(position.x); break;
					case 1: y = GetRandomValue(position.y); break;
					case 2: z = GetRandomValue(position.z); break;
				}
				return new float3(x, y, z);
			}

			int GetRandomValue(float value) {
				if ( _random.NextBool() ) {
					return value < 5 ? 1 : -1;
				} else {
					return value > -5 ? -1 : 1;
				}
			}
		}

		Random _random;

		protected override void OnCreateManager() {
			_random = new Random(1);
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps) {
			return new RandomMovementTargetJob(_random)
				.Schedule(this, inputDeps);
		}
	}
}