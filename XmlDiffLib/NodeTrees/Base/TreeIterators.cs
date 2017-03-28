using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlDiffLib.NodeTrees.Base
{
  public class TreeIterators
  {
    public static IEnumerable<ITreeNode> PostOrderTraversal(ITreeNode rootNode)
    {
      Stack<ITreeNode> nodeStack = new Stack<ITreeNode>();
      Stack<List<ITreeNode>> lastVisitedStack = new Stack<List<ITreeNode>>();
      ITreeNode currNode = rootNode;
      List<ITreeNode> lastVisited = new List<ITreeNode>();

      if (rootNode != null)
      {
        while (nodeStack.Count > 0 || currNode != null)
        {
          // Travel down descendants until you can't travel anymore
          if (currNode != null)
          {
            // save state before moving to the next descendant
            nodeStack.Push(currNode);

            // Make sure you move to a descendant you haven't visited yet
            if (currNode.Children != null)
              currNode = currNode.Children.Where(n => !lastVisited.Contains(n)).FirstOrDefault();

            // Save the state of all the siblings you've visited
            lastVisitedStack.Push(lastVisited);

            lastVisited = new List<ITreeNode>();
          }
          else
          {
            // Check to see if there are any siblings who haven't been visited yet,
            // if so let's visit them first.
            var peekNode = nodeStack.Peek();
            var otherNode = peekNode.Children.Where(n => !lastVisited.Contains(n)).FirstOrDefault();
            if (otherNode != null)
            {
              currNode = otherNode;
            }
            else
            {
              // Visit the node
              yield return peekNode;

              // Since we're about to move back up the tree, 
              // we need to restore the previous state
              lastVisited = lastVisitedStack.Pop();

              // Add the visited node to the list of visited
              lastVisited.Add(nodeStack.Pop());
            }
          }
        }
      }
    }
  }
}
