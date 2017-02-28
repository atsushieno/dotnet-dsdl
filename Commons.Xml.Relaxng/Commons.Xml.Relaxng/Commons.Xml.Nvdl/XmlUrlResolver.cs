using System;
using System.Net;

namespace Commons.Xml
{
	class XmlUrlResolver : XmlResolver
	{
		public override object GetEntity (Uri uri, object context, Type ofObjectToReturnn)
		{
			var wr = WebRequest.Create (uri);
			return wr.GetResponseAsync ().Result.GetResponseStream ();
		}
	}
}