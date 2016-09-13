using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlDiffLib
{
  internal class AnnotatedTree<T>
  {
    public ITreeNode<T> Root;
    public List<ITreeNode<T>> Nodes;
    public List<ITreeNode<T>> Ids;
    public List<ITreeNode<T>> LeftMostDescendants;
  }
}
