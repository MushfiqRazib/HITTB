var SETTING_CHECK_MESSAGE = ["Invalid Report code", 
                             "Invalid Order by field", 
                             "Invalid group by field",                             
                             "Invalid report function param(s)",
                             "Invalid where clause defition",
                             "Invalid visible field settings",
                             "Invalid field name in custom field definition",
                             "Invalid field name in query builder group item(s)"];


function SetSettingsToCore(repSettings, checkResult)
{
    for(var k=0; k<checkResult.length;k++)
    {
        if(checkResult[k] <= 3)
        {
            return checkResult[k];   
        }
    }

    //*** Cookie alternatives                
    OBSettings.SQL_WHERE     = repSettings.sql_where;
    OBSettings.SQL_GROUP_BY = repSettings.sql_groupby=="" ? "NONE": repSettings.sql_groupby; 
    OBSettings.SQL_ORDER_BY = (repSettings.sql_orderby != "") ? repSettings.sql_orderby : OBSettings.SQL_SELECT.replace(/,/,';').split(';')[0];
    OBSettings.SQL_ORDER_DIR = (repSettings.sql_orderdir != "") ? repSettings.sql_orderdir : 'ASC';
    
    var gridSettings = repSettings.report_settings;       
    SetGridSettings(gridSettings, checkResult);  
    return -1;                
}

function SetDatabaseFieldSettings(dbSettings)
{
    var gridSettings = dbSettings.report_settings;
    if(gridSettings)
    {
        var visible_fieldNames = "";
        for(var i=0;i<gridSettings.visible_fields.length;i++)
        {
            var curField = gridSettings.visible_fields[i].Name;
            var curFieldWidth = gridSettings.visible_fields[i].Width;
            visible_fieldNames += curField + ";";
            OBSettings.DB_FIELD_SIZE_IN_COOKIE[curField] = parseInt(curFieldWidth);
        }        
        OBSettings.DB_SELECTED_FIELDS = visible_fieldNames != "" ? visible_fieldNames.substring(0,visible_fieldNames.length-1) : "";        
    }else
    {
        ClearDatabaseFieldSettings();
    }
}

function ClearDatabaseFieldSettings()
{
    OBSettings.DB_FIELD_SIZE_IN_COOKIE = {}; 
    OBSettings.DB_SELECTED_FIELDS = "";  
}


function  SetGridSettings(gridSettings, checkResult)
{            
    if(gridSettings)
    {                           
        // Set settings according to received value from cookie or database
        OBSettings.QB_CUSTOM_FIELDS = gridSettings.qb_custom_fields ? gridSettings.qb_custom_fields : "";       
        OBSettings.QB_GB_SELECT_CLAUSE = gridSettings.qb_gb_select ? gridSettings.qb_gb_select : "";
        
        var visible_fieldNames = "";
        for(var i=0;i<gridSettings.visible_fields.length;i++)
        {
            var curField = gridSettings.visible_fields[i].Name;
            var curFieldWidth = gridSettings.visible_fields[i].Width;
            visible_fieldNames += curField + ";";
            OBSettings.FIELD_SIZE_IN_COOKIE[curField] = parseInt(curFieldWidth);
        }        
        OBSettings.COOKIE_SELECTED_FIELDS = visible_fieldNames != "" ? visible_fieldNames.substring(0,visible_fieldNames.length-1) : "";        
                               
        //Reset if any inconsistencies found during check       
        for(var k=0; k<checkResult.length;k++)
        {
            if(checkResult[k] >= 5 && checkResult[k]<=7) 
            {
                // visible field settings
                OBSettings.FIELD_SIZE_IN_COOKIE = {}; 
                OBSettings.COOKIE_SELECTED_FIELDS="";
                OBSettings.QB_CUSTOM_FIELDS = "";
                OBSettings.QB_GB_SELECT_CLAUSE = "";                
                break;
            }
        }
        
        //*** For sql_groupby and visible settings inconsistencies
        if(OBSettings.COOKIE_SELECTED_FIELDS)
        {
            for(var k=0; k<checkResult.length;k++)
            {
                if(checkResult[k] == 8) 
                {
                    //Keep backup of visibility settings.
                    OBSettings.DB_FIELD_SIZE_IN_COOKIE = OBSettings.FIELD_SIZE_IN_COOKIE; 
                    OBSettings.DB_SELECTED_FIELDS = OBSettings.COOKIE_SELECTED_FIELDS;
                        
                    if(OBSettings.SQL_GROUP_BY != 'NONE')
                    {
                        if(!OBSettings.QB_GB_SELECT_CLAUSE)
                        {
                            OBSettings.QB_GB_SELECT_CLAUSE = "COUNT(*) AS Nr";
                        }                        
                        OBSettings.COOKIE_SELECTED_FIELDS = OBSettings.SQL_GROUP_BY + ";"+ OBSettings.QB_GB_SELECT_CLAUSE;
                        OBSettings.FIELD_SIZE_IN_COOKIE = {};
                    }else
                    {
                        OBSettings.COOKIE_SELECTED_FIELDS = OBSettings.SQL_SELECT;
                        OBSettings.FIELD_SIZE_IN_COOKIE = {};
                    }
                    break;                        
                }
            }
        }
        
                                          
        if(parseInt(gridSettings.page_size) > 0)
        {
            OBSettings.PAGE_SIZE = gridSettings.page_size;
            OBSettings.SetPageSize(gridSettings.page_size);
        }        
                
        if(OBSettings.QB_CUSTOM_FIELDS)
        {        
            OBSettings.SQL_SELECT += ';' + OBSettings.QB_CUSTOM_FIELDS;
        }
                         
        if(OBSettings.QB_GB_SELECT_CLAUSE)
        {        
            OBSettings.GB_SQL_SELECT = OBSettings.SQL_GROUP_BY + ';' + OBSettings.QB_GB_SELECT_CLAUSE;
        }           
    }else
    {
        OBSettings.FIELD_SIZE_IN_COOKIE = {}; 
        OBSettings.COOKIE_SELECTED_FIELDS="";   
        OBSettings.QB_GB_SELECT_CLAUSE = "";
        OBSettings.QB_CUSTOM_FIELDS = "";
    }
}


//*** This function returns an array with some predefined code.
//*** The meaning of code is at the top of this file.
function CheckSettings(reportSettings)
{
        
    var checkResult = new Array;
    if(OBSettings.REPORT_CODE != reportSettings.report_code)
    {
        checkResult.push(0); // "INVALID_REPORT_CODE"
    }
    var sqlSelect = OBSettings.SQL_SELECT;
    var validFieldList = sqlSelect.split(';');
    
    //check sql_orderby
    var success = false;
    if(reportSettings.sql_orderby)
    {
        var fieldName = reportSettings.sql_orderby;
        if(reportSettings.sql_orderby.indexOf(" AS ")>-1)
        {
            fieldName = OBSettings.GetFields(fieldName)[0];                 
            if(fieldName=="*")
            {
                success = true;               
            }
        }
        if(!success)
        {    
            for(var k=0;k<validFieldList.length;k++)
            {               
                if(validFieldList[k].toUpperCase()==fieldName.toUpperCase())
                {
                    success = true;
                    break;
                }
            }
        }
        if(!success) checkResult.push(1); //"INVALID_ORDER_BY";
    }
    
    //check sql_groupby
    success = false;
    if(reportSettings.sql_groupby.toUpperCase() != 'NONE' && reportSettings.sql_groupby != "")
    {
        for(var k=0;k<validFieldList.length;k++)
        {
            if(validFieldList[k].toUpperCase()==reportSettings.sql_groupby.toUpperCase())
            {
                success = true;
                break;
            }
        }
    }else
    {
        success = true;
    }
    if(!success) checkResult.push(2); //"INVALID_GROUP_BY";
    
    
    //Check report_function_parameters    
    if(OBSettings.FUNCTION_LIST)
    {        
        for(var i=0; i < OBSettings.FUNCTION_LIST.length; i++)
        {                                   
            var funcParams = OBSettings.FUNCTION_LIST[i][3];                
            for(var j=0; j<funcParams.length; j++)
            {
                success = false;
                for(var k=0;k<validFieldList.length;k++)        
                {
                    if(validFieldList[k].toUpperCase() == funcParams[j].toUpperCase())
                    {
                        success = true;
                        break;
                    }
                }
                if(funcParams.length>0)
                {
                    if(funcParams[0] == "")
                    {
                        success = true;
                    }
                }
                
                if(!success)
                {
                    checkResult.push(3);//"INVALID_REPORT_FUNC_PARAM";
                    break;
                }
            }                    
        }        
    }    
    
    //sql_where checking
    if(reportSettings.sql_where)
    {
        if(reportSettings.sql_where == "INVALID_WHERE")
        {
            reportSettings.sql_where = "";
            success = false;
        } else 
        {        
            var whereConditions = reportSettings.sql_where.split(/(\s)AND(\s)|(\s)OR(\s)/ig);            
            var fieldsInWhere = new Array();
            for (var k = 0; k < whereConditions.length; k++) 
            {
                var cond = whereConditions[k].split(/[<>=]|(<|>)=|(<>)|(\sLIKE\s)/ig);
                fieldsInWhere = OBSettings.GetFields(cond[0]);
                for (var i = 0; i < fieldsInWhere.length; i++) 
                {
                    success = false;
                    for (var j = 0; j < validFieldList.length; j++) 
                    {
                        if (validFieldList[j].toUpperCase() == fieldsInWhere[i].toUpperCase()) 
                        {                        
                            success = true;
                            break;
                        }
                    }
                    if (!success) 
                    {
                        break;
                    }
                }
                if (!success) 
                {
                    break;
                }
            }
        }
        
        if(!success) checkResult.push(4);//"INVALID_SQL_WHERE";
    }
           
    
    //======================================================
    //============= check report_settings  =================
    //======================================================
    var gridSettings = reportSettings.report_settings;    
    if(gridSettings)
    {          
        //*** Visible fields 
        var visibleFieldSettings = gridSettings.visible_fields; 
        if(visibleFieldSettings.length>0)
        {           
            var fieldNames = new Array;
            for(var k=0; k<visibleFieldSettings.length;k++)
            {
                fieldNames = OBSettings.GetFields(visibleFieldSettings[k].Name);                       
                for(var i=0;i<fieldNames.length;i++)
                {
                    success = false;
                    if(fieldNames[i] == "*")
                    {
                        success = true;
                        continue;
                    }
                    
                    for(var j=0;j<validFieldList.length;j++)        
                    {
                        if(validFieldList[j].toUpperCase() == fieldNames[i].toUpperCase())
                        {
                            success = true;
                            break;
                        }
                    }
                   if(!success)
                   {
                       break;
                   }
                }
                
               if(!success)
               {
                   break;
               }
               
               //*** Width checking
               success = parseInt(visibleFieldSettings[k].Width)>0;              
               if(!success)
               {
                   break;
               }
            }
            
            if(!success) checkResult.push(5);//"INVALID_VISIBILE_FIELD_SETTINGS";
        }
            
        // Custom field checking
        if(gridSettings.qb_custom_fields)
        {
            var customFields = gridSettings.qb_custom_fields.split(';');
            for(var k=0;k<customFields.length; k++)
            {
                var customFieldParts = OBSettings.GetFields(customFields[k]);
                for(var i=0;i<customFieldParts.length;i++)
                {                    
                    success = false;                    
                    for(var j=0; j<validFieldList.length; j++)        
                    {
                        success = false;                
                        if(customFieldParts[i].toUpperCase()== validFieldList[j].toUpperCase())
                        {
                            success = true;
                            break;
                        }
                    }
                    if(!success)
                    {
                       checkResult.push(6);//"INVALID_CUSTOM_FIELDS";
                       break;
                    }                    
                }
            }        
        }       
       
        // group_by_fields checking
        if(gridSettings.qb_gb_select)
        {
            var groupByFields = gridSettings.qb_gb_select.split(';');
            for(var k=0;k<groupByFields.length; k++)
            {    
                var customFieldParts = OBSettings.GetFields(groupByFields[k]);
                for(var i=0;i<customFieldParts.length;i++)
                {                    
                    success = false;            
                    if(customFieldParts[i] == "*")
                    {
                        success = true;
                        break;
                    }                                
                    for(var j=0; j<validFieldList.length; j++)
                    {
                        success = false;    
                                                    
                        if(customFieldParts[i].toUpperCase()== validFieldList[j].toUpperCase())
                        {
                            success = true;
                            break;
                        }
                    }
                    if(!success)
                    {
                       break;
                    }                    
                }
                if(!success)
                {                   
                   break;
                } 
            }
            if(!success) checkResult.push(7); //"INVALID_GROUP_FIELDS";             
        }
        
        //*** Combined (Visible fields + SQL_GROUP_BY) consistency 
        var visibleFieldSettings = gridSettings.visible_fields; 
        if(visibleFieldSettings.length>0)
        {                                   
            if(reportSettings.sql_groupby.toUpperCase() != gridSettings.sql_group_by.toUpperCase())
            {
                checkResult.push(8);//"INCONSISTENT_GROUP_BY_AND_VISIBILE_FIELDS";
            }
        }    
        
    }else
    {
        OBSettings.FIELD_SIZE_IN_COOKIE = {}; 
        OBSettings.COOKIE_SELECTED_FIELDS="";   
        OBSettings.QB_GB_SELECT_CLAUSE = "";
        OBSettings.QB_CUSTOM_FIELDS = "";
    }
    
    return checkResult;   

}


function GetSettingsFromCookie()
{    
    // We consider that if any cookie value exists then all cookie 
    // exists for this application. and set all cookie values that has value.
    try
    {             
        if(Get_Cookie("REPORT_CODE"))
        {
            var settings = new Object;        
            var report_code = Get_Cookie("REPORT_CODE");
            var repSettings = Get_Cookie("REPORT_SETTINGS");               
            var sqlGroupBy = Get_Cookie("SQL_GROUP_BY");
            var sqlOrderBy = Get_Cookie("SQL_ORDER_BY");
            var sqlWhere = Get_Cookie("SQL_WHERE");
            var sqlOrderDir = Get_Cookie("SQL_ORDER_DIR");
            repSettings = eval('('+repSettings.replace(/@@@/g,'"')+')'); 
            settings['report_code'] = report_code;
            settings['report_settings'] = repSettings;
            settings['sql_where'] = sqlWhere;
            settings['sql_orderby'] = sqlOrderBy;
            settings['sql_groupby'] = sqlGroupBy;
            settings['sql_orderdir'] = sqlOrderDir;
            return settings;
        }
    }catch(e)
    {
        return null;
    }                
}

//*** Only allowed to save settings from main grid but not from 
//*** detail grid. So in group mode it will save settings from
//*** group grid and in normal mode it will save from normal grid
function SaveUserSettingsInCookie()
{
    var currentGrid = OBSettings.GetActiveGrid();//GetCurrentActiveGrid(); 
    if(currentGrid && OBSettings.APP_SETTINGS_SAVEABLE)
    {    
        //Delete expanded row if any.
        OBSettings.DeleteExpandedRow();
        // Save necessary settings in cookie.
        Set_Cookie("REPORT_CODE",OBSettings.REPORT_CODE,1);
        Set_Cookie("SQL_WHERE",OBSettings.SQL_WHERE,1);
        Set_Cookie("SQL_ORDER_BY",OBSettings.SQL_ORDER_BY,1);
        Set_Cookie("SQL_ORDER_DIR",OBSettings.SQL_ORDER_DIR,1);
        Set_Cookie("SQL_GROUP_BY",OBSettings.SQL_GROUP_BY,1);   
        Set_Cookie("REPORT_SETTINGS",GetCurrentReportSettings(),1);           
    }
}


//*** Updates the report settings from grid (visibility and width only)
function UpdateCurrentReportSetting()
{
    var selectedFieldsNames = GetCurrentVisibleFieldNames();   
    var selectedFieldsWidth = GetCurrentVisibleFieldWidths();            
    OBSettings.COOKIE_SELECTED_FIELDS = selectedFieldsNames;     
    
    if(OBSettings.QB_ACTION && OBSettings.QB_CUSTOM_FIELDS && OBSettings.SQL_GROUP_BY=='NONE')
    {
        OBSettings.COOKIE_SELECTED_FIELDS += ";" + OBSettings.QB_CUSTOM_FIELDS;
    }else if(OBSettings.QB_ACTION && OBSettings.QB_GB_SELECT_CLAUSE && OBSettings.SQL_GROUP_BY!='NONE')
    {
        OBSettings.COOKIE_SELECTED_FIELDS += ";" + OBSettings.QB_GB_SELECT_CLAUSE;
    }
    
    OBSettings.QB_ACTION = false;       
    ReOrderSqlSelectFields();
       
    var fieldWidthArray = selectedFieldsWidth.split(';');
    var fw ;
    for(var k=0; k<fieldWidthArray.length; k++)
    {
        fw = fieldWidthArray[k].split('=');
        OBSettings.FIELD_SIZE_IN_COOKIE[fw[0]] = parseInt(fw[1]);
    }
}


function ReOrderSqlSelectFields()
{         

    var selectedFields = ""; 
    if (OBSettings.SQL_GROUP_BY == 'NONE' || OBSettings.ACTIVE_GRID == 'DETAIL_GRID') 
    {
        selectedFields = OBSettings.SQL_SELECT.split(';');
    }
    else if(OBSettings.SQL_GROUP_BY != 'NONE' && OBSettings.ACTIVE_GRID == 'MAIN_GRID')
    {                    
        selectedFields  = OBSettings.GB_SQL_SELECT.split(';');  
    }
        
    var visibleFieldList = OBSettings.COOKIE_SELECTED_FIELDS.split(';');    
    for(var i=0;i<selectedFields.length;i++)
    {
        for(var k=0;k<visibleFieldList.length; k++)
        {
            if(selectedFields[i].toUpperCase() == visibleFieldList[k].toUpperCase())
            {
                visibleFieldList[k] = selectedFields[i];
                selectedFields[i] = null;
                break;
            }            
        }
    }
    
    var isPlaced;   
    for(var k=0; k<visibleFieldList.length; k++)
    {   
        isPlaced = false;
        for(var i=0; i<selectedFields.length;  i++)
        {
            if(selectedFields[i]==null)
            {
                selectedFields[i] = visibleFieldList[k];
                isPlaced = true;
                break;
            }            
        }
        if(!isPlaced && visibleFieldList[k])
        {
            selectedFields.push(visibleFieldList[k]);
        }    
    }    
    
    selectedFields = CreateUniqueArray(selectedFields);
               
    if (OBSettings.SQL_GROUP_BY == 'NONE' || OBSettings.ACTIVE_GRID == 'DETAIL_GRID') 
    {
        OBSettings.SQL_SELECT = selectedFields.join(';'); 
    }
    else if(OBSettings.SQL_GROUP_BY != 'NONE' && OBSettings.ACTIVE_GRID == 'MAIN_GRID')
    {                    
        OBSettings.GB_SQL_SELECT  = selectedFields.join(';');         
    }    
}


function CreateUniqueArray(arr)
{
    var newArray = new Array;
    for(var k=0;k<arr.length;k++)
    {
        if(!Array.contains(newArray, arr[k]) && arr[k] != "")
        {
            newArray.push(arr[k]);
        }
    }
    return newArray;
}

function GetCurrentVisibleFieldNames()
{
    var selectedFieldsName = "";
    try
    {   
        var currentVisibleFields = new Array();
        var currentGrid = OBSettings.GetActiveGrid();//GetCurrentActiveGrid();    
    
        if(currentGrid)
        {
            var colls = currentGrid.colModel.config;
            for(var i=0;i<colls.length;i++)
            {
                if(colls[i] && colls[i].hidden === false && isDataField(colls[i].header))
                {
                    currentVisibleFields[i] = colls[i].dataIndex ;
                }
            }
            
            for(var i=0;i<currentVisibleFields.length;i++)
            {                
                if(currentVisibleFields[i] != undefined)
                {
                   
                    if (IsExistInSqlSelect(currentVisibleFields[i]))
                    {
                        selectedFieldsName += currentVisibleFields[i] + ";";
                    }
                }
            }
        }               
        
        selectedFieldsName = selectedFieldsName.substring(0,selectedFieldsName.lastIndexOf(';'));
         
        //*** When group by changed from QB
        if(OBSettings.SQL_GROUP_BY != 'NONE' && OBSettings.ACTIVE_GRID == 'MAIN_GRID')
        {
            if(!Array.contains(selectedFieldsName.split(';'), OBSettings.SQL_GROUP_BY))
            {
                selectedFieldsName = OBSettings.SQL_GROUP_BY + ';' + selectedFieldsName;
            }
        }
    
        
    }catch(e)
    {    
    }
    
    return selectedFieldsName;

}

function GetCurrentVisibleFieldNamesWithWidth() {
    var selectedFieldsName = "";
    try {
        //debugger
        var currentVisibleFields = new Array();
        var currentGrid = OBSettings.GetActiveGrid(); //GetCurrentActiveGrid();    

        if (currentGrid) {
            var colls = currentGrid.colModel.config;
            for (var i = 0; i < colls.length; i++) {
                if (colls[i] && colls[i].hidden === false && isDataField(colls[i].header)) {
                    currentVisibleFields[i] = colls[i].dataIndex + '*' + colls[i].width;
                }
            }

            for (var i = 0; i < currentVisibleFields.length; i++) {
                if (currentVisibleFields[i] != undefined) {
                    visibleFieldsWithWidth = currentVisibleFields[i].split('*');
                    if (IsExistInSqlSelect(visibleFieldsWithWidth[0])) {
                        selectedFieldsName += visibleFieldsWithWidth[0] + '*' + visibleFieldsWithWidth[1] + ";";
                    }
                }
            }
        }

        selectedFieldsName = selectedFieldsName.substring(0, selectedFieldsName.lastIndexOf(';'));
//        debugger
//        //*** When group by changed from QB
//        if (OBSettings.SQL_GROUP_BY != 'NONE' && OBSettings.ACTIVE_GRID == 'MAIN_GRID') {
//            if (!Array.contains(selectedFieldsName.split(';'), OBSettings.SQL_GROUP_BY)) {
//                selectedFieldsName = OBSettings.SQL_GROUP_BY + ';' + selectedFieldsName;
//            }
//        }


    } catch (e) {
    }

    return selectedFieldsName;

}

function IsExistInSqlSelect(field)
{
 
    var sqlFields = OBSettings.SQL_SELECT.split(';');    
    if(OBSettings.ACTIVE_GRID == 'MAIN_GRID')
    {            
        if (OBSettings.SQL_GROUP_BY != 'NONE') 
        {
            sqlFields = OBSettings.GB_SQL_SELECT.split(';');            
        }
    }
        
    for(var i=0;i<sqlFields.length;i++)
    {
        if(field == sqlFields[i])
        {
            return true;
        }
    }
    
    return false;
}


function isDataField(headerTitle)
{
    if(headerTitle == "" || headerTitle == "#" || headerTitle.substring(0,5) == "<div ")
    {
        return false;
    }
    else
    {
        return true;
    }
}


function ClearUserSettingsFromCookie()
{
    Delete_Cookie("REPORT_CODE");
    Delete_Cookie("SQL_WHERE");
    Delete_Cookie("SQL_ORDER_BY");
    Delete_Cookie("SQL_ORDER_DIR");
    Delete_Cookie("SQL_GROUP_BY");
      
    Delete_Cookie("REPORT_SETTINGS");
        
    Delete_Cookie("QB_GB_SELECT_CLAUSE");
    OBSettings.COOKIE_SELECTED_FIELDS = "";
    OBSettings.FIELD_SIZE_IN_COOKIE = {};    
     
}

function GetCurrentVisibleFieldWidths()
{
    var selectedFieldsWidth = "";
    try
    {   
        var currentGrid = OBSettings.GetActiveGrid(); //GetCurrentActiveGrid();                            
        var colls = currentGrid.colModel.config;
        for(var i=0;i<colls.length;i++)
        {
            if(colls[i] && colls[i].hidden === false && isDataField(colls[i].header))
            {
                if(IsExistInSqlSelect(colls[i].dataIndex))
                {
                    selectedFieldsWidth += colls[i].dataIndex + "=" + colls[i].width + ";";
                }
            }
        }        
        
        selectedFieldsWidth = selectedFieldsWidth.substring(0,selectedFieldsWidth.lastIndexOf(';'));
        
    }
    catch(e)
    {
    }
    return selectedFieldsWidth;
}


function ResetFieldVisibility()
{        
     OBSettings.COOKIE_SELECTED_FIELDS = "";
     OBSettings.FIELD_SIZE_IN_COOKIE = {};     
}

function LoadDatabaseSettings()
{    
    if(OBSettings.DB_SELECTED_FIELDS!="")
    {
        OBSettings.COOKIE_SELECTED_FIELDS = OBSettings.DB_SELECTED_FIELDS;
        OBSettings.FIELD_SIZE_IN_COOKIE = OBSettings.DB_FIELD_SIZE_IN_COOKIE;
        if(OBSettings.QB_ACTION && OBSettings.QB_CUSTOM_FIELDS && OBSettings.SQL_GROUP_BY=='NONE')
        {
            OBSettings.COOKIE_SELECTED_FIELDS += ";" + OBSettings.QB_CUSTOM_FIELDS;
        }else if(OBSettings.QB_ACTION && OBSettings.QB_GB_SELECT_CLAUSE && OBSettings.SQL_GROUP_BY!='NONE')
        {
            OBSettings.COOKIE_SELECTED_FIELDS += ";" + OBSettings.QB_GB_SELECT_CLAUSE;
        }
    }
}    