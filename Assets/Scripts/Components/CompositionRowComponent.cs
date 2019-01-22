using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BotAssembler.Components {
	[Serializable]
	public struct CompositionRow : ISharedComponentData {
		public GameObject Prefab;
		public float Delay;
		public List<float2> Positions;
	}

	public class CompositionRowComponent : SharedComponentDataWrapper<CompositionRow> {}
}