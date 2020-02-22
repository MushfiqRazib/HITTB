<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="HITKITServer._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Kit Server</title>

    <script language="javascript">
     function OpenApp()
     {
        var info = document.getElementById("btnInfo").value;
       
        window.open(info);
     }
     
     function BackToApplication(url)
     {      
        window.location.href = url;
     }
     
    
    </script>

</head>
<body style="background-color: #eeffdd">
    <form id="form1" runat="server">
    <br /><br />
    <div style="position: relative; left: 40%; top: 40%; width: 251px; height: 251px;">
        <fieldset>
            <table>
                <tr>
                    <td>
                        UserName:
                    </td>
                    <td>
                        <asp:TextBox ID="txtUserName" runat="server" Width="150px" MaxLength="20"></asp:TextBox><asp:RequiredFieldValidator
                            ID="rfvUser" ControlToValidate="txtUserName" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
                    </td>
                </tr>
                <tr>
                    <td>
                        Password :
                    </td>
                    <td>
                        <asp:TextBox ID="txtPassword" TextMode="Password"  Width="150px" runat="server" MaxLength="10"></asp:TextBox><asp:RequiredFieldValidator
                            ID="rfvPass" ControlToValidate="txtPassword" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
                    </td>
                </tr>
                 <tr>
                    <td>
                        Application :
                    </td>
                    <td>
                        <asp:TextBox ID="txtAppl"  Width="150px" runat="server" MaxLength="20"></asp:TextBox><asp:RequiredFieldValidator
                            ID="RequiredFieldValidator1" ControlToValidate="txtAppl" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
                    </td>
                </tr>
            </table>
        </fieldset>
        <br />
        
        <div style="padding-left:170px">
            <asp:Button ID="btnLogin" runat="server" Text="Login" style="width:80px" OnClick="btnLogin_Click" />
        </div>
    </div>
    <div style="position: relative; left: 50%; top: 5%; width: 338px; height: 50px;">
        <asp:Label ID="lblLoginMsg" runat="server" Text="" ForeColor="Red"></asp:Label>
    </div>
    
    </form>
</body>
</html>
