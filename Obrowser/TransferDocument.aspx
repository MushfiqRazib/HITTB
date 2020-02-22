<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TransferDocument.aspx.cs" Inherits="UploadDocument" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Document Uploader</title>

    <script src="Scripts/Common.js" type="text/javascript"></script>
    
</head>
<body style="background-color: #c4d7db; margin: 0px; padding: 0px;">
    <form id="form1" runat="server">
    <div>
        <fieldset style="margin-left: 5%; margin-right: 5%; height: 100%;
            background-color: #eeffdd;">
            <legend>
                <asp:Label ID="lblTitle" runat="server" Text="Label"></asp:Label></legend>
            <table>
                <tr>
                    <td>
                        Select Project
                    </td>
                    <td>
                        <asp:DropDownList id="drpProject" runat="server" Width = "150px"></asp:DropDownList>
                    </td>
                </tr>
                <tr id="upload" runat="server">
                    <td align="left">
                        Select Document
                    </td>
                    <td align="left">
                        <input type="file" id="docUploader" runat="server" onChange="return ValidateExtension(this);"/><asp:RequiredFieldValidator
                            ID="RequiredFieldValidator1" runat="server" ErrorMessage="*" ForeColor="Red" ControlToValidate="docUploader" ValidationGroup="upload"></asp:RequiredFieldValidator>
                    </td>
                </tr>
                <tr id="download" runat="server">
                    <td>
                        Document Type
                    </td>
                    <td>
                        <asp:DropDownList id="drpDoctype" runat="server" Width = "150px"></asp:DropDownList>
                    </td>
                </tr>
                <tr><td></td><td >
                   <asp:Button ID="btnUpload" Text="Upload"
                        runat="server" onclick="btnUpload_Click" ValidationGroup="upload" />
                    <asp:Button ID="btnDownload" Text="Download Template"
                        runat="server" onclick="btnDownload_Click" /></td></tr>
            </table>
        </fieldset>
    </div>
    </form>
</body>
</html>
