using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using PdfSharp.Drawing;


/// <summary>
/// Summary description for ColorUtil
/// </summary>
public class ColorUtil
{
    string[] colors = HIT.OB.STD.Core.ColorCodes.colorList;
       
    
    public ColorUtil()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public Dictionary<string, string> GetGroupColors(string reportCode,DataTable dtGroups, string table, string groupby)
    {
        Dictionary<string, string> dicColors = new Dictionary<string, string>();

        try
        {
            //table parameter is not used
            //string selectGroupsByASC = "select count(*), " + groupby +
            //    " from " + table + " group by " + groupby + " order by " + groupby + " asc";

            //DataTable dtNewGroups = new DBUtil().GetDataTable(selectGroupsByASC); //it is not used

            int totalGroups = dtGroups.Rows.Count;

            int[] colorIndex = null;

            //if (totalGroups > 0 && totalGroups <= 24)
            //{
            //    colorIndex = new int[] { 17, 20, 27, 54, 49, 58, 86, 79, 96, 100, 139, 144, 
            //        153, 164, 200, 205, 215, 219, 224, 245, 249, 252, 255, 24 };
            //}
            //else if(totalGroups>24)
            {
                colorIndex = new int[colors.Length];

                for (int i = 0; i < colorIndex.Length; i++)
                {
                    colorIndex[i] = i;
                }
            }

            if (!string.IsNullOrEmpty(groupby) && !groupby.Equals("NONE"))
            {
                string select = "select COLORCODE,GROUPCODE from group_color where REPORTCODE='" + reportCode + "' and "
                    + " GROUPBY='" + groupby + "'";

                for (int i = 0; i < totalGroups; i++)
                {
                    string groupName = dtGroups.Rows[i][groupby].ToString().Replace("'", "\\'"); 

                    if (i == 0)
                    {
                        select += " AND GROUPCODE in ('" + groupName + "'";
                    }

                    if (i > 0)
                    {
                        select += ",'" + groupName + "'";
                    }

                    if (i == totalGroups - 1)
                    {
                        select += " )";
                    }
                }

                if (totalGroups > 0)
                {
                    DataTable dtSavedColor = new DBUtil().GetDataTable(select,reportCode);

                    for (int i = 0; i < dtSavedColor.Rows.Count; i++)
                    {
                        if (!dicColors.ContainsKey(dtSavedColor.Rows[i][1].ToString()))
                        {
                            dicColors.Add(dtSavedColor.Rows[i][1].ToString(), dtSavedColor.Rows[i][0].ToString());
                        }
                    }

                    for (int i = 0; i < totalGroups; i++)
                    {
                        if (!dicColors.ContainsKey(dtGroups.Rows[i][groupby].ToString()))
                        {
                            if (i < colorIndex.Length && colorIndex[i] < colors.Length)
                            {
                                dicColors.Add(dtGroups.Rows[i][groupby].ToString(),
                                    colors[colorIndex[i]]);
                            }
                            else
                            {
                                dicColors.Add(dtGroups.Rows[i][groupby].ToString(),
                                    "FFFFFF");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception oEx)
        {
            throw oEx;
        }

        return dicColors;
    }
}
