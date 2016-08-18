using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Diagnostics;
using Jokedst.GetOpt;
using XmlDiffLib;

namespace XmlDiffLibConsole
{
  class Program
  {
    public enum UsageErrors { InvalidNumberArguments, InvalidArguments }
    public static void ShowUsage(UsageErrors error)
    {
      switch (error)
      {
        case UsageErrors.InvalidNumberArguments:
          Console.WriteLine("ERROR: Invalid number of arguments.");
          break;
        case UsageErrors.InvalidArguments:
          Console.WriteLine("ERROR: Invalid arguments.");
          break;
        default:
          break;
      }
      Console.WriteLine("USAGE: xmldiff <srcfile.xml> <cmpfile.xml> [-o <filename>] [--csv]");
    }


    static void Main(string[] args)
    {
      XmlDiffOptions xDiffOptions = new XmlDiffOptions();
      string fromFile = string.Empty;
      string toFile = string.Empty;
      string outFile = string.Empty;
      bool toCsv = false;

      var options = new GetOpt("XmlDiff: Tool for finding the difference between two Xml files.",
        new[]
        {
          new CommandLineOption('o', "outfile", "Output file to write to. Files with csv extension open in Excel.", 
            ParameterType.String, o => outFile = (string)o),
          new CommandLineOption('\0', "csv", "Creates a diff csv file and opens in Excel. If no outfile is specified writes output to xmldiff.csv. Default=False",
            ParameterType.None, none => toCsv = true),
          new CommandLineOption('m', "nomatch", "Don't match text node value types (i.e. 0.00 != 0). Default=False", 
            ParameterType.None, none => xDiffOptions.MatchValueTypes = false),
          new CommandLineOption('\0', "ignoretypes", "If -m or --nomatch is NOT chosen, then this chooses which match types to ignore. " +
                                "Possible values are (string, integer, double, datetime). Multiple values may be separated by '|'", ParameterType.String,
                                (types) =>
                                  {
                                    string[] values = ((string)types).Split('|');
                                    foreach (string value in values)
                                    {
                                      switch (value.ToLower().Trim())
                                      {
                                        case "string":
                                          xDiffOptions.IgnoreTextTypes.Add(XmlDiffOptions.IgnoreTextNodeOptions.XmlString);
                                          break;
                                        case "integer":
                                          xDiffOptions.IgnoreTextTypes.Add(XmlDiffOptions.IgnoreTextNodeOptions.XmlInteger);
                                          break;
                                        case "double":
                                          xDiffOptions.IgnoreTextTypes.Add(XmlDiffOptions.IgnoreTextNodeOptions.XmlDouble);
                                          break;
                                        case "datetime":
                                          xDiffOptions.IgnoreTextTypes.Add(XmlDiffOptions.IgnoreTextNodeOptions.XmlDateTime);
                                          break;
                                        default:
                                          throw new CommandLineException("Error parsing enumerated values.", "ignoretypes");
                                      }
                                    }
                                  }),
          new CommandLineOption('d', "nodetail", "Will not display details of matching nodes. Default=False", ParameterType.None, none => xDiffOptions.MatchDescendants = false),
          new CommandLineOption('c', "case", "Case Sensitive. Default=False", ParameterType.None, none => xDiffOptions.IgnoreCase = false),
          new CommandLineOption('\0', "2way", "Does a comparison in both directions. Default=False", ParameterType.None, none => xDiffOptions.TwoWayMatch = true),       
          new CommandLineOption("Required. FromFile", ParameterType.String, file => fromFile = (string)file),
          new CommandLineOption("Required. ToFile", ParameterType.String, file => toFile = (string)file)
        });

      try
      {
        options.ParseOptions(args);
      }
      catch (CommandLineException ex)
      {
        Console.WriteLine("Error: {0}", ex.Message);
        return;
      }

      StreamWriter sw;
      XmlDiff xdiff;
      try
      {
        xdiff = new XmlDiff(File.ReadAllText(fromFile), File.ReadAllText(toFile));
        xdiff.CompareDocuments(xDiffOptions);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error: {0}", ex.Message);
        return;
      }

      if (toCsv)
      {
        try
        {
          string file;
          if (!string.IsNullOrEmpty(outFile))
            file = outFile;
          else
            file = "xmldiff.csv";
          sw = new StreamWriter(file);
          sw.Write((toCsv) ? xdiff.ToCSVString() : xdiff.ToJsonString());
          sw.Close();
          Process.Start(file);
        }
        catch (IOException ex)
        {
          Console.WriteLine("Error: {0}", ex.Message);
          return;
        }
      }
      else
      {
        if (string.IsNullOrEmpty(outFile))
          Console.WriteLine(xdiff.ToJsonString());
        else
        {
          try
          {
            sw = new StreamWriter(outFile);
            sw.WriteLine(xdiff.ToJsonString());
            sw.Close();
          }
          catch (IOException ex)
          {
            Console.WriteLine("Error: {0}", ex.Message);
            return;
          }
        }
      }
    }

  }

}
