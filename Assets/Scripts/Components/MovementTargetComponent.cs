using System;
using BotAssembler.Utils;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BotAssembler.Components {
	[Serializable]
	public struct MovementTarget : IComponentData {
		public float3 Start;
		public float3 End;
		public float Position;
	}
	
	[DisallowMultipleComponent]
	public class MovementTargetComponent : ComponentDataWrapper<MovementTarget> {}
}