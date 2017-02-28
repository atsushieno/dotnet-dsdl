using System;
using System.Net;

#if PORTABLE
namespace Commons.Xml
{
	public abstract class XmlResolver
	{
		public abstract object GetEntity (Uri uri, object context, Type ofObjectToReturnn);

		public virtual Uri ResolveUri (Uri sourceUri, string relativeUri)
		{
			return new Uri (sourceUri, relativeUri);
		}
	}

	public class XmlUrlResolver : XmlResolver
	{
		public override object GetEntity (Uri uri, object context, Type ofObjectToReturnn)
		{
			return new System.Net.Http.HttpClient ().GetStreamAsync (uri.AbsoluteUri).Result;
		}
	}
}
#endif
