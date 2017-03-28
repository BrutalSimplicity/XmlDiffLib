using System.Linq;
using System.Xml;
using XmlDiffLib.Diff.Xml;
using NUnit.Framework;
using XmlDiffLib.NodeTrees.Base;
using XmlDiffLib.NodeTrees.Xml;

namespace XmlDiffLib.Tests
{
  [TestFixture]
  public class XmlDiffLibTests
  {
    [Test]
    public void Should_create_xml_tree()
    {
      XmlDiff diff = new XmlDiff(TestResources.HAAR01000, TestResources.HAAR01001, new XmlDiffOptions(), "HAAR01000", "HAAR01001");
      Assert.NotNull(diff._sourceNode);
      Assert.NotNull(diff._targetNode);
      Assert.True(diff._sourceNode.Children.Count > 0);
      Assert.True(diff._targetNode.Children.Count > 0);
    }

    [Test]
    public void Should_return_post_ordered_tree()
    {
      XmlDiff diff = new XmlDiff(TestResources.HAAR01000, TestResources.HAAR01001, new XmlDiffOptions(), "HAAR01000", "HAAR01001");

      var postOrderTree = TreeIterators.PostOrderTraversal(diff._sourceNode).ToList();

      Assert.NotNull(postOrderTree);
      Assert.True(postOrderTree.Count > 0);
      Assert.AreEqual("Product", postOrderTree[0].Label);
      Assert.True(((XmlTreeNode)postOrderTree[0]).Attributes.Count > 0);
    }
  }
}
