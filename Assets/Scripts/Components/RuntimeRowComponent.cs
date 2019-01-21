using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BotAssembler.Components {
	public enum SpawnDirection {
		XPositive = 0,
		XNegative = 1,
		ZPositive = 2,
		ZNegative = 3
	}
	public struct RuntimeRow : ISharedComponentData {
		public GameObject Prefab;
		public float Timer;
		public float Interval;
		public NativeQueue<float2> Positions;
		public SpawnDirection Direction;
	}
	
	public class RuntimeRowComponent : SharedComponentDataWrapper<RuntimeRow> {}
}