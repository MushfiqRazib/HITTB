
function OpenAttachment(url) {
  
        if (url.indexOf('/') > -1) {
            popup = OpenChild(COMPONENT_BASE_PATH + url, "Attachment", true, 750, 677, 'no', 'no');
        } else {
            alert(url);
        }
    
}



function OpenPartlijst(id, revision) 
{    
     popup = OpenChild(COMPONENT_BASE_PATH + '/Templates/partlist.asp?id='+ id +'&rev='+ revision, '', true, 800, 600, 'yes', 'yes');           
}

function OpenHistory(id) 
{       
     popup = OpenChild(COMPONENT_BASE_PATH + '/Templates/history.asp?id='+ id, '', true, 800, 600, 'yes', 'no');     
    
}