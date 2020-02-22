using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;


/// <summary>
/// This class defines the column properties of a report
/// </summary>
public class Column
{
    #region Member variables

    private string _ColumnName;
    private EOperation _Operation;
    private EAlign _Align;
    private double _ColumnWidth;

    #endregion

    #region Constructor

    /// <summary>
    /// Contructs a column object with name, operation on column, text align in column & width
    /// </summary>
    /// <param name="ColumnName"></param>
    /// <param name="Operation"></param>
    /// <param name="Align"></param>
    /// <param name="ColumnWidth"></param>
    public Column(string ColumnName, EOperation Operation, EAlign Align, double ColumnWidth)
    {
        _ColumnName = ColumnName;
        _Operation = Operation;
        _Align = Align;
        _ColumnWidth = ColumnWidth;
    }

    #endregion

    #region Property

    /// <summary>
    /// Name of column
    /// </summary>
    public string ColumnName
    {
        get { return _ColumnName; }
        set { _ColumnName = value; }
    }

    /// <summary>
    /// Operation on column data
    /// </summary>
    public EOperation Operation
    {
        get { return _Operation; }
        set { _Operation = value; }
    }

    /// <summary>
    /// Text align in the column
    /// </summary>
    public EAlign Align
    {
        get { return _Align; }
        set { _Align = value; }
    }

    /// <summary>
    /// width of a column
    /// </summary>
    public double ColumnWidth
    {
        get { return _ColumnWidth; }
        set { _ColumnWidth = value; }
    }

    #endregion
}
