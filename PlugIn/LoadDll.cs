using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using Nature.Common;
using Nature.Data;

namespace Nature.Service.PlugIn
{
    /// <summary>
    /// 动态加载dll
    /// </summary>
    public class LoadDll
    {
        public void a()
        {
            DataAccessLibrary p = null;
         
            //string instanceType = ""; //ConfigurationSettings.AppSettings["TnstanceType"].Trim();
            string assembleFileName = @"D:\C#\Demo\Server\bin\Debug\Demo.Server.dll";

            //LoadFrom 载入dll文件及其引用的其他dll
            //只载入相应的dll文件
            Assembly assm = Assembly.LoadFile(assembleFileName);
            Type objType = assm.GetType("Demo.Server.American");
            object objInstance = Activator.CreateInstance(objType, true);

            p = objInstance as DataAccessLibrary;
            if (p != null) p.ExecuteExists("");
        }
    }
}