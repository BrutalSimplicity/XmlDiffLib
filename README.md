# XmlDiffLib

Use this library to find the differences between two Xml files. You can also clone this repo and build it to use the console application that makes use of this library.

# How to use

You can install the library as a nuget package using the nuget command line tool, or in Visual Studio, the Nuget Package Manager.

```
nuget install XmlDiffLib
```

## Sample

The setup is fairly simple, however there are quite a few options that you can include when comparing documents. I'll discuss a few here.

The basic setup looks like:

```
var exampleA = File.ReadAllText(@"exampleA.xml");
var exampleB = File.ReadAllText(@"exampleB.xml");

var diff = new XmlDiff(exampleA, exampleB);

diff.CompareDocuments(new XmlDiffOptions());
diff.ToString();
```

The results are from the comparison of the files under `XmlDiffLib.Tests/Resources`. In general, the default options are setup for the most common Xml cases. Order doesn't matter in attributes or elements, ignores namespace prefixes, trims whitespace on comparisons, etc... One option you might be interested in is having the comparison go both ways. That way you can see if nodes may exist in one xml that are not in the other. *Note: this always compares parameter one's xml to parameter two's xml by default.*

**Results:**

```json
{
"DiffNodeList": 
      [
         {
            "Edit": "Update",
            "XPath": "catalog[1]/book[@id=\"bk101\"][1]/publish_date[1]",
            "Diff ID": "1",
            "Description": "Text node does not match  |  2000-10-01 => 2015-10-12",
            "Node Type": "Text",
            "Origin Line No": 7,
            "Comp Line No": 7
         },
         {
            "Edit": "Update",
            "XPath": "catalog[1]/book[@id=\"bk102\"][1]/genre[1]",
            "Diff ID": "2",
            "Description": "Text node does not match  |  Fantasy => Mystery",
            "Node Type": "Text",
            "Origin Line No": 16,
            "Comp Line No": 16
         },
         {
            "Edit": "Update",
            "XPath": "catalog[1]/book[@id=\"bk102\"][1]/publish_date[1]",
            "Diff ID": "3",
            "Description": "Text node does not match  |  2000-12-16 => 2016-02-16",
            "Node Type": "Text",
            "Origin Line No": 18,
            "Comp Line No": 18
         },
         {
            "Edit": "Update",
            "XPath": "catalog[1]/book[@id=\"bk103\"][1]/price[1]",
            "Diff ID": "4",
            "Description": "Text node does not match  |  5.95 => 12.95",
            "Node Type": "Text",
            "Origin Line No": 29,
            "Comp Line No": 29
         },
         {
            "Edit": "Update",
            "XPath": "catalog[1]/book[@id=\"bk103\"][1]/publish_date[1]",
            "Diff ID": "5",
            "Description": "Text node does not match  |  2000-11-17 => 2016-08-17",
            "Node Type": "Text",
            "Origin Line No": 30,
            "Comp Line No": 30
         }
      ]
}
```

If you want compare two Xml documents and see which nodes are in parameter two's xml that are not in parameter one, you can just add the property `TwoWayMatch = true` to the `XmlDiffOptions` object.

```csharp
var exampleA = File.ReadAllText(@"C:\Users\KTaborn\Documents\repos\XmlDiffLib\XmlDiffLib.Tests\Resources\exampleA.xml");
var exampleB = File.ReadAllText(@"C:\Users\KTaborn\Documents\repos\XmlDiffLib\XmlDiffLib.Tests\Resources\exampleB.xml");

var diff = new XmlDiff(exampleA, exampleB);

diff.CompareDocuments(new XmlDiffOptions()
{
  TwoWayMatch = true
});

```

**Results:**

```json
{
"DiffNodeList": 
      [
         {
            "Edit": "Update",
            "XPath": "catalog[1]/book[@id=\"bk101\"][1]/publish_date[1]",
            "Diff ID": "1",
            "Description": "Text node does not match  |  2000-10-01 => 2015-10-12",
            "Node Type": "Text",
            "Origin Line No": 7,
            "Comp Line No": 7
         },
         {
            "Edit": "Update",
            "XPath": "catalog[1]/book[@id=\"bk102\"][1]/genre[1]",
            "Diff ID": "2",
            "Description": "Text node does not match  |  Fantasy => Mystery",
            "Node Type": "Text",
            "Origin Line No": 16,
            "Comp Line No": 16
         },
         {
            "Edit": "Update",
            "XPath": "catalog[1]/book[@id=\"bk102\"][1]/publish_date[1]",
            "Diff ID": "3",
            "Description": "Text node does not match  |  2000-12-16 => 2016-02-16",
            "Node Type": "Text",
            "Origin Line No": 18,
            "Comp Line No": 18
         },
         {
            "Edit": "Update",
            "XPath": "catalog[1]/book[@id=\"bk103\"][1]/price[1]",
            "Diff ID": "4",
            "Description": "Text node does not match  |  5.95 => 12.95",
            "Node Type": "Text",
            "Origin Line No": 29,
            "Comp Line No": 29
         },
         {
            "Edit": "Update",
            "XPath": "catalog[1]/book[@id=\"bk103\"][1]/publish_date[1]",
            "Diff ID": "5",
            "Description": "Text node does not match  |  2000-11-17 => 2016-08-17",
            "Node Type": "Text",
            "Origin Line No": 30,
            "Comp Line No": 30
         },
         {
            "Edit": "Insert",
            "XPath": "catalog[1]/book[@id=\"bk105\"][1]",
            "Diff ID": "2",
            "Description": "Node not found",
            "Node Type": "Tag",
            "Origin Line No": 37,
            "Comp Line No": 25,
            "Descendants": 
               [
                  {
                     "Edit": "Update",
                     "XPath": "catalog[1]/book[@id=\"bk105\"][1]",
                     "Diff ID": "1.1",
                     "Description": "No matching attribute @id = bk105",
                     "Node Type": "Node",
                     "Origin Line No": 37,
                     "Comp Line No": 25
                  }
               ]

         }
      ]
}
```
