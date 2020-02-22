using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Npgsql;

/// <summary>
/// This class handle the database connection & 
/// returns datatable for sql
/// </summary>
public class DBUtil
{
    #region Member variables

    private NpgsqlConnection _Connection = null;

    #endregion
	
    #region Public Properties

    public static string CONN_STRING = string.Empty;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructs DBUtuil object
    /// </summary>
    public DBUtil()
   {
		//
		// TODO: Add constructor logic here
		//
    }

    #endregion

    #region private methods

    /// <summary>
    /// creates an orcale connection & open the connection & returns it.
    /// </summary>
    /// <returns>NpgsqlConnection</returns>
    private NpgsqlConnection GetDBConnection()
    {
        try
        {
            if (_Connection == null)
            {
                _Connection = new NpgsqlConnection(DBUtil.CONN_STRING);
            }
        }
        catch (NpgsqlException oraExe)
        {
            //_Connection.Open();
            throw oraExe;
        }
        catch (Exception exp) 
        {
            throw exp;
        }
        
        return _Connection;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// executes a sql & retuns datatable
    /// </summary>
    /// <param name="sql"></param>
    /// <returns>DataTable</returns>
    public DataTable GetDataTable(string sql,string reportCode)
    {
        //sql = sql.Replace("''", "\"");
        DataTable oDataTable = new DataTable();

        HIT.OB.STD.Core.BLL.DBManagerFactory dbManagerFactory = new HIT.OB.STD.Core.BLL.DBManagerFactory();
        HIT.OB.STD.Core.DAL.IOBFunctions dbManager = dbManagerFactory.GetDBManager(reportCode);

        try
        {
            if (!string.IsNullOrEmpty(sql))
            {
               oDataTable = dbManager.GetDataTable(sql);            
            }
        }
        catch (Exception oEx)
        {
            throw oEx;
        }

        return oDataTable;
    }

    #endregion
}
