
/// <reference path="../../vswd-ext_2.2.js" />

function GetSyncJSONResult_Wrapper(serviceName, postData) 
{
    var url = GetWrapperServiceUrl() + "/" + serviceName;
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

function HttpRequest(url) {
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
    url = url + "&nocache=" + new Date().getTime();

    xmlhttp.open("GET", url, false); //false means synchronous
    xmlhttp.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    xmlhttp.send(null);
    return xmlhttp.responseText;
}
