//******************************************************************************
//*** Public Configuration parameters
//******************************************************************************
var CALENDERNAME     = "HITCalendar";

var BTN_PREV         = "./images/cal_prev.gif";
var BTN_NEXT         = "./images/cal_next.gif";

var FONT_YEAR        = "normal 12px Arial";
var FONT_DAYS        = "normal 11px Arial";

var CLR_BORDER       = "#4682B4";     //*** Calendar border.
var CLR_YEAR_BGR     = "#4682B4";
var CLR_YEAR_TXT     = "#FFFFFF";
var CLR_WEEK_BGR     = "#87CEFA";
var CLR_WEEK_TXT     = "#FFFFFF";
var CLR_TODAY_BGR    = "#FFB6C1";
var CLR_WEEKDAY_BGR  = "#FFFFFF";
var CLR_WEEKEND_BGR  = "#DBEAF5";
var CLR_OTHERMONTH   = "#BBBBBB";
var CLR_SELECTED_BGR = "#0A246A";
var CLR_SELECTED_TXT = "#FFFFFF";


//******************************************************************************
//*** Private Global Arrays
//******************************************************************************
//*** Set English arrays.
//var LIST_DAYS   = new Array("Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday");
//var LIST_MONTHS = new Array("January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December");

//*** Set Dutch arrays.
var LIST_DAYS   = new Array("Zondag", "Maandag", "Dinsdag", "Woensdag", "Donderdag", "Vrijdag", "Zaterdag");
var LIST_MONTHS = new Array("Januari", "Februari", "Maart", "April", "Mei", "Juni", "Juli", "Augustus", "September", "Oktober", "November", "December");


//******************************************************************************
//*** Private Global Constants
//******************************************************************************
var WEEKSTART = 1;     //*** Day week starts from (normally 0=Su or 1=Mo)


//******************************************************************************
//*** Private Global Variables
//******************************************************************************
var gCalendar     = null;
var gCalendarLeft = 0;
var gCalendarTop  = 0;
var gDateFormat   = "";
var gDisplay      = "";

var d_current     = new Date();
var d_selected    = new Date();



//******************************************************************************
//*** Public Methods
//******************************************************************************
function selectDate(display_id, format)
{
        
  var old_id = gDisplay;  
  
  //*** Set output format.
  gDateFormat = (format == null || format == "") ? "dd-mm-yyyy" : format;
  
  //*** Get calendar element.
  gCalendar = getCalendar();
  
  //*** Hide calendar.
  hideCalendar();
  
  //*** Show calendar?
  if ((old_id != display_id) && initCalendar(display_id)) 
  {
    fillCalendar();    
  }     
    
}



//******************************************************************************
//*** Private functions
//******************************************************************************
function compareDate(d1, d2)
{
  //*** Return if dates are equal.
  return ((d1.getDate() == d2.getDate()) && (d1.getMonth() == d2.getMonth()) && (d1.getFullYear() == d2.getFullYear()));
}


function fillCalendar()
{
  var content = '<TABLE cellspacing="1" cellpadding="2" style="font: ' + FONT_DAYS + '">';
  
  //*** Write 'Month + Year' header.
  content += getYearHeader();
  
  //*** Write days of week.
  content += getWeekHeader();
  
  //*** Write days of month.
  content += getDaysOfMonth();
  
  //*** Write end of calendar.
  content += '</TABLE>';
  
  //*** Set calendar content.
  gCalendar.innerHTML = content;
  
  //*** Now show calendar.
  gCalendar.style.display = "block";
}


function formatDate(dateObject, format)
{
  var day_of_week  = dateObject.getDay();
  var day_of_month = dateObject.getDate();
  var month        = dateObject.getMonth() + 1;
  var year         = dateObject.getFullYear();
  var date_str     = "";
  var i            = 0;
  var c            = "";
  var token        = "";
  var value        = new Object();
  
  //*** Create format array.
  value["d"]    = day_of_month;
  value["dd"]   = ((day_of_month < 10) ? "0" : "") + day_of_month;
  value["ddd"]  = getShortDay(day_of_week);
  value["dddd"] = getFullDay(day_of_week);
  value["m"]    = month;
  value["mm"]   = ((month < 10) ? "0" : "") + month;
  value["mmm"]  = getShortMonth(month - 1);
  value["mmmm"] = getFullMonth(month - 1);
  value["y"]    = year.toString().substr(2,2);
  value["yy"]   = year.toString().substr(2,2);
  value["yyy"]  = year;
  value["yyyy"] = year;
  
  //*** Set format to lower case.
  format = format.toLowerCase();
  
  while (i < format.length)
  {
    c     = format.charAt(i);
    token = "";
    
    while ((format.charAt(i) == c) && (i < format.length))
    {
      token += format.charAt(i++);
    }
    
    if (value[token] != null)
    {
      date_str += value[token];
    }
    else
    {
      date_str += token;
    }
  }
  
  return date_str;
}


function getCalendar()
{
  var cal = document.getElementById(CALENDERNAME);
  
  if (cal == null)
  {
    //*** Create container for calendar.
    cal = document.createElement("DIV");
    
    //*** Set attributes.
    cal.id = CALENDERNAME;
    
    document.body.appendChild(cal);
  }
  
  //*** Set style.
  cal.style.backgroundColor = CLR_BORDER;
  cal.style.display         = "none";
  cal.style.position        = "absolute";
  cal.style.width           = "176px";
  
  return cal;
}


function getDaysOfMonth()
{
  var d_today    = new Date();
  var d_firstday = new Date(d_current);
  var result     = '';
  var clr_font   = '';
  var clr_backgr = '';
  
  //*** Get first day of current month.
  d_firstday.setDate(1);
  d_firstday.setDate(1 - (7 + d_firstday.getDay() - WEEKSTART) % 7);
  
  var d_day = new Date(d_firstday);
  
  //*** Always display 6 weeks.
  for (var weeks = 0; weeks < 6; weeks++)
  {
    result += '<TR>';
    
    for (var i = 0; i < 7; i++)
    {
      //*** Day part of current month?
      clr_font = (d_day.getMonth() == d_current.getMonth()) ? '' : CLR_OTHERMONTH;
      
      if (compareDate(d_day, d_selected) == true)
      {
        //*** Selected date.
        clr_backgr = CLR_SELECTED_BGR;
        clr_font   = CLR_SELECTED_TXT;
      }
      else if (compareDate(d_day, d_today) == true)
      {
        //*** Current date.
        clr_backgr = CLR_TODAY_BGR;
      }
      else if (d_day.getDay() == 0 || d_day.getDay() == 6)
      {
        //*** Weekend day.
        clr_backgr = CLR_WEEKEND_BGR;
      }
      else
      {
      	//*** Working day.
      	clr_backgr = CLR_WEEKDAY_BGR;
      }
      
      //*** Write day.
      result += '<TD align="center" style="background-color: ' + clr_backgr + '; color: ' + clr_font + '; cursor: pointer" width="20" onclick="set_datetime(' + d_day.getTime() + ')">' + d_day.getDate() + '</TD>';
      
      //*** Next day.
      d_day.setDate(d_day.getDate() + 1);
    }
    
    result += '</TR>';
  }
  
  return result;
}


function getFullDay(day_number)
{
  //*** Return full expression of day.
  return LIST_DAYS[day_number];
}


function getFullMonth(month_number)
{
  //*** Return full expression of month.
  return LIST_MONTHS[month_number];
}


function getShortDay(day_number)
{
  //*** Return short expression of day.
  return LIST_DAYS[day_number].substr(0,2);
}


function getShortMonth(month_number)
{
  //*** Return short expression of month.
  return LIST_MONTHS[month_number].substr(0,3);
}


function getWeekHeader()
{
  var result = '<TR style="background-color: ' + CLR_WEEK_BGR + '; color: ' + CLR_WEEK_TXT + '">';
  
  for(var i = 0; i < 7; i++)
  {
    result += '<TD align="center" width="20">' + getShortDay((WEEKSTART + i) % 7) + '</TD>';
  }
  
  result += '</TR>';
  
  return result;
}


function getYearHeader()
{
  var result = '<TR style="background-color: ' + CLR_YEAR_BGR + '">';
  
  result += '<TD colspan="7">';
  result += '<TABLE cellspacing="0" cellpadding="0" style="color: ' + CLR_YEAR_TXT + '; font: ' + FONT_YEAR + '" width="100%">';
  result += '<TR>';
  result += '<TD><IMG src="' + BTN_PREV + '" style="cursor: pointer" alt="Previous Year" onclick="shiftYear(-1)"></TD>';
  result += '<TD>&nbsp;</TD>';
  result += '<TD><IMG src="' + BTN_PREV + '" style="cursor: pointer" alt="Previous Month" onclick="shiftMonth(-1)"></TD>';
  result += '<TD align="center" width="100%" nowrap>' + getFullMonth(d_current.getMonth()) + ' ' + d_current.getFullYear() + '</TD>';
  result += '<TD><IMG src="' + BTN_NEXT + '" style="cursor: pointer" alt="Next Month" onclick="shiftMonth(1)"></TD>';
  result += '<TD>&nbsp;</TD>';
  result += '<TD><IMG src="' + BTN_NEXT + '" style="cursor: pointer" alt="Next Year" onclick="shiftYear(1)"></TD>';
  result += '</TR>';
  result += '</TABLE>';
  result += '</TD>';
  result += '</TR>';
  
  return result;
}


function initCalendar(id)
{
  var elems = document.getElementsByName(id);
  
  //*** Does calendar exist?
  if (gCalendar == null)
  {
    alert("Calendar not found!");
    
    return false;
  }
  
  //*** Does input element exist?
  if (elems.length <= 0)
  {
    alert("Input element not found!");
    
    return false;
  }
  
  var elem = elems[0];
  var top  = 0;
  var left = 0;
  
  while (elem != null)
  {
    top  += elem.offsetTop;
    left += elem.offsetLeft;
    
    elem = elem.offsetParent;
  }
  
  gCalendar.style.top  = top + elems[0].offsetHeight;
  gCalendar.style.left = left;
  gDisplay             = id;
  
  return true;
}


//******************************************************************************
//*** Events
//******************************************************************************
//document.onclick = hideCalendar;
//document.onclick = new Function("hideCalendar()");

function hideCalendar()
{
  //*** Hide calendar, if present.
  if (gCalendar)
  {
    gCalendar.style.display = "none";
    gCalendar.style.left    = 0;
    gCalendar.style.top     = 0;        
  }        
  gDisplay = "";   
  
}


//*** function passing selected date to calling window.
function set_datetime(n_datetime)
{
  var elems = document.getElementsByName(gDisplay);
  
  if (elems.length > 0)
  {
    d_selected = new Date(n_datetime);
    
    elems[0].value = formatDate(d_selected, gDateFormat);
  }
  
  hideCalendar();
  
  
}


function shiftMonth(direction)
{
  var d_newdate = new Date(d_current);
  
  //*** Shift to previous/next month.
  d_newdate.setMonth(d_newdate.getMonth() + direction);
  
  if (d_newdate.getDate() != d_current.getDate())
  {
    //*** Goto first day of month.
    d_newdate.setDate(0);
  }
  
  //*** Set current date to new date.
  d_current = new Date(d_newdate);
  
  //*** Refresh calendar.
  fillCalendar();
}


function shiftYear(direction)
{
  var d_newdate = new Date(d_current);
  
  //*** Shift to previous/next month.
  d_newdate.setFullYear(d_newdate.getFullYear() + direction);
  
  if (d_newdate.getDate() != d_current.getDate())
  {
    //*** Goto first day of month.
    d_newdate.setDate(0);
  }
  
  //*** Set current date to new date.
  d_current = new Date(d_newdate);
  
  //*** Refresh calendar.
  fillCalendar();
}