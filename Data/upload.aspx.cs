/**
 * 自然框架之信息管理类项目的页面基类
 * http://www.natureFW.com/
 *
 * @author
 * 金洋（金色海洋jyk）
 * 
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
 * function: 上传文件的共用页面
 * history:  created by 金洋   
 * **********************************************
 */

using System;

namespace Nature.Upload
{
    /// <summary>
    /// 上传文件的共用页面
    /// </summary>
    public partial class UploadFile : System.Web.UI.Page
    {
        /// <summary>
        /// 控件ID
        /// </summary>
        protected string ControlID = "";
        /// <summary>
        /// 字段ID
        /// </summary>
        protected string ColID = "";
        /// <summary>
        /// 文件扩展名
        /// </summary>
        protected string FileExt = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            ControlID = Request.QueryString["cid"];

            if (string.IsNullOrEmpty(ControlID))
            {
                Response.Write("cid参数不正确！");
                Response.End();
            }

            int index = ControlID.LastIndexOf('_');
            ColID = ControlID.Substring(index + 1);


        }
         
    }
}
