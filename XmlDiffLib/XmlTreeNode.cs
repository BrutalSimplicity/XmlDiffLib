using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlDiffLib
{
  public class XmlTreeNode : ITreeNode<XmlNode>
  {

    public XmlTreeNode(XmlNode node)
    {
      _xNode = node;
    }

    private XmlNode _xNode;

    public List<ITreeNode<XmlNode>> Children
    {
      get
      {
        return _xNode.ChildNodes.Cast<XmlNode>()
                     .Select(n => (new XmlTreeNode(n) as ITreeNode<XmlNode>)).ToList();
      }
    }

    public string Label
    {
      get
      {
        return _xNode.LocalName;
      }
    }

    public ITreeNode<XmlNode> AddChild(ITreeNode<XmlNode> node, bool before = false)
    {
      if (before)
        _xNode.PrependChild(((XmlTreeNode)node)._xNode);
      else
        _xNode.AppendChild(((XmlTreeNode)node)._xNode);

      return this;
    }

    public ITreeNode<XmlNode> GetChild(string label)
    {
      if (Label == label)
        return this;
      else
      {
        foreach (var child in Children)
          if (child.Label == label)
            return child;
      }
      return null;
    }

    public IEnumerator<ITreeNode<XmlNode>> GetEnumerator()
    {
      foreach (var node in TreeIterators.PostOrderTraversal(this))
        if (((XmlTreeNode)node)._xNode.NodeType == XmlNodeType.Element)
          yield return node;
    }
      

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
