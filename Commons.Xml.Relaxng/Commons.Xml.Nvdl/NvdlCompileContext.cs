using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using Commons.Xml;

namespace Commons.Xml.Nvdl
{
	internal class NvdlCompileContext
	{
		NvdlRules rules;
		NvdlConfig config;
		XmlResolver resolver;
		Dictionary<object,SimpleMode> compiledModes = new Dictionary<object,SimpleMode> ();
		Dictionary<SimpleMode,NvdlModeCompileContext> modeContexts = new Dictionary<SimpleMode,NvdlModeCompileContext> ();
		Dictionary<SimpleRule, SimpleRule> cancelledRules = new Dictionary<SimpleRule,SimpleRule> ();
		Dictionary<SimpleRule, NvdlRule> ruleContexts = new Dictionary<SimpleRule, NvdlRule> ();

		public NvdlCompileContext (NvdlRules rules, NvdlConfig config, XmlResolver resolver)
		{
			this.rules = rules;
			this.config = config;
			this.resolver = resolver;
		}

		public NvdlRules Rules {
			get { return rules; }
		}

		public NvdlConfig Config {
			get { return config; }
		}

		internal XmlResolver XmlResolver {
			get { return resolver; }
		}

		internal void AddCompiledMode (string name, SimpleMode m)
		{
			compiledModes.Add (m.Name, m);
		}

		internal void AddCompiledMode (NvdlModeUsage u, SimpleMode m)
		{
			compiledModes.Add (u, m);
		}

		internal void AddCompiledMode (NvdlContext c, SimpleMode m)
		{
			compiledModes.Add (c, m);
		}

		internal SimpleMode GetCompiledMode (string name)
		{
			return compiledModes.ContainsKey (name) ? compiledModes [name] : null;
		}

		internal SimpleMode GetCompiledMode (NvdlModeUsage u)
		{
			return compiledModes.ContainsKey (u) ? compiledModes [u] : null;
		}

		internal SimpleMode GetCompiledMode (NvdlContext c)
		{
			return compiledModes.ContainsKey (c) ? compiledModes [c] : null;
		}

		internal ICollection<SimpleMode> GetCompiledModes ()
		{
			return compiledModes.Values;
		}

		internal NvdlModeCompileContext GetModeContext (SimpleMode m)
		{
			return modeContexts.ContainsKey (m) ? modeContexts [m] : null;
		}

		internal void AddModeContext (SimpleMode m,
			NvdlModeCompileContext mctx)
		{
			modeContexts.Add (m, mctx);
		}

		internal NvdlRule GetRuleContext (SimpleRule r)
		{
			return ruleContexts.ContainsKey (r) ? ruleContexts [r] : null;
		}

		internal void AddRuleContext (SimpleRule r, NvdlRule rctx)
		{
			ruleContexts.Add (r, rctx);
		}

		public IDictionary<SimpleRule,SimpleRule> CancelledRules {
			get { return cancelledRules; }
		}
	}

	internal class NvdlModeCompileContext
	{
		List<SimpleMode> included;

		public NvdlModeCompileContext (NvdlModeBase mode)
		{
			included = new List<SimpleMode> ();
		}

		public IList<SimpleMode> Included {
			get { return included; }
		}
	}
}
