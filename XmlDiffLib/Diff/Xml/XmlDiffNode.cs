using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlDiffLib.Diff.Xml
{
  public class XmlDiffNode
  {
    public enum DiffTypes { Removed, Added, Changed }
    public enum DiffNodeTypes { Tag, Text, Attribute, Node }

    public DiffNodeTypes DiffNodeType { get; set; }
    public DiffTypes DiffType { get; set; }
    public string Origin { get; set; }
    public string Comparison { get; set; }
    public string XPath { get; set; }
    public string Description { get; set; }
    public int OriginLineNo { get; set; }
    public int CompLineNo { get; set; }
    public string DiffId { get; set; }
    public List<XmlDiffNode> Descendants;
    public XmlDiffNode() { }
  }
}
