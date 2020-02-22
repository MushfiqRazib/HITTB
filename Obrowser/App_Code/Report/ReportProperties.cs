using System;
using System.Data;
using System.Configuration;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using System.Collections.Generic;

/// <summary>
/// ReportProperties class contains all report properties
/// and provides an object of it.
/// </summary>
public class ReportProperties
{
    #region Member variables

    private List<Column> _Columns = null;
    private List<Parameter> _Parameters = null;
    private DataTable _Data = null;
    private EPageSize _PageSize;
    private EPaperFormat _PaperFormat;
    private double _DefaultRowHeight=20;
    private double _LeftMargin = 20;
    private double _RightMargin = 20;
    private double _DefaultColumnWidth = 60;
    private double _TopMargin = 40;
    private double _BottomMargin = 20;
    private double _TableStartingXposition = 50;
    private string _ReportFilePhysicalPath = string.Empty;

    #endregion

    #region Constructor

    /// <summary>
    /// Contruct an object of ReportProperties
    /// </summary>
    public ReportProperties()
    { 
    
    }

    /// <summary>
    /// Construct an object of ReportProperties
    /// with list of columns in the report, list of parameters, data of report
    /// report page size, page format & report file physical path
    /// </summary>
    /// <param name="Columns"></param>
    /// <param name="Parameters"></param>
    /// <param name="Data"></param>
    /// <param name="PageSize"></param>
    /// <param name="PaperFormat"></param>
    /// <param name="reportFilePhysicalPath"></param>
    public ReportProperties(List<Column> Columns, List<Parameter> Parameters, DataTable Data, EPageSize PageSize, EPaperFormat PaperFormat, string reportFilePhysicalPath)
    {
        _Columns = Columns;
        _Parameters = Parameters;
        _Data = Data;
        _PageSize = PageSize;
        _PaperFormat = PaperFormat;
        _ReportFilePhysicalPath = reportFilePhysicalPath;
    }

    /// <summary>
    /// Construct an object of ReportProperties
    /// with list of columns in the report, list of parameters, data of report
    /// report page size & page format.
    /// </summary>
    /// <param name="Columns"></param>
    /// <param name="Parameters"></param>
    /// <param name="Data"></param>
    /// <param name="PageSize"></param>
    /// <param name="PaperFormat"></param>
    public ReportProperties(List<Column> Columns, List<Parameter> Parameters, DataTable Data, EPageSize PageSize, EPaperFormat PaperFormat)
    {
        _Columns = Columns;
        _Parameters = Parameters;
        _Data = Data;
        _PageSize = PageSize;
        _PaperFormat = PaperFormat;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Columns provides list of column names in a report
    /// </summary>
    public List<Column> Columns
    {
        get { return _Columns; }
        set { _Columns = value; }
    }

    /// <summary>
    /// Parameters provides list of parameters in a report
    /// </summary>
    public List<Parameter> Parameters
    {
        get { return _Parameters; }
        set { _Parameters = value; }
    }

    /// <summary>
    /// Data provides the datatable for the report
    /// </summary>
    public DataTable Data
    {
        get { return _Data; }
        set { _Data = value; }
    }

    /// <summary>
    /// PageSize provides report page size
    /// </summary>
    public EPageSize PageSize
    {
        get { return _PageSize; }
        set { _PageSize = value; }
    }

    /// <summary>
    /// PaperFormat provides report page forat
    /// </summary>
    public EPaperFormat PaperFormat
    {
        get { return _PaperFormat; }
        set { _PaperFormat = value; }
    }

    /// <summary>
    /// DefaultRowHeight prvides default row height 
    /// </summary>
    public double DefaultRowHeight
    {
        get { return _DefaultRowHeight; }
        set { _DefaultRowHeight = value; }
    }

    /// <summary>
    /// LeftMargin provides left margin of a report
    /// </summary>
    public double LeftMargin
    {
        get { return _LeftMargin; }
        set { _LeftMargin = value; }
    }

    /// <summary>
    /// RightMargin provides right margin of report
    /// </summary>
    public double RightMargin
    {
        get { return _RightMargin; }
        set { _RightMargin = value; }
    }

    /// <summary>
    /// DefaultColumnWidth provides default column width of report
    /// </summary>
    public double DefaultColumnWidth
    {
        get { return _DefaultColumnWidth; }
        set { _DefaultColumnWidth = value; }
    }

    /// <summary>
    /// TopMargin provides report top margin
    /// </summary>
    public double TopMargin
    {
        get { return _TopMargin; }
        set { _TopMargin = value; }
    }

    /// <summary>
    /// BottomMargin provides report bottom margin
    /// </summary>
    public double BottomMargin
    {
        get { return _BottomMargin; }
        set { _BottomMargin = value; }
    }

    /// <summary>
    /// TableStartingXPosition provides starting X position 
    /// of first datacolumn in the data table
    /// </summary>
    public double TableStartingXPosition
    {
        get { return _TableStartingXposition; }
        set { _TableStartingXposition = value; }
    }

    /// <summary>
    /// ReportFilePhysicalPath provides physical report file path 
    /// which will contain all report data
    /// </summary>
    public string ReportFilePhysicalPath
    {
        get { return _ReportFilePhysicalPath; }
        set { _ReportFilePhysicalPath = value; }
    }

    #endregion
}
