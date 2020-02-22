using System;
using System.Data;
using System.Configuration;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using System.IO;
using System.Collections.Generic;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using System.Drawing;
using PdfSharp;
using PdfSharp.Charting;
using PdfSharp.Drawing.Pdf;

/// <summary>
/// Summary description for ReportUtil
/// </summary>
public class ReportUtil
{
    #region Constructor
    public ReportUtil()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Provides an object of ReportProperties class by populating all report data
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="dicparameters"></param>
    /// <param name="pageSize"></param>
    /// <param name="paperFormat"></param>
    /// <param name="pdfFilePath"></param>
    /// <returns>ReportProperties</returns>
    public ReportProperties GetReportProperties(string sql,string filedsWithWidth ,string reportCode, Dictionary<string, string> dicparameters, EPageSize pageSize, EPaperFormat paperFormat, string pdfFilePath) // column width must be given as parameter for each field
    {
        ReportProperties oReportProperties = new ReportProperties();

        try
        {
            List<Parameter> parameters = new List<Parameter>();
            foreach (string key in dicparameters.Keys)
            {
                if (key == "username")
                {
                    parameters.Add(new Parameter("Gebruikersnaam", dicparameters[key], EAlign.Left));
                }
                else if (key == "titel")
                {
                    parameters.Add(new Parameter("Titel", dicparameters[key], EAlign.Left));
                }
                else if (key == "showfilter")
                {
                    parameters.Add(new Parameter("Toon filte", dicparameters[key], EAlign.Left));
                }
                else if (key == "curdate")
                {
                    parameters.Add(new Parameter("Datum", dicparameters[key], EAlign.Right));
                }
            }

            DataTable rptTable = new DBUtil().GetDataTable(sql,reportCode);

            List<Column> columns = new List<Column>();
            for (int i = 0; i < rptTable.Columns.Count; i++)
            {
                if (filedsWithWidth == "")
                {
                    columns.Add(new Column(GetCamelFormat(rptTable.Columns[i].ColumnName), EOperation.None, EAlign.Left, oReportProperties.DefaultColumnWidth));
                }
                else
                {
                    columns.Add(new Column(GetCamelFormat(rptTable.Columns[i].ColumnName), EOperation.None, EAlign.Left, getColumnWidth(filedsWithWidth, rptTable.Columns[i].ColumnName)));
                }
            }

            oReportProperties = new ReportProperties(columns, parameters, rptTable, pageSize, paperFormat, pdfFilePath);
        }
        catch (Exception oEx)
        {
            throw oEx;
        }

        return oReportProperties;
    }

    /// <summary>
    /// Provides record count for each group
    /// </summary>
    /// <param name="sql"></param>
    /// <returns>DataTable</returns>
    public DataTable GetRecordCountForEachGroup(string sql, string reportCode)
    {
        try
        {
            return new DBUtil().GetDataTable(sql,reportCode);
        }
        catch (Exception oEx)
        {
            throw oEx;
        }
    }

    /// <summary>
    /// This method generates the pdf report and renders the report 
    /// to the browser.
    /// </summary>
    /// <param name="rptProperties"></param>
    /// <param name="recordCountTableForAGroup"></param>
    /// <param name="response"></param>
    public void GenerateRegularReport(ReportProperties rptProperties, DataTable recordCountTableForAGroup, HttpResponse response)
    {
        try
        {
            if (rptProperties == null)
            {
                return;
            }

            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();

            page = PopulatePagePropertiesByReportProperties(page, rptProperties);

            double PageWidth = page.Width.Value;
            double PageHeight = page.Height.Value;

            double TableYposition = rptProperties.TopMargin;
            double TableBottomYPosition = rptProperties.BottomMargin;

            XGraphics gfx = XGraphics.FromPdfPage(page);
            XPen pen = new XPen(XColor.FromArgb(50, 0, 0, 0), 2);
            XTextFormatter tf = null;

            TableYposition = DrawParameters(rptProperties, gfx, TableYposition, tf, page);

            TableYposition += 5;
            gfx.DrawLine(pen, rptProperties.LeftMargin, TableYposition, PageWidth - rptProperties.RightMargin, TableYposition);

            TableYposition += 20;
            TableYposition += DrawColumns(ref rptProperties, gfx, PageWidth, TableYposition).Height - 5;

            if (rptProperties.Data != null)
            {
                XFont fontData = new XFont("Arial", 8, XFontStyle.Regular);
                tf = new XTextFormatter(gfx);
                tf.Alignment = XParagraphAlignment.Left;

                double NextYposition = TableYposition;

                int recordSum = 0;

                int recordIndex = 0;

                int totalGroupCount = 0;

                if (recordCountTableForAGroup.Rows.Count == 0)
                {
                    totalGroupCount = 1;
                }
                else
                {
                    totalGroupCount = recordCountTableForAGroup.Rows.Count;
                }

                for (int k = 0; k < totalGroupCount; k++)
                {
                    int totalRecords = 0; //int.Parse(recordCountTableForAGroup.Rows[k][0].ToString());

                    if (recordCountTableForAGroup.Rows.Count == 0)
                    {
                        totalRecords = rptProperties.Data.Rows.Count;
                    }
                    else
                    {
                        totalRecords = int.Parse(recordCountTableForAGroup.Rows[k][0].ToString());
                    }

                    recordSum += totalRecords;

                    for (int i = recordIndex; i < recordSum; i++)
                    {
                        //draw serial number for each row.
                        if (i == 11028)
                        {
                            string name = "";
                            int test = 0;
                        }
                        #region Calculation purpose

                        XRect SerialSentenceRect = DrawSentence((i + 1).ToString(), gfx, fontData, rptProperties.TableStartingXPosition - rptProperties.LeftMargin, tf, XBrushes.Black, new XPoint(rptProperties.LeftMargin, NextYposition), false);

                        double NextXposition = rptProperties.TableStartingXPosition;
                        double MaxSentenceHeight = SerialSentenceRect.Height;

                        for (int j = 0; j < rptProperties.Data.Columns.Count; j++)
                        {
                            XRect sentenceRect = DrawSentence(rptProperties.Data.Rows[i][j].ToString(), gfx, fontData, rptProperties.Columns[j].ColumnWidth, tf, XBrushes.Black, new XPoint(NextXposition, NextYposition), false);

                            NextXposition += rptProperties.Columns[j].ColumnWidth;
                            if (sentenceRect.Height > MaxSentenceHeight)
                            {
                                MaxSentenceHeight = sentenceRect.Height;
                            }
                        }
                        #endregion

                        if (NextYposition + MaxSentenceHeight > PageHeight - TableBottomYPosition)
                        {
                            page = document.AddPage();

                            page = PopulatePagePropertiesByReportProperties(page, rptProperties);

                            PageWidth = page.Width.Value;
                            PageHeight = page.Height.Value;

                            TableYposition = rptProperties.TopMargin;
                            TableBottomYPosition = rptProperties.BottomMargin;

                            gfx = XGraphics.FromPdfPage(page);
                            pen = new XPen(XColor.FromArgb(50, 0, 0, 0), 2);
                            tf = new XTextFormatter(gfx);
                            tf.Alignment = XParagraphAlignment.Left;

                            TableYposition += DrawColumns(ref rptProperties, gfx, PageWidth, TableYposition).Height - 5;
                            NextYposition = TableYposition;
                        }

                        #region Draw purpose

                        //SerialSentenceRect is already calculated, so, no need to calculate again
                        SerialSentenceRect = DrawSentence((i + 1).ToString(), gfx, fontData, rptProperties.TableStartingXPosition - rptProperties.LeftMargin, tf, XBrushes.Black, new XPoint(rptProperties.LeftMargin, NextYposition), true);
                        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(120, 170, 170, 170)), SerialSentenceRect.X, SerialSentenceRect.Y, SerialSentenceRect.Width, MaxSentenceHeight);

                        NextXposition = rptProperties.TableStartingXPosition;

                        for (int j = 0; j < rptProperties.Data.Columns.Count; j++)
                        {
                            XRect sentenceRect = DrawSentence(rptProperties.Data.Rows[i][j].ToString(), gfx, fontData, rptProperties.Columns[j].ColumnWidth, tf, XBrushes.Black, new XPoint(NextXposition, NextYposition), true);
                            //draw cellrectangle
                            if (j % 2 != 0)
                            {
                                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(120, 204, 204, 204)), sentenceRect.X, sentenceRect.Y, sentenceRect.Width, MaxSentenceHeight);
                            }
                            else
                            {
                                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(80, 204, 204, 204)), sentenceRect.X, sentenceRect.Y, sentenceRect.Width, MaxSentenceHeight);
                            }

                            NextXposition += rptProperties.Columns[j].ColumnWidth;
                        }

                        #endregion

                        NextYposition += MaxSentenceHeight;

                        #region old code

                        #endregion
                    }

                    if (recordCountTableForAGroup.Rows.Count > 0)
                    {
                        //if (NextYposition <= (PageHeight - TableBottomYPosition - fontData.Height - 10))
                        if (NextYposition <= PageHeight)
                        {
                            string subtotal = "Group Total : " + totalRecords.ToString();

                            XFont fontSubtotal = new XFont("Arial", 8, XFontStyle.Regular);
                            XTextFormatter tfSubtotal = new XTextFormatter(gfx);
                            tfSubtotal.Alignment = XParagraphAlignment.Left;
                            XSize subtotalSize = gfx.MeasureString("Group_Total_:_" + totalRecords.ToString(), fontSubtotal);

                            tf.DrawString(subtotal, fontData, XBrushes.Black, new XRect(rptProperties.LeftMargin, NextYposition, subtotalSize.Width + 20, subtotalSize.Height + 10));

                            NextYposition += subtotalSize.Height + 10;
                        }
                    }

                    recordIndex = recordSum;
                }

                //Print Total group count

                if (NextYposition > PageHeight - TableBottomYPosition - 50)
                {
                    page = document.AddPage();

                    page = PopulatePagePropertiesByReportProperties(page, rptProperties);

                    PageWidth = page.Width.Value;
                    PageHeight = page.Height.Value;

                    TableYposition = rptProperties.TopMargin;
                    TableBottomYPosition = rptProperties.BottomMargin;

                    gfx = XGraphics.FromPdfPage(page);
                    pen = new XPen(XColor.FromArgb(50, 0, 0, 0), 2);
                    tf = new XTextFormatter(gfx);
                    tf.Alignment = XParagraphAlignment.Left;

                    NextYposition = TableYposition;
                }

                XFont fontTotal = new XFont("Arial", 8, XFontStyle.Regular);
                XSize totalInformationSize = gfx.MeasureString("Total_Information:", fontTotal);

                NextYposition += 20;

                gfx.DrawLine(pen, rptProperties.LeftMargin, NextYposition, PageWidth - rptProperties.RightMargin, NextYposition);
                NextYposition += 5;
                tf.DrawString("Total Information:", fontTotal, XBrushes.Black, new XRect(new XPoint(rptProperties.LeftMargin, NextYposition), new XSize(totalInformationSize.Width + 100, totalInformationSize.Height)));
                NextYposition += 15;

                gfx.DrawRectangle(XBrushes.Gray, rptProperties.LeftMargin, NextYposition, PageWidth - rptProperties.RightMargin - rptProperties.LeftMargin, 15);
                //draw text
                XSize totalRecordssize = gfx.MeasureString("Total_Records_:_" + recordSum.ToString(), fontTotal);
                tf.DrawString("Total Records : " + recordSum.ToString(), fontTotal, XBrushes.White, new XRect(rptProperties.LeftMargin, NextYposition + 2, totalRecordssize.Width + 20, totalRecordssize.Height));
            }

            PrintReport(document, response);
        }
        catch (Exception oEx)
        {
            throw oEx;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rptProperties"></param>
    /// <param name="recordCountTableForAGroup"></param>
    /// <param name="response"></param>
    /// <param name="groupList"></param>
    public void GeneratePieChart(ReportProperties rptProperties, HttpResponse response, List<Group> groupList)
    {
        try
        {
            if (rptProperties == null)
            {
                return;
            }

            if (groupList.Count > 0)
            {
                PdfDocument document = new PdfDocument();
                PdfPage page = document.AddPage();

                page = PopulatePagePropertiesByReportProperties(page, rptProperties);

                double PageWidth = page.Width.Value;
                double PageHeight = page.Height.Value;

                double NextYposition = rptProperties.TopMargin;
                double PageBottomYPosition = rptProperties.BottomMargin;

                double pageHeightPercentage = 0.5;
                double ChartLocationOffset = 70; // this is calculated visually

                double defaultPadding = 10;
                double groupInformationWidth = 180;
                double defaultRowHeight = PageHeight * pageHeightPercentage - defaultPadding * 2;


                XGraphics gfx = XGraphics.FromPdfPage(page);
                XPen pen = new XPen(XColor.FromArgb(50, 0, 0, 0), 2);
                XTextFormatter tfLeft = new XTextFormatter(gfx);

                NextYposition = DrawParameters(rptProperties, gfx, NextYposition, tfLeft, page);

                NextYposition += 5;
                gfx.DrawLine(pen, rptProperties.LeftMargin, NextYposition, PageWidth - rptProperties.RightMargin, NextYposition);

                NextYposition += 5;

                #region draw piechart

                ChartFrame chartFrame = new ChartFrame();
                chartFrame.Location = new XPoint(rptProperties.LeftMargin + ChartLocationOffset, NextYposition);
                chartFrame.Size = new XSize(PageWidth, PageHeight * pageHeightPercentage); //chart height=30% of the page height

                ChartingUtil oChartingUtil = new ChartingUtil();

                chartFrame.Add(oChartingUtil.GetPieChart(groupList));

                chartFrame.Draw(gfx, oChartingUtil.GetChartColours(groupList));

                PrintLegend(gfx, groupList, groupInformationWidth, defaultRowHeight, rptProperties, NextYposition + 10, tfLeft);

                #endregion

                NextYposition += PageHeight * pageHeightPercentage + 5;

                gfx.DrawLine(pen, rptProperties.LeftMargin, NextYposition, PageWidth - rptProperties.RightMargin, NextYposition);

                #region draw Group Information

                PrintGroupInformation(gfx, tfLeft, NextYposition, ref document, page, PageWidth,
                    PageHeight, groupList, rptProperties, PageBottomYPosition, pen);

                #endregion

                PrintReport(document, response);
            }
        }
        catch (Exception oEx)
        {
            throw oEx;
        }
    }

    public void GenerateColumnChart(ReportProperties rptProperties, HttpResponse response, List<Group> groupList, string groupBy)
    {
        try
        {
            if (rptProperties == null)
            {
                return;
            }

            groupBy = groupBy == "NONE" ? string.Empty : groupBy;

            if (groupList.Count > 0)
            {
                PdfDocument document = new PdfDocument();
                PdfPage page = document.AddPage();
                PdfPage defaultPage = new PdfPage();

                page = PopulatePagePropertiesByReportProperties(page, rptProperties);

                double PageWidth = page.Width.Value;
                double PageHeight = page.Height.Value;

                double NextYposition = rptProperties.TopMargin;
                double PageBottomYPosition = rptProperties.BottomMargin;

                double pageHeightPercentage = 0.5;
                double defaultPadding = 10;
                double groupInformationWidth = 180;
                double LineOffset = 0;
                double ArrowLength = 5;

                double defaultColumnWidth = page.Width.Value
                    - rptProperties.LeftMargin - rptProperties.RightMargin
                    - groupInformationWidth - 4 * defaultPadding;
                double defaultRowHeight = PageHeight * pageHeightPercentage - defaultPadding * 2;

                double columnWidth = defaultColumnWidth / groupList.Count;
                double RowHeight = defaultRowHeight / MaxGroupValue(groupList);
                double penWidth = 1;

                XGraphics gfx = XGraphics.FromPdfPage(page);
                XPen pen = new XPen(XColor.FromArgb(50, 0, 0, 0), 2);
                XTextFormatter tfLeft = new XTextFormatter(gfx);
                tfLeft.Alignment = XParagraphAlignment.Left;
                XTextFormatter tfCenter = new XTextFormatter(gfx);
                tfCenter.Alignment = XParagraphAlignment.Center;
                XFont fontData = new XFont("Arial", 8, XFontStyle.Regular);

                NextYposition = DrawParameters(rptProperties, gfx, NextYposition, tfLeft, page);

                NextYposition += 5;
                gfx.DrawLine(pen, rptProperties.LeftMargin, NextYposition, PageWidth - rptProperties.RightMargin, NextYposition);

                NextYposition += 5 + defaultPadding * 2;

                #region Draw Column Chart

                XPen penColumnChart = new XPen(XColor.FromKnownColor(XKnownColor.Black), penWidth);

                //draw Vertical Line
                gfx.DrawLine(penColumnChart, rptProperties.LeftMargin + groupInformationWidth + defaultPadding - LineOffset,
                    NextYposition + defaultRowHeight + LineOffset,
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding - LineOffset,
                    NextYposition);

                //draw top arrow
                gfx.DrawLine(penColumnChart,
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding - LineOffset,
                    NextYposition,
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding - LineOffset,
                    NextYposition - ArrowLength * 4);
                gfx.DrawLine(penColumnChart,
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding - LineOffset,
                    NextYposition - ArrowLength * 4,
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding - LineOffset - ArrowLength,
                    NextYposition - 3 * ArrowLength);
                gfx.DrawLine(penColumnChart,
                                rptProperties.LeftMargin + groupInformationWidth + defaultPadding - LineOffset,
                                NextYposition - ArrowLength * 4,
                                rptProperties.LeftMargin + groupInformationWidth + defaultPadding - LineOffset + ArrowLength,
                                NextYposition - 3 * ArrowLength);

                //draw Horizontal Line
                gfx.DrawLine(penColumnChart, rptProperties.LeftMargin + groupInformationWidth + defaultPadding - LineOffset,
                    NextYposition + defaultRowHeight + LineOffset,
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding + defaultColumnWidth,
                    NextYposition + defaultRowHeight + LineOffset);

                //draw right arrow
                gfx.DrawLine(penColumnChart,
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding + defaultColumnWidth,
                    NextYposition + defaultRowHeight + LineOffset,
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding + defaultColumnWidth + ArrowLength * 4,
                    NextYposition + defaultRowHeight + LineOffset);
                gfx.DrawLine(penColumnChart,
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding + defaultColumnWidth + ArrowLength * 4,
                    NextYposition + defaultRowHeight + LineOffset,
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding + defaultColumnWidth + ArrowLength * 3,
                    NextYposition + defaultRowHeight + LineOffset - ArrowLength);
                gfx.DrawLine(penColumnChart,
                   rptProperties.LeftMargin + groupInformationWidth + defaultPadding + defaultColumnWidth + ArrowLength * 4,
                   NextYposition + defaultRowHeight + LineOffset,
                   rptProperties.LeftMargin + groupInformationWidth + defaultPadding + defaultColumnWidth + ArrowLength * 3,
                   NextYposition + defaultRowHeight + LineOffset + ArrowLength);

                //draw bars
                for (int i = 0; i < groupList.Count; i++)
                {
                    gfx.DrawRectangle(new XSolidBrush(groupList[i].GroupColour),
                       rptProperties.LeftMargin + groupInformationWidth + defaultPadding + (double)i * columnWidth,
                           NextYposition + defaultRowHeight - RowHeight * groupList[i].GroupValue,
                           columnWidth,
                           RowHeight * groupList[i].GroupValue);
                }

                //draw group by information

                tfCenter.DrawString(groupBy, fontData, XBrushes.Black, new XRect(
                    rptProperties.LeftMargin + groupInformationWidth + defaultPadding + defaultColumnWidth / 2,
                    NextYposition + defaultRowHeight + LineOffset + defaultPadding / 2,
                    getSizeOfAString(groupBy, gfx, fontData).Width,
                    getSizeOfAString(groupBy, gfx, fontData).Height));

                PrintLegend(gfx, groupList, groupInformationWidth, defaultRowHeight, rptProperties, NextYposition, tfLeft);

                #endregion

                NextYposition += PageHeight * pageHeightPercentage + 10;

                gfx.DrawLine(pen, rptProperties.LeftMargin, NextYposition, PageWidth - rptProperties.RightMargin, NextYposition);

                PrintGroupInformation(gfx, tfLeft, NextYposition, ref document, page,
                    PageWidth, PageHeight, groupList, rptProperties, PageBottomYPosition, pen);

                PrintReport(document, response);
            }
        }
        catch (Exception oEx)
        {
            throw oEx;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// This method renders the report data to the browser.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="response"></param>
    private void PrintReport(PdfDocument document, HttpResponse response)
    {   
        
        //2nd
        try
        {
            MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            response.Clear();
            response.ContentType = "application/pdf";
            response.AddHeader("content-length", stream.Length.ToString());
            response.BinaryWrite(stream.ToArray());
            response.Flush();
            stream.Close();
            response.End();
        }
        catch (Exception oEx)
        {
            throw oEx;
        }
    }

    /// <summary>
    /// This method save the report data to an existing 
    /// pdf file.
    /// </summary>
    /// <param name="reportFilePath"></param>
    /// <param name="document"></param>
    /// <param name="response"></param>
    private void PrintReport(string reportFilePath, PdfDocument document, HttpResponse response)
    {

        //1st
        //if (File.Exists(reportFilePath))
        //{
        //    try
        //    {
        //        document.Save(reportFilePath);

        //        FileStream fs = new FileStream(reportFilePath, FileMode.Open,
        //        FileAccess.Read, FileShare.ReadWrite);
        //        byte[] data = new byte[(int)fs.Length];
        //        fs.Read(data, 0, (int)fs.Length);
        //        fs.Close();
        //        response.Clear();
        //        response.ContentType = "application/pdf";
        //        response.BinaryWrite(data);
        //        response.Flush();
        //        fs.Dispose();
        //        response.End();
        //    }
        //    catch (Exception ex)
        //    {
        //        response.ContentType = "application/pdf";
        //        response.Expires = -1;
        //        response.Buffer = true;
        //        response.Write("<html><pre>" + ex.ToString() +
        //            "</pre></html>");
        //    }
        //}

        //2nd
        //MemoryStream stream = new MemoryStream();
        //document.Save(stream, false);
        //response.Clear();
        //response.ContentType = "application/pdf";
        //response.AddHeader("content-length", stream.Length.ToString());
        //response.BinaryWrite(stream.ToArray());
        //response.Flush();
        //stream.Close();
        //response.End();

        //3rd
        //MemoryStream stream = new MemoryStream();
        //document.Save(stream, false);
        //document.Close();
        //response.Clear();
        //response.AddHeader("content-disposition", "attachment;filename=Report.pdf");
        //response.ContentType = "application/pdf";
        //response.BinaryWrite(stream.ToArray());
        //response.Flush();
        //stream.Close();
        //response.End(); 

        //4th
        try
        {
            if (File.Exists(reportFilePath))
            {
                document.Save(reportFilePath);
                document.Close();

                response.Redirect(reportFilePath.Substring(reportFilePath.LastIndexOf(@"\") + 1));
            }
        }
        catch (Exception oEx)
        {
            throw oEx;
        }
    }

    /// <summary>
    /// Draw columns for a page
    /// </summary>
    /// <param name="rptProperties"></param>
    /// <param name="gfx"></param>
    /// <param name="PageWidth"></param>
    /// <param name="TableYposition"></param>
    /// <returns>XRect</returns>
    private XRect DrawColumns(ref ReportProperties rptProperties, XGraphics gfx, double PageWidth, Double TableYposition)
    {
        XFont fontColumnName = new XFont("Arial", 8, XFontStyle.Regular);
        double MaxHeight = 0;
        if (rptProperties.Columns != null)
        {
            double TableWidth = PageWidth - (rptProperties.TableStartingXPosition + rptProperties.RightMargin);

            XTextFormatter tf = new XTextFormatter(gfx);
            tf.Alignment = XParagraphAlignment.Left;

            double totalColumnWidth = 0;
            for (int i = 0; i < rptProperties.Columns.Count; i++)
            {
                totalColumnWidth += rptProperties.Columns[i].ColumnWidth;
            }

            //set actual columnwidth
            for (int i = 0; i < rptProperties.Columns.Count; i++)
            {
                rptProperties.Columns[i].ColumnWidth = (TableWidth / totalColumnWidth) * rptProperties.Columns[i].ColumnWidth;
            }

            XRect RectColumn = DrawSentence("Serial#", gfx, fontColumnName, rptProperties.TableStartingXPosition - rptProperties.LeftMargin, tf, XBrushes.White, new XPoint(rptProperties.LeftMargin, TableYposition), false);

            MaxHeight = RectColumn.Height;

            double NextXposition = rptProperties.TableStartingXPosition;
            for (int i = 0; i < rptProperties.Columns.Count; i++)
            {
                //for measure purpose
                RectColumn = DrawSentence(rptProperties.Columns[i].ColumnName, gfx, fontColumnName, rptProperties.Columns[i].ColumnWidth, tf, XBrushes.White, new XPoint(NextXposition, TableYposition), false); // this is for measure
                NextXposition += rptProperties.Columns[i].ColumnWidth;
                if (RectColumn.Height > MaxHeight)
                {
                    MaxHeight = RectColumn.Height;
                }
            }

            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(120, 70, 70, 70)), rptProperties.LeftMargin, TableYposition - 5, rptProperties.TableStartingXPosition - rptProperties.LeftMargin, MaxHeight);

            DrawSentence("Serial#", gfx, fontColumnName, rptProperties.TableStartingXPosition - rptProperties.LeftMargin, tf, XBrushes.White, new XPoint(rptProperties.LeftMargin, TableYposition), true);

            NextXposition = rptProperties.TableStartingXPosition;
            for (int i = 0; i < rptProperties.Columns.Count; i++)
            {
                //for draw purpose

                if (i % 2 != 0)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(120, 70, 70, 70)), NextXposition, TableYposition - 5, rptProperties.Columns[i].ColumnWidth, MaxHeight);
                }
                else
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(90, 70, 70, 70)), NextXposition, TableYposition - 5, rptProperties.Columns[i].ColumnWidth, MaxHeight);
                }

                DrawSentence(rptProperties.Columns[i].ColumnName, gfx, fontColumnName, rptProperties.Columns[i].ColumnWidth, tf, XBrushes.White, new XPoint(NextXposition, TableYposition), true);
                NextXposition += rptProperties.Columns[i].ColumnWidth;
            }
        }

        return new XRect(rptProperties.LeftMargin, TableYposition - 5, PageWidth - rptProperties.LeftMargin - rptProperties.RightMargin, MaxHeight);
    }

    /// <summary>
    /// Split a string into words and draw.
    /// If the string does not fit within the column width,
    /// then it breakes the sentence into multiple line
    /// </summary>
    /// <param name="s"></param>
    /// <param name="g"></param>
    /// <param name="f"></param>
    /// <param name="maxWidth"></param>
    /// <param name="tf"></param>
    /// <param name="brush"></param>
    /// <param name="DrawLocation"></param>
    /// <param name="isDraw"></param>
    /// <returns>XRect</returns>
    private XRect DrawSentence(string s, XGraphics g, XFont f, double maxWidth, XTextFormatter tf, XBrush brush, XPoint DrawLocation, bool isDraw)
    {
        s = s.Trim();
	    s = s.Replace("\r\n", " ");
        s = s.Replace("\n", " ");
        s = s.Replace("\r", " ");
        string[] words = s.Split(new char[] { ' ' }); // split sentence into words by using spaces as delimiters
        int lineCount = 1;  // start on first line

        string line = "";
        for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
        {
            if (string.IsNullOrEmpty(words[wordIndex]))
            {
                continue;
            }
	    string candidateLine = line + words[wordIndex] + " ";
            // see whether the candidate fits onto a line
            XSize lineSize = getSizeOfAString(candidateLine, g, f);
            if (lineSize.Width > maxWidth)
            {
                // it does not fit, so put it at the start of next line
                if (line.Trim() != string.Empty)
                {
                    if (getSizeOfAString(line, g, f).Width <= maxWidth)
                    {
                        if (isDraw)
                        {
                            tf.DrawString(line, f, brush, new XRect(new XPoint(DrawLocation.X, DrawLocation.Y + (lineCount - 1) * f.Height), getSizeOfAString(line, g, f)));
                        }
                        lineCount++;
                    }
                    else
                    {
                        int TotalLine = 0;
                        DrawString(line, g, f, maxWidth, tf, brush, new XPoint(DrawLocation.X, DrawLocation.Y + (lineCount - 1) * f.Height), out TotalLine, isDraw);
                        lineCount += TotalLine;
                    }
                }

                line = words[wordIndex] + " ";
            }
            else
            {
                // it does fit, so the candidate line becomes the new line
                line = candidateLine;
            }
            // in either case the word has been placed so it is incremented in for loop
        }

        if (line.Trim() != string.Empty)
        {
            if (getSizeOfAString(line, g, f).Width <= maxWidth)
            {
                if (isDraw)
                {
                    tf.DrawString(line, f, brush, new XRect(new XPoint(DrawLocation.X, DrawLocation.Y + (lineCount - 1) * f.Height), getSizeOfAString(line, g, f)));
                }

                lineCount++;
            }
            else
            {
                int TotalLine = 0;
                DrawString(line, g, f, maxWidth, tf, brush, new XPoint(DrawLocation.X, DrawLocation.Y + (lineCount - 1) * f.Height), out TotalLine, isDraw);
                lineCount += TotalLine;
            }
        }

        double height = lineCount * f.Height;
        return new XRect(DrawLocation, new XSize(maxWidth, height));
    }

    /// <summary>
    /// Split a string into character.
    /// If the string does not fit within the column width,
    /// then it breakes the string into multiple line
    /// </summary>
    /// <param name="s"></param>
    /// <param name="g"></param>
    /// <param name="f"></param>
    /// <param name="maxWidth">Max Column Width</param>
    /// <param name="tf"></param>
    /// <param name="brush"></param>
    /// <param name="DrawLocation"></param>
    /// <param name="TotalLine"></param>
    /// <param name="isDraw"></param>
    /// <returns>XRect</returns>
    private XRect DrawString(string s, XGraphics g, XFont f, double maxWidth, XTextFormatter tf, XBrush brush, XPoint DrawLocation, out int TotalLine, bool isDraw)
    {
        s = s.Trim();
	TotalLine = 1;

        char[] arr = s.ToCharArray();
        int lineCount = 1;  // start on first line

        string line = "";

        for (int i = 0; i < arr.Length; i++)
        {
            string candidateLine = line + arr[i].ToString();
            // see whether the candidate fits onto a line
            XSize lineSize = getSizeOfAString(candidateLine, g, f);
            if (lineSize.Width > maxWidth)
            {
                // it does not fit, so put it at the start of next line
                if (line.Trim() != string.Empty)
                {
                    if (isDraw)
                    {
                        tf.DrawString(line, f, brush, new XRect(new XPoint(DrawLocation.X, DrawLocation.Y + (lineCount - 1) * f.Height), getSizeOfAString(line, g, f)));
                    }

                    lineCount++;
                }

                line = arr[i].ToString();
            }
            else
            {
                // it does fit, so the candidate line becomes the new line
                line = candidateLine;
            }
        }

        if (line.Trim() != string.Empty)
        {
            if (isDraw)
            {
                tf.DrawString(line, f, brush, new XRect(new XPoint(DrawLocation.X, DrawLocation.Y + (lineCount - 1) * f.Height), getSizeOfAString(line, g, f)));
            }
        }

        double height = lineCount * f.Height;

        TotalLine = lineCount;

        return new XRect(DrawLocation, new XSize(maxWidth, height));
    }

    /// <summary>
    /// Make the first character of a string uppercase and
    /// rest of character to lower case.
    /// </summary>
    /// <param name="s"></param>
    /// <returns>string</returns>
    private string GetCamelFormat(string s)
    {
        string sUpper = s;
        string sLower = string.Empty;

        if (s.Length > 0)
        {
            sUpper = s[0].ToString().ToUpper();
        }

        if (s.Length > 1)
        {
            sLower = s.Substring(1).ToLower();
        }

        return sUpper + sLower;
    }

    /// <summary>
    /// Measure the size of a string
    /// </summary>
    /// <param name="s">String to be measured</param>
    /// <param name="gfx">XGraphics</param>
    /// <param name="f">XFont</param>
    /// <returns>XSize</returns>
    private XSize getSizeOfAString(string s, XGraphics gfx, XFont f)
    {
        s = s.Replace(' ', '_');

        return gfx.MeasureString(s, f);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rptProperties"></param>
    /// <param name="gfx"></param>
    /// <param name="parameterStartingYPosition"></param>
    /// <param name="tf"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    private double DrawParameters(ReportProperties rptProperties, XGraphics gfx, double parameterStartingYPosition, XTextFormatter tf, PdfPage page)
    {
        double TableYposition = parameterStartingYPosition;

        if (rptProperties.Parameters != null)
        {
            XFont fontHeader = new XFont("Arial", 8, XFontStyle.Regular);
            tf = new XTextFormatter(gfx);

            //print left parameters
            tf.Alignment = XParagraphAlignment.Left;

            double HeightIndicator = 0;

            for (int i = 0; i < rptProperties.Parameters.Count; i++)
            {
                if (rptProperties.Parameters[i].Align == EAlign.Left)
                {
                    if (rptProperties.Parameters[i].Value.ToLower().Equals("null") || string.IsNullOrEmpty(rptProperties.Parameters[i].Value))
                    {
                        continue;
                    }

                    string parameter = rptProperties.Parameters[i].Name + " : " + rptProperties.Parameters[i].Value;
                    XSize parametersize = gfx.MeasureString(rptProperties.Parameters[i].Name + "_:_" + rptProperties.Parameters[i].Value, fontHeader);
                    tf.DrawString(parameter, fontHeader, XBrushes.Black, new XRect(rptProperties.LeftMargin, rptProperties.TopMargin + parametersize.Height * HeightIndicator, parametersize.Width + 100, parametersize.Height));

                    //set TableStartingYposition
                    TableYposition += parametersize.Height;

                    HeightIndicator++;
                }
            }

            //print right parameters
            tf.Alignment = XParagraphAlignment.Right;
            HeightIndicator = 0;

            for (int i = 0; i < rptProperties.Parameters.Count; i++)
            {
                if (rptProperties.Parameters[i].Align == EAlign.Right)
                {
                    if (rptProperties.Parameters[i].Value.ToLower().Equals("null") || string.IsNullOrEmpty(rptProperties.Parameters[i].Value))
                    {
                        continue;
                    }

                    string parameter = rptProperties.Parameters[i].Name + " : " + rptProperties.Parameters[i].Value;
                    XSize parametersize = gfx.MeasureString(rptProperties.Parameters[i].Name + "_:_" + rptProperties.Parameters[i].Value, fontHeader);
                    tf.DrawString(parameter, fontHeader, XBrushes.Black, new XRect(new XPoint(page.Width - rptProperties.RightMargin - parametersize.Width, rptProperties.TopMargin + parametersize.Height * HeightIndicator), parametersize));

                    //for test right align tf.DrawString(parameter + "test", fontHeader, XBrushes.Black, new XRect(new XPoint(PageWidth - rptProperties.RightMargin - gfx.MeasureString(parameter + "test", fontHeader).Width, rptProperties.TopMargin + parametersize.Height * HeightIndicator + 20), gfx.MeasureString(parameter + "test", fontHeader)));

                    //set TableYposition
                    if (TableYposition < rptProperties.TopMargin + parametersize.Height * HeightIndicator)
                    {
                        TableYposition = rptProperties.TopMargin + parametersize.Height * HeightIndicator;
                    }

                    HeightIndicator++;
                }
            }
        }

        return TableYposition;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="page"></param>
    /// <param name="rptProperties"></param>
    /// <returns></returns>
    private PdfPage PopulatePagePropertiesByReportProperties(PdfPage page, ReportProperties rptProperties)
    {
        //Set the page size
        switch (rptProperties.PageSize)
        {
            case EPageSize.A4:
                page.Size = PdfSharp.PageSize.A4;
                break;
            case EPageSize.A3:
                page.Size = PdfSharp.PageSize.A3;
                break;
        }

        //get page height & width
        switch (rptProperties.PaperFormat)
        {
            case EPaperFormat.LandScape:
                page.Orientation = PdfSharp.PageOrientation.Landscape;
                break;
            case EPaperFormat.Portrait:
                page.Orientation = PdfSharp.PageOrientation.Portrait;
                break;
        }

        return page;
    }

    /// <summary>
    /// Get default page properties according to page orientation
    /// The default page is assumed to be A4
    /// </summary>
    /// <param name="page"></param>
    /// <param name="rptProperties"></param>
    /// <returns></returns>
    //private PdfPage GetDefaultPageProperties(PdfPage page, ReportProperties rptProperties)
    //{
    //    page.Size = PdfSharp.PageSize.A4;

    //    //get page height & width
    //    switch (rptProperties.PaperFormat)
    //    {
    //        case EPaperFormat.LandScape:
    //            page.Orientation = PdfSharp.PageOrientation.Landscape;
    //            break;
    //        case EPaperFormat.Portrait:
    //            page.Orientation = PdfSharp.PageOrientation.Portrait;
    //            break;
    //    }

    //    return page;
    //}

    private double MaxGroupValue(List<Group> oListGroup)
    {
        double MaxValue = 0;

        for (int i = 0; i < oListGroup.Count; i++)
        {
            if (oListGroup[i].GroupValue >= MaxValue)
            {
                MaxValue = oListGroup[i].GroupValue;
            }
        }

        return MaxValue;
    }

    private void PrintGroupInformation(XGraphics gfx, XTextFormatter tfLeft, double NextYposition, ref PdfDocument document, PdfPage page, double PageWidth, double PageHeight, List<Group> groupList, ReportProperties rptProperties, double PageBottomYPosition, XPen pen)
    {
        NextYposition += 15;

        XFont fontData = new XFont("Arial", 8, XFontStyle.Regular);
        tfLeft = new XTextFormatter(gfx);
        tfLeft.Alignment = XParagraphAlignment.Left;

        XSize stringSize = getSizeOfAString("Group Information:", gfx, fontData);
        tfLeft.DrawString("Group Information:", fontData, XBrushes.Black, new XRect(rptProperties.LeftMargin, NextYposition, stringSize.Width, stringSize.Height));

        NextYposition += 20;

        XTextFormatter tfRight = new XTextFormatter(gfx);
        tfRight.Alignment = XParagraphAlignment.Right;

        double groupTotalSum = 0;

        for (int i = 0; i < groupList.Count; i++)
        {
            groupTotalSum += groupList[i].GroupValue;

            stringSize = getSizeOfAString(groupList[i].GroupName, gfx, fontData);
            tfLeft.DrawString(groupList[i].GroupName, fontData, XBrushes.Black, new XRect(rptProperties.LeftMargin, NextYposition, stringSize.Width, stringSize.Height));

            stringSize = getSizeOfAString(Convert.ToInt32(groupList[i].GroupValue).ToString(), gfx, fontData);
            tfRight.DrawString(Convert.ToInt32(groupList[i].GroupValue).ToString(), fontData, XBrushes.Black, new XRect(PageWidth - rptProperties.RightMargin - stringSize.Width, NextYposition, stringSize.Width, stringSize.Height));

            NextYposition += 12;

            if (i < (groupList.Count - 1) && NextYposition > (PageHeight - PageBottomYPosition))
            {
                page = document.AddPage();
                page = PopulatePagePropertiesByReportProperties(page, rptProperties);

                PageWidth = page.Width.Value;
                PageHeight = page.Height.Value;

                NextYposition = rptProperties.TopMargin;
                PageBottomYPosition = rptProperties.BottomMargin;

                gfx = XGraphics.FromPdfPage(page);
                pen = new XPen(XColor.FromArgb(50, 0, 0, 0), 2);
                tfLeft = new XTextFormatter(gfx);
                tfLeft.Alignment = XParagraphAlignment.Left;
                tfRight = new XTextFormatter(gfx);
                tfRight.Alignment = XParagraphAlignment.Right;

                stringSize = getSizeOfAString("Group Information:", gfx, fontData);
                tfLeft.DrawString("Group Information:", fontData, XBrushes.Black, new XRect(rptProperties.LeftMargin, NextYposition, stringSize.Width, stringSize.Height));
                NextYposition += 20;
            }
        }

        //Print Total group count

        if (NextYposition > PageHeight - PageBottomYPosition - 20)
        {
            page = document.AddPage();
            page = PopulatePagePropertiesByReportProperties(page, rptProperties);

            PageWidth = page.Width.Value;
            PageHeight = page.Height.Value;

            NextYposition = rptProperties.TopMargin;
            PageBottomYPosition = rptProperties.BottomMargin;

            gfx = XGraphics.FromPdfPage(page);
            pen = new XPen(XColor.FromArgb(50, 0, 0, 0), 2);
            tfLeft = new XTextFormatter(gfx);
            tfLeft.Alignment = XParagraphAlignment.Left;
            tfRight = new XTextFormatter(gfx);
            tfRight.Alignment = XParagraphAlignment.Right;
        }

        NextYposition += 10;
        gfx.DrawLine(pen, rptProperties.LeftMargin, NextYposition, PageWidth - rptProperties.RightMargin, NextYposition);
        NextYposition += 10;

        stringSize = getSizeOfAString("Total", gfx, fontData);
        tfLeft.DrawString("Total", fontData, XBrushes.Black, new XRect(rptProperties.LeftMargin, NextYposition, stringSize.Width, stringSize.Height));

        stringSize = getSizeOfAString(Convert.ToInt32(groupTotalSum).ToString(), gfx, fontData);
        tfRight.DrawString(Convert.ToInt32(groupTotalSum).ToString(), fontData, XBrushes.Black, new XRect(PageWidth - rptProperties.RightMargin - stringSize.Width, NextYposition, stringSize.Width, stringSize.Height));
    }

    private void PrintLegend(XGraphics gfx, List<Group> groupList, double groupInformationWidth, double defaultRowHeight, ReportProperties rptProperties, double NextYposition, XTextFormatter tfLeft)
    {
        double fontSize = 8;
        XFont legendFont = new XFont("Arial", fontSize, XFontStyle.Regular);
        double LegendGap = 5;
        double TextGap = 5;
        double LegendHeightWidth = groupList.Count > 0 ? getSizeOfAString(groupList[0].GroupName, gfx, legendFont).Height + LegendGap : 10 + LegendGap;
        int TotalGroupCount = groupList.Count;
        double GroupInfoTotalHeight = TotalGroupCount * LegendHeightWidth;
        int TotalGroupColumn = 1;
        int groupsPerColumn = groupList.Count;
        double perColumnWidth = groupInformationWidth;
        double totalRecordCount = GetTotalRecordCount(groupList);

        if (GroupInfoTotalHeight > defaultRowHeight)
        {
            LegendGap = 1.5;
            TextGap = 2;

            TotalGroupColumn = 2;
            TotalGroupCount = groupList.Count % TotalGroupColumn == 0 ? groupList.Count : groupList.Count + 1;

            double NewLegendHeightWidth = (defaultRowHeight * LegendHeightWidth) / (GroupInfoTotalHeight / TotalGroupColumn);
            double NewfontSize = NewLegendHeightWidth > LegendGap ? NewLegendHeightWidth - LegendGap : NewLegendHeightWidth;
            LegendHeightWidth = NewLegendHeightWidth > LegendHeightWidth ? LegendHeightWidth : NewLegendHeightWidth;
            fontSize = NewfontSize > fontSize ? fontSize : NewfontSize;
            legendFont = new XFont("Arial", fontSize, XFontStyle.Regular);
            groupsPerColumn = TotalGroupCount / TotalGroupColumn;
            perColumnWidth = perColumnWidth / TotalGroupColumn;
        }

        #region Calculate the fontsize for max width group name

        XSize maxGroupNameSize = GetMaxGroupNameSize(groupList, legendFont, totalRecordCount, gfx);

        if (maxGroupNameSize.Width > perColumnWidth - TextGap - LegendHeightWidth)
        {
            fontSize = (fontSize * (perColumnWidth - TextGap - LegendHeightWidth)) / maxGroupNameSize.Width;
            legendFont = new XFont("Arial", fontSize, XFontStyle.Regular);
        }

        #endregion

        int j = 0;
        int k = 0;

        for (int i = 0; i < TotalGroupColumn; i++)
        {
            int heightIndicator = 0;

            for (k = j; k < groupsPerColumn; k++)
            {
                string groupName = groupList[k].GroupName + " (" + string.Format("{0:0.00}",
                    (groupList[k].GroupValue / totalRecordCount) * 100) + "%" + ")";

                XSize groupNameSize = getSizeOfAString(groupName, gfx, legendFont);

                gfx.DrawRectangle(new XSolidBrush(groupList[k].GroupColour),
                    rptProperties.LeftMargin + perColumnWidth * i,
                    NextYposition + heightIndicator * LegendHeightWidth,
                    LegendHeightWidth - LegendGap,
                    LegendHeightWidth - LegendGap);

                tfLeft.DrawString(groupName
               , legendFont, XBrushes.Black,
                   new XRect(rptProperties.LeftMargin + perColumnWidth * i + LegendHeightWidth - LegendGap + TextGap,
                       NextYposition + heightIndicator * LegendHeightWidth,
                       perColumnWidth - TextGap,
                       groupNameSize.Height));

                heightIndicator++;
            }
            j = k;
            if (i == TotalGroupColumn - 2)
            {
                groupsPerColumn = groupList.Count;
            }
        }
    }

    private double GetTotalRecordCount(List<Group> chartGroup)
    {
        double totalCount = 0;

        for (int i = 0; i < chartGroup.Count; i++)
        {
            totalCount += chartGroup[i].GroupValue;
        }

        return totalCount;
    }

    private XSize GetMaxGroupNameSize(List<Group> groupList, XFont legendFont, double totalRecordCount, XGraphics gfx)
    {
        XSize maxGroupNameSize = new XSize(0, 0);

        for (int i = 0; i < groupList.Count; i++)
        {
            string groupName = groupList[i].GroupName + " (" + string.Format("{0:0.00}",
                    (groupList[i].GroupValue / totalRecordCount) * 100) + "%" + ")";

            XSize groupNameSize = getSizeOfAString(groupName, gfx, legendFont);

            maxGroupNameSize = groupNameSize.Width > maxGroupNameSize.Width ? groupNameSize : maxGroupNameSize;
        }

        return maxGroupNameSize;
    }

    private double getColumnWidth(string Fields,string ColumnName)
    {
        string[] Field = Fields.ToUpper().Split(',');
        double ColumnWidth = 0.0;

        foreach (string item in Field)
        { 
            if(item.ToUpper().Contains(ColumnName.ToUpper()))
            {
                string[] Width = item.Split('*');
                ColumnWidth = Convert.ToDouble(Width[1]);
                break;

        
            }
        }

        return ColumnWidth;
    }

    #endregion
}
