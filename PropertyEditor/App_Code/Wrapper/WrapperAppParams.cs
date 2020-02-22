using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Params
/// </summary>
public class WrapperAppParams
{

    public WrapperAppParams()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    public static void SetParams(string fileName, string tableName, string fieldNames, string fieldValues, string groupName)
    {
        FileName = fileName;
        TableName = tableName;
        FieldNames = fieldNames;
        FieldValues = fieldValues;
        GroupName = groupName;
    }

    public static string FileName
    {
        get;
        set;
    }

    public static string TableName
    {
        get;
        set;
    }

    public static string FieldNames
    {
        get;
        set;
    }

    public static string FieldValues
    {
        get;
        set;
    }

    public static string GroupName
    {
        get; set;
    }
}
