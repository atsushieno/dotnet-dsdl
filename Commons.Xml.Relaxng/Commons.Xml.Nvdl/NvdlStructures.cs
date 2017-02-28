using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Commons.Xml.Relaxng;

//using Map = Commons.Xml.Relaxng.ObjectMapping.RelaxngMapping;
//using Choice = Commons.Xml.Relaxng.ObjectMapping.RelaxngMappingChoice;

namespace Commons.Xml.Nvdl
{
	public class Nvdl
	{
		private Nvdl () { }

		public const string Namespace = "http://purl.oclc.org/dsdl/nvdl/ns/structure/1.0";
		public const string BuiltInValidationNamespace = "http://purl.oclc.org/dsdl/nvdl/ns/predefinedSchema/1.0";

		public const string InstanceNamespace = "http://purl.oclc.org/dsdl/nvdl/ns/instance/1.0";

		internal const string XmlNamespaceUri = "http://www.w3.org/xml/1998/namespace";

		private static void OnDefaultEvent (object o,
			NvdlMessageEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine (e.Message);
		}

		internal static NvdlMessageEventHandler HandlePrintMessage =
			new NvdlMessageEventHandler (OnDefaultEvent);

		readonly static NvdlConfig defaultConfig;

		static Nvdl ()
		{
			defaultConfig = new NvdlConfig ();
#if !PORTABLE
			defaultConfig.AddProvider (new NvdlXsdValidatorProvider ());
#endif
			defaultConfig.AddProvider (new NvdlRelaxngValidatorProvider ());
		}

		internal static NvdlConfig DefaultConfig {
			get { return defaultConfig; }
		}

		internal static readonly char [] Whitespaces =
			new char [] {' ', '\r', '\n', '\t'};

		// See 6.4.12.
		internal static bool NSMatches (string n1, int i1, string w1,
			string n2, int i2, string w2)
		{
			// quick check
			if (n1 == n2)
				return true;

			// Case 1:
			if (n1.Length <= i1 && n2.Length <= i2)
				return true;
			// Case 2:
			if (n1.Length <= i1 && n2 == w2 ||
				n2.Length <= i2 && n1 == w1)
				return true;
			// Case 3:
			if (n1.Length > i1 && n2.Length > i2 &&
				n1 [i1] == n2 [i2] &&
				(w1.Length == 0 || n1 [i1] != w1 [0]) &&
				(w2.Length == 0 || n2 [i2] != w2 [0]) &&
				NSMatches (n1, i1 + 1, w1, n2, i2 + 1, w2))
				return true;
			// Case 4:
			if (w1 != "" &&
				n1.Length > i1 && n1 [i1] == w1 [0] &&
				NSMatches (n1, i1, w1, n2, i2 + 1, w2))
				return true;
			// Case 5:
			if (w2 != "" &&
				n2.Length > i2 && n2 [i2] == w2 [0] &&
				NSMatches (n1, i1 + 1, w1, n2, i2, w2))
				return true;
			return false;
		}
	}

	public class NvdlMessageEventArgs : EventArgs
	{
		string message;

		public NvdlMessageEventArgs (string message)
		{
			this.message = message;
		}

		public string Message {
			get { return message; }
		}
	}

	public delegate void NvdlMessageEventHandler (object o, NvdlMessageEventArgs e);

	public class NvdlElementBase : IXmlLineInfo
	{
		int line, column;
		string sourceUri;

		public int LineNumber {
			get { return line; }
			set { line = value; }
		}
		
		public int LinePosition {
			get { return column; }
			set { column = value; }
		}
		
		public bool HasLineInfo ()
		{
			return line > 0 && column > 0;
		}

		public string SourceUri {
			get { return sourceUri; }
			set { sourceUri = value; }
		}
	}

	public class NvdlAttributable : NvdlElementBase
	{
		List<XObject> foreign = new List<XObject> ();

		public IList<XObject> Foreign {
			get { return foreign; }
		}
	}

	/*
	element rules {
		(schemaType?,
		trigger*,
		(rule* | (attribute startMode { xsd:NCName }, mode+)))
		& foreign
	}
	*/
	public class NvdlRules : NvdlAttributable
	{
		string schemaType;
		Collection<NvdlTrigger> triggers = new Collection<NvdlTrigger> ();
		Collection<NvdlRule> rules = new Collection<NvdlRule> ();
		Collection<NvdlMode> modes = new Collection<NvdlMode> ();
		string startMode;

//		[Map.Optional]
//		[Map.Attribute]
		public string SchemaType {
			get { return schemaType; }
			set { schemaType = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}

//		[Map.ZeroOrMore]
		public Collection<NvdlTrigger>  Triggers {
			get { return triggers; }
		}

//		[Map.ZeroOrMore]
		public Collection<NvdlRule> Rules {
			get { return rules; }
		}

//		[Map.Attribute]
//		[MapType ("NCName", XmlSchema.Namespace)]
		public string StartMode {
			get { return startMode; }
			set { startMode = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}

//		[Map.OneOrMore]
		public Collection<NvdlMode> Modes {
			get { return modes; }
		}
	}

	/*
	element trigger {
		(attribute ns { xsd:string },
		attribute nameList { list { xsd:NCName } })
		& foreign
	}
	*/
	public class NvdlTrigger : NvdlAttributable
	{
		string ns;
		string nameList;

//		[Map.Attribute]
		public string NS {
			get { return ns; }
			set { ns = value; }
		}

//		[Map.Attribute]
//		[Map.List]
		public string NameList {
			get { return nameList; }
			set { nameList = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}
	}

	/*
	element mode {
		(attribute name { xsd:NCName },
		includedMode*,
		rule*)
		& foreign
	}
	*/
	public abstract class NvdlModeBase : NvdlAttributable
	{
		Collection<NvdlModeBase> includedModes = new Collection<NvdlModeBase> ();
		Collection<NvdlRule> rules = new Collection<NvdlRule> ();

//		[Map.ZeroOrMore]
		public Collection<NvdlModeBase> IncludedModes {
			get { return includedModes; }
		}

//		[Map.ZeroOrMore]
		public Collection<NvdlRule> Rules {
			get { return rules; }
		}
	}

	public class NvdlNestedMode : NvdlModeBase
	{
	}

	public class NvdlMode : NvdlModeBase
	{
		string name;

//		[Map.Attribute]
//		[MapType ("NCName", XmlSchema.Namespace)]
		public string Name {
			get { return name; }
			set { name = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}
	}

	public class NvdlIncludedMode : NvdlModeBase
	{
		string name;

//		[Map.Attribute]
//		[Map.Optional]
//		[MapType ("NCName", XmlSchema.Namespace)]
		public string Name {
			get { return name; }
			set { name = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}
	}

	public enum NvdlRuleTarget {
		None,
		Elements,
		Attributes,
		Both
	}

	public abstract class NvdlRule : NvdlAttributable
	{
		NvdlRuleTarget match;
		Collection<NvdlAction> actions = new Collection<NvdlAction> ();

		public NvdlRuleTarget Match {
			get { return match; }
			set { match = value; }
		}

		public Collection<NvdlAction> Actions {
			get { return actions; }
		}
	}

	/*
	element namespace {
		(attribute ns { xsd:string },
		attribute wildCard {xsd:string{maxLength = "1"}}?,
		ruleModel)
		& foreign
	}
	*/
	public class NvdlNamespace : NvdlRule
	{
		string ns;
		string wildcard;

//		[Map.Attribute]
		public string NS {
			get { return ns; }
			set { ns = value; }
		}

//		[Map.Attribute]
		public string Wildcard {
			get { return wildcard; }
			set {
				if (value != null && value.Length > 1)
					throw new ArgumentException ("wildCard attribute can contain at most one character.");
				wildcard = value;
			}
		}
	}

	/*
	element anyNamespace { ruleModel & foreign}
	*/
	public class NvdlAnyNamespace : NvdlRule
	{
	}

	public abstract class NvdlAction : NvdlAttributable
	{
	}

	/*
	element cancelNestedActions {foreign}
	*/
	public class NvdlCancelAction : NvdlAction
	{
	}

	public abstract class NvdlNoCancelAction : NvdlAction
	{
		NvdlModeUsage modeUsage;
		string messageAttr;
		Collection<NvdlMessage> messages = new Collection<NvdlMessage> ();

		public NvdlModeUsage ModeUsage {
			get { return modeUsage; }
			set { modeUsage = value; }
		}

		public string SimpleMessage {
			get { return messageAttr; }
			set { messageAttr = value; }
		}

		public Collection<NvdlMessage> Messages {
			get { return messages; }
		}
	}

	public abstract class NvdlNoResultAction : NvdlNoCancelAction
	{
	}

	public enum NvdlResultType {
		Attach,
		AttachPlaceholder,
		Unwrap
	}

	public abstract class NvdlResultAction : NvdlNoCancelAction
	{
		public abstract NvdlResultType ResultType { get; }
	}

	public class NvdlAttach : NvdlResultAction
	{
		public override NvdlResultType ResultType {
			get { return NvdlResultType.Attach; }
		}
	}

	public class NvdlAttachPlaceholder : NvdlResultAction
	{
		public override NvdlResultType ResultType {
			get { return NvdlResultType.AttachPlaceholder; }
		}
	}

	public class NvdlUnwrap : NvdlResultAction
	{
		public override NvdlResultType ResultType {
			get { return NvdlResultType.Unwrap; }
		}
	}

	/*
	element validate {
		(schemaType?,
		(message | option)*,
		schema,
		modeUsage) & foreign
	}

	schema =
		attribute schema { xsd:anyURI } |
		element schema {(text | foreignElement), foreignAttribute*}
	*/
	public class NvdlValidate : NvdlNoResultAction
	{
		string schemaType;
		Collection<NvdlOption> options = new Collection<NvdlOption> ();
		string schemaUri;
		XElement schemaBody;

//		[Map.Attribute]
//		[MapType ("NCName", XmlSchema.Namespace)]
		public string SchemaType {
			get { return schemaType; }
			set { schemaType = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}

		public Collection<NvdlOption> Options {
			get { return options; }
		}

//		[MapType ("anyURI", XmlSchema.Namespace)]
		public string SchemaUri {
			get { return schemaUri; }
			set { schemaUri = value; }
		}

		public XElement SchemaBody {
			get { return schemaBody; }
			set { schemaBody = value; }
		}
	}

	public class NvdlAllow : NvdlNoResultAction
	{
	}

	public class NvdlReject : NvdlNoResultAction
	{
	}

	public class NvdlMessage : NvdlElementBase
	{
		string text;
		string xmlLang;
		List<XObject> foreignAttributes = new List<XObject> ();

		public string Text {
			get { return text; }
			set { text = value; }
		}

		public string XmlLang {
			get { return xmlLang; }
			set { xmlLang = value; }
		}

		public List<XObject> ForeignAttributes {
			get { return foreignAttributes; }
		}
	}

	public class NvdlOption : NvdlAttributable
	{
		string name;
		string arg;
		string mustSupport;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Arg {
			get { return arg; }
			set { arg = value; }
		}

		public string MustSupport {
			get { return mustSupport; }
			set { mustSupport = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}
	}

	/*
	(attribute useMode { xsd:NCName } | nestedMode)?,
	element context {
		(attribute path { path },
		(attribute useMode { xsd:NCName } | nestedMode)?)
		& foreign
	}*
	*/
	public class NvdlModeUsage
	{
		string useMode;
		NvdlNestedMode nestedMode;
		Collection<NvdlContext> contexts = new Collection<NvdlContext> ();

		public string UseMode {
			get { return useMode; }
			set { useMode = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}

		public NvdlNestedMode NestedMode {
			get { return nestedMode; }
			set { nestedMode = value; }
		}

		public Collection<NvdlContext> Contexts {
			get { return contexts; }
		}
	}

	public class NvdlContext : NvdlAttributable
	{
		string path;
		string useMode;
		NvdlNestedMode nestedMode;

		public string Path {
			get { return path; }
			set { path = value; }
		}

		public string UseMode {
			get { return useMode; }
			set { useMode = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}

		public NvdlNestedMode NestedMode {
			get { return nestedMode; }
			set { nestedMode = value; }
		}
	}
}
