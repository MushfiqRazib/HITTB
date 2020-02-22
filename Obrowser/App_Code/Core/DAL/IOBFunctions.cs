using System.Data;
using System.Collections;

/// <summary>
/// Summary description for IOBFunctions
/// </summary>
namespace HIT.OB.STD.Core.DAL
{
    public interface IOBFunctions
    {
        //*** The database connection has to be opened automatically
        //*** during instantiation of the database manager. So Database
        //*** connection will be active for the lifetime of the manager.
        //*** When the manager no longer used then it has to be closed.
        //*** There is no database open function to restrict frequent database
        //*** connection creation.

        ArrayList GetFieldList(string tableName);
        DataTable GetDataTable(string query);
        //string GetColumnAliasList(string tableName);
        //string GetGroupByRowCount();
        //string GetGroupByResultsetForGrid();
        
        int GetNormalGridRowCount(string tableName, string sqlWhere);
        int GetGroupByGridRowCount(string columnName, string sqlWhere, string tableName);
        DataTable GetGroupByGridData(string sqlSelectFields,string columnName, string tableName, string sqlWhere,string startRow, string pageSize, string SQL_ORDER_BY, string SQL_ORDER_DIR, string QB_GB_SELECT_CLAUSE, string GIS_THEME_LAYER);
        DataTable GetNormalGridData(string tableName, string sqlSelect, string sqlWhere, string startRow, string pageSize, string SQL_ORDER_BY, string SQL_ORDER_DIR);     
		DataTable GetItemData(string sqlFrom, string keyList, string valueList);

        DataTable GetColorCodeTable(string columnName, string REPORT_CODE, DataTable groupedTable);
        DataTable GetSpecificRowDetailData(string tableName, string selectedField, string keyList, string valueList);
        DataTable GetRelativeFileName(string tableName, string keyList, string valueList);
        string GetFullFileName(string tableName, string keyList, string valueList);
        ArrayList GetFieldValues(string SQL_FROM, string FIELD_NAME);
        string CheckCustomFieldValidation(string REPORT_CODE, string SQL_FROM, string customFields);
        string ValidateWhereClause(string SQL_FROM, string WHERE_CLAUSE);
        string CheckGroupBySelectValidation(string REPORT_CODE, string SQL_FROM, string QB_GB_SELECT_CLAUSE);
        string GetFieldNameType(string SQL_FROM);

    }
}