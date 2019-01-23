using System;

namespace BotAssembler.Utils {
	public class ResultHolder<T> {
		T    _value;
		bool _isSet = false;

		public void Set(T value) {
			_isSet = true;
			_value = value;
		}

		public T Get() {
			if ( !_isSet ) {
				throw new InvalidOperationException("Trying to get result without set it before");
			}
			return _value;
		}
	}
}