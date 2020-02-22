//******************************************************************************
//***                                                                        ***
//*** Author     : Rashidul                                                  ***
//*** Date       : 02-08-2009                                                ***
//*** Copyright  : (C) 2004 HawarIT BV                                       ***
//*** Email      : info@hawarIT.com                                          ***
//***                                                                        ***
//*** Description:                                                           ***
//*** This file is used for controlling and integrating                      ***
//*** Wrapper and object browser stuffs. It picks data from                  ***
//*** Wrapper and sends to Object browser.                                   ***
//******************************************************************************


var showTabPanel = true;
var OBSettings = new OBCore(showTabPanel);

var tabPanel;
Ext.onReady(function() {
 
    if(OBSettings.SHOW_TAB_PANEL_FLAG)
    {
        tabPanel = new Ext.TabPanel({
            renderTo: 'tabContainer',
            id: 'obTabs',
            activeTab: 0,
            frame: true,
            defaults: { autoHeight: true },
            items: [{
                title: 'Report',
                contentEl: 'reportContainer',
                listeners: { activate: ShowReportTab }
            }, {
                title: 'Detail',
                contentEl: 'nonReportTabPanel1',
                listeners: { activate: ShowDetailTab }
            }, {
                title: 'PDF Viewer',
                contentEl: 'nonReportTabPanel2',
                listeners: { activate: ShowPDFTab }
            },
            {
                title: 'DWF Viewer',
                contentEl: 'nonReportTabPanel3',
                listeners: { activate: ShowDWFTab }
            }
          ]
        });
    }
    
    LoadReportList();   
    
       //RenderButton('footer','Goto Page', function(){OBSettings.GotoPage();});
        
});



function InitReport() 
{       

    var groupByField = GetGroupByField();
    OBSettings.SQL_GROUP_BY = groupByField;        
    OBSettings.START_ROW = 0;
    OBSettings.ACTIVE_GRID = 'MAIN_GRID';
    
    //*** For Employee/user report only
    if (OBSettings.REPORT_CODE == "EMPLOYEE" && OBSettings.HasPermission('ADD')) {
        Ext.get("divAddUser").setStyle('display', 'block');
    } else {
        Ext.get("divAddUser").setStyle('display', 'none');
    }
    
    if (OBSettings.SQL_GROUP_BY == "NONE") {        
        Ext.get("divThemeColor").setStyle('display','none');
        Ext.get("divSaveColor").setStyle('display','none');
        OBSettings.EXPANDED_GROUP_ID = -1;   
        OBSettings.COLOR_MODE = 0;
        if(!OBSettings.COOKIE_CHECKED)
        {     
            //OBSettings.QB_GB_SELECT_CLAUSE = "";
            OBSettings.SQL_ORDER_BY = "";
        }
        OBSettings.ShowMainLoadingImage();
        setTimeout(function()
                   {                        
                        OBSettings.CreateNormalGrid();      
                        ClearUserSettingsFromCookie();
                                      
                   }, 1);
    } 
    else 
    {   
        if(OBSettings.GIS_THEME_LAYER.toString() == 'true' || OBSettings.GIS_THEME_LAYER.toString() == '1')
        {
            Ext.get("divThemeColor").setStyle('display','block');
            //Ext.get("divSaveColor").setStyle('display','block');  
            OBSettings.COLOR_MODE = 1;                     
        }
        else
        {            
            Ext.get("divThemeColor").setStyle('display','none');
        }
        
        if(!OBSettings.COOKIE_CHECKED || !OBSettings.QB_GB_SELECT_CLAUSE)
        {                         
            OBSettings.SQL_ORDER_BY = groupByField;
            OBSettings.SQL_ORDER_DIR = "ASC";            
            if(!OBSettings.QB_GB_SELECT_CLAUSE)
            {
                OBSettings.QB_GB_SELECT_CLAUSE = "count(*) AS Nr";            
            }            
            OBSettings.GB_SQL_SELECT = OBSettings.SQL_GROUP_BY + ';' + OBSettings.QB_GB_SELECT_CLAUSE;            
            OBSettings.COOKIE_SELECTED_FIELDS = OBSettings.GB_SQL_SELECT;
            OBSettings.COOKIE_CHECKED = true;
        }
                
        OBSettings.ShowMainLoadingImage();
        setTimeout(function()
                   {                        
                        OBSettings.CreateGroupByGrid();   
                        ClearUserSettingsFromCookie(); 
                                                   
                   }, 1);
    }
}






