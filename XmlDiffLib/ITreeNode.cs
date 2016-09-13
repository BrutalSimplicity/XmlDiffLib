using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlDiffLib
{
  public interface ITreeNode<T> : IEnumerable<ITreeNode<T>>
  {
    List<ITreeNode<T>> Children { get; }
    string Label { get; }
    ITreeNode<T> AddChild(ITreeNode<T> node, bool before = false);
    ITreeNode<T> GetChild(string label);
  }
}
