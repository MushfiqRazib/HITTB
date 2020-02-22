<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UserAddForm.aspx.cs" Inherits="UserAddForm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript" src="scripts/calendar.js"></script>
    <script src="Scripts/Wrapper/WrapperServiceProxy.js" type="text/javascript"></script>
    <script>

        window.onload = function() {
            var maxHeight = document.documentElement.offsetHeight;
            if (navigator.userAgent.indexOf("Firefox") != -1) {
                maxHeight = document.documentElement.clientHeight;
            }
            document.body.height = maxHeight;
            document.getElementById('centerDiv').style.height = maxHeight;
        }
        
        function AddUser() {
  
            var userCode = document.getElementById("txtUserCode").value;
            if (!userCode) {
                alert('User Code Empty');
                return;
            }

            var userName = document.getElementById("txtUserName").value;
            var fullName = document.getElementById("txtFullName").value;
            var address = document.getElementById("txtAddress").value;
            var city = document.getElementById("txtCity").value;
            var phoneNr = document.getElementById("txtPhoneNr").value;
            var mobile = document.getElementById("txtMobile").value;
            var skype = document.getElementById("txtSkype").value;
            var birthDay = document.getElementById("txtBirthDay").value;
            var _function = document.getElementById("txtFunction").value;
            var joinedHawarit = document.getElementById("txtJoinedHawarit").value;
            var status = document.getElementById("txtStatus").value;
            var internNr = document.getElementById("txtInternNr").value;
            var comment = document.getElementById("txtComment").value;
            var department = document.getElementById("txtDepartment").value;
            var email = document.getElementById("txtEmail").value;
            var nickName = document.getElementById("txtNickName").value;
         
            var params = '{"userCode":"'+ userCode +'",' + 
                         '"userName":"'+ userName +'",' + 
                         '"fullName":"'+ fullName +'",' + 
                         '"address":"'+ address +'",' + 
                         '"city":"'+ city +'",' + 
                         '"phoneNr":"'+ phoneNr +'",' + 
                         '"mobile":"'+ mobile +'",' + 
                         '"skype":"'+ skype +'",' + 
                         '"birthDay":"'+ birthDay +'",' + 
                         '"_function":"'+ _function +'",' + 
                         '"joinedHawarit":"'+ joinedHawarit +'",' + 
                         '"status":"'+ status +'",' + 
                         '"internNr":"'+ internNr +'",' + 
                         '"comment":"'+ comment +'",' + 
                         '"department":"'+ department +'",' + 
                         '"email":"'+ email +'",' +
                         '"nickName":"' + nickName + '"}';
            
            var serviceName = "AddUser";
            var result = CallWrapperServiceSync(serviceName, params);
            if (result != 'true') {
                alert(result);
            } else {
                opener.OBSettings.RefreshPage();
                self.close();
            }
        }


        function CallWrapperServiceSync(serviceName, postData) {
        
            var url = "WrapperServices.asmx" + "/" + serviceName;
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
            return xmlhttp.responseText;
            
        }
    
    </script>
</head>
<body style="background-color:#c4d7db;margin:0px;padding:0px;">
    <form id="form1" runat="server">
    <div id="centerDiv">
    <fieldset style="margin-left:5%;margin-right:5%;text-align:center;height:100%;background-color:#eeffdd;">
      <legend>New User</legend>
          <table>
          <tr>
          <td>User Code</td>
          <td><input type="text" value="" id="txtUserCode" /> </td>
          </tr>

          <tr>
          <td>User Name</td>
          <td><input type="text" value="" id="txtUserName" /> </td>
          </tr>

          <tr>
          <td>Nick Name</td>
          <td><input type="text" value="" id="txtNickName" /> </td>
          </tr>
          
          <tr>
          <td>Full Name</td>
          <td><input type="text" value="" id="txtFullName" /> </td>
          </tr>
          
            <tr>
          <td>Address</td>
          <td><input type="text" value="" id="txtAddress" /> </td>
          </tr>

          <tr>
          <td>City</td>
          <td><input type="text" value="" id="txtCity" /> </td>
          </tr>

        <tr>
          <td>Phone Nr</td>
          <td><input type="text" value="" id="txtPhoneNr" /> </td>
          </tr>
          
          <tr>
          <td>Mobile</td>
          <td><input type="text" value="" id="txtMobile" /> </td>
          </tr>
          
          <tr>
          <td>Skype</td>
          <td><input type="text" value="" id="txtSkype" /> </td>
          </tr>
          
          <tr>
          <td>Birth Day</td>
          <td><input type="text" readonly value="" id="txtBirthDay" style="width:125px" /> 
          <img class="Calendar" vspace="-3" id="date_btn" src="./images/cal_select.gif" onclick="selectDate('txtBirthDay')">
          </td>
          </tr>
          
          <tr>
          <td>Function</td>
          <td><input type="text" value="" id="txtFunction" /> </td>
          </tr>
           
          <tr>
          <td>Joined Hawarit</td>
           <td><input type="text" readonly value="" id="txtJoinedHawarit" style="width:125px" /> 
          <img class="Calendar" vspace="-3" id="Img1" src="./images/cal_select.gif" onclick="selectDate('txtJoinedHawarit')">
          </td>
          </tr>  
          
          <tr>
          <td>Status</td>
          <td><input type="text" value="" id="txtStatus" /> </td>
          </tr>
            
          <tr>
          <td>Intern Nr.</td>
          <td><input type="text" value="" id="txtInternNr" /> </td>
          </tr>
          
          <tr>
          <td>Comment</td>
          <td><input type="text" value="" id="txtComment" /> </td>
          </tr>
         
          <tr>
          <td>Department</td>
          <td><input type="text" value="" id="txtDepartment" /> </td>
          </tr>
          
          <tr>
          <td>Comment</td>
          <td><input type="text" value="" id="Text1" /> </td>
          </tr>
          
          <tr>
          <td>E-Mail</td>
          <td><input type="text" value="" id="txtEmail" /> </td>
          </tr>
          
           
          <tr>
          <td></td>
          <td align=right><input type="button" style="width:60px;" value="Add" onclick="AddUser()" /></td>
          </tr>
  
   
  </table>
     </fieldset>
    </div>
    </form>
</body>
</html>
