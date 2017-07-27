using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.XPath;
using XmlDiffLib.NodeTrees.Xml;
[assembly: InternalsVisibleTo("XmlDiffLib.Tests")]

namespace XmlDiffLib.Diff.Xml
{
  public class XmlDiff
  {
    internal XmlTreeNode _sourceNode;
    internal XmlTreeNode _targetNode;
    private string _sourceName;
    private string _targetName;
    private XmlDiffOptions _options;

    public XmlDiff(string sourceXml, string targetXml, XmlDiffOptions options = null, string sourceName = "FromXml", string targetName = "ToXml")
    {
      try
      {
        _options = options ?? new XmlDiffOptions();
        _sourceNode = XmlTreeBuilder.Build(CreateDocumentNavigator(sourceXml, _options), _options);
        _targetNode = XmlTreeBuilder.Build(CreateDocumentNavigator(targetXml, _options), _options);
        _sourceName = sourceName;
        _targetName = targetName;
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

    private XPathNavigator CreateDocumentNavigator(string xml, XmlDiffOptions option)
    {
      var reader = XmlReader.Create(new StringReader(xml));
      var document = new XPathDocument(reader);

      return document.CreateNavigator();
    }
  }
}
