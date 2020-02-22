<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Object Browser</title>
    <!-- ***************  CSS Links ***************** -->
    <link href="ext/resources/css/ext-all.css" rel="stylesheet" type="text/css" />
    <link href="styles/obrowser.css" rel="stylesheet" type="text/css" />
    <!--[if IE 6]>
        <link href="styles/ie6.css" rel="stylesheet" type="text/css" />
    <![endif]-->

    <script type="text/javascript">
          var kitServerPath = "<%= kitServerPath %>";
          kitServerUrl = unescape(kitServerPath);          
          var LOGIN_URL = kitServerPath + "/Default.aspx";
          
          var SIDValid = eval("<%= SIDValid %>");
          var REPEATER = eval("<%= repeater %>");
          
          var authInformation = "<%= authInformation %>";
          authInformation = unescape(authInformation);
          if (authInformation) {
              authInformation = eval('(' + authInformation + ')');
          }
                  
          window.onunload = CookieUpdate;

          function CookieUpdate() {
              SaveUserSettingsInCookie();
          }

          function GetOBServiceUrl() {
              return '<%= Page.ResolveUrl("~/OBServices.asmx") %>';
          }

          function GetWrapperServiceUrl() {
              return '<%= Page.ResolveUrl("~/WrapperServices.asmx") %>';
          }


          window.onerror = function() 
          {
              //code to run when error has occured on page
              try {
                  if (arguments[0].indexOf("REPORT") > -1) {
                      alert("There is no report configured for this user");
                  }
              } catch (e) {

              }
          }
    </script>

    <!-- ***************  Javascript Links ***************** -->

    <script src="ext/adapter/ext/ext-base.js" type="text/javascript"></script>

    <script src="ext/ext-all.js" type="text/javascript"></script>

    <script src="Scripts/Common.js" type="text/javascript"></script>

    <script src="Scripts/SecurityManager.js" type="text/javascript"></script>

    <script src="Scripts/Core/OBrowser.js" type="text/javascript"></script>

    <script src="Scripts/Core/Navigations.js" type="text/javascript"></script>

    <script src="Scripts/OBController.js" type="text/javascript"></script>

    <script src="Scripts/Core/ext-override.js" type="text/javascript"></script>

    <script src="Scripts/Wrapper/SettingsProcessor.js" type="text/javascript"></script>

    <script src="Scripts/Wrapper/Wrapper.js" type="text/javascript"></script>

    <script src="Scripts/Wrapper/Toolbar.js" type="text/javascript"></script>

    <script src="Scripts/Wrapper/WrapperServiceProxy.js" type="text/javascript"></script>

    <script type="text/javascript" for="ADViewer" event="OnEndLoadItem(bstrItemType,vData,vResult)">     
      if (bstrItemType == 'DOCUMENT')
      {         
      
        // var ADViewer = document.getElementById("ADViewer");
        // var ECompViewer = ADViewer.ECompositeViewer;
        //ECompViewer.ToolbarVisible = false;     	       
        //ECompViewer.MarkupsVisible = false;
                  
      }      


    </script>

</head>
<body id="docbody">
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnableScriptGlobalization="true"
        EnableScriptLocalization="true">
        <Services>
            <asp:ServiceReference InlineScript="true" Path="~/OBServices.asmx" />
            <asp:ServiceReference InlineScript="true" Path="~/WrapperServices.asmx" />
        </Services>
    </asp:ScriptManager>
    <div id="tabContainer">
    </div>
    <div id="nonReportTabPanel1" class="hideDiv" style="overflow: auto">
    </div>
    <div id="nonReportTabPanel2" class="hideDiv" style="overflow: auto">
    </div>
    <div id="nonReportTabPanel3" class="hideDiv" style="overflow: auto">
    </div>
    <div id="reportContainer" style="width: 100%;">
        <div id="header-wrap">
            <div id="header-container">
                <div id="header">
                    <table style="width: 100%">
                        <tr>
                            <td>
                                <div style="float: left">
                                    Report:
                                    <select name="drpReportList" style="width: 90px;" id="drpReportList" onchange="LoadReportArguments()">
                                    </select>
                                    Group By:
                                    <select id="drpGroupBy" style="width: 130px;" onchange="InitReport()">
                                    </select>
                                </div>
                                <div style="float: right; padding-right: 15px">
                                    <div id="divAddUser" style="float: left; margin-right: 2px;">
                                        <a onclick="OpenAddUserForm()">
                                            <img src="./images/user.png" style="padding-top: 5px;" title="Add User" /></a>
                                    </div>
                                    <div id="divSaveColor" style="float: left; margin-right: 2px; display: none;">
                                        <a onclick="SaveSelectedThemeColors()">
                                            <img src="./images/save-colors.png" style="padding-top: 5px;" title="Save Color" /></a>
                                    </div>
                                    <div id="div1" style="float: left; margin-right: 2px">
                                        <a onclick="ADD(event)">
                                            <img src="./images/add.png" style="padding-top: 5px;" title="Add new record" /></a>
                                    </div>
                                    <div id="divSaveReportSettings" style="float: left; margin-right: 2px">
                                        <a onclick="SaveUsersCurrentSettings()">
                                            <img src="./images/save-settings.png" style="padding-top: 5px;" title="Save Settings" /></a>
                                    </div>
                                    <div id="divReport" style="float: left; margin-right: 2px">
                                        <a onclick="OpenReportOption()">
                                            <img src="./images/report.png" style="padding-top: 5px;" title="Create Report" /></a>
                                    </div>
                                    <div id="divThemeColor" style="float: left; margin-right: 2px">
                                        <a name="cmdTheme" onclick="ShowThemeColor()">
                                            <img id="Img2" src="./images/themecolor.png" style="padding-top: 5px;" title="Theme Color" /></a>
                                    </div>
                                    <div style="float: left;">
                                        <a onclick="OpenQueryBuilder()">
                                            <img src="./images/querybuilder.gif" style="padding-top: 5px;" title="Query Builder" /></a>
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>
        <div id="Obrowser">
            <div id="gridContainer" style="background-color: #eeffdd; position: relative; overflow: hidden;
                top: 1px; left: 1px;">
            </div>
        </div>
    </div>
    <div id="footer-wrap">
        <div id="footer-container">
            <div id="footer">
                <div style="float: left; width: 275px;">
                    <a onclick="OBSettings.GotoFirstPage()">
                        <img src="./images/nav_firstpage.gif" style="padding-top: 5px;" title="First page" /></a>
                    <a onclick="OBSettings.GotoPreviousPage()">
                        <img src="./images/nav_prevpage.gif" title="Previous page" /></a> <a onclick="OBSettings.GotoPrevRow()">
                            <img src="./images/nav_prevrow.gif" title="Previous Item" /></a>
                    <input type="text" id="txtSelectedRow" style="width: 25px; height: 15px;" value="1" />
                    <a onclick="OBSettings.GotoNextRow()">
                        <img src="./images/nav_nextrow.gif" title="Next Item" /></a> <a onclick="OBSettings.GotoNextPage()">
                            <img src="./images/nav_nextpage.gif" title="Next page" /></a> <a onclick="OBSettings.GotoLastPage()">
                                <img src="./images/nav_lastpage.gif" title="Last page" /></a> of
                    <label id="lblTotalRow">
                    </label>
                    &nbsp;&nbsp;
                    <input type="text" value="" style="width: 25px; height: 15px;" id="txtGotoPage" />
                    <a onclick="OBSettings.GotoPage()">
                        <img id="Img1" src="./images/gotopage.png" style="margin-left: -5px;" title="Goto Page" /></a>
                    <a href="#">
                        <img id="Img3" src="./images/separator.png" style="margin-left: 3px;" /></a>
                </div>
                <div id="divSearch" style="float: left; margin-left: 5px;">
                    <label id="lblSortedFieldName">
                    </label>
                    &nbsp;
                    <select id="quickSearchOperator" style="height: 20px;">
                    </select>
                    <input id="txtSearch" type="text" style="width: 98px;" />
                    <a onclick="OBSettings.QuickSearchOnUserData()">
                        <img src="./images/filter.png" style="margin-left: -5px;" title="Add to filter" /></a>
                    <a onclick="OBSettings.ClearFilterString()">
                        <img src="./images/clear.png" style="margin-left: -8px;" id="btnClearFilter" title="Clear filter" /></a>
                </div>
                <div id="dvImage" style="float: left; display: none; margin-top: 3px">
                    <a href="javascript:DownloadTemplate()">
                        <img id="img5" src="Images/download2.png" style="margin-left: 3px;" alt="Download Template" title="Download Template" /></a>
                    <a href="javascript:UploadDocument()">
                        <img id="imgUpload" src="Images/upload.png" style="margin-left: 3px;" alt="Upload Document" title="Upload Document" /></a>
                </div>
                <div style="float: right; padding-right: 5px;">
                    <label>
                        Page Size:</label>
                    <input type="text" id="txtPageSize" style="width: 30px;" value="25" />
                    <a onclick="OBSettings.RefreshPage()">
                        <img id="btnRefresh" src="./images/refresh.gif" style="padding-top: 5px;" title="Refresh" /></a>
                </div>
            </div>
        </div>
    </div>
    <iframe id="iframUploadRedline" height="0px" width="0px"></iframe>
    <asp:HiddenField ID="hdnUserName" runat="server"></asp:HiddenField>
    <iframe id="iframeDownload" height="0px" width="0px"></iframe>
    </form>
</body>
</html>
