using System.Collections.Generic;
using XmlDiffLib.NodeTrees.Xml;

namespace XmlDiffLib.Diff.Xml
{
  public class XmlDiffOptions
  {
    public enum IgnoreTextNodeOptions { XmlString, XmlInteger, XmlDouble, XmlDateTime }
    public bool IgnoreCase { get; set; }
    public bool IgnoreAttributeOrder { get; set; }
    public bool IgnoreChildOrder { get; set; }
    public bool IgnoreAttributes { get; set; }
    public HashSet<XmlTreeNodeType> IgnoreNodes { get; set; }
    public bool IgnoreNamespace { get; set; }
    public bool IgnorePrefix { get; set; }
    public bool TrimWhitespace { get; set; }
    public bool StripWhitespace { get; set; }
    public bool MatchDescendants { get; set; }
    public bool MatchValueTypes { get; set; }
    public bool TwoWayMatch { get; set; }
    public int MaxAttributesToDisplay { get; set; }
    public HashSet<IgnoreTextNodeOptions> IgnoreTextTypes { get; set; }

    public XmlDiffOptions()
    {
      IgnoreAttributes = false;
      IgnoreCase = false;
      IgnoreAttributeOrder = true;
      IgnoreChildOrder = true;
      IgnoreNamespace = true;
      IgnorePrefix = true;
      TrimWhitespace = true;
      StripWhitespace = false;
      MatchDescendants = true;
      MatchValueTypes = true;
      TwoWayMatch = false;
      IgnoreNodes = new HashSet<XmlTreeNodeType>();
      IgnoreTextTypes = new HashSet<IgnoreTextNodeOptions>();
      MaxAttributesToDisplay = -1;
    }
  }
}
