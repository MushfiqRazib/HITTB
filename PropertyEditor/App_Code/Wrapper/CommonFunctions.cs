using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for CommonFunctions
/// </summary>
public class CommonFunctions
{
	public CommonFunctions()
	{
		//
		// TODO: Add constructor logic here
		//
	}


    public static string GetConnectionString()
    {
        return ConfigurationManager.ConnectionStrings["DatabaseConnection_Core"].ToString();
    }

    public static string GetWrapperConenctionString()
    {
        return ConfigurationManager.ConnectionStrings["DatabaseConnection_Wrapper"].ToString();
    }

    public static ArrayList GetSkipFieldList()
    {
        string skipfields = ConfigurationManager.AppSettings["skipfields"].ToString();
        string[] skipFieldList = skipfields.Split(',');
        ArrayList skipList = new ArrayList();
        foreach (string fieldName in skipFieldList)
        {
            skipList.Add(fieldName.ToLower());
        }
        return skipList;
    }

    public static Dictionary<string, string> GetFieldValueList(string filePath)
    {
        XmlDocument doc = new XmlDocument();
        Dictionary<string, string> fieldValDictionary = new Dictionary<string, string>();
        doc.Load(filePath);
        ArrayList list = new ArrayList();
        XmlNodeList parentNode = doc.SelectNodes("/CAD/field");
        XmlNode nameNode, valueNode;

        foreach (XmlNode fieldNode in parentNode)
        {
            XmlNodeList fieldValPairNode = fieldNode.ChildNodes;
            nameNode = fieldValPairNode[0];
            valueNode = fieldValPairNode[1];
            fieldValDictionary.Add(nameNode.InnerXml, valueNode.InnerXml); // node.InnerXml;
        }
        return fieldValDictionary;
    }



    public static string GetMetaTableName()
    {
        return ConfigurationManager.AppSettings["metatable"].ToString();
    }

    public static string GetActiveDatabaseName()
    {
        string dbName = "";
        try
        {
            dbName = ConfigurationManager.AppSettings["core_datasource"].ToString();
        }
        catch
        {
            throw new Exception("DB_NAME_ERROR");
        }

        if (dbName == "")
        {
            throw new Exception("DB_NAME_ERROR");
        }
        return dbName;
    }

    //internal static string GetMetaDataStorageName()
    //{
    //    string metadataStorage = "";
    //    try
    //    {
    //        metadataStorage = ConfigurationManager.AppSettings["metadataStorage"].ToString();
    //    }
    //    catch
    //    {
    //        throw new Exception("DB_STORAGE_ERROR");
    //    }

    //    if (metadataStorage == "")
    //    {
    //        throw new Exception("DB_STORAGE_ERROR");
    //    }
    //    return metadataStorage;
    //}
}
