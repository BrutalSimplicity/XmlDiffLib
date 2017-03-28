using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using XmlDiffLib.NodeTrees.Xml;

namespace XmlDiffLib.Diff.Xml
{
  public class XmlTreeBuilderException : Exception
  {
    public XmlTreeBuilderException(string message) : base(message)
    {

    }
  }

  public static class XmlTreeBuilder
  {
    public static XmlTreeNode Build(XPathNavigator nav, XmlDiffOptions options)
    {
      try
      {
        return new XmlTreeNode(nav, new XmlTreeNodeNameResolver(options != null ? options : new XmlDiffOptions()));
      }
      catch (Exception ex)
      {
        throw new XmlTreeBuilderException("Unable to build the Xml tree.\nMessage: " + ex.Message);
      }
    }
  }
}
