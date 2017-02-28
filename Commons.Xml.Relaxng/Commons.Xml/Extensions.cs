using System;
using System.Collections.Generic;

namespace Commons.Xml
{
	static class Extensions
	{
		public static V Get<K,V> (this IDictionary<K,V> dic, K key)
		{
			return dic.ContainsKey (key) ? dic [key] : default (V);
		}
	}
}
