<%@ Page Language="C#" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="HIT.OB.STD.Wrapper.DAL" %>
<%@ Import Namespace="HIT.OB.STD.Wrapper.BLL" %>
<%@ Import Namespace="System.IO" %>
<head>
    <style type="text/css">
        .style1
        {
            height: 26px;
            width: 131px;
        }
    </style>
</head>
<table align="center" cellspacing="20" style="width: 50%">
    <tr>
        <td style="height: 123px" colspan="4">
            <fieldset>
                <legend>Document Gegevens</legend>
                <table align="center" cellspacing="4" style="margin: 8px 0px">
                    <%             
                                          
                        string keyList = Request.QueryString["KEYLIST"].ToString();
                        string valueList = Request.QueryString["VALUELIST"].ToString();                        
                        string reportCode = Request.QueryString["REPORT_CODE"];

                        
                        DataTable dtDetailInfo;
                        DBManagerFactory dbManagerFactory = new DBManagerFactory();
                        IWrapFunctions iWrapFunc = dbManagerFactory.GetDBManager();
                        DataRow drRepportConfigInfo = iWrapFunc.GetReportConfigInfo(reportCode);      
                        
                        string fieldCaps = drRepportConfigInfo["field_caps"].ToString();
                        string sqlFrom = drRepportConfigInfo["SQL_FROM"].ToString();
                        string[] capsList = fieldCaps.Split(';');
                        string matcode = string.Empty;
                        string caption;
                        string fileSubpath = string.Empty;
                        string selectedFields = drRepportConfigInfo["detail_fieldsets"].ToString().Trim(new char[] { ';' }).Replace(';', ',');

                        HIT.OB.STD.Core.BLL.DBManagerFactory dbManagerFactoryCore = new HIT.OB.STD.Core.BLL.DBManagerFactory();
                        HIT.OB.STD.Core.DAL.IOBFunctions iCoreFunctions = dbManagerFactoryCore.GetDBManager(reportCode);
                        dtDetailInfo = iCoreFunctions.GetSpecificRowDetailData(sqlFrom, selectedFields, keyList, valueList);
                        if (dtDetailInfo.Rows.Count > 0)
                        {
                            foreach (System.Data.DataColumn dcItem in dtDetailInfo.Columns)
                            {
                                caption = HIT.OB.STD.Wrapper.CommonFunctions.GetCaption(dcItem.ColumnName, capsList);
                                                                            
                    %>
                    <tr>
                         <td class="Detail" nowrap>
                            <%=caption%>
                        </td>
                         <td class="Detail">
                            :&nbsp;&nbsp;
                            <% if ((reportCode.Equals("NAW_BEST") && (caption.ToLower().Equals("opmerking"))) || (reportCode.Equals("PERSONEN") && (caption.ToLower().Equals("verslag") || caption.ToLower().Equals("standaard") || caption.ToLower().Equals("bedrijftyp"))))
                               { %>
                                <textarea readonly rows=15 cols=135 class="textarea" ><%= dtDetailInfo.Rows[0][dcItem]%> </textarea>
                            <% }
                               else
                               { %>
                                 <%= dtDetailInfo.Rows[0][dcItem]%>
                                
                            <%} %>
                        </td>
                    </tr>     
                   
                    <%                            
                        }
                        }                			
                    %>
                </table>
            </fieldset>
        </td>
    </tr>
    
</table>
