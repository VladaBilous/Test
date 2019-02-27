using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument master = new XmlDocument();
            master.Load(@"D:\TestResult_Copy_UBS.trx");
            XmlElement rootElement =  master.GetElementsByTagName("TestRun").Item(0) as XmlElement;
            XmlNodeList nodes = rootElement.GetElementsByTagName("UnitTestResult");
            var parentNode = nodes.Item(0).ParentNode;

            List<XmlElement> xmlElements = new List<XmlElement>();
            foreach (var node in nodes)
            {
                xmlElements.Add(node as XmlElement);
            }

            var isChanged = false;
            xmlElements.ForEach(x =>
            {
                if (x.GetAttribute("outcome").ToLowerInvariant().Equals("failed"))
                {
                    var currentTestId = x.GetAttribute("testId");
                    var nodesWithSameTestId = xmlElements.Where(y => y.GetAttribute("testId").Equals(currentTestId)).ToList();
                    if (nodesWithSameTestId.Any(z => z.GetAttribute("outcome").ToLowerInvariant().Equals("passed")))
                    {
                        var currentExecutionId = x.GetAttribute("executionId");
                        var nodesToRemove = nodesWithSameTestId.Where(n => !n.GetAttribute("executionId").Equals(currentExecutionId)).ToList();
                        nodesToRemove.ForEach(a => parentNode?.RemoveChild(a));
                        x.SetAttribute("outcome", "Passed");
                        isChanged = true;
                    }
                }
            });

            if (isChanged)
                UpdateCounters();

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                CheckCharacters = false
            };

            using (var xmlWriter = XmlWriter.Create(@"D:\TestResult_UPDATED_UBS.trx", xmlWriterSettings))
            {
                master.WriteTo(xmlWriter);
            }

            void UpdateCounters()
            {
                var countersNode = master.GetElementsByTagName("Counters").Item(0) as XmlElement;
                countersNode.SetAttribute("total", xmlElements.Count.ToString());
                countersNode.SetAttribute("executed", xmlElements.Count.ToString());
                countersNode.SetAttribute("passed", xmlElements.Count(x => x.GetAttribute("outcome").ToLowerInvariant().Equals("passed")).ToString());
                countersNode.SetAttribute("failed", xmlElements.Count(x => x.GetAttribute("outcome").ToLowerInvariant().Equals("failed")).ToString());
            }
        }
    }
}
