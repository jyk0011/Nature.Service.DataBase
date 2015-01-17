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
using System.IO;
using System.Web;
using System.Web.Services;
using Nature.Common;
using Nature.MetaData.Enum;

namespace Nature.Service.Data
{
    /// <summary>
    /// 上传文件的共用页面，这个是要改的，没弄好。
    /// </summary>
    [WebService(Namespace = "http://natureFW.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class UploadHandler : BaseAshxCrud
    {

        /// <summary>
        /// 接受上传的文件
        /// </summary>
        /// <param name="context"></param>
        public override void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Charset = "utf-8";

            string columnID = @context.Request["folder"];

            int index = columnID.LastIndexOf('/');
            columnID = columnID.Substring(index + 1);

            if (!Functions.IsInt(columnID))
            {
                context.Response.Write("文件夹设置不正确，不能上传文件！" + index + "_" + columnID);
                return;
            }

            //string sql = "select ControlInfo from Manage_Columns where ColumnID= " + columnID;
            //string sql = "select ControlInfo from Manage_Columns where ColumnID= {0}";
            //sql = string.Format(sql,columnID );

            //string controlInfo = base.Dal.DalMetadata.ExecuteString(sql);

            //FileUploadKind fileUploadKind = FileUploadKind.SiampleImage ;
            FileNameKind fileNameKind = FileNameKind.UserIDTime ;

            string[] tmpInfo = columnID.Split('|')[0].Split('~');

            //if (tmpInfo.Length < 3)
            //{
            //    context.Response.Write("配置信息不正确，不能上传文件！" + columnID + "_" + columnID);
            //    return;
            //}

            string filePath = "aaa";// tmpInfo[0];

            HttpPostedFile file = context.Request.Files["Filedata"];

            string uploadPath = HttpContext.Current.Server.MapPath("/" + filePath);

            if (file != null)
            {
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string fileName = file.FileName;
                string fileExt = fileName.Substring(fileName.LastIndexOf('.'));

                if (fileNameKind == FileNameKind.UserIDTime)
                {
                    //Nature.User.BaseUserInfo myUser = new Nature.User.BaseUserInfo();+ myUser.UserID + "_"
                    //myUser = (Nature.User.BaseUserInfo)context.Session[UserLoginSign + "sysUserInfo"];
                    fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + fileExt;
                    file.SaveAs(uploadPath + "\\" + fileName);
                }
                else
                {
                    file.SaveAs(uploadPath + fileName);

                }
                //file.SaveAs(uploadPath + "aaa.txt");
                //下面这句代码缺少的话，上传成功后上传队列的显示不会自动消失
                context.Response.Write(filePath + "/" + fileName + '`' + fileName);
            }
            else
            {
                context.Response.Write("0");
            }
        }
    }
}
