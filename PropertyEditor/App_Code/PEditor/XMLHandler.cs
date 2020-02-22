using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Xml;

namespace HIT.PEditor.Core
{
    public class XMLHandler
    {
        public XMLHandler(string path, string fileName)
        {
            XMLFilePath = path;
            FileName = fileName;
        }

        public string FileName
        {
            get;
            set;
        }
        public string XMLFilePath
        {
            get;
            set;
        }
        /// <summary>
        /// XmlElement will be available after calling GetFields Method
        /// </summary>
        public XElement XmlElement
        {
            set;
            get;
        }

        public Dictionary<string, string> GetFields()
        {
            Dictionary<string, string> fieldValuePair = new Dictionary<string, string>();
            string fileURI = string.Concat(XMLFilePath, FileName);
            XmlElement = XElement.Load(fileURI);
            var data = from xmlValues in XmlElement.Descendants("field")
                       select xmlValues;
            foreach (var item in data)
            {
                fieldValuePair.Add(item.Element("name").Value, item.Element("value").Value);
            }
            return fieldValuePair;
        }

        public string CreateXML(Dictionary<string, string> fieldValuePair)
        {
            string sourceFile = string.Concat(XMLFilePath, FileName);
            XElement XmlElement = XElement.Load(sourceFile);

            //*** Delete extra fields from xml file
            var allElements = XmlElement.Descendants("field");
            foreach (XElement item in allElements.ToList())
            {
                if (!fieldValuePair.ContainsKey(item.Descendants("name").Single().Value))
                {
                    item.Remove();                    
                }                
            }

            //*** UPdate field values in xml file
            foreach (var item in fieldValuePair)
            {
                var elm = XmlElement.Descendants("name").Single(node => node.Value == item.Key);
                if (elm != null)
                {
                    var valueNode = elm.ElementsAfterSelf("value").Single();
                    valueNode.SetValue(item.Value);
                }
            }

            XmlElement.Save(sourceFile);
            return sourceFile;
        }

    }
}