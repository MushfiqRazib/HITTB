var SECURITY_KEY = "";
var LOGIN_URL = kitServerPath + "/Default.aspx";
var NO_PERMISSION_MSG = "Access Denied!";
var appService = "AppServicesForKit.asmx";

(function CheckAuthentication() {
    var currentUrl = window.location.href.replace(window.location.search, '');
    if (window.location.search != "") {
        var qStrings = window.location.search.substring(1).split('&');
        for (var i = 0; i < qStrings.length; i++) {
            var keyValPair = qStrings[i].split('=');
            if (keyValPair[0] == "sid") {
                SECURITY_KEY = keyValPair[1];
                break;
            }
        }
    }

    if (!SIDValid && kitServerPath != 'undefined' && kitServerPath != '') {
        window.location.href = LOGIN_URL;
    } else if (!SIDValid) {
        window.location.href = "./PermissionError.aspx";
    }

})();


function UpdateReportListBySecurityAssignment(reportList) 
{
    var matchFound = false;
    for (var i = 0; i < reportList.length; i++) {
        matchFound = false;
        for (var k = 0; k < authInformation.REPORT.length; k++) {            
                if (reportList[i].report_code == authInformation.REPORT[k]) {
                    matchFound = true;
                    break;
                }            
        }
        if (!matchFound) {
            reportList.splice(i, 1);
            i--;
        }
    }
    return reportList;
}

function UpdateFunctionListBySecurityAssignment(reportArgs) 
{
    var matchFound = false;
    for (var i = 0; i < reportArgs.functionlist.length; i++) {
        matchFound = false;
        if (authInformation.OB) {
            for (var k = 0; k < authInformation.OB.length; k++) {
                if (reportArgs.functionlist[i][0] == authInformation.OB[k]) {
                    matchFound = true;
                    break;
                }
            }
        }
        if (!matchFound && reportArgs.functionlist[i][2] != 'true') {
            reportArgs.functionlist.splice(i, 1);
            i--;
        }
    }
    return reportArgs;
}


function SessionExists() 
{    
    if(!REPEATER)
    {
        return true;
    }
    
    var url = "./AppServicesForKit.asmx/UpdateLastAccessTime";
    var param = "{'securityKey':'" + SECURITY_KEY + "'}";
    var sessionExist = CallServiceMethodSync(url, param);
    
    if(sessionExist == false)
    {
        window.location.replace(LOGIN_URL);
        return false;
    }    
    return true;
}


function CallServiceMethodSync(fullServiceURL, serviceJSONParams) {
    var url = fullServiceURL;
    var param = serviceJSONParams;
    var xmlhttp = null;
    if (window.XMLHttpRequest) {
        xmlhttp = new XMLHttpRequest();
    }
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
    try {
        if (param != "")
            xmlhttp.send(param);
        else
            xmlhttp.send();
    }
    catch (e) {
        return e;
    }

    return eval("(" + xmlhttp.responseText + ")").d;

}