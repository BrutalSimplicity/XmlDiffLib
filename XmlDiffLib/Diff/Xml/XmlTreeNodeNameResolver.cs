using System;
using XmlDiffLib.NodeTrees.Base;

namespace XmlDiffLib.Diff.Xml
{
  public class XmlTreeNodeNameResolver : INameResolver
  {
    bool includePrefix, includeContext;

    public XmlTreeNodeNameResolver(XmlDiffOptions options)
    {
      includePrefix = !options.IgnorePrefix;
      includeContext = !options.IgnoreNamespace;
    }

    public string Resolve(string prefix, string context, string name)
    {
      string resolvedName = name;
      if (includeContext)
        resolvedName = (!string.IsNullOrEmpty(context)) ? context + ":" + resolvedName : resolvedName;
      if (includePrefix)
        resolvedName = (!string.IsNullOrEmpty(prefix)) ? prefix + ":" + resolvedName : resolvedName;

      return resolvedName;
    }
  }
}
