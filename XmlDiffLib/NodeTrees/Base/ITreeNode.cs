using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlDiffLib.NodeTrees.Base
{
  public interface ITreeNode : IEnumerable<ITreeNode>
  {
    List<ITreeNode> Children { get; }
    string Label { get; }
    string Type { get; }
    ITreeNode AddChild(ITreeNode node, bool before = false);
    ITreeNode GetChild(string label, Func<string, string, bool> comparer);
  }
}
