using System;
using Unity.Entities;
using UnityEngine;

namespace BotAssembler.Components {
	[Serializable]
	public struct MovementSpeed : IComponentData {
		public float Value;
	}
	
	[DisallowMultipleComponent]
	public class MovementSpeedComponent : ComponentDataWrapper<MovementSpeed> {}
}