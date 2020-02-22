<%@ Page Language="C#" AutoEventWireup="true" CodeFile="QueryBuilder.aspx.cs" Inherits="Query_Builder_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Query Builder</title>
    <link href="../ext/resources/css/ext-all.css" rel="stylesheet" type="text/css" />
    <link href="styles/QBuilder.css" rel="stylesheet" type="text/css" />
    <link href="../styles/obrowser.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript"> </script>

    <script src="../ext/adapter/ext/ext-base.js" type="text/javascript"></script>

    <script src="../ext/ext-all-debug.js" type="text/javascript"></script>

    <script src="Scripts/QueryBuilder.js" type="text/javascript"></script>

    <style type="text/css">
        .style1
        {
            height: 260px;
        }
    </style>
</head>
<body id="qbuilder">
    <form id="form1" runat="server">
    <div id="tabContainer">
    </div>
    <div id="filterSetup" class="Obuilder" style="overflow: auto; width: 100%;">
        <fieldset style="height: 310px; width: 98%; margin-left: 5px; padding-bottom: 8px;">            
            <div>
                <table>
                    <tr>
                        <th style="padding-left: 5px;">
                            Field name
                        </th>
                        <th style="padding-left: 5px;">
                            Comparison
                        </th>
                        <th>
                            Value
                        </th>
                    </tr>
                    <tr>
                        <td align="left" class="clause_input" style="padding-left: 5px;">
                            <select id="FieldList" onchange="SelectField(this.value)" style="width: 115px">
                            </select>
                        </td>
                        <td align="left" class="clause_input" style="padding-left: 5px;">
                            <select id="FieldComp" style="width: 80px;">
                            </select>
                        </td>
                        <td valign="top" style="padding-top: 1px; width: 192px; height: 25px;">
                            <div id="showValueList">
                            </div>
                        </td>
                    </tr>
                </table>
            </div>
            <div style="padding-top: 5px;">
                <table>
                    <tr>
                        <td style="padding-left: 5px;">
                            <select id="WhereList" size="14" style="width: 380px; height: 254px;">
                            </select>
                        </td>
                        <td valign="top">
                            <table>
                                <tr>
                                    <td class="clause">
                                        <input type="button" class="btn" style="width: 103px; height: 22px;" value="Add"
                                            onclick="AddWhereClause()" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <input type="button" class="btn" style="width: 103px; height: 22px;" value="Delete"
                                            class="edit" onclick="RemoveWhereClause()" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
        </fieldset>
        <div style="float: right; padding-right: 13px; padding-top: 6px; padding-bottom: 6px;">
            <input type="button" style="width: 103px; height: 22px;" class="btn" id="btnfielter"
                name="Execute" value="Submit" onclick='SubmitWhere()' />
        </div>
        <div id="EditDiv" style="position: absolute; top: 281px; left: 16px; width: 373px;
            height: 60px; background-color: #eeffdd; color: Black; visibility: hidden; border: 1px solid #000000;">
            <table style="width: 375px">
                <tr style="height: 5px; width: 100%">
                    <td colspan="3">
                    </td>
                </tr>
                <tr>
                    <td style="padding-left: 7px; width: 133px; height: 29px;">
                        <input type="text" id="edtFieldName" style="width: 130px" readonly />
                    </td>
                    <td style="width: 55px; height: 29px;">
                        <select name="edtFieldComp" style="width: 51px;">
                            <option value="=" selected>=</option>
                            <option value="&lt;&gt;">&lt;&gt;</option>
                            <option value="&lt;">&lt;</option>
                            <option value="&gt;">&gt;</option>
                            <option value="&gt;=">&gt;=</option>
                            <option value=""></option>
                            <option value="<= " ">&lt;=</option>
                            <option value="LIKE">%LIKE%</option>
                            <option value="LIKE">%LIKE</option>
                            <option value="LIKE">LIKE%</option>
                        </select>
                    </td>
                    <td style="width: 154px; height: 25px;">
                        <input type="text" class="Combobox" id="edtFieldValue" name="FieldEditValue" style="top: 9px;
                            height: 21px" />
                        <select class="Combobox" id="ValueEditList" name="FieldEditValue" onchange="setElementAttrib('FieldEditValue', 'value', this.value)"
                            style="top: 9px">
                        </select>
                    </td>
                </tr>
                <tr>
                    <td colspan="2" style="height: 22px">
                    </td>
                    <td align="right" style="width: 154px; padding-right: 9px; height: 22px;">
                        <button style="width: 70px; height: 22px;" class="edit" onclick="EditWhereClause('EditDiv')">
                            Ok</button>
                        <button style="width: 70px; height: 22px;" class="edit" onclick="HideElement('EditDiv')">
                            Cancel</button>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <div id="customFields" class="Obuilder" style="overflow: auto; width: 100%;">
        <fieldset style="height: 310px; width: 98%; margin-left: 5px; padding-bottom: 8px;">            
            <div>
                <table>
                    <tr>
                        <th style="padding-left: 5px;">
                            Expression
                        </th>
                        <th style="padding-left: 5px;">
                            Show as
                        </th>
                        <th>
                        </th>
                    </tr>
                    <tr>
                        <td class="clause_input" style="width: 210px; padding-left: 5px;">
                            <input type="text" id="txtExpression" style="width: 210px; height: 19px;" />
                        </td>
                        <td class="clause_input">
                            <input type="text" id="txtShowas" style="width: 156px; height: 19px;" />
                        </td>
                    </tr>
                </table>
            </div>
            <div>
                <table>
                    <tr>
                        <td style="padding-left: 5px;">
                            <select id="drpCustomFields" size="14" style="width: 380px; height: 252px;">
                            </select>
                        </td>
                        <td valign="top">
                            <table>
                                <tr>
                                    <td class="clause">
                                        <input type="button" class="btn" style="width: 103px; height: 22px;" value="Add"
                                            onclick="AddCustomField()" id="btnAddField" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <input type="button" class="btn" style="width: 103px; height: 22px;" value="Delete"
                                            onclick="RemoveCustomFields()" id="btnDeleteField" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
        </fieldset>
        <div style="float: right; padding-right: 13px; padding-top: 6px; padding-bottom: 6px;">
            <input type="button" style="width: 103px; height: 22px;" class="btn" value="Submit"
                onclick="ExecuteCustomFieldAdd()" id="btnCustomeClause" />
        </div>
    </div>
    <div id="groupBy" class="Obuilder" style="overflow: auto">
        <div>
            <fieldset style="height: 38px; width: 98%; margin-left: 5px; padding-top: 8px;">                
                <table>
                    <tr>
                        <th >
                            Group on field:
                        </th>
                        <td >
                            <select id="lstGroupbyFields" onchange="SetElementAttrib('GroupBy','value',this.value)"
                                style="width: 120px;">
                                <option value=""></option>
                            </select>
                        </td>
                    </tr>
                </table>
            </fieldset>
        </div>
        <fieldset style="height: 259px; width: 98%; margin-left: 5px; padding-top: 5px;">            
            <div>
                <table>
                    <tr>
                        <th style="padding-left: 5px;">
                            Totals type
                        </th>
                        <th style="padding-left: 5px;">
                            Field name
                        </th>
                        <th style="padding-left: 5px;">
                            Show as
                        </th>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" class="clause_input"  style="padding-left: 5px;">
                            <select id="GroupingType" style="width: 100px; height: 21px;padding-left: 5px;">
                                <option value="count" selected>COUNT</option>
                            </select>
                        </td>
                        <td align="left" class="clause_input" style="padding-left: 5px;">
                            <select name="FieldAction" id="FieldAction" onchange="SelectGroupingField(FieldAction)"
                                style="width: 120px; height: 21px;">
                            </select>
                        </td>
                        <td align="left" class="clause_input"  style="padding-left: 5px;">
                            <input type="text" id="GroupAs" style="width: 130px; height: 19px;" onkeypress="return IsValidField(event);"
                                onpaste="return false;" />
                        </td>
                        <td align="left" class="clause">
                            <input type="button" style="width: 103px; height: 22px;" class="edit" onclick="AddGroupByClause()"
                                id="btnAdd" value="Add" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3"  style="padding-left: 5px;">
                            <select id="GroupList" name="GroupList" size="11" 
                                style="width: 377px; height: 197px;">
                            </select>
                        </td>
                        <td valign="top">
                            <input type="button" style="width: 103px; height: 22px;" class="edit" onclick="DeleteGroupByClause()"
                                id="btnDelete" value="Delete" />
                        </td>
                    </tr>
                </table>
            </div>
        </fieldset>
        <div style="float: right; padding-right: 13px; padding-top: 6px; padding-bottom: 6px;">
            <input type="button" style="width: 103px; height: 22px;" class="btn" onclick="ExecuteGroupByOperation()"
                id="btnSubmitGB" value="Submit" />
                
        </div>
    </div>
    </form>
</body>
</html>
