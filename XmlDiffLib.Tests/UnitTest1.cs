using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XmlDiffLib.Tests
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestXmlDiff()
    {
      
      XmlDiff diff = new XmlDiff(TestResources.HAAR01000, TestResources.HAAR01001, "HAAR01000", "HAAR01001");
      XmlDiffOptions options = new XmlDiffOptions();
      options.IgnoreAttributeOrder = true;
      options.IgnoreAttributes = false;
      options.MaxAttributesToDisplay = 3;
      diff.CompareDocuments(options);
      Assert.AreEqual(diff.DiffNodeList.Count, 0);
      XmlDocument xdoc = new XmlDocument();
      xdoc.LoadXml(TestResources.HAAR01001);
      XmlNode node = xdoc.SelectSingleNode("//Product");
      node.Attributes["EffDate"].Value = "01/01/3000";
      diff = new XmlDiff(TestResources.HAAR01000, xdoc.InnerXml, "HAAR01000", "HAAR01001");
      diff.CompareDocuments(options);
      Assert.AreEqual(diff.DiffNodeList.Count, 1);
      Assert.IsTrue(diff.DiffNodeList[0].Description.Contains("EffDate"));
      xdoc.LoadXml(TestResources.HAAR01001);
      node = xdoc.SelectSingleNode("//Company");
      node.Attributes["CompanyID"].Value = "1000";
      diff = new XmlDiff(TestResources.HAAR01000, xdoc.InnerXml);
      diff.CompareDocuments(options);
      Assert.AreEqual(diff.DiffNodeList.Count, 1);
      Assert.IsTrue(diff.DiffNodeList[0].Description.Contains("CompanyID"));
    }
  }
}
