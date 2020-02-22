using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections;

/// <summary>
/// Summary description for IOBFunctions
/// </summary>
namespace HIT.PEditor.Core
{
    public interface IDatabaseFunctionsManager
    {
        string GetQuery(string tabelName,string groupName);
        DataTable GetDataTable(string query);
        
    }
}
