//******************************************************************************
//***                                                                        ***
//*** Author     : Rashidul                                                  ***
//*** Date       : 02-08-2009                                                ***
//*** Copyright  : (C) 2004 HawarIT BV                                       ***
//*** Email      : info@hawarIT.com                                          ***
//***                                                                        ***
//*** Description:                                                           ***
//*** This file is only used for calling Wrapper                             ***
//*** web services to retrieve data from server.                             ***
//******************************************************************************

Ext.BLANK_IMAGE_URL = "./ext/resources/images/default/s.gif";

var OBCore = function(showTabPanel) 
{
    //*** Report Parameters
    this.REPORT_CODE = "";
    this.REPORT_NAME = "";
    this.FIELD_CAPS = "";
    this.SQL_SELECT = "*";
    this.SQL_FROM = "";
    this.SQL_WHERE = "";
    this.SQL_ORDER_BY = "";
    this.SQL_ORDER_DIR = "ASC";
    this.SQL_DETAIL_WHERE = ""
    this.SQL_DETAIL_ORDER_BY = "";
    this.SQL_DETAIL_ORDER_DIR = "ASC";
    this.SQL_MANDATORY = "";
    this.DETAIL_KEY_FIELDS ="";
    this.GIS_THEME_LAYER = "";
    this.COLOR_MODE = 0;
    //*** OB properties
    this.START_ROW = 0;
    this.PAGE_SIZE = 25;
    this.SELECTED_ROW = 1;
    this.TOTAL_ROW = 0;
    this.SQL_GROUP_BY = "NONE";
    this.FUNCTION_LIST = "";
    this.ACTIVE_GRID = 'MAIN_GRID';

    this.DETAIL_GRID_TOTAL_ROW = 0;
    this.DETAIL_GRID_START_ROW = 0;
    this.EXPANDED_GROUP_ID = -1;
    this.QB_GB_SELECT_CLAUSE = "";
    this.GB_SQL_SELECT = "";
    this.QB_CUSTOM_FIELDS = "";
    this.QB_ACTION = false;
    this.grid = null;
    this.groupedGrid = null;
    this.FIXED_FIELD_SIZE_LIST = { '#': 25, ' ': 25, '': 25, 'Nr': 55, 'ADD': 25, 'EDIT': 30, 'DELETE': 25, 'ZOOM': 25, 'VIEW': 25, 'BASKET': 25, 'THEME_COLOR': 23, 'GROUP2FILTER': 25, 'EXPANDCOLLAPSE': 25, 'MULTISELECT': 25, 'DOWNLOAD': 25, 'GETNAW': 25, 'GETPERSONEN': 25 };
    this.STATIC_CHANGED_COLUMN_HEADER_LIST = { 'THEME_COLOR': '', 'GROUP2FILTER':'','nr':'Nr','EXPANDCOLLAPSE':''};
    this.NON_DRAGGABLE_FIELD_LIST = ['#','', ' ', 'GROUP2FILTER','EXPANDCOLLAPSE','MULTISELECT','THEME_COLOR'];
    this.MULTI_SELECT = false;
    this.SHOW_TAB_PANEL_FLAG = showTabPanel;    
    this.COOKIE_SELECTED_FIELDS = "";
    this.FIELD_SIZE_IN_COOKIE = {};   
    this.DB_SELECTED_FIELDS = "";
    this.DB_FIELD_SIZE_IN_COOKIE = {};    
    this.COOKIE_CHECKED = false;
    this.SQL_FIELD_TYPE = {};
    this.TOOLTIPS = { 'ADD': 'Add', 'EDIT': 'Edit', 'DELETE': 'Delete', 'ZOOM': 'Zoom', 'VIEW': 'View', 'GROUP2FILTER': 'Set current group as filter', 'BASKET': 'basket', 'collapse': 'Expand', 'GETPERSONEN': 'View Personen data', 'GETNAW': 'View NAW data', 'DOWNLOAD': 'Download Document' };
    this.INTERVALTIMEID = 0;
    this.APP_SETTINGS_SAVEABLE = true;
    
}
 

OBCore.prototype = 
{
    CreateNormalGrid : function () 
    {     
        try
        {  
        
            if(!this.COOKIE_CHECKED && this.grid)
            {
                UpdateCurrentReportSetting();
            }else if(!this.COOKIE_CHECKED && !this.grid)
            {
                LoadDatabaseSettings();
            }            
                               
            this.UpdateSortInfoInNavigation();
            var reportInfo = this.GetNormalGridData();  
                                            
            reportInfo = eval('(' + reportInfo + ')');
            reportInfo = reportInfo.d.replace(/"/g, "\"");              
            reportInfo = eval('(' + reportInfo + ')');
            
            this.DisableDetailTabs(false);
            var me = this;        
            var sm = new Ext.grid.CheckboxSelectionModel();
            
            Ext.grid.DynamicColumnModel = function(store) 
            {            
                var cols = [];
                var recordType = store.recordType;
                var fields = recordType.prototype.fields;                
                me.ResetRowNumberColumnSize();
                
                //*** Calculate total width needed for constant sized columns
                var constantSize = 0, counter = 0;
                for (var i = 0; i < fields.keys.length; i++) {
                    var fieldName = fields.keys[i];                    
                    if (!(me.IsColumnVisible(fieldName))) 
                    {
                        for (var item in me.FIXED_FIELD_SIZE_LIST) {
                            if (fieldName == item) {
                                counter++;
                                constantSize += me.FIXED_FIELD_SIZE_LIST[item];
                                break;
                            }
                        }
                        for (var item in me.FIELD_SIZE_IN_COOKIE) {
                            if (fieldName == item) {
                                counter++;
                                constantSize += me.FIELD_SIZE_IN_COOKIE[item];
                                break;
                            }
                        }                        
                    }
                }           
               
                var gridLeftPos = 0;
                if (me.ACTIVE_GRID == "DETAIL_GRID") {            
                    gridLeftPos = me.FIXED_FIELD_SIZE_LIST['#'] + me.FIXED_FIELD_SIZE_LIST[' '];
                }
                var defaultWidth = (Ext.lib.Dom.getViewWidth() - constantSize - gridLeftPos) / (fields.keys.length - counter);
                                
                for (var i = 0; i < fields.keys.length; i++) {
                    var fieldName = fields.keys[i];
                    var field = recordType.getField(fieldName);                                
                    var colHeader = me.GetAlias(field.name);
                    
                    var colWidth  = me.GetColumnWidth(fieldName, defaultWidth);                              
                    var isVisible = me.IsColumnVisible(field.name);
                    
                    if(fieldName=='MULTISELECT')
                    {                     
                         cols[i] = sm;
                    }
                    else if(me.ContextMenuRequired(colHeader))
                    {
                        var actualFieldName = me.GetOriginalFieldName(field.name);
                        var fldType = me.SQL_FIELD_TYPE[actualFieldName];
                        if(fldType == "DATE")
                        {
                            cols[i] = { header: colHeader, dataIndex: field.name, width: colWidth, hidden: !isVisible, sortable: true,xtype:'datecolumn',format:'d/m/Y' };
                        }
                        else if(fldType == "TIMESTAMP")
                        {
                            cols[i] = { header: colHeader, dataIndex: field.name, width: colWidth, hidden: !isVisible, sortable: true,xtype:'datecolumn',format:'d/m/Y h:i:s A' };
                        }
                        else
                        {
                            cols[i] = { header: colHeader, dataIndex: field.name, width: colWidth, hidden: !isVisible, sortable: true };
                        }
                    }
                    else
                    {
                       cols[i] = { header: colHeader, dataIndex: field.name, width: colWidth, hidden: !isVisible, sortable: false,menuDisabled: true, resizable: false,fixed:true };
                    }
                }
                Ext.grid.DynamicColumnModel.superclass.constructor.call(this, cols);
            };
            Ext.extend(Ext.grid.DynamicColumnModel, Ext.grid.ColumnModel, {});

            this.DestoryGrid('NormalGrid');
            this.grid = new Ext.grid.GridPanel({
                id: 'NormalGrid',
                deferRowRender: false,    
                sm: sm,               
                listeners: {
                    rowclick: function(grid, rowIndex, e) {                    
                        e.stopPropagation();               
                        if (me.EXPANDED_GROUP_ID > -1) {
                            me.ACTIVE_GRID = "DETAIL_GRID";
                        } else {
                            me.ACTIVE_GRID = "MAIN_GRID";
                        }
                        var rowNum = parseInt(me.START_ROW)+ parseInt(rowIndex) + 1;
                        Ext.getDom('txtSelectedRow').value =  rowNum;
                        Ext.get('lblTotalRow').update(reportInfo.rowCount);
                    },
                    rowselect: function(sm, rowIndex, record) {
                        e.stopPropagation();                               
                        if (me.EXPANDED_GROUP_ID > -1) {
                            me.ACTIVE_GRID = "DETAIL_GRID";
                        } else {
                            me.ACTIVE_GRID = "MAIN_GRID";
                        }
                        //Ext.getDom('txtSelectedRow').value = rowIndex;
                        var rowNum = parseInt(me.START_ROW)+ parseInt(rowIndex) + 1;
                        Ext.getDom('txtSelectedRow').value = rowNum;
                    },
                    afterrender: function() {
                    me.INTERVALTIMEID = setInterval(
                            function() {
                                me.SetToolTips(me.TOOLTIPS);
                            }, 1000);
                    },
                    mouseover: function(ev) {
                        ev.stopPropagation();
                    }
                }
                   
            });

            var store2 = new Ext.data.ArrayStore({
                fields: reportInfo.columnInfo,
                autoDestroy: true
            });   

            store2.loadData(reportInfo.gridInfo);
            this.grid.store = store2;
            this.grid.colModel = new Ext.grid.DynamicColumnModel(store2);
            this.grid.width = "100%";
            this.grid.title = '';    
            this.grid.viewConfig = { autoFill: true,forceFit: true };
            this.grid.sm = new Ext.grid.RowSelectionModel({ singleSelect: false });

            if (this.ACTIVE_GRID == 'MAIN_GRID')  //For Main grid
            {
                this.TOTAL_ROW = reportInfo.rowCount;           
                Ext.fly('gridContainer').update('');
                this.grid.height = this.GetMainGridHeight();
                this.grid.render('gridContainer');            
            }
            else  // For detail grid
            {
                var rowNum = this.EXPANDED_GROUP_ID;
                var rowDiv = document.getElementById('detailDiv' + rowNum);
                rowDiv.innerHTML = "";
                this.grid.height = this.GetDetailGridHeight(this.DETAIL_GRID_TOTAL_ROW);
                this.grid.width = "100%";
                this.grid.render("detailDiv" + rowNum);            
            }
            
            this.DefaultRowSelect(this.grid, reportInfo.rowCount);                  
            if(this.groupedGrid)
            {
                this.groupedGrid.getSelectionModel().clearSelections();     
            }
            Ext.get('lblTotalRow').update(reportInfo.rowCount);            
            this.COOKIE_CHECKED = false;            
            this.APP_SETTINGS_SAVEABLE = true;
        }catch(e)
        {
            alert("Json Error, Failed to create report");
            this.APP_SETTINGS_SAVEABLE = false;
        }   
    },
    
    CreateGroupByGrid : function () 
    {        
        try
        {
        
            if(!this.COOKIE_CHECKED && this.groupedGrid)
            {
                UpdateCurrentReportSetting();
            }else if(!this.COOKIE_CHECKED && !this.groupedGrid )
            {
                LoadDatabaseSettings();
            }
                                                                   
            this.UpdateSortInfoInNavigation();      
            this.DisableDetailTabs(true);
            var reportInfo = this.GetGroupByGridData();
            
            reportInfo = eval('(' + reportInfo + ')');
            reportInfo = reportInfo.d.replace(/"/g, "\"")                
            reportInfo = eval('(' + reportInfo + ')');        
            this.TOTAL_ROW = reportInfo.rowCount;    
            Ext.get('lblTotalRow').update(reportInfo.rowCount);
               
            Ext.fly('gridContainer').update('');
            Ext.state.Manager.setProvider(new Ext.state.CookieProvider());    
            var me = this;
            var sm = new Ext.grid.CheckboxSelectionModel();
            
            Ext.grid.DynamicColumnModel = function(store) {
                var cols = [];
                var recordType = store.recordType;
                var fields = recordType.prototype.fields;
                //*** Calculate total width needed for constant sized columns
                var constantSize = 0, counter = 0;
                for (var i = 0; i < fields.keys.length; i++) {
                    var fieldName = fields.keys[i];
                    
                        for (var item in me.FIXED_FIELD_SIZE_LIST) {
                            if (fieldName == item) {
                                counter++;
                                constantSize += me.FIXED_FIELD_SIZE_LIST[item];
                                break;
                            }
                        }
                        
                        for (var item in me.FIELD_SIZE_IN_COOKIE) {
                            if (fieldName == item) {
                                counter++;
                                constantSize += me.FIELD_SIZE_IN_COOKIE[item];
                                break;
                            }
                        }                    
                }

                var defaultWidth = (Ext.lib.Dom.getViewWidth() - constantSize) / (fields.keys.length - counter);
                                
                //*** Define grid columns and their properties
                for (var i = 0; i < fields.keys.length; i++) {
                    var fieldName = fields.keys[i];
                    var field = recordType.getField(fieldName);                    
                    fieldName = me.GetFieldSpellingAsDataBase(fieldName);
                    var colWidth = me.GetColumnWidth(fieldName, defaultWidth);                                         
                    var colHeader = me.GetAlias(fieldName);                                  
                    var isVisible = me.IsColumnVisible(fieldName);
                   
                    if(fieldName=='MULTISELECT')
                    {                     
                         cols[i] = sm;
                    }
                    else if(me.ContextMenuRequired(colHeader))
                    {
                        var actualFieldName = me.GetOriginalFieldName(fieldName);
                        var fldType = me.SQL_FIELD_TYPE[actualFieldName];
                        if(fldType == "DATE")
                        {
                            cols[i] = { header: colHeader, dataIndex: field.name, width: colWidth, hidden: !isVisible, sortable: true,xtype:'datecolumn',format:'d/m/Y' };
                        }
                        else if(fldType == "TIMESTAMP")
                        {
                            cols[i] = { header: colHeader, dataIndex: field.name, width: colWidth, hidden: !isVisible, sortable: true,xtype:'datecolumn',format:'d/m/Y h:i:s A' };
                        }
                        else
                        {
                            cols[i] = { header: colHeader, dataIndex: field.name, width: colWidth, hidden: !isVisible, sortable: true };
                        }
                    }
                    else
                    {
                       cols[i] = { header: colHeader, dataIndex: field.name, width: colWidth, hidden: !isVisible, sortable: false, menuDisabled: true,resizable: false,fixed:true };
                    }          
                }
                Ext.grid.DynamicColumnModel.superclass.constructor.call(this, cols);
            };
            Ext.extend(Ext.grid.DynamicColumnModel, Ext.grid.ColumnModel, {});
            
            var store2 = new Ext.data.ArrayStore({
                fields: reportInfo.columnInfo,
                autoDestroy: true
            });
           
            this.DestoryGrid('GroupGrid');
            this.groupedGrid = new Ext.grid.GridPanel({
                id: "GroupGrid",
                sm: sm,   
                listeners: {
                    rowclick: function(groupedGrid, rowIndex, e) {                
                        me.ACTIVE_GRID = "MAIN_GRID";
                        //Ext.getDom('txtSelectedRow').value = rowIndex;
                        //Ext.get('lblTotalRow').update(reportInfo.rowCount);

                    },
                    rowselect: function(groupedGrid, rowIndex, e) {                                              
                        me.ACTIVE_GRID = "MAIN_GRID";
                        //Ext.getDom('txtSelectedRow').value = rowIndex;
                    },
                    afterrender: function() {
                    me.INTERVALTIMEID = setInterval(
                            function() {
                                me.SetToolTips(me.TOOLTIPS);
                            }, 1000);
                    }
                }
            });
         
            var height = document.documentElement.clientHeight;
            store2.loadData(reportInfo.gridInfo);
            this.groupedGrid.store = store2;
            this.groupedGrid.colModel = new Ext.grid.DynamicColumnModel(store2);
            this.groupedGrid.height = this.GetMainGridHeight();
            this.groupedGrid.width = "100%";
            this.groupedGrid.title = '';
            this.groupedGrid.viewConfig = { autoFill: true, forceFit: true };
            this.groupedGrid.sm = new Ext.grid.RowSelectionModel({ singleSelect: false });
            this.groupedGrid.render('gridContainer');
            //this.groupedGrid.getSelectionModel().selectFirstRow(); 
            this.DefaultRowSelect(this.groupedGrid, reportInfo.rowCount);       
            this.COOKIE_CHECKED = false;
            this.QB_ACTION = false;
            
            this.APP_SETTINGS_SAVEABLE = true;
        }catch(e)
        {
            alert("Json Error, Failed to create report");
            this.APP_SETTINGS_SAVEABLE = false;
        }
    },
    ShowInnerGrid : function(rowNumber, antal, event) 
    {   
                                              
        var imgid = 'img' + rowNumber;          
        rowNumber = parseInt(rowNumber);
        var imageDiv = Ext.DomQuery.select("div[id=" + imgid + "]");
        this.StopPropagation(event);
      
        if (imageDiv[0].className.toString().indexOf('collapse') != -1) {            
            //Delete expanded if any
            this.DeleteExpandedRow();                     
            
            //*** Create new detail grid within a dynamic row
            var tableName = this.SQL_FROM;
            var columnName = this.SQL_GROUP_BY;        
            var storeRowNumber = rowNumber - this.START_ROW;
            
            columnName = this.GetFieldNameInDataStore(this.groupedGrid.getStore().fields.items,columnName);
            var columnValue = this.groupedGrid.getStore().data.items[storeRowNumber-1].data[columnName];
            
            var fldType = this.SQL_FIELD_TYPE[columnName];
            if(fldType == "DATE" || fldType == "TIMESTAMP")
            {
                columnValue = this.GetDBDateFormat(columnValue);
            } 
            
            this.ACTIVE_GRID = 'DETAIL_GRID';
            this.EXPANDED_GROUP_ID = storeRowNumber;
            this.DETAIL_GRID_TOTAL_ROW = parseInt(antal);                                 
            var filterString = "";
            if(!columnValue)
            {
                var actualFieldName = this.GetOriginalFieldName(this.SQL_GROUP_BY);
                var fldType = this.SQL_FIELD_TYPE[actualFieldName];
                if(fldType == "NUMERIC")
                {
                    this.SQL_DETAIL_WHERE = columnName + " IS NULL";
                }
                else
                {                
                    this.SQL_DETAIL_WHERE =  columnName + "='$$$$' OR " + columnName + " IS NULL";
                }            
            }else
            {
                this.SQL_DETAIL_WHERE = columnName + "='" + columnValue.replace(/''/g,'\"').replace(/'/g,"''") + "' ";   
            }                       
                        
            this.DETAIL_GRID_START_ROW = 0;
            this.PAGE_SIZE = this.GetPageSize();
            imageDiv[0].className = 'expand';
            imageDiv[0].title = 'Collapse';
            
            var reader2 = new Ext.data.ArrayReader({ name: '#' });
            var row = Ext.data.Record.create(reader2);
            row = new row({
                '#': "<div id='dummyDiv'></div>"
            });
            
            this.groupedGrid.store.insert(storeRowNumber, row);
            
            var newDiv = Ext.getDom('dummyDiv');
            var rowDiv = newDiv.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode;
            var detailLeftPos = this.FIXED_FIELD_SIZE_LIST['#'] + this.FIXED_FIELD_SIZE_LIST[' '];
            rowDiv.innerHTML = "<div style='padding-left:" + detailLeftPos + "px;' class='detailGrid'  id='detailDiv" + storeRowNumber + "'>";
            this.ShowDetailLoadingImage();
            var me = this;
             
            setTimeout(function(){                        
                        ResetFieldVisibility();                   
                        me.CreateNormalGrid();                        
                        me.groupedGrid.getSelectionModel().lock();
                        }, 1);
            return;
        }
        else if (imageDiv[0].className.indexOf('expand') != -1) 
        {
            this.DeleteExpandedRow();
            if (this.groupedGrid) 
            {
                this.groupedGrid.view.refresh(); 
            }
            
            Ext.get('lblTotalRow').update(this.TOTAL_ROW);
            this.groupedGrid.getSelectionModel().unlock();
            this.DisableDetailTabs(true);
            this.SetToolTips(this.TOOLTIPS);
            return;
        }
    },
    ShowGroup2Filter : function (rowNumber)
    { 
        var columnValue; 
        var columnName = this.GetFieldNameInDataStore(this.groupedGrid.getStore().fields.items,this.SQL_GROUP_BY);
        var storeRowNumber = rowNumber - this.START_ROW;
        this.START_ROW = 0;
        if(this.EXPANDED_GROUP_ID>-1 && this.EXPANDED_GROUP_ID<storeRowNumber)
        {
            columnValue = this.groupedGrid.getStore().data.items[storeRowNumber].data[columnName];
        }else
        {
            columnValue = this.groupedGrid.getStore().data.items[storeRowNumber-1].data[columnName];
        }
       
        var actualFieldName = this.GetOriginalFieldName(columnName);
        var fldType = this.SQL_FIELD_TYPE[actualFieldName];
     
        if(fldType == "DATE" || fldType == "TIMESTAMP")
        {
            columnValue = this.GetDBDateFormat(columnValue);
        }
        
        var nullSupportQuery = "";
        var filterString = "";
        if(!columnValue)
        {
            if(fldType == "NUMERIC")
            {
                filterString = this.SQL_GROUP_BY + " IS NULL";
            }
            else
            {                
                filterString =  this.SQL_GROUP_BY + "='$$$$' OR " + this.SQL_GROUP_BY + " IS NULL";
            }            
        }else
        {
            filterString = this.SQL_GROUP_BY + " = '" + columnValue.replace(/''/g,'\"').replace(/'/g,"''") + "'";   
        }
        
        
        
        if(this.SQL_WHERE)
        {
            this.SQL_WHERE += " AND "+ filterString;
        }
        else
        {
            this.SQL_WHERE = filterString;
        }  
        this.SQL_GROUP_BY = "NONE";
        this.SQL_ORDER_BY = "";
        this.ACTIVE_GRID  = 'MAIN_GRID'
        var drpGroupList = document.getElementById("drpGroupBy");
        setSelectedIndex(drpGroupList, this.SQL_GROUP_BY) ;
        this.ShowMainLoadingImage();
        
        var me = this;
        setTimeout(function()
		        {
		            ResetFieldVisibility();
		            me.CreateNormalGrid();
		        }, 1);      
    },
    GetNormalGridData : function () 
    {       	
        var sqlWhere = escape(this.SQL_WHERE);
        var sqlFrom = escape(this.SQL_FROM);
        var sqlOrderBy = this.SQL_ORDER_BY;
        var sqlOrderDir = this.SQL_ORDER_DIR;
        var startRow = this.START_ROW;
        
        if(this.SQL_REPORT_SWITCH){
        	sqlWhere = escape(this.SQL_REPORT_SWITCH);
        	this.SQL_WHERE = this.SQL_REPORT_SWITCH;
        	this.SQL_REPORT_SWITCH = null;
        }
        
        if (this.ACTIVE_GRID == 'DETAIL_GRID') 
        {
            sqlWhere += (sqlWhere == "") ? escape(this.SQL_DETAIL_WHERE) : (" AND " + escape(this.SQL_DETAIL_WHERE));
            sqlOrderBy = this.SQL_DETAIL_ORDER_BY;
            sqlOrderDir = escape(this.SQL_DETAIL_ORDER_DIR);
            startRow = escape(this.DETAIL_GRID_START_ROW);
        }
        
        if(sqlOrderBy.indexOf(" AS ")>-1)
        {
            sqlOrderBy = sqlOrderBy.split(" AS ")[0];
        }
        
        sqlOrderBy = escape(sqlOrderBy);
        
        this.ShowHideClearFilterButton();        
        var params = "{ REPORT_CODE:'" + escape(this.REPORT_CODE) + "', " +
                    "FIELD_CAPS:'" + escape(this.FIELD_CAPS) + "', " +
                    "SQL_SELECT:'" + escape(this.SQL_SELECT) + "', " +                   
                    "SQL_FROM:'" + sqlFrom + "', " +
                    "SQL_WHERE:'" + sqlWhere + "', " +
                    
                    "SQL_ORDER_BY:'" + sqlOrderBy + "', " +
                    "SQL_ORDER_DIR:'" + sqlOrderDir + "', " +

                    "START_ROW:'" + startRow + "', " +
                    "PAGE_SIZE:'" + this.PAGE_SIZE + "', " +
                    
                    "SQL_MANDATORY:'" + escape(this.SQL_MANDATORY) + "'," +
                    "MULTI_SELECT:'" + this.MULTI_SELECT.toString() + "'," +
                    "FUNCTION_LIST:'" + escape(this.GetReportFunctionsForOB()) + "'}";

        var serviceName = "GetNormalGridData";
        var reportInfo = this.GetSyncJSONResult(serviceName, params);
        return reportInfo;
    },
    GetGroupByGridData : function() 
    {   
        var sqlWhere = escape(this.SQL_WHERE); 
        var themeLayerShow = false;
        if(this.GIS_THEME_LAYER=='true' || this.GIS_THEME_LAYER=='1')
        {
            themeLayerShow = this.COLOR_MODE ? 'true' : 'false';
        }
        this.EXPANDED_GROUP_ID = -1; 
        this.ShowHideClearFilterButton();
        
        var customSqlSelect = this.GB_SQL_SELECT; 
        
        //debugger
        var sqlQbWhere = escape(this.QB_GB_SELECT_CLAUSE); 
        var params = "{ REPORT_CODE:'" + escape(this.REPORT_CODE) + "', " +
                        "FIELD_CAPS:'" + escape(this.FIELD_CAPS) + "', " +
                        "SQL_SELECT:'" + escape(customSqlSelect) + "', " +
                       
                        "SQL_FROM:'" + escape(this.SQL_FROM) + "', " +
                        "SQL_WHERE:'" + sqlWhere + "', " +
                        
                        "SQL_ORDER_BY:'" + escape(this.SQL_ORDER_BY) + "', " +
                        "SQL_ORDER_DIR:'" + this.SQL_ORDER_DIR + "', " +
                        "QB_GB_SELECT_CLAUSE:'" + sqlQbWhere + "', " +

                        "START_ROW:'" + this.START_ROW + "', " +
                        "PAGE_SIZE:'" + this.PAGE_SIZE + "', " +
                        "SQL_GROUP_BY:'" + escape(this.SQL_GROUP_BY) + "', " + 
                        "MULTI_SELECT:'" + this.MULTI_SELECT.toString() + "'," +
                        "GIS_THEME_LAYER:'" + themeLayerShow +  "' }";

        var serviceName = "GetGroupByGridData";
        var reportInfo = this.GetSyncJSONResult(serviceName, params);
        return reportInfo;
    },
    ShowHideClearFilterButton : function()
    {            
        if(this.SQL_WHERE)
        {
            Ext.get("btnClearFilter").setStyle('visibility','visible');           
        }else
        {
            Ext.get("btnClearFilter").setStyle('visibility','hidden');           
        }
            
    },    
    ResetRowNumberColumnSize : function()
    {       
        if(this.ACTIVE_GRID == "MAIN_GRID")
        {                        
            this.FIXED_FIELD_SIZE_LIST['#'] = (this.START_ROW + 10).toString().length * 10;
        }else
        {
            this.FIXED_FIELD_SIZE_LIST['#'] = (this.DETAIL_GRID_START_ROW + 10).toString().length * 10;        
        }
    
    },
    IsShowableInHeaderForContextMenu : function(field) 
    {        
        if(this.SQL_MANDATORY[0])
        {
            var mandatoryList = this.SQL_MANDATORY[0].split(';');
            var selectedList = this.SQL_SELECT.split(';');        
            for (var k = 0; k < mandatoryList.length; k++) {
                if (field.toUpperCase() == mandatoryList[k].toUpperCase()) {
                   for (var j = 0; j < selectedList.length; j++) {
                        if ( mandatoryList[k].toUpperCase() == selectedList[j].toUpperCase()) {
                            return true;
                        }
                        else if(j == selectedList.length-1)
                        {
                            return false;
                        }
                    }
                }
            }       
        }
        return true;
    },   
    GetNonGridHeight: function () 
    {
        var headerDiv = Ext.DomQuery.select("div[id='header-wrap']");
        var footerDiv = Ext.DomQuery.select("div[id='footer-wrap']");    
        var header =  headerDiv[0];
        var footer = footerDiv[0];
        var height = 56 + footer.scrollHeight; 
        return height;
    },
    GetMainGridHeight: function () 
    {
        var height = document.documentElement.clientHeight;

        var gridHeight = height - this.GetNonGridHeight() ;   
        if(!tabPanel)
        {
            gridHeight += 25;
        } 
        return gridHeight ;
    },
    GetOtherTabPanelHeight : function () 
    {
        var height = document.documentElement.clientHeight;
        var footerDiv = Ext.DomQuery.select("div[id='footer-wrap']");
        var headerDiv = Ext.DomQuery.select("div[id='header-wrap']");
        
        if (Ext.isIE6) { //Need to be tested
            return height - (headerDiv[0].scrollHeight + footerDiv[0].scrollHeight);  
        } else if (Ext.isIE7) {
            return height - (headerDiv[0].scrollHeight + footerDiv[0].scrollHeight);  
        } else { // for firefox
            return height - (footerDiv[0].scrollHeight + 30);  //Header height = 30 but found 0 here
        }           
    },
    SetDetailKeys : function(rec) 
    {       
      for (var prop in this.DETAIL_KEY_FIELDS) {
            var value = rec.get(prop.toUpperCase())?rec.get(prop.toUpperCase()) : rec.get(prop.toLowerCase());
            this.DETAIL_KEY_FIELDS[prop] = value;
        }
    },    
    GetDelimittedKeyValuePair : function (delimeter) 
    {    
        var fields = "", values="";    
        for (var prop in this.DETAIL_KEY_FIELDS) {
            fields += (fields == "" ? "" : ";") + prop;
            values += (values == "" ? "" : ";") + this.DETAIL_KEY_FIELDS[prop];            
        }
        return fields + delimeter + values; 
    },
    GetDelimittedFunctionParamValuePair : function(functionName, delimeter)
    {
        var fields = "", values="";                                
        for(var k=0; k < this.FUNCTION_LIST.length; k++)
        {
            if(functionName.toUpperCase() == this.FUNCTION_LIST[k][0].toUpperCase())
            {                            
                var funcParams = this.FUNCTION_LIST[k][3];                
                var selectedRecord = this.grid.getSelectionModel().getSelected();                                
                for(var i=0; i<funcParams.length; i++)
                {  
                    fields += (fields == "" ? "" : ";") + funcParams[i];
                    values += (values == "" ? "" : ";") + selectedRecord.data[funcParams[i]];                           
                }
                break;
            }        
        }
        return fields + delimeter + values;     
    },
    GetKeyValuesAsQueryString : function () 
    {    
        var params = "";    
        for (var prop in this.DETAIL_KEY_FIELDS) {
            params += (params == "" ? "" : "&") + prop.toUpperCase() + "=" + this.DETAIL_KEY_FIELDS[prop].toLowerCase() + "";
        }
        return params; 
    },      
    SetActiveRowNav : function () {
        var startRow = this.GetStartRow();
        var reqGrid = this.GetActiveGrid();
        if(reqGrid)
        {
            var rowNum = parseInt(startRow)+ parseInt(reqGrid.getSelectionModel().lastActive) + 1;
            Ext.getDom('txtSelectedRow').value = rowNum;
        }
    },
    GetActiveGrid : function ()
    {    
        if (this.SQL_GROUP_BY != "NONE" && this.ACTIVE_GRID == 'MAIN_GRID') {
           return this.groupedGrid;
        } else 
        {
            return this.grid;
        }
    },
    ResizeGrid : function()
    {        
        var mainGrid = this.grid;
        if(this.SQL_GROUP_BY != 'NONE')
        {
            mainGrid = this.groupedGrid;
        }
        if(mainGrid)
        {
            var width = document.documentElement.clientWidth;
            var height = this.GetMainGridHeight();
            Ext.get("gridContainer").setHeight(height); 
            mainGrid.setSize(width, height);
        }
        
        if(this.SQL_GROUP_BY != 'NONE' && this.EXPANDED_GROUP_ID>-1)
        {            
            var width = document.documentElement.clientWidth;
            var height = this.GetDetailGridHeight(this.DETAIL_GRID_TOTAL_ROW);
            Ext.get('detailDiv' + this.EXPANDED_GROUP_ID).setHeight(height); 
            this.grid.setSize(width, height);        
        }        
    },
    GetStartRow : function () 
    {
        var startRow = this.DETAIL_GRID_START_ROW;
        if (this.ACTIVE_GRID == 'MAIN_GRID') {
            startRow = this.START_ROW;
        }
        return startRow;
    },
    GetTotalRow : function () {
        var totalRow = this.DETAIL_GRID_TOTAL_ROW;
        if (this.ACTIVE_GRID == 'MAIN_GRID') {
            totalRow = this.TOTAL_ROW;
        }
        return totalRow;
    },        
    DisableDetailTabs : function (flag) {
        if(tabPanel)
        {
            for (var k = 0; k < tabPanel.items.length; k++) {
                tabPanel.items.items[k].disabled = flag;
            }
        }    
    },    
    StopPropagation : function (event) {
        if (event.stopPropagation) {
            event.stopPropagation();
        } else {
            event.cancelBubble = true;
        }
    },
    DeleteExpandedRow : function () {
        try 
        {        
            if (this.EXPANDED_GROUP_ID != -1) {    
                                                                         
                var str = 'img' + (this.START_ROW + this.EXPANDED_GROUP_ID);
                var deleteImageDiv = Ext.DomQuery.select("div[id=" + str + "]");
                deleteImageDiv[0].className = 'collapse';           
                this.groupedGrid.store.removeAt(this.EXPANDED_GROUP_ID);
                this.EXPANDED_GROUP_ID = -1;
                this.ACTIVE_GRID = 'MAIN_GRID';                
                Ext.getDom('txtSelectedRow').value = 0;
            }
        } catch (e) {

        }
    },    
    IsCustomSortable : function(field) 
    {    
        
        //*** In group mode allow to sort only by field name
        if(this.ACTIVE_GRID == 'MAIN_GRID' && this.SQL_GROUP_BY !='NONE') 
        {
            if(this.SQL_GROUP_BY != field)
            {
                return false;
            }
        }   
        
        //*** For report function fields [restrict them]
        for (var k = 0; k < this.FUNCTION_LIST.length; k++) {
            if (field.toUpperCase() == this.FUNCTION_LIST[k][0].toUpperCase()) {
                return false;
            }
        }
        
        return true;    
    },        
    GetReportFunctionsForOB : function ()
    {
        var funcList = new Array();
        if (this.FUNCTION_LIST != "") {
            for (var k = 0; k < this.FUNCTION_LIST.length; k++) {
                var list = this.FUNCTION_LIST[k];
                for (var x = 0; x < list.length; x++) {
                    if (x == 3) {
                        funcList.push("|");
                        funcList.push(list[x]);
                        funcList.push("#");
                    }
                    else {
                        funcList.push(list[x]);
                    }
                }
            }
        }
        else {
            funcList.push("");
        }
        return funcList.toString();
    },
    GetOrderByField : function () 
    {
        var sortField;
        if (this.ACTIVE_GRID == "MAIN_GRID") {
            this.SQL_ORDER_BY = this.SQL_SELECT.split(';')[0];
            sortField = this.SQL_ORDER_BY;
        } else {
            sortField = this.SQL_DETAIL_ORDER_BY;
        }
        return sortField;
    },
    GetSortOrderDir : function () 
    {
        var sortDir;
        if (this.ACTIVE_GRID == "MAIN_GRID") {
            sortDir = this.SQL_ORDER_DIR;
        } else {
            sortDir = this.SQL_DETAIL_ORDER_DIR;
        }
        return sortDir;
    },
    GetDetailGridHeight : function (rowCount) {
        var detailRowCount = parseInt(rowCount);    
        if (detailRowCount > this.PAGE_SIZE) {
            detailRowCount = this.PAGE_SIZE;
        }
        //Header=27; row=24
        var height = (detailRowCount * 24) + 27; 
        return height;
    },    
    ShowMainLoadingImage : function () {
        Ext.fly('gridContainer').update('');
        //*** Show loading image        
        var containerHeight = this.GetMainGridHeight();
        
        Ext.get("gridContainer").setHeight(containerHeight);    
        var html = '<div style="height:'+containerHeight+'px" id={id}>{text}</div>';
        
        var tpl = new Ext.DomHelper.createTemplate(html);
        tpl.append('gridContainer', {
            id: 'loading',         
            text: this.GetLoadingPage("#000000")     
        });   
    },
    ShowDetailLoadingImage : function () 
    {
        Ext.fly('detailDiv' + this.EXPANDED_GROUP_ID).update('');
        var html = '<div id={id}>{text}</div>';
        var tpl = new Ext.DomHelper.createTemplate(html);
        tpl.append('detailDiv'+this.EXPANDED_GROUP_ID, {
            id: 'loading',         
            text: this.GetLoadingPage("#000000")     
        });    
    },
    GetLoadingPage : function (textColor) 
    {
        var page = "";
        page += '<table height="100%" width="100%" border="1">';
        page += '  <tr>';
        page += '    <td align="center" style="background-color:#eeffdd; color: ' + textColor + '; padding: 30px">';
        page += '      <b>Loading...</b><br/><img border="0" src="./images/progressbar.gif">';
        page += '    </td>';
        page += '  </tr>';
        page += '</table>';
        return page;
    },    
    GetPropertyEditorParams : function () 
    {    
    
        var fields = "", values="";    
        for (var prop in this.DETAIL_KEY_FIELDS) {
            fields += (fields == "" ? "" : ";") + prop;
            values += (values == "" ? "" : ";") + this.DETAIL_KEY_FIELDS[prop];            
        }
        return fields + "$" + values; 
    },
    
    IsDraggable : function (value)
    {
        value = value ? value : '';
        for(var k=0; k<this.NON_DRAGGABLE_FIELD_LIST.length; k++)
        {
            if(this.NON_DRAGGABLE_FIELD_LIST[k] == value)
            {
                return false;
            }    
        }
        return true;   
     },
    GetSyncJSONResult : function (serviceName, postData) 
    {    
        var url = GetOBServiceUrl() + "/" + serviceName;
        
        var xmlhttp = null;
        if (window.XMLHttpRequest)
            xmlhttp = new XMLHttpRequest();
        else if (window.ActiveXObject) {
            if (new ActiveXObject("Microsoft.XMLHTTP"))
                xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
            else
                xmlhttp = new ActiveXObject("Msxml2.XMLHTTP");
        }
        // to be ensure non-cached version of response
        url = url + "?rnd=" + Math.random();

        xmlhttp.open("POST", url, false); //false means synchronous
        xmlhttp.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        xmlhttp.send(postData);
        var responseText = xmlhttp.responseText;
        return responseText;       
    },
    IsColumnVisible : function(field) 
    {                
        //*** When all field selected [ allow all]
        if (this.SQL_SELECT == "" || this.SQL_SELECT == "*" || field == "#") {
            return true;
        }
        
        for (var k = 0; k < this.NON_DRAGGABLE_FIELD_LIST.length; k++)        
        {
            if(this.NON_DRAGGABLE_FIELD_LIST[k]== field)
            {
                return true;
            }        
        }

        //*** For report function fields [allow]
        for (var k = 0; k < this.FUNCTION_LIST.length; k++) {
            if (field.toUpperCase() == this.FUNCTION_LIST[k][0].toUpperCase()) {
                return true;
            }
        }
                    
        var result = false;            
        var visibleFields = this.SQL_SELECT;   
        if(this.COOKIE_SELECTED_FIELDS)
        {
            visibleFields = this.COOKIE_SELECTED_FIELDS;   
        }
        var sqlfield = visibleFields.split(';');       
        for (var k = 0; k < sqlfield.length; k++) {
            if (field.toString().toLowerCase() == sqlfield[k].toString().toLowerCase()) {
                result = true;
                break;
            }
        }
        
        return result;
    },
    GetAlias : function(field) 
    {        
    
      //*** For hardcoded changed headers   
        for (var prop in this.STATIC_CHANGED_COLUMN_HEADER_LIST) {
            var colHeader = this.STATIC_CHANGED_COLUMN_HEADER_LIST[prop];
            if (field.toUpperCase() == prop.toUpperCase()) {
                return colHeader;
            }
        }
                
        //*** For report function fields
        for (var k = 0; k < this.FUNCTION_LIST.length; k++) {
            if (field.toUpperCase() == this.FUNCTION_LIST[k][0].toUpperCase()) {
                return '';
            }
        }   	      

        var capDef;
        if (this.FIELD_CAPS) 
        {        
            var capsList = this.FIELD_CAPS.split(';');
            for (var k = 0; k < capsList.length; k++) 
            {
                capDef = capsList[k].split('=');
                if (field.toUpperCase() == capDef[0].toUpperCase()) {
                    return capDef[1];
                }
            }        
        }
                
        if (this.QB_CUSTOM_FIELDS) 
        {        
            var customFieldList = this.QB_CUSTOM_FIELDS.split(';');
            for (var k = 0; k < customFieldList.length; k++) 
            {
                capDef = customFieldList[k].split(' AS ');
                if (field == customFieldList[k]) {
                    return capDef[1];
                }
            }        
        }
        
        if (this.QB_GB_SELECT_CLAUSE) 
        {        
            var customFieldList = this.QB_GB_SELECT_CLAUSE.split(';');
            for (var k = 0; k < customFieldList.length; k++) 
            {
                capDef = customFieldList[k].split(' AS ');
                if (field == customFieldList[k]) {
                    return capDef[1];
                }
            }        
        }


        return field;
    },    
    GetColumnWidth : function(fieldName, defaultWidth) 
    {
        for (var item in this.FIXED_FIELD_SIZE_LIST) {
            if (fieldName.toUpperCase() == item.toUpperCase()) {
                defaultWidth = this.FIXED_FIELD_SIZE_LIST[item];
                break;
            }
        }
        for (var item in this.FIELD_SIZE_IN_COOKIE) {
            if (fieldName.toUpperCase() == item.toUpperCase()) {
                defaultWidth = this.FIELD_SIZE_IN_COOKIE[item];
                break;
            }
        }
        return defaultWidth;
    },       
    ContextMenuRequired : function(headerTitle)
    {
        //if(headerTitle === "" || headerTitle === "#" || headerTitle.toLowerCase().indexOf('checkbox') > -1)
        if(headerTitle === "" || headerTitle === "#" )
        {
            return false;
        }
        else 
        {
            return true;
        }
    },    
    GetCurrentRowValues: function(functionName)
    {
        var dataRow = new Array();                             
        for(var k=0; k < this.FUNCTION_LIST.length; k++)
        {
            if(functionName.toUpperCase() == this.FUNCTION_LIST[k][0].toUpperCase())
            {                            
                var funcParams = this.FUNCTION_LIST[k][3];                
                var selectedRecord = this.grid.getSelectionModel().getSelected();                                
                for(var i=0; i<funcParams.length; i++)
                {
                    dataRow.push(selectedRecord.data[funcParams[i]]);                        
                }
                break;
            }        
        }
        return dataRow;
    },    
    GetMultiSelectValues: function(functionName)
    {
        var dataList = new Array();                             
        for(var k=0; k < this.FUNCTION_LIST.length; k++)
        {
            if(functionName.toUpperCase() == this.FUNCTION_LIST[k][0].toUpperCase())
            {                            
                var funcParams = this.FUNCTION_LIST[k][3];
                var selectedRecords = this.grid.getSelectionModel().getSelections();
                                
                //*** Add field data to the collection 
                for(var i=0; i<selectedRecords.length; i++)
                {
                    var dataRow = new Array();  
                    for(var j=0; j<funcParams.length; j++)
                    {
                        dataRow.push(selectedRecords[i].data[funcParams[j]]);                        
                    } 
                    dataList.push(dataRow);                
                } 
                break;                                                      
            }        
        }
        
      return dataList;
    },
    GetFunctionParams: function(functionName)
    {
        var paramNames = new Array();                             
        for(var k=0; k < this.FUNCTION_LIST.length; k++)
        {
            if(functionName.toUpperCase() == this.FUNCTION_LIST[k][0].toUpperCase())
            {                    
                var funcParams = this.FUNCTION_LIST[k][3];                                       
                //*** Add field names to the collection                
                for(var k=0; k<funcParams.length; k++)
                {
                    paramNames.push(funcParams[k]);                        
                }                                                                   
            }        
        }        
        return paramNames;
    },
    QuickSearchOnUserData : function()
    {
        var userSearchString = trim(Ext.get('txtSearch').dom.value);
        var operator = Ext.get('quickSearchOperator').dom.value;
        if(userSearchString)
        {            
            if(operator.indexOf("LIKE")>-1)
            {
                userSearchString  = "LIKE '" +  operator.replace('LIKE', userSearchString) + "'";
            }
            else
            {
                userSearchString  = operator + " '" +  userSearchString + "'";
            }
                        
            userSearchString = this.GetQuickSearchableFieldName() + " " + userSearchString;            
            var isFilterStringValid = this.CheckWhereClause(userSearchString);
            if(isFilterStringValid == "true")
            {
                if(this.SQL_WHERE)
                {
                    this.SQL_WHERE += " AND "+ userSearchString;
                }
                else
                {
                    this.SQL_WHERE = userSearchString;
                }
                   
                this.RefreshPage();
            }
            else
            {
                alert(isFilterStringValid);
                return;
            }
        }
        
    },    
    ClearFilterString : function()
    {
        if(this.SQL_WHERE)
        {
            this.SQL_WHERE = "";
            document.getElementById("txtSearch").value = "";
            document.getElementById("quickSearchOperator").selectedIndex = 0;
            this.RefreshPage();
        }
        
    }, 
    GetQuickSearchableFieldName : function()
    {
        var fieldName = "";
        if(this.SQL_GROUP_BY != 'NONE')
        {
            fieldName = this.SQL_GROUP_BY;
        }
        else if(this.SQL_GROUP_BY == 'NONE' && this.SQL_ORDER_BY )
        {               
            fieldName = this.SQL_ORDER_BY;
        }
        else if (this.SQL_GROUP_BY == 'NONE' && !this.SQL_ORDER_BY)
        {
            fieldName = this.SQL_SELECT.split(';')[0];
        }
        
        // truncate the the alias name with as if exists
        if(fieldName.match(/ AS /i))
        {
            fieldName = fieldName.split(/ AS /i)[0];
        }
        return fieldName;    
    },
    GetQuickSearchableFieldLabel : function()
    {
        var fieldName = "";
        if(this.SQL_GROUP_BY != 'NONE')
        {
            fieldName = this.GetAlias(this.SQL_GROUP_BY);
        }
        else if(this.SQL_GROUP_BY == 'NONE' && this.SQL_ORDER_BY )
        {               
            fieldName = this.GetAlias(this.SQL_ORDER_BY);
        }
        else if (this.SQL_GROUP_BY == 'NONE' && !this.SQL_ORDER_BY)
        {
            fieldName = this.GetAlias(this.SQL_SELECT.split(';')[0]);
        }
        return fieldName;    
    },
    UpdateSortInfoInNavigation : function()
    {        
        var fieldName = this.GetQuickSearchableFieldLabel();
      
        if(Ext.get('lblSortedFieldName').dom.innerHTML != fieldName)
        {
            Ext.fly('lblSortedFieldName').update(fieldName);
            this.LoadOperatorList(this.SQL_ORDER_BY);
            Ext.get('txtSearch').dom.value = "";
        }
        
    },
    LoadOperatorList : function(fieldName)
    {        
        fieldName = this.GetOriginalFieldName(fieldName);
        var fldType = this.SQL_FIELD_TYPE[fieldName];
        var oSelect = document.getElementById("quickSearchOperator");
        var optList;
        if (fldType == "NUMERIC")
        {
            optList = new Array("=", "<>", ">","<",">=","<=");
        }
        else
        {
            optList = new Array("=", "<>", ">","<",">=","<=","%LIKE%","%LIKE","LIKE%");
        }
        
        oSelect.options.length = 0;
          
        for(var i = 0; i < optList.length; i++)
        {
            oSelect.options[i] = new Option(optList[i], optList[i]);
        }
    },
    GetFieldNameInDataStore : function(storeFields,columnName)
    {
        for(var i=0;i<storeFields.length;i++)
        {
            if(storeFields[i].name.toLowerCase() == columnName.toLowerCase())
            {
                columnName = storeFields[i].name;
                break;
            }
        }
        
        return columnName;
    },
    GetFieldSpellingAsDataBase : function(fieldName)
    {
        var fields = this.SQL_SELECT.split(';');
        for(var i=0;i<fields.length;i++)
        {
            if(fields[i].toLowerCase() == fieldName.toLowerCase())
            {
                fieldName = fields[i];
                break;
            }
        }
        
        return fieldName;
    },
    GetOriginalFieldName:function(fieldCap)
    {
        var capDef;
        if(fieldCap.match(/ AS /i))
        {
            var fields = this.GetFields(fieldCap);
            if(fields)
            {
                return fields[0];
            }
        }
        
        capsList = this.FIELD_CAPS.split(';');
        for (var k = 0; k < capsList.length; k++) {
            capDef = capsList[k].split('=');
            if (fieldCap == capDef[1]) {
                return capDef[0];
            }
        }
        return fieldCap;
    },
    GetFieldNameFromCaption:function(fieldCap)
    {                
        var capsList = this.FIELD_CAPS.split(';');
        for (var k = 0; k < capsList.length; k++) {
            var capDef = capsList[k].split('=');
            if (fieldCap.toUpperCase() == capDef[1].toUpperCase()) {
                return capDef[0];
            }
        }
        return fieldCap;
    },
    GetFields:function(fieldName)
    {
        try
        {
            var fieldList = new Array;
            fieldName = fieldName.split(' AS ')[0];
            var possibleFieldName = fieldName.match(/(\(\s*\*\s*\))|(\(\s*[a-z,A-Z,0-9,_]+\s*\))/ig);
            if(possibleFieldName)   //found as (fn)
            {
                fieldList.push(possibleFieldName[0].replace(/^\(|\)$/g,''));
            }else  // custom qb fields
            {        
                var tempFieldNames = fieldName.split(/\s*\*|\/|\+|\-\s*/g);
                for(var x=0;x<tempFieldNames.length;x++)
                {
                    var actualName = tempFieldNames[x].replace(/^(\s*[a-z,A-Z,0-9,_]*\(+|\s*)|(\)|\s*)+$/ig,'');
                    if(isNaN(actualName))
                    {
                        fieldList.push(actualName);
                    }
                }                         
            }
            return fieldList;
         }catch(e)
         {
         }   
        return null;

    },
    SetToolTips: function(tooltips) {
        var me = this;
        for (var tooltip in tooltips) {

            Ext.select('.' + tooltip).each(function(item) {

                if (item.dom != null) {
                    item.dom.title = tooltips[tooltip];
                }
            });
        }

        clearInterval(me.INTERVALTIMEID);
    },
    DefaultRowSelect : function(gridId, rowCount)
    {
        gridId.getSelectionModel().selectFirstRow(); 
        if(rowCount<1)
        {
            Ext.getDom('txtSelectedRow').value = 0;
            this.DisableDetailTabs(true);
        }
    },
    DestoryGrid : function(param)
    {
        try
        {
            if (param=='NormalGrid') 
            {
                
                if(this.grid)
                {       
                    this.grid.getStore().destroy();
                    this.grid.getSelectionModel().destroy()
                    Ext.destroy(this.grid.view.hmenu);                    
                    Ext.destroy(this.grid);
                    this.grid = null;
                }
            }else
            {
                if(this.groupedGrid)
                {                    
                    this.groupedGrid.getStore().destroy();
                    this.groupedGrid.getSelectionModel().destroy()
                    Ext.destroy(this.groupedGrid.view.hmenu);                    
                    Ext.destroy(this.groupedGrid);
                    this.groupedGrid =null;
                }
            
            }  
        }
        catch(e)
        {
        
        }  
    },
    GetDBDateFormat:function(date)
    {
        if(date)
        {
            var formatedDae = "";
            try
            {
                formatedDae = date.split('/')[1] + '/' + date.split('/')[0] + '/' + date.split('/')[2];
            }
            catch(e)
            {
                return "";
            }
            
            return formatedDae;
        }
        else return "";
    },
    CheckWhereClause:function(value) 
    {
        var report = this.REPORT_CODE;
        var sql_from = this.SQL_FROM;

        var params = "{ REPORT_CODE:'" + escape(report) + "', " +
                        "SQL_FROM:'" + escape(sql_from) + "', " +
                        "whereClause:'" + escape(value) + "'}";

        var serviceName = "ValidateWhereClause";
        var valuesInfo = this.GetSyncJSONResult(serviceName, params);

        valuesInfo = eval('(' + valuesInfo + ')').d;
        if(valuesInfo == undefined || valuesInfo == null) 
        {
            valuesInfo = "";
        }
        return valuesInfo;
    },
    HasPermission: function(functionName) {
        for (var k = 0; k < this.FUNCTION_LIST.length; k++) {
            if (functionName.toUpperCase() == this.FUNCTION_LIST[k][0].toUpperCase()) {
                return true;
            }
        }
        return false;
    }
                    
 }
 
window.onresize = function()
{
   OBSettings.ResizeGrid();        
}
   
 
    
String.prototype.startsWith = function(str) {
    return (this.match("^" + str) == str)
}


