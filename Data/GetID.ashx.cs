using System;
using System.Collections.Generic;
using System.Web;
using Nature.Common;

namespace Nature.Service.Data
{
    /// <summary>
    /// 获取ID + 随机数 ，用于上传图片的预订ID
    /// </summary>
    public class GetID : BaseAshxCrud
    {
        /// <summary>
        /// 获取ID + 随机数 ，用于上传图片的预订ID
        /// </summary>
        /// <param name="context"></param>
        override public void ProcessRequest(HttpContext context)
        {
            base.ProcessRequest(context);
            //强制不缓存
            context.Response.Cache.SetNoStore();

            string id = "22";

            if (!string.IsNullOrEmpty(CallBack))
            {
                context.Response.Write(CallBack + "({");
            }

            switch (ModuleID)
            {
                case 1:
                    //新闻ID的最新
                    id = GetNewsID();
                    break;

            }

            context.Response.Clear();
            context.Response.Write(CallBack);
            context.Response.Write("({");
            
            context.Response.Write("\"id\":"+id);

            if (!string.IsNullOrEmpty(CallBack))
            {
                context.Response.Write("})");
            }

        }

        private string GetNewsID()
        {
            string id = "";
            string sql = "select max (NewsId) from web_News ";
            id = Dal.DalCustomer.ExecuteString(sql);
            id += Functions.RndInt(100, 999);
            return id;
        }


       
    }
}