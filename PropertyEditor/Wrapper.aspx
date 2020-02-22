<%@ Page Language="C#" EnableEventValidation="false" AutoEventWireup="true" CodeFile="Wrapper.aspx.cs" Inherits="Wrapper" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <script type="text/javascript" src="./script/sarissa.js"></script>

    <script src="script/jquery-1.3.min.js" type="text/javascript"></script>
    <script language="javascript">
        var timerObj;
        var objectMe = 9;
        $(document).ready(function(){
            var docHeight = ($(window).height()-25) +'px';
            $("#peditor").css({height:docHeight});
        });
        
        
        function ShowError() {
            if (window.frames["peditor"].dataSaved == "error") {
                window.clearInterval(timerObj);
                
                var fileName = document.getElementById("fileName").value;
                var fieldName = document.getElementById("fieldName").value;
                var fieldValue = document.getElementById("fieldValue").value;
                var tableName = document.getElementById("tableName").value;
                var groupName = document.getElementById("groupName").value;
               
                window.location = "Wrapper.aspx?erroroccur=true&fileName=" + fileName
                                    + "&fieldName=" + fieldName + "&fieldValue=" + fieldValue
                                    + "&tableName=" + tableName + "&groupName=" + groupName;

            }
        }

        function SaveData(isDataSaved) {
           
            //var isDataSaved = window.iframes["peditor"].dataSaved;

            if (isDataSaved) {
                var fileName = document.getElementById("fileName").value;
                var fieldNames = document.getElementById("fieldNames").value;
                var fieldValues = document.getElementById("fieldValues").value;
                var tableName = document.getElementById("tableName").value;
                var groupName = document.getElementById("groupName").value;
                var mode = document.getElementById("mode").value;
                fieldValues = fieldValues.replace("&", "@$@");
                var response = HttpRequest("Wrapper.aspx?savedata=true&fileName=" + fileName
                                    + "&fieldNames=" + fieldNames + "&fieldValues=" + fieldValues
                                    + "&tableName=" + tableName + "&groupName=" + groupName + "&mode=" + mode);

                //                opener.parent.parent.parent.document.frames["mainview"].GetMapFrame().ClearSelection();
                //                opener.parent.parent.parent.document.frames["mainview"].GetMapFrame().Refresh();                           
            
                
                var msg = response.split('<!DOCTYPE');
                if(msg[0].length > 200)
                {
                    alert(msg[0].substring(0,200)+"...");
                }
                else 
                {
                    //alert(msg[0]);
                    //opener.refresh_notify();
                    self.close();                
                }
            }
        }
        
        function ErrorLoadingProperyEditor()
        {
            document.getElementById("peditor").src = "";
        }
    </script>

</head>
<body style="background-color:#E0E4EA">
    <form id="form1" runat="server">
    <div>
        <iframe id="peditor" name="peditor" style="width: 100%;" src="PropertyEditor.aspx?file=<%=fileName %>">
        </iframe>
        <input type="hidden" id="fileName" value="<%=fileName %>" />
        <input type="hidden" id="fieldNames" value="<%=fieldNames %>" />
        <input type="hidden" id="fieldValues" value="<%=fieldValues %>" />
        <input type="hidden" id="tableName" value="<%=tableName %>" />
        <input type="hidden" id="groupName" value="<%=groupName %>" />
        <input type="hidden" id="mode" value="<%=mode %>" />
    </div>
    </form>
</body>
</html>
