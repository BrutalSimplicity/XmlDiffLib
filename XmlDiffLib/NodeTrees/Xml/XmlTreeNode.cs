using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using XmlDiffLib.NodeTrees.Base;

namespace XmlDiffLib.NodeTrees.Xml
{
  public enum XmlTreeNodeType
  {
    Root,
    Attribute,
    Comment,
    Element,
    Text
  }

  public class XmlTreeNode : ITreeNode
  {

    public XmlTreeNode(XPathNavigator node, INameResolver resolver)
    {
      Create(node, resolver);
    }

    private void Create(XPathNavigator node, INameResolver resolver)
    {
      Children = new List<ITreeNode>();
      Attributes = new List<XmlTreeNode>();
      if (node.HasChildren)
      {
        var iterNode = node.Clone();
        iterNode.MoveToFirstChild();
        do
        {
          Children.Add(new XmlTreeNode(iterNode.Clone(), resolver));
        } while (iterNode.MoveToNext());
      }
      if (node.HasAttributes)
      {
        var iterNode = node.Clone();
        iterNode.MoveToFirstAttribute();
        do
        {
          Attributes.Add(new XmlTreeNode(iterNode.Clone(), resolver));
        } while (iterNode.MoveToNextAttribute());
      }

      if (node.NodeType == XPathNodeType.Text)
        Label = node.Value;
      else
        Label = resolver.Resolve(node.Prefix, node.NamespaceURI, node.LocalName);
      Type = node.NodeType.ToString();

      var lineInfo = node as IXmlLineInfo;
      if (lineInfo != null && lineInfo.HasLineInfo())
      {
        LineNumber = ((IXmlLineInfo)node).LineNumber;
        ColumnNumber = ((IXmlLineInfo)node).LinePosition;
      }
    }

    public List<XmlTreeNode> Attributes { get; private set; }
    public List<ITreeNode> Children { get; private set; }
    public string Label { get; private set; }
    public string Type { get; private set; }
    public int LineNumber { get; private set; }
    public int ColumnNumber { get; private set; }
    public XmlTreeNodeType NodeType
    {
      get
      {
        return (XmlTreeNodeType)Enum.Parse(typeof(XmlTreeNodeType), Type);
      }
    }

    public ITreeNode AddChild(ITreeNode node, bool before = false)
    {
      if (before)
        Children.Insert(0, node);
      else
        Children.Add(node);

      return this;
    }

    public ITreeNode GetChild(string label, Func<string, string, bool> comparer)
    {
      foreach (var child in Children)
        if (comparer(child.Label, label))
          return child;
      return null;
    }

    public IEnumerator<XmlTreeNode> GetEnumerator()
    {
      foreach (var node in TreeIterators.PostOrderTraversal(this))
        yield return (XmlTreeNode)node;
    }
      

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
