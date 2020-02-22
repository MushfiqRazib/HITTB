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
using PdfSharp.Charting;
using PdfSharp.Drawing;
using System.Drawing;

/// <summary>
/// Summary description for GetChart
/// </summary>
public class ChartingUtil
{
    public ChartingUtil()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public Chart GetPieChart(List<Group> chartGroup)
    {
        Chart chart = new Chart(ChartType.Pie2D);

        Series series = chart.SeriesCollection.AddSeries();
        XSeries xseries = chart.XValues.AddXSeries();

        for (int i = 0; i < chartGroup.Count; i++)
        {
            series.Add(chartGroup[i].GroupValue);
            xseries.Add(chartGroup[i].GroupName);
        }

        return chart;
    }

    /// <summary>
    /// Provides colours for each group
    /// </summary>
    /// <param name="chartGroup"></param>
    /// <returns></returns>
    public XColor[] GetChartColours(List<Group> chartGroup)
    {
        XColor[] colours = new XColor[chartGroup.Count];

        for (int i = 0; i < chartGroup.Count; i++)
        {
            colours[i] = chartGroup[i].GroupColour;
        }

        return colours;
    }

    
}
