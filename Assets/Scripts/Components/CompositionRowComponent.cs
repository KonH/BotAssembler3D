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
		public float Interval;
		public List<float2> Positions;
	}

	public class CompositionRowComponent : SharedComponentDataWrapper<CompositionRow> {
		void OnDrawGizmosSelected() {
			var points = Value.Positions;
			foreach ( var point in points ) {
				Gizmos.DrawWireCube(transform.position + new Vector3(point.x, 0.0f, point.y), Vector3.one);
			}
		}
	}
}