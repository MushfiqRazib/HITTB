function GetCurrentReportSettings()
{
    var fieldWidth;
    var selectedFieldsWidths = GetCurrentVisibleFieldWidths().split(';');
    var vis_fields = '{"visible_fields":[';
    for(var i=0;i<selectedFieldsWidths.length;i++)
    {
        fieldWidth = selectedFieldsWidths[i].split("=");
        vis_fields += '{"Name":"'+fieldWidth[0]+'","Width":"'+fieldWidth[1]+'"},';
    }
    
    vis_fields = vis_fields.substring(0,vis_fields.length-1);
    vis_fields += ']';
         
    vis_fields +=   ',"qb_custom_fields":"'+OBSettings.QB_CUSTOM_FIELDS+
                    '","qb_gb_select":"'+OBSettings.QB_GB_SELECT_CLAUSE+
                    '","sql_group_by":"'+OBSettings.SQL_GROUP_BY+
                    '","page_size":'+OBSettings.PAGE_SIZE+'}';
 
    return vis_fields;
}

function SaveUsersCurrentSettings()
{    
    try
    {    
        var currentGrid = OBSettings.GetActiveGrid(); 
        if(currentGrid)
        {
            var report_settings = GetCurrentReportSettings();       
            var params  =  "{REPORT_CODE:'" + OBSettings.REPORT_CODE + "', " +
                            "SQL_WHERE:\"" + OBSettings.SQL_WHERE + "\", " +
                            "GROUP_BY:'" + OBSettings.SQL_GROUP_BY + "', " +
                            "ORDER_BY:'" + OBSettings.SQL_ORDER_BY + "', " +
                            "ORDER_BY_DIR:'" + OBSettings.SQL_ORDER_DIR + "', " +
                            "report_settings:'" + report_settings +  "' }";
                            
            var serviceName = "UpdateUserDefinedReportSettings";
            var isSuccess = GetSyncJSONResult_Wrapper(serviceName, params);
            if(isSuccess)
            {
                alert('Report settings saved to database successfully');
            }
        }else
        {
            alert("No settings available to save");
        }
     }
     catch(e)
     { 
        
     }   
}

function SaveSelectedThemeColors(HeaderChekBoxName)
{	              
    var currentGrid = OBSettings.GetActiveGrid(); 
    if(currentGrid)
    {
        var groupCode = '';
        var colorCode = '';
        var selectedRecords = OBSettings.groupedGrid.getSelectionModel().getSelections();                                
        //*** Add field data to the collection 
        for(var i=0; i<selectedRecords.length; i++)
        {
            colorCode += selectedRecords[i].data.THEME_COLOR.split('colorcode=')[1].split('>')[0].replace(/"/,'').replace(/"/,'') + ",";
            groupCode += selectedRecords[i].data[OBSettings.SQL_GROUP_BY] + ",";                          
        }    
       
        if(colorCode == '')
        {
            return;
        }

        var params  =  "{ REPORT_CODE:'" + OBSettings.REPORT_CODE + "', " +
                        "GROUP_BY:'" + OBSettings.SQL_GROUP_BY + "', " +
                        "GROUP_CODE:'" + groupCode + "', " +
                        "COLOR_CODE:'" + colorCode +  "' }";	

        var serviceName = "InsertGroupColor";
        var reportInfo = GetSyncJSONResult_Wrapper(serviceName, params);
        reportInfo = eval('(' + reportInfo + ')');
        reportInfo = eval('(' + reportInfo.d + ')');
        if(reportInfo.result)
        {
            alert("Color saved successfully.");
        }  
    }else
    {
        alert("No color information available to save");
    }
}

function OpenQueryBuilder() 
{   
   var popup = OpenChild("./Query Builder/QueryBuilder.aspx", "QueryBuilder", true, 518, 395, "no", "no", false);
   popup.focus();
}


function ShowThemeColor()
{	
    var currentGrid = OBSettings.GetActiveGrid(); 
    if(currentGrid)
    {
	    document.getElementById("divThemeColor").style.display = 'block';       	
        OBSettings.COLOR_MODE = OBSettings.COLOR_MODE ? 0 : 1;
        
//        if(OBSettings.COLOR_MODE)
//        {
//            Ext.get("divSaveColor").setStyle('display','block');  
//        }else
//        {
//            Ext.get("divSaveColor").setStyle('display','none');    
//        }

        OBSettings.ShowMainLoadingImage();
        setTimeout(function(){
                    OBSettings.CreateGroupByGrid();
                       }, 1);
    }else
    {
        alert("Unable to continue");
    }
}


//**************************** Report section ***************************

function OpenReportOption()
{
    var currentVisibleFields = "";
    var currentVisibleFieldsWithWidth = "";
 
    if(OBSettings.SQL_GROUP_BY == "NONE" ) {
        currentVisibleFields = GetCurrentVisibleFieldNames();
        currentVisibleFieldsWithWidth = GetCurrentVisibleFieldNamesWithWidth();
    }
    else {
        currentVisibleFields = OBSettings.SQL_SELECT;

         //currentVisibleFields = GetCurrentVisibleFieldNamesWithWidth();
        if (OBSettings.ACTIVE_GRID == 'DETAIL_GRID') {
            currentVisibleFields = GetCurrentVisibleFieldNames();
            currentVisibleFieldsWithWidth = GetCurrentVisibleFieldNamesWithWidth();
        }

                
    }
    
    if(OBSettings.COLOR_MODE == 1)
    {
        OpenChild('./Report/common_template.html?isGroupColoredID=1&fields='+currentVisibleFields+'&fieldswidth='+currentVisibleFieldsWithWidth, 'PDFReportOptions', true, 330, 150, 'no', 'no');
    }
    else
    {
        OpenChild('./Report/common_template.html?isGroupColoredID=null&fields='+currentVisibleFields+'&fieldswidth='+currentVisibleFieldsWithWidth, 'PDFReportOptions', true, 330, 150, 'no', 'no');
    }
}
