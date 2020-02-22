
OBCore.prototype.ExecuteNavigation = function () 
{
    if(tabPanel)
    {
        tabPanel.activate(tabPanel.items.items[0]);
    }
    var me = this;
    if(me.ACTIVE_GRID == 'MAIN_GRID'){
        me.ShowMainLoadingImage();
        if (me.SQL_GROUP_BY != 'NONE') {
            setTimeout(function()
                {
                    me.CreateGroupByGrid();
                }, 1);
        } else {
            setTimeout(function()
                {
                    me.CreateNormalGrid();
                }, 1);
        }
    } else {
        me.ShowDetailLoadingImage();
        setTimeout(function(){
                    me.CreateNormalGrid();
                }, 1);
    }
}

OBCore.prototype.GotoPage = function() 
{      
    var reqPageNo = parseInt(document.getElementById("txtGotoPage").value);
    if(!reqPageNo) return;
    reqPageNo--;
    var pageSize = this.GetPageSize();    
    this.PAGE_SIZE = pageSize;
    if (this.ACTIVE_GRID == 'MAIN_GRID') 
    {
        var totalRow = parseInt(this.TOTAL_ROW);
        if ( reqPageNo * pageSize > totalRow) 
        {
            return;
        }

        this.START_ROW = reqPageNo * pageSize;
    } else 
    {       
        var totalRow = parseInt(this.DETAIL_GRID_TOTAL_ROW);
        if (reqPageNo * pageSize > totalRow) {
            return;
        }
        this.DETAIL_GRID_START_ROW = reqPageNo * pageSize;
    }
    this.ExecuteNavigation();
}


OBCore.prototype.GotoFirstPage = function() 
{    
    if ((this.START_ROW == 0 && this.ACTIVE_GRID == 'MAIN_GRID' ) ||
            (this.DETAIL_GRID_START_ROW == 0 && this.ACTIVE_GRID == 'DETAIL_GRID')) {
        return;
    }
    
    if (this.ACTIVE_GRID == 'MAIN_GRID') {        
        this.START_ROW = 0;
    } else {        
        this.DETAIL_GRID_START_ROW = 0;
    }
    this.ExecuteNavigation();
}

OBCore.prototype.GotoLastPage = function() 
{    
    var pageSize = this.GetPageSize();
    this.PAGE_SIZE = pageSize;
    
    if ((this.START_ROW + this.PAGE_SIZE > this.TOTAL_ROW && this.ACTIVE_GRID == 'MAIN_GRID') ||
            (this.DETAIL_GRID_START_ROW + this.PAGE_SIZE > this.DETAIL_GRID_TOTAL_ROW && this.ACTIVE_GRID == 'DETAIL_GRID')) {
        return;
    }    
    
    var totalRow ;
    var pageNum;
    if (this.ACTIVE_GRID == 'MAIN_GRID') {
        totalRow = parseInt(this.TOTAL_ROW);
        pageNum = parseInt(totalRow / pageSize);
        this.START_ROW = pageNum * pageSize;
    } else {
        totalRow = parseInt(this.DETAIL_GRID_TOTAL_ROW);
        pageNum = parseInt(totalRow / pageSize);
        this.DETAIL_GRID_START_ROW = pageNum * pageSize;
    }
        
    this.ExecuteNavigation();
}


OBCore.prototype.GotoNextPage = function() {
        
    var pageSize = this.GetPageSize();
    this.PAGE_SIZE = pageSize;
    //alert((this.DETAIL_GRID_START_ROW + this.PAGE_SIZE) +" : "+  this.DETAIL_GRID_TOTAL_ROW)
    if ((this.START_ROW + this.PAGE_SIZE >= this.TOTAL_ROW && this.ACTIVE_GRID == 'MAIN_GRID') ||
            (this.DETAIL_GRID_START_ROW + this.PAGE_SIZE >= this.DETAIL_GRID_TOTAL_ROW && this.ACTIVE_GRID == 'DETAIL_GRID')) {
        return;
    }
  
    if (this.ACTIVE_GRID == 'MAIN_GRID') {
        this.START_ROW += pageSize;
    } else {
        this.DETAIL_GRID_START_ROW += pageSize;
    }
    
    this.ExecuteNavigation();
}

OBCore.prototype.GotoPreviousPage = function() 
{
    
    var pageSize = this.GetPageSize();
    this.PAGE_SIZE = pageSize;
    
    if ((this.START_ROW == 0 && this.ACTIVE_GRID == 'MAIN_GRID') ||
            (this.DETAIL_GRID_START_ROW == 0 && this.ACTIVE_GRID == 'DETAIL_GRID')) {
        return;
    }

    var pageSize = this.GetPageSize();
    this.PAGE_SIZE = pageSize;
    if (this.ACTIVE_GRID == 'MAIN_GRID') {
        this.START_ROW -= pageSize;
    } else {
        this.DETAIL_GRID_START_ROW -= pageSize;
    }
    this.ExecuteNavigation();
}

OBCore.prototype.RefreshPage = function () 
{    
   this.PAGE_SIZE = this.GetPageSize();
   this.ExecuteNavigation();
}

OBCore.prototype.GetPageSize = function() {    
    var pageSize = Ext.getDom('txtPageSize').value;
    pageSize = pageSize ? parseInt(pageSize) : this.START_ROW;
    return pageSize;
}

OBCore.prototype.SetPageSize = function(size) {    
    Ext.getDom('txtPageSize').value = size;
}

OBCore.prototype.GotoNextRow = function() 
{
    var reqGrid = this.GetActiveGrid();    
    var startRow = this.GetStartRow();   
    reqGrid.getSelectionModel().selectNext();
    Ext.getDom('txtSelectedRow').value = startRow + reqGrid.getSelectionModel().lastActive + 1;

    if (tabPanel && tabPanel.getActiveTab().title != 'Report') {        
        var activeTab = tabPanel.getActiveTab();
        tabPanel.activate(tabPanel.items.items[0]);
        tabPanel.activate(activeTab);
    }
}

OBCore.prototype.GotoPrevRow = function() 
{
    var reqGrid = this.GetActiveGrid();
    reqGrid.getSelectionModel().selectPrevious();
    var startRow = this.GetStartRow();   
    Ext.getDom('txtSelectedRow').value = startRow + reqGrid.getSelectionModel().lastActive + 1;

    if (tabPanel && tabPanel.getActiveTab().title != 'Report') {        
        var activeTab = tabPanel.getActiveTab();
        tabPanel.activate(tabPanel.items.items[0]);
        tabPanel.activate(activeTab);
    }
    

}

