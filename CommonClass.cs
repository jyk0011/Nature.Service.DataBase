
/* 
 * @copyright
 * Copyright (C) 2005-2013 金洋.
 *
 * Licensed under a GNU Lesser General Public License.
 * http://creativecommons.org/licenses/LGPL/2.1/
 *
 * 自然框架之信息管理类项目的页面基类 is free software. You are allowed to download, modify and distribute 
 * the source code in accordance with LGPL 2.1 license, however if you want to use 
 * 自然框架之信息管理类项目的页面基类 on your site or include it in your commercial software, you must  be registered.
 * http://www.natureFW.com/registered
 */

/* ***********************************************
 * author :  金洋（金色海洋jyk）
 * email  :  jyk0011@live.cn  
 * function: 后台管理的页面基类。实例化数据访问函数库的各个实例
 * history:  created by 金洋 
 *           2012-11-12 整理
 * **********************************************
 */

using System.Configuration;
using System.Web;
using System.Web.Configuration;
using Nature.Common;
using Nature.Data;

namespace Nature.Service
{
    /// <summary>
    /// 实例化数据访问函数库的各个实例
    /// 验证URL里的参数
    /// </summary>
    /// user:jyk
    /// time:2012/11/12 16:09
    public static class CommonClass
    {
        #region 设置连接元数据所在的数据库的实例
        /// <summary>
        /// 设置连接元数据所在的数据库的实例
        /// </summary>
        public static DalCollection SetMetadataDal()
        {
            //根据配置信息设置访问元数据的实例

            string kind = "1";       //1：与客户数据库相同；2：从配置信息里面获取；3：从数据库获取

            if (WebConfigurationManager.AppSettings["MetadataKind"] != null)
            {
                kind = WebConfigurationManager.AppSettings["MetadataKind"];
            }

            var dal = new DalCollection {};

            string cnString;
            string providerName;
            switch (kind)
            {
                case "1":   //与客户数据库相同
                    dal.DalCustomer = DalFactory.CreateDal();
                    
                    dal.DalMetadata = dal.DalCustomer;
                    dal.DalRole = dal.DalCustomer;
                    dal.DalUser = dal.DalCustomer;
                    break;
                case "2":   //webconfig

                    dal.DalCustomer = DalFactory.CreateDal();
                    
                    cnString = ConfigurationManager.ConnectionStrings["CnStringMeta"].ConnectionString;
                    providerName = ConfigurationManager.ConnectionStrings["CnStringMeta"].ProviderName;
                    dal.DalMetadata = DalFactory.CreateDal(cnString, providerName);

                    cnString = ConfigurationManager.ConnectionStrings["CnStringRole"].ConnectionString;
                    providerName = ConfigurationManager.ConnectionStrings["CnStringRole"].ProviderName;
                    dal.DalRole = DalFactory.CreateDal(cnString, providerName);

                    cnString = ConfigurationManager.ConnectionStrings["CnStringUser"].ConnectionString;
                    providerName = ConfigurationManager.ConnectionStrings["CnStringUser"].ProviderName;
                    dal.DalUser = DalFactory.CreateDal(cnString, providerName);

                    break;

                case "3":   //数据库控制
                    dal.DalCustomer = DalFactory.CreateDal();
                    
                    cnString = ConfigurationManager.ConnectionStrings["CnStringMeta"].ConnectionString;
                    providerName = ConfigurationManager.ConnectionStrings["CnStringMeta"].ProviderName;
                    dal.DalMetadata = DalFactory.CreateDal(cnString, providerName);

                    cnString = ConfigurationManager.ConnectionStrings["CnStringRole"].ConnectionString;
                    providerName = ConfigurationManager.ConnectionStrings["CnStringRole"].ProviderName;
                    dal.DalRole = DalFactory.CreateDal(cnString, providerName);

                    cnString = ConfigurationManager.ConnectionStrings["CnStringUser"].ConnectionString;
                    providerName = ConfigurationManager.ConnectionStrings["CnStringUser"].ProviderName;
                    dal.DalUser = DalFactory.CreateDal(cnString, providerName);

                    break;

                case "4":   //自然框架支撑平台专用
                    //元数据、用户、权限——webconfig 获取
                    
                    cnString = ConfigurationManager.ConnectionStrings["CnStringMeta"].ConnectionString;
                    providerName = ConfigurationManager.ConnectionStrings["CnStringMeta"].ProviderName;
                    dal.DalMetadata = DalFactory.CreateDal(cnString, providerName);

                    cnString = ConfigurationManager.ConnectionStrings["CnStringRole"].ConnectionString;
                    providerName = ConfigurationManager.ConnectionStrings["CnStringRole"].ProviderName;
                    dal.DalRole = DalFactory.CreateDal(cnString, providerName);

                    cnString = ConfigurationManager.ConnectionStrings["CnStringUser"].ConnectionString;
                    providerName = ConfigurationManager.ConnectionStrings["CnStringUser"].ProviderName;
                    dal.DalUser = DalFactory.CreateDal(cnString, providerName);

                    //客户 —— 根据选择的项目来定，读取客户项目的元数据数据库

                    //从cookies里获取项目id
                    HttpCookie ck = HttpContext.Current.Request.Cookies["DataBaseID"];

                    if (ck == null)
                    {
                        //没有cookies
                    }
                    else
                    {
                        //有cookies，获取项目ID，然后获取连接字符串
                        string dataBaseID = ck.Value;
                        if (!Functions.IsInt( dataBaseID))
                        {
                            dataBaseID = "1";
                        }

                        string sql = "SELECT TOP 1 ConnString, Provider FROM Manage_DataBase WHERE DataBaseID = " + dataBaseID;
                        
                        //string[] str = dal.DalMetadata.ExecuteStringsBySingleRow(string.Format(sql, ck.Value));
                        string[] str = dal.DalMetadata.ExecuteStringsBySingleRow(sql);

                        
                        dal.DalCustomer = str != null ? DalFactory.CreateDal(str[0], str[1]) : DalFactory.CreateDal();
                    }
                    break;

                case "5"://根据DataBaseID 申请。
                    break;

            }

            if (dal.DalCustomer == null )
                dal.DalCustomer = DalFactory.CreateDal();

            return dal;

        }
        #endregion

      
    }
}
