using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlDiffLib.NodeTrees.Base
{
  internal class AnnotatedTree
  {
    public ITreeNode Root;
    public List<ITreeNode> Nodes;
    public List<ITreeNode> Ids;
    public List<ITreeNode> LeftMostDescendants;
  }
}
