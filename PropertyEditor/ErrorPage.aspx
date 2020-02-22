<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ErrorPage.aspx.cs" Inherits="ErrorPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Error Occurred</title>
</head>
<body bgcolor="#E0E4EA">
    <form id="form1" runat="server">
     <div style="height: 650px; padding: 50px; margin-left: 20%; margin-right: 20%; background-color: #C6D4FC">
       <h2>Error Occurred</h2>
       <asp:Label ID="lblDBError" runat="server" Text="Data must be consistent in database"></asp:Label>
       <br />
       <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
    </div>
   
    </form>
</body>
</html>
