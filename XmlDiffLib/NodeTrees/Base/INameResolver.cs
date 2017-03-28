using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlDiffLib.NodeTrees.Base
{
  public interface INameResolver
  {
    string Resolve(string prefix, string context, string name);
  }
}
