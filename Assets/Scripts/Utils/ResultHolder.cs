using System;

namespace BotAssembler.Utils {
	public class ResultHolder<T> {
		public bool IsSet { get; private set; }

		public T Value {
			get {
				if ( !IsSet ) {
					throw new InvalidOperationException("Trying to get result without set it before");
				}
				return _value;
			}
			set {
				_value = value;
				IsSet = true;
			}
		}

		T _value;
	}
}