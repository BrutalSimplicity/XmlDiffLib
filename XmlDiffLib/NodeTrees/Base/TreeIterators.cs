using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlDiffLib.NodeTrees.Base
{
  public class TreeIterators
  {
    // My own try at this, didn't get it quite right, so I borrowed from here.
    // https://blogs.msdn.microsoft.com/daveremy/2010/03/16/non-recursive-post-order-depth-first-traversal-in-c/
    public static IEnumerable<ITreeNode> PostOrderTraversal(ITreeNode rootNode)
    {
      if (rootNode == null)
        yield return null;

      var toVisit = new Stack<ITreeNode>();
      var visitedAncestors = new Stack<ITreeNode>();

      toVisit.Push(rootNode);
      while (toVisit.Count > 0)
      {
        var node = toVisit.Peek();

        // if the node has children add them to the list to be visited
        if (node.Children.Count > 0)
        {
          // if we are visiting this node for the first time, grab 
          // its children and save it to the visited ancestors so that
          // we can do the **real** work when we visit them again
          // (on the way back up the tree)
          if (visitedAncestors.PeekOrDefault() != node)
          {
            visitedAncestors.Push(node);
            toVisit.PushReverse(node.Children);
            continue;
          }
          visitedAncestors.Pop();
        }
 
        // do work
        yield return node;
        toVisit.Pop();
      }
    }
  }

  internal static class TreeIteratorExtensions
  {
    public static ITreeNode PeekOrDefault(this Stack<ITreeNode> stack)
    {
      return stack.Count == 0 ? null : stack.Peek();
    }

    public static void PushReverse(this Stack<ITreeNode> stack, List<ITreeNode> children)
    {
      foreach (var child in children.ToArray().Reverse())
        stack.Push(child);
    }
  }
}
