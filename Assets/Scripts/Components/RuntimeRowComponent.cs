using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BotAssembler.Components {
	public struct RuntimeRow : ISharedComponentData {
		public GameObject Prefab;
		public float Timer;
		public float Interval;
		public NativeQueue<float2> Positions;
	}
	
	public class RuntimeRowComponent : SharedComponentDataWrapper<RuntimeRow> {}
}