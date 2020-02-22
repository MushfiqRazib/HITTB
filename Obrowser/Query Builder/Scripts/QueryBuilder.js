//******************************************************************************
//***                                                                        ***
//*** Author     : Faisal                                                    ***
//*** Date       : 03-02-2010                                                ***
//*** Copyright  : (C) 2004 HawarIT BV                                       ***
//*** Email      : info@hawarIT.com                                          ***
//***                                                                        ***
//*** Description:                                                           ***
//***                                                                        ***
//***                                                                        ***
//***                                                                        ***
//******************************************************************************
var showTabPanel = true;
var tabPanel;

Ext.onReady(function() {

if (showTabPanel) {
    
          tabPanel = new Ext.TabPanel({
                renderTo: 'tabContainer',
                id: 'qbTabs',
                activeTab: 0,
                frame: true,
                defaults: { autoHeight: true },
                items: [{
                    title: 'Filter Setup',
                    contentEl: 'filterSetup',                    
                    listeners: { activate: ShowFilterTab }
                }, {
                    title: 'More Fields',
                    contentEl: 'customFields',
                    listeners: { activate: ShowCustomFieldTab }                  
                }, {
                    title: 'Group By',
                    contentEl: 'groupBy',                    
                    listeners: { activate: ShowGroupByTab }
                }
              ]
            });
    }


});

//************************ 1st tab functions ********************************
function SelectField(fieldValue) {
    
    var value = fieldValue.split(';')[1];
    var fldType = fieldValue.split(';')[0];
    SetElementAttrib('FieldValue', 'value', null);
    
    var report = window.opener.OBSettings.REPORT_CODE;
    var sql_from = window.opener.OBSettings.SQL_FROM;

    var params = "{ REPORT_CODE:'" + escape(report) + "', " +
                    "SQL_FROM:'" + escape(sql_from) + "', " +
                    "FIELD_NAME:'" + escape(value) + "'}";

    var serviceName = "GetFieldValues";    
    var myMask = new Ext.LoadMask(Ext.get("filterSetup"), { msg: "Loading...", removeMask: false });    
    myMask.show();
    
    var valuesInfo = window.opener.OBCore.prototype.GetSyncJSONResult(serviceName, params);   
    valuesInfo = eval('(' + valuesInfo + ')').d;
   // alert(valuesInfo);
    var values = valuesInfo.split("###");
    
    var data = new Array();
    
    for (var k = 0; k < values.length; k++) {
        if(fldType == "DATE" || fldType == "TIMESTAMP")
        {
            data.push(new Array(window.opener.OBCore.prototype.GetDBDateFormat(values[k])));
        }
        else
        {
            data.push(new Array(values[k]));
        }
         
    }
    
    var store = new Ext.data.ArrayStore({
    fields: ['displayText'],
    data: data
    });
    
    Ext.fly('showValueList').update('');
    //debugger;
    var combo = new Ext.form.ComboBox({
        typeAhead: true,
        id:'valuelistcmb',
        triggerAction: 'all',
        emptyText: '',
        lazyRender: true,
        renderTo: 'showValueList',
        mode: 'local',
        store: store,
        valueField: 'displayText',  
        displayField: 'displayText'
    });
    
    FillOperatorList(fldType);
    myMask.hide();
}

function FillOperatorList(fldType)
{
    var oSelect = document.getElementById("FieldComp");
    var optList;
    if (fldType == NUMERIC)
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
}

function RemoveOption(ID) {
    var valuelist = document.getElementById(ID);

    for (var i = valuelist.options.length-1; i >=0 ; i--) {
        valuelist.removeChild(valuelist.options[i]);
    }
}

function ShowFilterTab() {
    Ext.get("filterSetup").setStyle('display','block');
    Ext.get("customFields").setStyle('display','none');
    Ext.get("groupBy").setStyle('display', 'none');

    RemoveOption("WhereList");
    var whereClauses;
   
    if(window.opener.OBSettings.SQL_WHERE)
    {
        whereClauses = window.opener.OBSettings.SQL_WHERE.split(/(\s)AND(\s)/ig);
    }
    var optionText;
    
    if (whereClauses) {
        var drpWhereList = document.getElementById('WhereList');
        for (var k = 0; k < whereClauses.length; k++) {
            
           
            var cond = whereClauses[k].split(/(<|>)=|(<>)|[=<>]|(\sLIKE\s)/ig);
            cond = opener.OBSettings.GetFields(cond[0]);
            if (cond.length > 0) {
                var alias = GetAlias(cond[0]);

                optionText = whereClauses[k].replace(/''/g, "'").replace('$$$$','');
                if (alias != "") {
                    optionText = whereClauses[k].replace(cond[0], alias + " ");
                }

                AddOption(drpWhereList, optionText, whereClauses[k]);
            }
           
        }
    }
  
    var fieldList = GetFieldNames();
    var field_name = fieldList[0];
    
    RemoveOption("FieldList");
    var drpFieldList = document.getElementById('FieldList');
    for(var k=0; k<fieldList.length; k++)
    {
        var field = fieldList[k].split(';')[1];
        var text = GetAlias(field) == "" ? field : GetAlias(field);
        AddOption(drpFieldList, text, fieldList[k]);
    }
   
    SelectField(field_name);
}



function GetAlias(field) {
    var capDef;
    
    capsList = window.opener.OBSettings.FIELD_CAPS.split(';');
    for (var k = 0; k < capsList.length; k++) {
        capDef = capsList[k].split('=');
        if (field.toUpperCase() == capDef[0].toUpperCase()) {
            return capDef[1];
        }
    }
    return "";
}

function GetFieldName(alias) {
    var capDef;

    capsList = window.opener.OBSettings.FIELD_CAPS.split(';');
    for (var k = 0; k < capsList.length; k++) {
        capDef = capsList[k].split('=');
        if (alias == capDef[1]) {
            return capDef[0];
        }
    }
    return "";
}

function AddWhereClause() {
    
    var valuelistcmb = Ext.getCmp('valuelistcmb');
    var oList = document.getElementById("WhereList");
    
    if (oList) {    
        
        var text;
        var value;
        
        var fldName = document.getElementById("FieldList");
        var fldNameText = fldName.options[fldName.selectedIndex].text;
        var fldNameValue = fldName.options[fldName.selectedIndex].value.split(';')[1];
        
        var fldComp = document.getElementById("FieldComp");
        var fldCompText = fldComp.options[fldComp.selectedIndex].text;
        var fldCompValue = fldComp.options[fldComp.selectedIndex].value;

        var fldVal = valuelistcmb.getValue();

        if (fldVal == '')
            return;
        var expression;
        var submitVal;
        
        if (fldCompText.indexOf("LIKE") > -1) {

            expression = " LIKE '" + fldCompText.replace('LIKE', fldVal) + "'";
            submitVal = " LIKE '" + fldCompText.replace('LIKE', fldVal.replace(/'/g,"''")) + "'"
        }
        else {
            expression = fldCompValue + " '" + fldVal + "'";
            submitVal = fldCompValue + " '" + fldVal.replace(/'/g, "''") + "'";
        }
        
        text = fldNameText + " " + expression;
        value = fldNameValue + " " + submitVal;
        
        
        var myMask = new Ext.LoadMask(Ext.get("filterSetup"), { msg: "Checking...", removeMask: false });
        myMask.show();
        valuesInfo = CheckWhereClause(value);
        myMask.hide();
        
        
        if (valuesInfo != "true") {
            alert(valuesInfo);
            return;
        }
        
        if(IsDuplicateListText("WhereList",value)) {
            alert("Duplicate Entry");
            return;
        }        
        var oOption = document.createElement("OPTION");
        oOption.text = text;
        oOption.value = value;
        
        oList.options.add(oOption);
        valuelistcmb.reset();
    }
}

function CheckWhereClause(value) {
    var report = window.opener.OBSettings.REPORT_CODE;
    var sql_from = window.opener.OBSettings.SQL_FROM;

    var params = "{ REPORT_CODE:'" + escape(report) + "', " +
                    "SQL_FROM:'" + escape(sql_from) + "', " +
                    "whereClause:'" + escape(value) + "'}";

    var serviceName = "ValidateWhereClause";
    var valuesInfo = window.opener.OBCore.prototype.GetSyncJSONResult(serviceName, params);

    valuesInfo = eval('(' + valuesInfo + ')').d;
    return valuesInfo;
}

function IsDuplicateListText(listID,text)
{
    var cmbx = document.getElementById(listID);
    
    for (i = 0; i < cmbx.options.length; i++) {
        if (text == cmbx.options[i].text) {
            return true;
        }
    }
    return false;
}

function RemoveWhereClause() {
    var oList = document.getElementById("WhereList");

    if (oList && oList.selectedIndex > -1) {
     
        oList.remove(oList.selectedIndex);
    }
}

function SetElementAttrib(key, attrib, value) {
  var elem = document.getElementById(key);
  
  if (!elem) return false;
  
  elem[attrib] = value;
}


function SubmitWhere() 
{
    var oList = document.getElementById("WhereList");
    var fieldValues = document.getElementById("FieldList");  
    var optionvalue;
    var wherevalue = "";
    if (oList.options.length > 0) 
    {
        var pattern = "";
        for (i = 0; i < oList.options.length; i++) 
        {
            wherevalue = wherevalue + oList.options[i].value + " AND ";
        }
        wherevalue = wherevalue.substring(0, wherevalue.length - 5);     
    }
    if(opener.OBSettings.SQL_WHERE != wherevalue)
    {
        window.opener.SetReportByWhereFromQB(wherevalue);
    }
    close();     
}     


function AddOption(drpItem, text, value)
{    
    var opt = document.createElement("option");
    drpItem.options.add(opt);
    opt.text = text;
    opt.value = value;       
}


//************************ 2nd tab functions ********************************

function ShowCustomFieldTab()
{
    Ext.get("filterSetup").setStyle('display','none');
    Ext.get("customFields").setStyle('display','block');
    Ext.get("groupBy").setStyle('display','none');
    RemoveOption("drpCustomFields");
    RemoveCustomFields();
    PopulateCustomFields();    
}


function GetCorrectedExpression(expression)
{
    try
    {  
        var newExpression = expression;      
        var tempFieldNames = expression.split(/\s*\*|\/|\+|\-\s*/g);                        
        for(var x=0;x<tempFieldNames.length;x++)
        {
            var baseName = tempFieldNames[x].replace(/^(\s*[a-z,A-Z]*\(+|\s*)|(\)|\s*)+$/ig,'');  
            if(isNaN(baseName))
            {                                    
                var origName = opener.OBSettings.GetFieldNameFromCaption(baseName);                              
                var startIndex = newExpression.indexOf(baseName);            
                while(startIndex>-1)
                {            
                    var endIndex = startIndex + baseName.length;
                    var preChar = newExpression.substring(startIndex-1, startIndex);
                    var postChar = newExpression.substring(endIndex, endIndex+1);            
                    if(preChar.match(/[a-z,A-Z,0-9,_]/g) == null && postChar.match(/[a-z,A-Z,0-9,_]/g) == null)
                    {
                        newExpression = newExpression.substring(0,startIndex) + origName + newExpression.substring(endIndex);                        
                    }
                    startIndex = newExpression.indexOf(baseName, (startIndex + origName.length));                    
                }
            }
        }                         
       
     }catch(e)
     {
     }   
    return newExpression;
}

function GetCaptionExpression(expression)
{
    try
    {  
        var newExpression = expression;      
        var tempFieldNames = expression.split(/\s*\*|\/|\+|\-\s*/g);                        
        for(var x=0;x<tempFieldNames.length;x++)
        {
            var baseName = tempFieldNames[x].replace(/^(\s*[a-z,A-Z]*\(+|\s*)|(\)|\s*)+$/ig,'');  
            if(isNaN(baseName))
            {                                    
                var caption = opener.OBSettings.GetAlias(baseName);                              
                var startIndex = newExpression.indexOf(baseName);            
                while(startIndex>-1)
                {            
                    var endIndex = startIndex + baseName.length;
                    var preChar = newExpression.substring(startIndex-1, startIndex);
                    var postChar = newExpression.substring(endIndex, endIndex+1);            
                    if(preChar.match(/[a-z,A-Z,0-9,_]/g) == null && postChar.match(/[a-z,A-Z,0-9,_]/g) == null)
                    {
                        newExpression = newExpression.substring(0,startIndex) + caption + newExpression.substring(endIndex);                        
                    }
                    startIndex = newExpression.indexOf(baseName, (startIndex + caption.length));                    
                }
            }
        }                         
       
     }catch(e)
     {
     }   
    return newExpression;
}


function GetCorrectedErrorMessage(msg, expression)
{
    try
    {      
        var newExpression = expression;      
        var tempFieldNames = expression.split(/\s*\*|\/|\+|\-\s*/g);                        
        for(var x=0;x<tempFieldNames.length;x++)
        {
            var baseName = tempFieldNames[x].replace(/^(\s*[a-z,A-Z]*\(+|\s*)|(\)|\s*)+$/ig,'');  
            if(isNaN(baseName))
            {                                    
                var caption = opener.OBSettings.GetAlias(baseName);                              
                var startIndex = newExpression.indexOf(baseName);            
                while(startIndex>-1)
                {            
                    var endIndex = startIndex + baseName.length;
                    var preChar = newExpression.substring(startIndex-1, startIndex);
                    var postChar = newExpression.substring(endIndex, endIndex+1);            
                    if(preChar.match(/[a-z,A-Z,0-9,_]/g) == null && postChar.match(/[a-z,A-Z,0-9,_]/g) == null)
                    {
                        newExpression = newExpression.substring(0,startIndex) + caption + newExpression.substring(endIndex);                        
                    }
                    startIndex = newExpression.indexOf(baseName, (startIndex + caption.length));                    
                }
            }
        }                         
       
     }catch(e)
     {
     }   
     var msgParts = msg.split(expression);
     msg = msgParts[0] + newExpression + msgParts[1];
    return msg;
}

    
function AddCustomField()
{   
    var txtExpression =  document.getElementById("txtExpression");
    var txtShowas = document.getElementById("txtShowas");
    var expression = txtExpression.value;
    var showAs = txtShowas.value; 
    if(showAs && expression)
    {                 
        var text = expression + " AS " + showAs;
        var value = GetCorrectedExpression(expression) + " AS " + showAs;
        
        if(!IsDuplicateListText("drpCustomFields",text))
        {   
            var myMask = new Ext.LoadMask(Ext.get("customFields"), { msg: "Checking...", removeMask: false });
            myMask.show();
            var msg = CheckCustomFields(value);
            myMask.hide();
            
            if (msg != 'true') 
            {
                msg = GetCorrectedErrorMessage(msg, expression);
                alert(msg);
                return;
            }
            
            var drpCustomFields = document.getElementById("drpCustomFields");                   
            AddOption(drpCustomFields, text, value);
            txtExpression.value = "";
            txtShowas.value = ""; 
        }else
        {
            alert("Duplicate Entry");
        }
    }else if(!expression)
    {        
        alert("Expression not given");
    }else
    {
        alert("Alias not given");        
    }
}


function CheckCustomFields(clause) {

    var reportCode = opener.OBSettings.REPORT_CODE;
    var sqlFrom = opener.OBSettings.SQL_FROM;

    var params = "{ REPORT_CODE:'" + escape(reportCode) + "', " +
                        "SQL_FROM:'" + sqlFrom + "', " +
                        "customFields:'" + escape(clause) + "'}";

    var serviceName = "CheckCustomFieldValidation";
    var result = opener.OBSettings.GetSyncJSONResult(serviceName, params);
    result = eval('(' + result + ')').d;
    return result;
}


function ExecuteCustomFieldAdd()
{
    
     var customFields = "",newField;
     var drpCustomFields = document.getElementById("drpCustomFields");     
     for(k=0; k<drpCustomFields.options.length; k++)
     {
        newField = drpCustomFields.options[k].value;
        customFields += customFields==""? newField : ( ";" + newField);
     }
     
     window.opener.ExecuteCustomFieldAction(customFields);            
     self.close();
         
}

function RemoveCustomFields()
{
     var drpCustomFields = document.getElementById("drpCustomFields");
     if (drpCustomFields && drpCustomFields.selectedIndex > -1)
     {
        //*** Remove selected option from 'Where' list.
        drpCustomFields.remove(drpCustomFields.selectedIndex);
     }
}

function  PopulateCustomFields()
{
    if(opener.OBSettings.QB_CUSTOM_FIELDS)
    {
        var customFieldList = opener.OBSettings.QB_CUSTOM_FIELDS.split(';');        
        var drpCustomFields = document.getElementById("drpCustomFields");
        
        for(var k=0; k<customFieldList.length; k++)
        {
            var customFieldParts = customFieldList[k].split('AS');
            var text = GetCaptionExpression(customFieldParts[0]) + " AS " + customFieldParts[1];
            AddOption(drpCustomFields,text, customFieldList[k]);
        }
    }

}

//******************** Group By Tab **********

var NUMERIC = "NUMERIC";

function ShowGroupByTab()
{
    Ext.get("filterSetup").setStyle('display','none');
    Ext.get("customFields").setStyle('display','none');
    Ext.get("groupBy").setStyle('display', 'block');

    var myMask = new Ext.LoadMask(Ext.get("groupBy"), { msg: "Loading...", removeMask: false });
    myMask.show();
    var fieldList = GetFieldNames();//(" ;"+window.opener.OBSettings.SQL_SELECT).replace(window.opener.OBSettings.QB_CUSTOM_FIELDS,'' ).split(";");
    
    FillGroupByFieldList(fieldList);
    FillActionGroupField(fieldList);
    FillExistingGroupSelectClause();
    myMask.hide();
    
    var field_name = window.opener.OBSettings.SQL_GROUP_BY;
    if(field_name != "NONE")
    {
        SelectGroupByField(field_name);
    }
    
    SelectGroupingField('FieldAction');
}

function FillExistingGroupSelectClause()
{
    RemoveOption("GroupList");
    
    if(window.opener.OBSettings.QB_GB_SELECT_CLAUSE)
    {
        var selectClauses = window.opener.OBSettings.QB_GB_SELECT_CLAUSE.split(';');
        var drpGroupList = document.getElementById('GroupList');
        for(var k=0; k<selectClauses.length; k++)
        {
            AddOption(drpGroupList, selectClauses[k], selectClauses[k]);
        }
    }
}


function SelectGroupByField(selectedValue)
{
    var drpFieldList = document.getElementById('lstGroupbyFields');
    for(var i=0;i<drpFieldList.options.length;i++)
    {
        if(drpFieldList.options[i].value == selectedValue)
        {
            drpFieldList.selectedIndex = i;
            return;
        }
    }   
}

function GetFieldNames()
{
    var report = window.opener.OBSettings.REPORT_CODE;
    var sql_from = window.opener.OBSettings.SQL_FROM;

    var params = "{ REPORT_CODE:'" + escape(report) + "', " +
                    "SQL_FROM:'" + escape(sql_from) + "'}";
                   
    var serviceName = "GetFieldNameType";
    var valuesInfo = window.opener.OBCore.prototype.GetSyncJSONResult(serviceName, params);
    var fieldList = eval('(' + valuesInfo + ')').d.split('|'); 
    return fieldList;
}

function FillGroupByFieldList(fieldList)
{
    RemoveOption("lstGroupbyFields");
    var drpFieldList = document.getElementById('lstGroupbyFields');
    AddOption(drpFieldList, "", "");
    for(var k=0; k<fieldList.length; k++)
    {
        var field = fieldList[k].split(';')[1];
        var text = GetAlias(field) == "" ? field : GetAlias(field);
        AddOption(drpFieldList, text, field);
    }
}

function FillActionGroupField(fieldList)
{
    RemoveOption("FieldAction");
    drpFieldList = document.getElementById('FieldAction');
    
    for(var k=0; k<fieldList.length; k++)
    {
        var text = fieldList[k].split(';')[1];
        text = GetAlias(text) == "" ? text : GetAlias(text);
        AddOption(drpFieldList, text, fieldList[k]);
    }
    
}

function SelectGroupingField(elem)
{
    var oSelect = GetElement("GroupingType");
    var oAction = GetElementAttrib("FieldAction","value");
    var fieldProps = oAction.split(';')
    var fldType = fieldProps[0];
    var fldName = fieldProps[1];
       
    if (oSelect == null)
    {
        alert("Combobox 'GroupingType' niet gevonden!");    
        return false;
    }
      
    //*** Set list of available options.
    if (fldType == NUMERIC)
    {
        //*** Numeric field totals.
        var optList = new Array("AVG", "COUNT", "SUM");
    }
    else
    {
  	    //*** Except numeric field totals.
        var optList = new Array("COUNT");
    }
    
    //*** Clear options.
    oSelect.options.length = 0;
      
    for(var i = 0; i < optList.length; i++)
    {
        oSelect.options[i] = new Option(optList[i], optList[i]);
    }
      
    //*** Select first option.
    oSelect.selectedIndex = 0;
      
    //*** Set grouping field.
    SetElementAttrib("GroupingField", "value", fldName);
}

function AddGroupByClause() {

    var oList = document.getElementById("GroupList");
    
    if (oList)
    {
        var oOption  = document.createElement("OPTION");
        var grpType  = document.getElementById("GroupingType").value;
        var grpAlias = document.getElementById("GroupAs").value;        
        
        var fldCombx = document.getElementById("FieldAction");
        var fldName  = fldCombx.options[fldCombx.selectedIndex].text;
        var fldValue  = fldCombx.value;
        
        fldValue = fldValue.split(';')[1];
        
        if (grpAlias == "")
        {
            //*** Empty alias not allowed...
            //alert("'Weergeven als' is een verplicht veld!");
            alert("Alias value cannot be empty");
        }
        else if (IsValid(grpAlias.charAt(0), numRange))
        {
            //*** Alias can not start with numeric character.
            //alert("'Weergeven als' mag niet met een numeriek karakter beginnen!");
            alert('Alias can not start with numeric character.');
        }
        else
        {
            oOption.text = (grpType + "(" + fldName + ")" + " AS " + grpAlias);
            oOption.value = (grpType + "(" + fldValue + ")" + " AS " + grpAlias);
            if (!IsDuplicateListText("GroupList", oOption.text)) {
                var myMask = new Ext.LoadMask(Ext.get("groupBy"), { msg: "submitting...", removeMask: false });
                myMask.show();
                var isOk = IsGroupByClauseCorrect(oOption.value);
                myMask.hide();
                if (isOk == "true") {
                    oList.options.add(oOption);
                    document.getElementById("GroupAs").value = "";
                }
                else {
                    alert(isOk);
                }
            }
            else {
                alert("Duplicate Entry");
            }  
        }
        
        
        //*** TODO: Check with Sarissa if this will give a valid query...
    }
}

function IsGroupByClauseCorrect(selectClause)
{
    var reportCode = opener.OBSettings.REPORT_CODE;
    var sqlFrom = opener.OBSettings.SQL_FROM;
         
    var params = "{ REPORT_CODE:'" + escape(reportCode) + "', " +                 
                  "SQL_FROM:'" + sqlFrom + "', " +                    
                  "QB_GB_SELECT_CLAUSE:'" + escape(selectClause) + "'}";

    var serviceName = "CheckGroupBySelectValidation";
    var result = opener.OBSettings.GetSyncJSONResult(serviceName, params);  
    result = eval('(' + result + ')').d;
    return result;
}

function DeleteGroupByClause() {
    var oList = document.getElementById("GroupList");

    if (oList && oList.selectedIndex > -1) {
     
        oList.remove(oList.selectedIndex);
    }
}

function ExecuteGroupByOperation()
{
    var sqlgroupby = document.getElementById('lstGroupbyFields').value;
    var groupClauseListOptions =  document.getElementById('GroupList').options;
    var sqlorderby = sqlgroupby;
    var qbgbselectclause = "";
    sqlgroupby = window.opener.trim(sqlgroupby);
        
    for(var i=0;i<groupClauseListOptions.length;i++)
    {
        qbgbselectclause += ";" + groupClauseListOptions[i].value;
    }
    
    if(qbgbselectclause)
    {
        qbgbselectclause = qbgbselectclause.substring(1);
    }
    
    if(sqlgroupby && qbgbselectclause=="")
    {
        alert("Add group select clause");
        return;
    }
    
    if(sqlgroupby=="" && qbgbselectclause)
    {
        alert("Add group by field");
        return;
    }
   
    if (opener.OBSettings.SQL_GROUP_BY != sqlgroupby || opener.OBSettings.QB_GB_SELECT_CLAUSE != qbgbselectclause || opener.OBSettings.SQL_GROUP_BY == 'NONE') 
    {
        window.opener.SetReportGroupByFromQB(sqlorderby, qbgbselectclause, sqlgroupby);
       
    }
    self.close();
           
}

//**************** Common validate functions ***************

var numRange  = "0123456789";
var charRange = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
var signRange = "_";

function IsValidField(e)
{
    var key = GetCharKey(e);  
    return IsValid(key, numRange + charRange + signRange);
}

function IsValid(key, range)
{
    if ((key == "") || (range.indexOf(key, 0) >= 0))
        //*** Key found!
        return true;
    else
        return false;
}


function GetCharKey(e)
{
    //*** Return char.
    return String.fromCharCode(GetASCIIKey(e));
}

function GetASCIIKey(e)
{
    //*** IE or other event?
    //return (window.event) ? window.event.keyCode : e.which;
    return (navigator.appName.indexOf("Microsoft") != -1) ? e.keyCode : e.which;
}


function IsValid(key, range)
{
    if ((key == "") || (range.indexOf(key, 0) >= 0))
        //*** Key found!
        return true;
    else
        return false;
}

function GetElement(key)
{
    //*** First check if specified key is element id.
    var elem = document.getElementById(key);
  
    if (!elem)
    {
        //*** Now check if specified key is element name.
        elem = document.getElementsByName(key);
    
        //*** Element array found, set element to first one.
        if (elem.length > 0) elem = elem[0];
    }
  
    if (!elem)
    {
        //*** Element not found, raise error message.
        alert("Could not find element '" + key + "'");
    
        return false;
    }
  
    return elem;
}

function GetElementAttrib(key, attrib)
{
    var elem = GetElement(key);
  
    //*** Element not found.
    if (!elem) return false;
  
    //*** Return value of specified attribute.
    return elem[attrib];
}
