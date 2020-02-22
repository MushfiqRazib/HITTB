
//*** Container for window handlers ***
var wndChilds = new Array();


function OpenChild(uri, name, replace, width, height, resizable, scrollbars, isFocus) {
    var wnd = null;
    var wndName = parent.name + "_" + name;
    var idx = wndChilds.length;

    //*** Check if window already exists.
    for (var i = 0; i < idx; i++) {
        if (wndChilds[i][0] == wndName) {
            wnd = wndChilds[i][1];
            idx = i;
        }
    }

    if (!wnd || wnd.closed || replace) {
        var top = (screen.availHeight - height) / 2;
        var left = (screen.availWidth - width) / 2;
        var features = "toolbar=no,directories=no,status=no,scrollbars=" + scrollbars + ",menubar=no,location=no,resizable=" + resizable + ",width=" + width + ",height=" + height + ",left=" + left + ",Top=" + top;

        //*** open new window and/or load new URL.
        wnd = window.open(uri, wndName, features, replace);
    }

    //*** Check for popup blocker!
    if (!wnd) {
        alert("Please disable your popup blocker!");
    }
    else {
        //*** Set focus to dialog.
        if (isFocus != false) {
            wnd.focus();
        }
    }

    //*** Add or update to array.
    wndChilds[idx] = Array(wndName, wnd);

    //*** return the handle of the windwo
    return wnd;
}

function Set_Cookie( name, value, expires, path, domain, secure )
{
    // set time, it's in milliseconds
    var today = new Date();
    today.setTime( today.getTime() );

    /*
    if the expires variable is set, make the correct
    expires time, the current script below will set
    it for x number of days, to make it for hours,
    delete * 24, for minutes, delete * 60 * 24
    */
    if ( expires )
    {
    expires = expires * 1000 * 60 * 60 * 24;
    }
    var expires_date = new Date( today.getTime() + (expires) );

    document.cookie = name + "=" +escape( value ) +
    ( ( expires ) ? ";expires=" + expires_date.toGMTString() : "" ) +
    ( ( path ) ? ";path=" + path : "" ) +
    ( ( domain ) ? ";domain=" + domain : "" ) +
    ( ( secure ) ? ";secure" : "" );
}


function Get_Cookie( check_name ) {
	// first we'll split this cookie up into name/value pairs
	// note: document.cookie only returns name=value, not the other components
	var a_all_cookies = document.cookie.split( ';' );
	var a_temp_cookie = '';
	var cookie_name = '';
	var cookie_value = '';
	var b_cookie_found = false; // set boolean t/f default f

	for ( i = 0; i < a_all_cookies.length; i++ )
	{
		// now we'll split apart each name=value pair
		a_temp_cookie = a_all_cookies[i].split( '=' );


		// and trim left/right whitespace while we're at it
		cookie_name = a_temp_cookie[0].replace(/^\s+|\s+$/g, '');

		// if the extracted name matches passed check_name
		if ( cookie_name == check_name )
		{
			b_cookie_found = true;
			// we need to handle case where cookie has no value but exists (no = sign, that is):
			if ( a_temp_cookie.length > 1 )
			{
				cookie_value = unescape( a_temp_cookie[1].replace(/^\s+|\s+$/g, '') );
			}
			// note that in cases where cookie is initialized but no value, null is returned
			return cookie_value;
			break;
		}
		a_temp_cookie = null;
		cookie_name = '';
	}
	if ( !b_cookie_found )
	{
		return null;
	}
}

// this deletes the cookie when called
function Delete_Cookie( name, path, domain ) {
    if ( Get_Cookie( name ) ) document.cookie = name + "=" +
    ( ( path ) ? ";path=" + path : "") +
    ( ( domain ) ? ";domain=" + domain : "" ) +
    ";expires=Thu, 01-Jan-1970 00:00:01 GMT";
}

function trim(myString)
{
    return myString.replace(/^\s+|\s+$/g, '');
}

function ValidateExtension(elem) {
    var filePath = elem.value;

    if (filePath.indexOf('.') == -1)
        return false;

    var validExtensions = new Array();
    var ext = filePath.substring(filePath.lastIndexOf('.') + 1).toLowerCase();

    validExtensions[0] = 'doc';
    validExtensions[1] = 'docx';
    validExtensions[2] = 'odt';

    for (var i = 0; i < validExtensions.length; i++) {
        if (ext == validExtensions[i])
            return true;
    }

    alert('The file extension ' + ext.toUpperCase() + ' is not allowed!');
    elem.value = "";
    return false;
}




