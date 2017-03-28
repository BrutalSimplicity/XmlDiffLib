using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XmlDiffLib.Diff.Base;
using XmlDiffLib.NodeTrees.Base;
using XmlDiffLib.NodeTrees.Xml;

namespace XmlDiffLib.Diff.Xml
{
  class XmlTreeNodeComparer : ITreeNodeComparer
  {
    XmlDiffOptions _options;

    public XmlTreeNodeComparer(XmlDiffOptions options)
    {
      _options = options;
    }

    public int Compare(ITreeNode x, ITreeNode y)
    {
      throw new NotImplementedException();
    }
  }
}
