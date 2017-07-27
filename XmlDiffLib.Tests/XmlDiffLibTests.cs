using System.Linq;
using System.Xml;
using XmlDiffLib.Diff.Xml;
using NUnit.Framework;
using XmlDiffLib.NodeTrees.Base;
using XmlDiffLib.NodeTrees.Xml;
using System.Collections.Generic;

namespace XmlDiffLib.Tests
{
  [TestFixture]
  public class XmlDiffLibTests
  {
    [Test]
    public void Should_create_xml_tree()
    {
      XmlDiff diff = new XmlDiff(TestResources.exampleD, TestResources.exampleD, new XmlDiffOptions(), "HAAR01000", "HAAR01001");
      Assert.NotNull(diff._sourceNode);
      Assert.NotNull(diff._targetNode);
      Assert.True(diff._sourceNode.Children.Count > 0);
      Assert.True(diff._targetNode.Children.Count > 0);
    }

    [Test]
    public void Should_return_post_ordered_tree()
    {
      void ValidateNodes(Stack<XmlTreeNode> source, Stack<XmlTreeNode> target)
      {
        void ValidateNode(XmlTreeNode sourceNode, XmlTreeNode targetNode)
        {
          Assert.AreEqual(sourceNode.Label, targetNode.Label);
          Assert.AreEqual(sourceNode.LineNumber, targetNode.LineNumber);
          Assert.AreEqual(sourceNode.ColumnNumber, targetNode.ColumnNumber);
          Assert.AreEqual(sourceNode.NodeType, targetNode.NodeType);
        }

        Assert.AreEqual(source.Count(), target.Count());
        while (source.Count > 0)
        {
          var sourceNode = source.Pop();
          var targetNode = target.Pop();

          ValidateNode(sourceNode, targetNode);
          if (sourceNode.Attributes.Count > 0)
            ValidateNodes(
              new Stack<XmlTreeNode>(sourceNode.Attributes),
              new Stack<XmlTreeNode>(targetNode.Attributes));
        }
      }

      XmlDiff diff = new XmlDiff(TestResources.exampleC, TestResources.exampleC, new XmlDiffOptions(), "HAAR01000", "HAAR01001");

      var stackSource = new Stack<XmlTreeNode>();
      var stackTarget = new Stack<XmlTreeNode>();

      foreach (var node in diff._sourceNode)
        stackSource.Push(node);

      foreach (var node in diff._targetNode)
        stackTarget.Push(node);

      ValidateNodes(stackSource, stackTarget);

    }
  }
}
