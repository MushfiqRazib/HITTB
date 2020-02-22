using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

/// <summary>
/// Summary description for CommonFunctions
/// </summary>
namespace HIT.OB.STD.Wrapper
{
    public class CommonFunctions
    {
        public CommonFunctions()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public static string GetCaption(string field, string[] capList)
        {
            foreach(string fieldCapDef in capList)
            {
                string[] capdefParts = fieldCapDef.Split('=');
                if (field.ToLower().Equals(capdefParts[0].ToLower()))
                {
                    return capdefParts[1];   
                }   
            }
            return field;
        }

        

        public static string GetDocBasePath()
        {
            return HIT.OB.STD.Wrapper.DAL.ConfigManager.GetDocBasePath();
           
            
            //*** Following section were for .Net
            //string line, basePath = string.Empty;
            //string configPath = AppDomain.CurrentDomain.BaseDirectory + "\\Components\\config.asp";
            //constantName = constantName.ToLower();
            ////string configPath = @"E:\Projecten\ID01\ID01_Remote\trunk\www\ID01\" + "\\config.php";

            //// Read the file and retrieve the connection string
            //using (System.IO.StreamReader file = new System.IO.StreamReader(configPath))
            //{
            //    while ((line = file.ReadLine().ToLower()) != null)
            //    {
            //        if (line.StartsWith("const") && line.IndexOf(constantName)>0)
            //        {
            //            LogWriter.WriteLog("line:"+line);
            //            System.Text.RegularExpressions.Regex rsRegEx = new System.Text.RegularExpressions.Regex(@"\s+");
            //            basePath = line.Replace("const", "").Replace("=", "").Replace(constantName, "").Replace("\"","").Trim();                                          
            //            break;
            //        }
            //    }
            //}
            
        }
        public static List<Template> GetTemplates(string netDir)
        {
            //determine our valid file extensions
            //create a string array of our filters by splitting the
            //string of valid filters on the delimiter
            List<FileInfo> allFiles = new DirectoryInfo(netDir).GetFiles("*", SearchOption.AllDirectories).ToList();
            var templateFiles = from file in allFiles
                                where file.Name.ToUpper().StartsWith("H_")
                                select file;
            var templateInfo = from file in templateFiles
                               let templateType = file.Name.Split('_')[1]
                               orderby templateType
                               select new { Name = file.Name, Type = templateType, Fullname = file.FullName };
            List<Template> templates = new List<Template>();
            foreach (var ti in templateInfo)
            {
                Template temp = new Template();
                temp.Name = ti.Name;
                temp.Type = ti.Type;
                temp.Fullname = ti.Fullname;
                templates.Add(temp);
            }
            return templates.ToList();
        }



    }


     
}
