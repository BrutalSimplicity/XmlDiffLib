using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.IO;

namespace XmlDiffLib
{
  public class XmlDiffNode
  {
    public enum DiffTypes { Removed, Added, Changed }
    public enum DiffNodeTypes { Tag, Text, Attribute, Node }

    public DiffNodeTypes DiffNodeType { get; set; }
    public DiffTypes DiffType { get; set; }
    public string Origin { get; set; }
    public string Comparison { get; set; }
    public string XPath { get; set; }
    public string Description { get; set; }
    public int OriginLineNo { get; set; }
    public int CompLineNo { get; set; }
    public string DiffId { get; set; }
    public List<XmlDiffNode> Descendants;
    public XmlDiffNode() { }
  }

  public class XmlDiffOptions
  {
    public enum IgnoreTextNodeOptions { XmlString, XmlInteger, XmlDouble, XmlDateTime }
    public bool IgnoreCase { get; set; }
    public bool IgnoreAttributeOrder { get; set; }
    public bool IgnoreChildOrder { get; set; }
    public bool IgnoreAttributes { get; set; }
    public HashSet<XPathNodeType> IgnoreNodes { get; set; }
    public bool IgnoreNamespace { get; set; }
    public bool IgnorePrefix { get; set; }
    public bool TrimWhitespace { get; set; }
    public bool StripWhitespace { get; set; }
    public bool MatchDescendants { get; set; }
    public bool MatchValueTypes { get; set; }
    public bool TwoWayMatch { get; set; }
    public int MaxAttributesToDisplay { get; set; }
    public HashSet<IgnoreTextNodeOptions> IgnoreTextTypes { get; set; }

    public XmlDiffOptions()
    {
      IgnoreAttributes = false;
      IgnoreCase = false;
      IgnoreAttributeOrder = true;
      IgnoreChildOrder = true;
      IgnoreNamespace = true;
      IgnorePrefix = true;
      TrimWhitespace = true;
      StripWhitespace = false;
      MatchDescendants = true;
      MatchValueTypes = true;
      TwoWayMatch = false;
      IgnoreNodes = new HashSet<XPathNodeType>();
      IgnoreTextTypes = new HashSet<IgnoreTextNodeOptions>();
      MaxAttributesToDisplay = -1;
    }
  }

  public class XmlDiff
  {
    private XPathDocument xmlFromDoc;
    private XPathDocument xmlToDoc;
    private string fromFilename;
    private string toFilename;
    private XmlDiffOptions options;

    public List<XmlDiffNode> DiffNodeList { get; set; }
    public XmlDiff(string fromXml, string toXml, string sourceFromName = "FromXml", string sourceToName = "ToXml")
    {
      try
      {
        xmlFromDoc = new XPathDocument(new StringReader(fromXml));
        xmlToDoc = new XPathDocument(new StringReader(toXml));
        this.fromFilename = sourceFromName;
        this.toFilename = sourceToName;
      }
      catch (XmlException ex)
      {
        throw new XmlException(String.Format("ERROR: An error was encountered in the XML data. Make sure the document is a valid XML document.\nMessge: {0}", ex));
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }

    public bool CompareDocuments(XmlDiffOptions options)
    {
      try
      {
        this.options = options;
        DiffNodeList = CompareNodes(xmlFromDoc.CreateNavigator(), xmlToDoc.CreateNavigator());
        if (options.TwoWayMatch)
        {
          List<XmlDiffNode> tempNodeList = CompareNodes(xmlToDoc.CreateNavigator(), xmlFromDoc.CreateNavigator());
          DiffNodeList.AddRange(tempNodeList.Where((node) => node.DiffType == XmlDiffNode.DiffTypes.Removed)
                                            .Select((node) => { node.DiffType = XmlDiffNode.DiffTypes.Added; return node; }));
        }
      }
      catch (Exception ex)
      {
        throw new Exception(String.Format("ERROR: An error occurred while comparing XML documents.\nMessage: {0}", ex));
      }

      if (DiffNodeList.Count > 0)
        return false;

      return true;
    }

    private bool MatchAttributes(XPathNavigator fromNav, XPathNavigator toNav, out XmlDiffNode nodeInfo)
    {
      XPathNavigator xFrom = fromNav.Clone();
      XPathNavigator xTo = toNav.Clone();
      nodeInfo = null;

      if (xFrom.HasAttributes)
      {
        xFrom.MoveToFirstAttribute();
        if (!options.IgnoreAttributeOrder)
          xTo.MoveToFirstAttribute();

        do
        {
          if (!options.IgnoreAttributeOrder)
          {
            if (!options.IgnoreNamespace && xFrom.Prefix != xTo.Prefix)
            {
              nodeInfo = new XmlDiffNode()
              {
                XPath = null,
                DiffType = XmlDiffNode.DiffTypes.Changed,
                Description = "No matching namespace @" + xFrom.NamespaceURI,
                DiffNodeType = XmlDiffNode.DiffNodeTypes.Text,
                Origin = fromFilename,
                Comparison = toFilename,
                OriginLineNo = ((IXmlLineInfo)xFrom).LineNumber,
                CompLineNo = ((IXmlLineInfo)xTo).LineNumber,
                DiffId = string.Empty, // To be filled in by compare method
              };
              return false;
            }
            if (xFrom.LocalName != xTo.LocalName || xFrom.Value != xTo.Value)
            {
              nodeInfo = new XmlDiffNode()
              {
                XPath = null,
                DiffType = XmlDiffNode.DiffTypes.Changed,
                Description = "No matching attribute @" + xFrom.LocalName + " = " + xFrom.Value,
                DiffNodeType = XmlDiffNode.DiffNodeTypes.Text,
                Origin = fromFilename,
                Comparison = toFilename,
                OriginLineNo = ((IXmlLineInfo)xFrom).LineNumber,
                CompLineNo = ((IXmlLineInfo)xTo).LineNumber,
                DiffId = string.Empty, // To be filled in by compare method
              };
              return false;
            }

            xTo.MoveToNextAttribute();
          }
          else
          {
            if (xTo.GetAttribute(xFrom.LocalName, (!options.IgnoreNamespace) ? xFrom.NamespaceURI : "") != xFrom.Value)
            {
              nodeInfo = new XmlDiffNode()
              {
                XPath = null,
                DiffType = XmlDiffNode.DiffTypes.Changed,
                Description = "No matching attribute @" + xFrom.LocalName + " = " + xFrom.Value,
                DiffNodeType = XmlDiffNode.DiffNodeTypes.Text,
                Origin = fromFilename,
                Comparison = toFilename,
                OriginLineNo = ((IXmlLineInfo)xFrom).LineNumber,
                CompLineNo = ((IXmlLineInfo)xTo).LineNumber,
                DiffId = string.Empty, // To be filled in by compare method
              };
              return false;
            }
          }
        } while (xFrom.MoveToNextAttribute());
      }

      return true;
    }

    private bool MatchElement(XPathNavigator fromNav, XPathNavigator toNav, out XmlDiffNode nodeInfo)
    {
      XPathNavigator xFrom = fromNav.Clone();
      XPathNavigator xTo = toNav.Clone();
      nodeInfo = null;

      if (!options.IgnoreNamespace && (xFrom.NamespaceURI != xTo.NamespaceURI))
        return false;
      if (!options.IgnorePrefix && (xFrom.Prefix != xTo.Prefix))
        return false;
      if (xFrom.LocalName != xTo.LocalName)
        return false;
      if (!options.IgnoreAttributes && !MatchAttributes(xFrom, xTo, out nodeInfo))
      {
        return false;
      }


      return true;
    }

    private List<XPathNavigator> SelectSiblings(XPathNavigator fromNav, XPathNavigator toNav, out XmlDiffNode nodeInfo)
    {
      XPathNavigator xFrom = fromNav.Clone();
      XPathNavigator xTo = toNav.Clone();
      List<XPathNavigator> xToList = new List<XPathNavigator>();
      nodeInfo = null;

      xTo.MoveToFirst();
      if (xTo.NodeType != XPathNodeType.Element)
        xTo.MoveToNext(XPathNodeType.Element);

      do
      {
        if (MatchElement(xFrom, xTo, out nodeInfo))
          xToList.Add(xTo.Clone());
      } while (xTo.MoveToNext(XPathNodeType.Element));

      return xToList;
    }

    private List<XPathNavigator> SelectAllMatchingSiblings(XPathNavigator fromNav)
    {
      XmlDiffNode ni;
      return SelectSiblings(fromNav, fromNav, out ni);
    }

    private int GetSiblingPosition(XPathNavigator aSibling)
    {
      List<XPathNavigator> siblings = SelectAllMatchingSiblings(aSibling);
      for (int index = 0; index < siblings.Count; index++)
      {
        if (siblings[index].IsSamePosition(aSibling))
          return index + 1;
      }

      return 0;
    }

    private List<XmlDiffNode> CompareNodes(XPathNavigator xmlFromNav, XPathNavigator xmlToNav, string parentDiffId = "")
    {
      int diffNumber = 1;
      List<XmlDiffNode> diffNodeList = new List<XmlDiffNode>();
      Queue<XPathNavigator> xFromQueue = new Queue<XPathNavigator>();
      Queue<XPathNavigator> xToQueue = new Queue<XPathNavigator>();

      XPathNavigator xFrom = xmlFromNav.Clone();
      XPathNavigator xTo = xmlToNav.Clone();
      xFrom.MoveToChild(XPathNodeType.Element);
      xTo.MoveToChild(XPathNodeType.Element);
      xFromQueue.Enqueue(xFrom.Clone());
      xToQueue.Enqueue(xTo.Clone());

      bool isMatch = false;
      List<XPathNavigator> xMatch = new List<XPathNavigator>();

      while (xFromQueue.Count > 0 && xToQueue.Count > 0)
      {
        xFrom = xFromQueue.Dequeue();
        xTo = xToQueue.Dequeue();

        do
        {
          if (options.IgnoreNodes.Contains(xFrom.NodeType))
            continue;

          XmlDiffNode nodeInfo;
          if (!options.IgnoreChildOrder)
            isMatch = MatchElement(xFrom, xTo, out nodeInfo);
          else
            xMatch = SelectSiblings(xFrom, xTo, out nodeInfo);

          if (isMatch || xMatch.Count == 1)
          {
            xTo = (isMatch) ? xTo : xMatch[0];
            if (xFrom.HasChildren && xTo.HasChildren)
            {
              XPathNavigator tempFrom, tempTo;
              tempFrom = xFrom.Clone();
              tempTo = xTo.Clone();
              tempFrom.MoveToFirstChild();
              tempTo.MoveToFirstChild();
              XmlDiffNode result;
              if (!options.IgnoreNodes.Contains(XPathNodeType.Text) && !CompareText(tempFrom, tempTo, out result, ref diffNumber))
              {
                diffNodeList.Add(result);
              }
              else
              {
                xFromQueue.Enqueue(tempFrom.Clone());
                xToQueue.Enqueue(tempTo.Clone());
              }
            }
            else if (xFrom.HasChildren && !xTo.HasChildren)
            {
              diffNodeList.Add(new XmlDiffNode
              {
                XPath = GetXPath(xFrom),
                DiffType = XmlDiffNode.DiffTypes.Removed,
                Description = "Node children not found",
                DiffNodeType = XmlDiffNode.DiffNodeTypes.Tag,
                Origin = fromFilename,
                Comparison = toFilename,
                OriginLineNo = ((IXmlLineInfo)xFrom).LineNumber,
                CompLineNo = ((IXmlLineInfo)xTo).LineNumber,
                DiffId = (diffNumber++).ToString()
              });
            }
          }
          else if (xMatch.Count > 1)
          {
            List<Tuple<int, List<XmlDiffNode>>> matchNodes = new List<Tuple<int, List<XmlDiffNode>>>();
            foreach (XPathNavigator node in xMatch)
              matchNodes.Add(new Tuple<int, List<XmlDiffNode>>(((IXmlLineInfo)node).LineNumber, CompareNodes(xFrom, node, diffNumber.ToString())));

            var bestMatchNodes = from node in matchNodes
                                 where node.Item2.Count == matchNodes.OrderBy(node_sub => node_sub.Item2.Count).First().Item2.Count()
                                 select node;

            // We take the first best matching node here
            Tuple<int, List<XmlDiffNode>> bestMatchNode = bestMatchNodes.First();
            bestMatchNode.Item2.ForEach(node => { node.DiffId = (!string.IsNullOrEmpty(parentDiffId) ? parentDiffId + "." : "") + diffNumber.ToString() + "." + node.DiffId; });

            if (bestMatchNode.Item2.Count > 0)
            {
              diffNodeList.Add(new XmlDiffNode
              {
                XPath = GetXPath(xFrom),
                DiffType = XmlDiffNode.DiffTypes.Removed,
                Description = "No matching node found.",
                DiffNodeType = XmlDiffNode.DiffNodeTypes.Node,
                Descendants = (options.MatchDescendants) ? bestMatchNode.Item2 : null,
                Origin = fromFilename,
                Comparison = toFilename,
                OriginLineNo = ((IXmlLineInfo)xFrom).LineNumber,
                CompLineNo = (options.MatchDescendants) ? bestMatchNode.Item1 : ((IXmlLineInfo)xTo).LineNumber,
                DiffId = (diffNumber++).ToString()
              });
            }
          }
          else
          {
            XPathNavigator xToParent = xTo.Clone();
            if (nodeInfo != null)
            {
              nodeInfo.DiffId = (diffNumber++).ToString() + ".1";
              nodeInfo.XPath = GetXPath(xFrom);
              nodeInfo.DiffNodeType = XmlDiffNode.DiffNodeTypes.Node;
              diffNodeList.Add(new XmlDiffNode
              {
                XPath = GetXPath(xFrom),
                DiffType = XmlDiffNode.DiffTypes.Removed,
                Description = "Node not found",
                DiffNodeType = XmlDiffNode.DiffNodeTypes.Tag,
                Origin = fromFilename,
                Comparison = toFilename,
                OriginLineNo = ((IXmlLineInfo)xFrom).LineNumber,
                CompLineNo = ((IXmlLineInfo)xToParent).LineNumber,
                DiffId = (diffNumber).ToString(),
                Descendants = new List<XmlDiffNode>() { nodeInfo }
              });
              
              diffNodeList.Add(nodeInfo);
            }
            else
            {
              diffNodeList.Add(new XmlDiffNode
              {
                XPath = GetXPath(xFrom),
                DiffType = XmlDiffNode.DiffTypes.Removed,
                Description = "Node not found",
                DiffNodeType = XmlDiffNode.DiffNodeTypes.Tag,
                Origin = fromFilename,
                Comparison = toFilename,
                OriginLineNo = ((IXmlLineInfo)xFrom).LineNumber,
                CompLineNo = ((IXmlLineInfo)xToParent).LineNumber,
                DiffId = (diffNumber++).ToString()
              });
            }
          }
        } while (xFrom.MoveToNext(XPathNodeType.Element));
      }
      return diffNodeList;
    }

    private void MergeDiffs(List<XmlDiffNode> fromList, List<XmlDiffNode> mergeList)
    {
      foreach (XmlDiffNode node in mergeList)
        fromList.Add(node);
    }

    private bool CompareText(XPathNavigator xmlFromNav, XPathNavigator xmlToNav, out XmlDiffNode result, ref int diffNumber)
    {
      XPathNavigator xFrom = xmlFromNav.Clone();
      XPathNavigator xTo = xmlToNav.Clone();
      result = new XmlDiffNode();

      if (xFrom.NodeType == XPathNodeType.Text && xTo.NodeType == XPathNodeType.Text)
      {
        if (!CompareTextValue(xFrom.Value, xTo.Value))
        {
          result = new XmlDiffNode
          {
            XPath = GetXPath(xFrom),
            DiffType = XmlDiffNode.DiffTypes.Changed,
            Description = "Text node does not match  |  " + xFrom.Value.Trim() + " => " + xTo.Value.Trim(),
            DiffNodeType = XmlDiffNode.DiffNodeTypes.Text,
            Origin = fromFilename,
            Comparison = toFilename,
            OriginLineNo = ((IXmlLineInfo)xFrom).LineNumber,
            CompLineNo = ((IXmlLineInfo)xTo).LineNumber,
            DiffId = (diffNumber++).ToString()
          };
          return false;
        }
      }

      return true;
    }

    private bool CompareTextValue(string fromValue, string toValue)
    {
      if (options.TrimWhitespace)
      {
        fromValue.Trim();
        toValue.Trim();
      }

      if (options.StripWhitespace)
      {
        fromValue = Regex.Replace(fromValue, @"\s", "");
        toValue = Regex.Replace(toValue, @"\s", "");
      }

      if (options.MatchValueTypes)
      {
        DateTime fromDTResult, toDTResult;
        if (DateTime.TryParse(fromValue, out fromDTResult) && DateTime.TryParse(toValue, out toDTResult))
        {
          if (fromDTResult == toDTResult || options.IgnoreTextTypes.Contains(XmlDiffOptions.IgnoreTextNodeOptions.XmlDateTime))
            return true;
          else
            return false;
        }

        int iFromResult, iToResult;
        if (int.TryParse(fromValue, out iFromResult) && int.TryParse(toValue, out iToResult))
        {
          if (iFromResult == iToResult || options.IgnoreTextTypes.Contains(XmlDiffOptions.IgnoreTextNodeOptions.XmlInteger))
            return true;
          else
            return false;
        }

        double dFromResult, dToResult;
        if (double.TryParse(fromValue, out dFromResult) && double.TryParse(toValue, out dToResult))
        {
          if (dFromResult == dToResult || options.IgnoreTextTypes.Contains(XmlDiffOptions.IgnoreTextNodeOptions.XmlDouble))
            return true;
          else
            return false;
        }
      }

      if (options.IgnoreTextTypes.Contains(XmlDiffOptions.IgnoreTextNodeOptions.XmlString))
        return true;
      else
      {
        if (options.IgnoreCase)
        {
          if (!fromValue.Equals(toValue, StringComparison.OrdinalIgnoreCase))
            return false;
        }
        else
        {
          if (fromValue != toValue)
            return false;
        }
      }

      return true;
    }

    private string GetXPath(XPathNavigator nav)
    {
      Func<XPathNavigator, string> addAttrib =
        (node) =>
        {
          StringBuilder attribs = new StringBuilder();
          XPathNavigator xNode = node.Clone();
          if (xNode.HasAttributes)
          {
            int count = 0;
            xNode.MoveToFirstAttribute();
            do
            {
              attribs.Append("[@" + xNode.LocalName + "=" + "\"" + xNode.Value + "\"]");
              count++;
              if (options.MaxAttributesToDisplay > 0 && count >= options.MaxAttributesToDisplay)
                break;
            } while (xNode.MoveToNextAttribute());
          }
          return attribs.ToString();
        };

      XPathNavigator xNav = nav.Clone();
      StringBuilder result = new StringBuilder();

      do
      {
        if (string.IsNullOrEmpty(xNav.LocalName))
          continue;
        string tempLabel = xNav.LocalName + addAttrib(xNav);
        tempLabel += "[" + GetSiblingPosition(xNav) + "]";
        tempLabel += "/";

        result.Insert(0, tempLabel);
      } while (xNav.MoveToParent());

      return result.ToString().TrimEnd(new char[] { '/' });
    }

    public string PrettyPrintXPath(string xpath)
    {
      StringBuilder sb = new StringBuilder();
      int depth = 0;
      for (int i = 0; i < xpath.Length; i++)
      {
        sb.Append(xpath[i]);
        if (xpath[i] == '/')
        {
          sb.Append(Environment.NewLine + new string(' ', (++depth * 2)));
        }
      }
      return sb.ToString();
    }

    private string EscapeQuotes(string s)
    {
      char[] str = new char[s.Length * 2];
      int c_idx = 0;
      for (int idx = 0; idx < s.Length; idx++, c_idx++)
      {
        if (s[idx] == '"' || s[idx] == '\\')
          str[c_idx++] = '\\';
        str[c_idx] = s[idx];
      }
      return new string(str, 0, c_idx);
    }



    public string ToCSVString()
    {

      Func<List<XmlDiffNode>, string> walkToCsv = null;
      walkToCsv =
        (diffList) =>
        {
          StringBuilder resultCsv = new StringBuilder();
          foreach (XmlDiffNode node in diffList)
          {
            resultCsv.Append("\"" + node.DiffId + "\",");

            switch (node.DiffType)
            {
              case XmlDiffNode.DiffTypes.Removed:
                resultCsv.Append("(-),");
                break;
              case XmlDiffNode.DiffTypes.Added:
                resultCsv.Append("(+),");
                break;
              case XmlDiffNode.DiffTypes.Changed:
                resultCsv.Append("(*),");
                break;
              default:
                break;
            }

            resultCsv.Append("\"" + node.XPath + "\",");
            resultCsv.Append("\"" + EscapeQuotes(node.Description) + "\"" + ",");
            resultCsv.Append(node.DiffNodeType + ",");
            resultCsv.Append(node.OriginLineNo + ",");
            resultCsv.Append(node.CompLineNo + ",");
            resultCsv.Append(node.Origin);
            resultCsv.AppendLine();

            if (node.DiffNodeType == XmlDiffNode.DiffNodeTypes.Node && node.Descendants != null)
            {
              resultCsv.Append(walkToCsv(node.Descendants));
            }
          }

          return resultCsv.ToString();
        };

      return "\"ID\",Result,XPath,Description,Type,\"OriginLineNo\",\"CompLineNo\",Origin\r\n" + walkToCsv(DiffNodeList);
    }

    public override string ToString()
    {
      return ToJsonString();
    }

    public virtual string ToJsonString()
    {
      const int IndentSize = 3;
      // define and assign delegate before definition so that
      // the delegate is captured by the recursive call
      Func<List<XmlDiffNode>, int, string> walkToString = null;
      walkToString =
        (diffList, depth) =>
        {
          StringBuilder diffLine = new StringBuilder();
          if (depth == 0)
          {
            diffLine.AppendLine("{");
            depth++;
            diffLine.AppendLine("\"DiffNodeList\": ");
            depth++;
          }
          diffLine.AppendLine(new string(' ', depth * IndentSize) + "[");
          depth++;
          foreach (XmlDiffNode node in diffList)
          {
            diffLine.AppendLine(new string(' ', depth * IndentSize) + "{");
            depth++;
            string[] lines = ToString(node).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines.Take(lines.Count() - 1))
            {
              diffLine.AppendLine(new String(' ', depth * IndentSize) + line);
            }

            if (node.Descendants != null)
            {
              diffLine.AppendLine(new string(' ', depth * IndentSize) + lines.Last());
              diffLine.AppendLine(new string(' ', depth * IndentSize) + "\"Descendants\": ");
              diffLine.AppendLine(walkToString(node.Descendants, depth + 1));
            }
            else
              diffLine.AppendLine(new string(' ', depth * IndentSize) + lines.Last().TrimEnd(','));

            depth--;
            diffLine.AppendLine(new string(' ', depth * IndentSize) + "},");
          }
          diffLine.Replace(",", "", diffLine.Length - 5, 5);
          depth--;
          diffLine.AppendLine(new string(' ', depth * IndentSize) + "]");
          depth -= 2;
          if (depth == 0)
          {
            diffLine.AppendLine(new string(' ', depth * IndentSize) + "}");
          }
          return diffLine.ToString();
        };

      return walkToString(DiffNodeList, 0);
    }

    public string ToString(XmlDiffNode node)
    {
      StringBuilder diffLine = new StringBuilder();

      diffLine.Append("\"Edit\": ");
      switch (node.DiffType)
      {
        case XmlDiffNode.DiffTypes.Removed:
          diffLine.AppendLine("\"Delete\",");
          break;
        case XmlDiffNode.DiffTypes.Added:
          diffLine.AppendLine("\"Insert\",");
          break;
        case XmlDiffNode.DiffTypes.Changed:
          diffLine.AppendLine("\"Update\",");
          break;
        default:
          break;
      }

      diffLine.AppendLine("\"XPath\": " + "\"" + EscapeQuotes(node.XPath) + "\",");

      diffLine.AppendLine("\"Diff ID\": " + "\"" + node.DiffId + "\",");

      diffLine.AppendLine("\"Description\": " + "\"" + EscapeQuotes(node.Description) + "\",");

      diffLine.AppendLine("\"Node Type\": " + "\"" + node.DiffNodeType + "\",");

      diffLine.AppendLine("\"Origin Line No\": " + node.OriginLineNo + ",");

      diffLine.AppendLine("\"Comp Line No\": " + node.CompLineNo + ",");

      return diffLine.ToString();
    }

  }
}
