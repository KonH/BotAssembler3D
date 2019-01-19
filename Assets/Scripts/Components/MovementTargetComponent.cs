using System;
using BotAssembler.Utils;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BotAssembler.Components {
	[Serializable]
	public struct MovementTarget : IComponentData {
		public float3 Value;
		public TBool Set;
	}
	
	[DisallowMultipleComponent]
	public class MovementTargetComponent : ComponentDataWrapper<MovementTarget> {}
}