using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Web;
using Nature.Client.SSOApp;
using Nature.Data;
using Nature.DebugWatch;
using Nature.MetaData.Entity.WebPage;
using Nature.MetaData.ManagerMeta;
using Nature.Service.Ashx;
using Nature.Service.Data;
using Nature.User;

namespace Nature.Service
{
    /// <summary>
    /// 增删改查服务的基类
    /// </summary>
    public class BaseAshxCrud : BaseAshx
    {
        /// <summary>
        /// 数据访问函数库的实例的集合
        /// </summary>
        /// user:jyk
        /// time:2012/10/8 14:14
        public DalCollection Dal { get; set; }

        /// <summary>
        /// 当前节点的描述信息
        /// </summary>
        public PageViewMeta PageViewMeta { get; set; }

        /// <summary>
        /// 当前登录人的一些信息，和权限相关的信息
        /// </summary>
        /// user:jyk
        /// time:2013/2/5 14:25
        public UserOnlineInfo MyUser { get; set; }

        public override void Process()
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Service.BaseAshxMeta]根据webconfig的配置创建数据库访问实例" };

            //设置元数据的数据库的实例
            Dal = CommonClass.SetMetadataDal();
            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);

           
            if (!string.IsNullOrEmpty(DataBaseID))
            {
                debugInfo = new NatureDebugInfo { Title = "[Nature.Service.BaseAshxMeta]根据DataBaseID设置访问实例" };
                //根据DataBaseID，重新设置数据库访问实例
                string[] tmp = DataBaseID.Split(',');
                if (tmp.Length == 1)
                {
                    //采用统一数据库。
                    DataBaseConnInfo dataBaseConn = GetDataBaseConnInfo(tmp[0]);
                    Dal.DalCustomer = DalFactory.CreateDal(dataBaseConn.ConnString, dataBaseConn.Provider);
                    Dal.DalMetadata = Dal.DalCustomer;
                    Dal.DalRole = Dal.DalCustomer;
                    Dal.DalUser = Dal.DalCustomer;

                    debugInfo.Remark = "一个数据库";
                }
                else
                {
                    //两个数据库
                    DataBaseConnInfo dataBaseConn = GetDataBaseConnInfo(tmp[1]);
                    Dal.DalCustomer = DalFactory.CreateDal(dataBaseConn.ConnString, dataBaseConn.Provider);
               
                    dataBaseConn = GetDataBaseConnInfo(tmp[0]);
                    Dal.DalMetadata = DalFactory.CreateDal(dataBaseConn.ConnString, dataBaseConn.Provider);
                    Dal.DalRole = Dal.DalMetadata;
                    Dal.DalUser = Dal.DalMetadata;

                    debugInfo.Remark = "两个个数据库";

                }
                debugInfo.Stop();
                BaseDebug.DetailList.Add(debugInfo);
            }
            
            //调用父类的 Procss
            base.Process();


        }
         
        /// <summary>
        /// 获取页面视图的元数据
        /// </summary>
        /// user:jyk
        /// time:2012/10/10 11:07
        /// <param name="pageViewID">页面视图ID</param>
        /// <param name="debugInfoList">子步骤的列表</param>
        protected void GetPageViewMeta(int pageViewID, IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "创建页面视图管理类的实例" };
 
            var managerPageView = new ManagerPageViewMeta {DalCollection = Dal, PageViewID = pageViewID};
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);

            debugInfo = new NatureDebugInfo { Title = "获取页面视图的元数据 PageViewID:" + pageViewID };

            PageViewMeta = managerPageView.GetPageViewMeta(debugInfo.DetailList);

            debugInfo.Stop();
            debugInfoList.Add(debugInfo);

        }

        #region 验证用户是否登录 —— 单用户版

        protected override void CheckUser(IList<NatureDebugInfo> debugInfoList)
        {
            var manageUser = new ManageUser {Dal = Dal};
            var debugInfo = new NatureDebugInfo {Title = "[Nature.Service.BaseAshxMeta]单用户版验证是否登录"};

            ManageUser mUser = new ManageUser();

            string userId = mUser.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                //没有登录
                debugInfo.Title = "[Nature.Service.BaseAshxMeta]没有登录";

                Response.Write("\"msg\":\"没有登录\"");

                debugInfo.Stop();
                debugInfoList.Add(debugInfo);

                base.ProcessEnd();
                Response.End();

                MyUser = null;
            }
            else
            {
                MyUser = manageUser.CreateUser(userId, debugInfo.DetailList);
                //Response.Write("\"msg\":\"登录\"");

            }

        }

        #endregion

        #region 验证用户是否登录 多用户版

        /// <summary>
        /// 坚持当前访问者是否登录。
        /// 两种情况：
        /// 1、登录页面：这个不能检查，所以做个钩子，登录页面重新函数搞定
        /// 2、其他页面：已登录页面，需要检查了，放在基类里，子类省事了。
        /// </summary>
        /// <param name="debugInfoList">子步骤的列表</param>
        protected void CheckUser2(IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Service.BaseAshxMeta]询问SSO是否登录" };

            //验证是否已经登录
            //如果已经登录了，加载登录人员的信息，
            var manageUser = new ManageUser {Dal = Dal};
            UserWebappInfo userWebappInfo = AppManage.UserWebappInfoByCookies(debugInfo.DetailList );

            debugInfo.Stop();
            debugInfoList.Add(debugInfo);

            //第二步
            debugInfo = new NatureDebugInfo();
            if (userWebappInfo.State != UserState.NormalAccess)
            {
                //查看是否webapp服务器直接访问。这里先模拟一下

                //没有登录。
                debugInfo.Title = "[Nature.Service.BaseAshxMeta]没有登录";

                Response.Write("\"msg\":\"-1\"");

                debugInfo.Stop();
                debugInfoList.Add(debugInfo);

                base.ProcessEnd();
                Response.End();

                MyUser = null;

            }
            else
            {
                debugInfo.Title = "[Nature.Service.BaseAshxMeta]创建用户信息";
                MyUser = manageUser.CreateUser(Convert.ToString(userWebappInfo.UserWebappID), debugInfo.DetailList);

                //Response.Write(MyUser.BaseUser.UserID);
                //Response.End();

                debugInfo.Remark = userWebappInfo.UserSsoID.ToString(CultureInfo.InvariantCulture);
                BaseDebug.UserId = int.Parse(MyUser.BaseUser.UserID);

            }

            debugInfo.Stop();
            debugInfoList.Add(debugInfo);

        }

        #endregion


        #region 获取 访问数据库的连接字符串

        private DataBaseConnInfo GetDataBaseConnInfo(string dataBaseID)
        {
            DataBaseConnInfo dataBaseConn = DataBaseConnInfoCache(dataBaseID);

            if (dataBaseConn == null)
            {
                //没有指定的数据库
            }
            else
            {
                //判断权限
            }

            return dataBaseConn;

        }

        private DataBaseConnInfo DataBaseConnInfoCache(string dataBaseID)
        {
            Dictionary<string, DataBaseConnInfo> tmp;

            if (HttpContext.Current.Cache["DataBaseConnInfo"] == null)
            {
                tmp = new Dictionary<string, DataBaseConnInfo>();

                //加载
                const string sql = "SELECT DataBaseID, ConnString , Provider FROM Manage_DataBase";
                DataTable dt = Dal.DalMetadata.ExecuteFillDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    var dataBaseConn = new DataBaseConnInfo
                    {
                        ConnString = dr[1].ToString(),
                        Provider = dr[2].ToString()
                    };

                    tmp.Add(dr[0].ToString(), dataBaseConn);

                }

                HttpContext.Current.Cache.Insert("DataBaseConnInfo", tmp);
            }
            else
            {
                //获取
                tmp = (Dictionary<string, DataBaseConnInfo>)HttpContext.Current.Cache["DataBaseConnInfo"];

            }

            return tmp.ContainsKey(dataBaseID) ? tmp[dataBaseID] : null;

        }
        #endregion
   
    }
}