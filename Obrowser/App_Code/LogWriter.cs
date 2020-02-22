using System;

public class LogWriter
{
	static bool DEBUG =false;
	public static void WriteLog(string msg)
	{		
		if(DEBUG)
		{
			System.IO.StreamWriter sw = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\App_Code\log.txt",true);
			sw.WriteLine(msg);
			sw.Close();
		}
	}
}