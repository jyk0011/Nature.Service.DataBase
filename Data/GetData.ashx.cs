using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Web;
using Nature.Common;
using Nature.Data;
using Nature.DebugWatch;
using Nature.MetaData.Entity;
using Nature.MetaData.Manager;
using Nature.MetaData.ManagerMeta;
using Nature.UI.WebControl.QuickPagerSQL;

namespace Nature.Service.Data
{
    /// <summary>
    /// 访问数据库的连接字符串和驱动名称
    /// </summary>
    /// user:jyk
    /// time:2013/3/7 11:17
    public class DataBaseConnInfo
    {
        public string ConnString;
        public string Provider;
    }

    /// <summary>
    /// 获取指定页面列表视图里的记录，带分页和查询
    /// </summary>
    /// user:jyk
    /// time:2012/10/10 8:58
    public class GetData : BaseAshxCrud
    {
        public override void Process()
        {
            base.Process();
            //强制不缓存
            Response.Cache.SetNoStore();

            //定义操作日志
            var operateLog = new ManagerLogOperate
            {
                AddUserID = Int32.Parse(MyUser.BaseUser.UserID),
                Dal = Dal.DalCustomer,
                ModuleID = ModuleID,
                ButtonID = ButtonID,
                PageViewID = MasterPageViewID
            };


            var json = new StringBuilder(3000);

            switch (Request.QueryString["action"].ToLower())
            {
                case "listkey": //替换列表里的标识
                    operateLog.OperateKind = 11;
                    BaseDebug.Title = "替换列表里的标识";
                    GetDataListSource(json, Dal.DalCustomer, true, true, false);
                    break;

                case "list": //多条记录，分页获取
                    operateLog.OperateKind = 12;
                    BaseDebug.Title = "多条记录，分页获取";
                    GetDataListSource(json, Dal.DalCustomer, true, false, true);
                    break;

                case "metalistrole": //多条元数据——角色管理里面，权限设置里面，获取指定项目的元数据
                    operateLog.OperateKind = 13;
                    BaseDebug.Title = "metalistrole";
                    GetDataListSource(json, Dal.DalCustomer, false, true, false);
                    break;

                case "rolelist": //多条角色记录
                    operateLog.OperateKind = 14;
                    BaseDebug.Title = "多条角色记录";
                    GetDataListSource(json, Dal.DalCustomer, false, true, false);
                    break;

                case "listall": //全部记录，不分页
                    operateLog.OperateKind = 15;
                    BaseDebug.Title = "全部记录，不分页";
                    GetDataListSource(json, Dal.DalCustomer, true, false, false);
                    break;

                case "one": //一条记录
                    operateLog.OperateKind = 16;
                    BaseDebug.Title = "获取指定的一条记录";
                    GetDataOne(json);
                    break;

                case "user": //获取用户信息
                    operateLog.OperateKind = 17;
                    BaseDebug.Title = "获取用户信息";
                    GetUserInfo(json);
                    break;

                case "userthis": //获取当前用户信息
                    operateLog.OperateKind = 17;
                    BaseDebug.Title = "获取用户信息";
                    GetUserInfoThis(json);
                    break;

                case "nextorder": //下一个分类的序号
                    operateLog.OperateKind = 18;
                    BaseDebug.Title = "下一个分类的序号";
                    GetNextOrder(json);
                    break;

                case "yearmonth":
                    operateLog.OperateKind = 25;
                    BaseDebug.Title = "获取服务器的年月";
                    json.Append(string.Format("\"re\":\"{0}\"", DateTime.Now.ToString("yyyyMM")));
                    break;
                case "date":
                    operateLog.OperateKind = 19;
                    BaseDebug.Title = "获取服务器的年月日";
                    json.Append(string.Format("\"re\":\"{0}\"", DateTime.Now.ToString("yyyy-MM-dd")));
                    break;
                case "datetime":
                    operateLog.OperateKind = 20;
                    BaseDebug.Title = "获取服务器的年月日 时分";
                    json.Append(string.Format("\"re\":\"{0}\"", DateTime.Now.ToString("yyyy-MM-dd HH:mm")));
                    break;
                case "time":
                    operateLog.OperateKind = 21; 
                    BaseDebug.Title = "获取服务器的小时和分钟";
                    json.Append(string.Format("\"re\":\"{0}\"", DateTime.Now.ToString("HH:mm")));
                    break;

                case "piclist":
                    operateLog.OperateKind = 22;
                    BaseDebug.Title = "获取上传图片的图片ID集合";
                    GetPicList(json);
                    break;

                case "picdel":
                    operateLog.OperateKind = 23;
                    BaseDebug.Title = "删除指定的图片";
                    PicDel(json);
                    break;

                default: //没有
                    operateLog.OperateKind = 24;
                    BaseDebug.Title = "没有这个action" + Action;
                    json.Append("\"err\":\"没有这个action\"");
                    break;
            }


            var debugInfo2 = new NatureDebugInfo { Title = "保存操作记录" };
            #region 保存操作记录
            operateLog.WriteOperateLog(debugInfo2.DetailList);
            debugInfo2.Stop();
            BaseDebug.DetailList.Add(debugInfo2);
            #endregion

            Response.Write(json.ToString());

        }

        #region 获取图片ID

        /// <summary>
        /// 获取图片ID
        /// </summary>
        /// <param name="json">The json.</param>
        /// user:jyk
        /// time:2013/8/16 10:05
        private void GetPicList(StringBuilder json)
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Service.Data.GetData.GetPicList]获取图片ID" };
            BaseDebug.DetailList.Add(debugInfo);

            string attachCalssId = Request.QueryString["classID"];
            string mapID = Request.QueryString["mapID"];
            if (!Functions.IsInt(attachCalssId))
            {
                json.Append("\"err\":\"classID不正确！\"");
                debugInfo.Stop();
                return;
            }

            if (!Functions.IsInt(mapID))
            {
                json.Append("\"err\":\"mapID不正确！\"");
                debugInfo.Stop();
                return;
            }

            const string sql = "SELECT AttachId FROM pub_Attachment WHERE (AttachCalssId = {0}) AND (MapID = {1}) AND isdel = 0";

            string[] data = Dal.DalCustomer.ExecuteStringsByColumns(string.Format(sql, attachCalssId, mapID));

            json.Append("\"AttachIds\":{");

            if (data != null)
            {
                foreach (string  pic in data)
                {
                    json.Append("\"");
                    json.Append(pic);
                    json.Append("\":1,");

                }
            }
            if (json[json.Length - 1] == '{')
                json.Append('}');
            else
                json[json.Length - 1] = '}';

            debugInfo.Stop();

        }

        #endregion

        #region 逻辑删除图片

        /// <summary>
        /// 获取图片ID
        /// </summary>
        /// <param name="json">The json.</param>
        /// user:jyk
        /// time:2013/8/16 10:05
        private void PicDel(StringBuilder json)
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Service.Data.GetData.PicDel]获取图片ID" };
            BaseDebug.DetailList.Add(debugInfo);

            //const string sql = "update pub_Attachment SET isdel = 1 WHERE (AttachId = {0} )";
            const string sql = "update pub_Attachment set isdel = 1 where AttachID = {0}";

            Dal.DalCustomer.ExecuteNonQuery(string.Format(sql,DataID));

            if (Dal.DalCustomer.ErrorMessage.Length > 1)
            {
                json.Append("\"err\":\"删除时出现意外！\"");
                debugInfo.Stop();
                return;
            }

            json.Append("\"msg\":\"\"");
             

            debugInfo.Stop();

        }

        #endregion

        #region 获取用户信息

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="json">The json.</param>
        /// user:jyk
        /// time:2012/10/27 10:05
        private void GetUserInfo(StringBuilder json)
        {
            var debugInfo = new NatureDebugInfo {Title = "[Nature.Service.Data.GetData.GetUserInfo]根据appUserID获取用户信息"};

            const string sql =
                "select top 1 UserID,UserID,UserCode,PersonName,WebAppID from Person_User_Info where WebAppID = {0} and  UserID ={1}";

            string data = Dal.DalCustomer.ManagerJson.ExecuteFillJsonByColName(string.Format(sql, WebAppID, UserAppID));
            json.Append(data);
            //json.Append(CreateJson(data, "获取用户信息", debugTime.ToString(), "获取用户信息", string.Format(sql, WebAppID, UserAppID)));

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);

        }

        #endregion

        #region 获取用户信息

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <param name="json">The json.</param>
        /// user:jyk
        /// time:2012/12/16 10:05
        private void GetUserInfoThis(StringBuilder json)
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Service.Data.GetData.GetUserInfoThis]获取当前用户信息" };

            json.Append("\"PersonID\":\"");
            json.Append(MyUser.BaseUser.UserID);
            json.Append("\",\"PersonName\":\"");
            json.Append(MyUser.BaseUser.PersonName);
            json.Append("\",\"UserCode\":\"");
            json.Append(MyUser.BaseUser.UserCode);
            json.Append("\",\"UserID\":\"");
            json.Append(MyUser.BaseUser.UserID);
            json.Append("\",\"DepartmentID\":\"");
            json.Append(MyUser.BaseUser.DepartmentID[0]);
            json.Append("\",\"DepartmentName\":\"");
            json.Append(MyUser.BaseUser.DepartmentName);
            json.Append("\",\"RoleIDs\":\"");
            json.Append(MyUser.UserPermission.RoleIDs);
            json.Append("\"");
           
            //json.Append(CreateJson(data, "获取用户信息", debugTime.ToString(), "获取用户信息", string.Format(sql, WebAppID, UserAppID)));

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);

        }

        #endregion

        #region 获取下一个分类的序号
        private void GetNextOrder(StringBuilder json)
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Service.Data.GetData.GetNextOrder]获取下一个同级分类的序号" };

            string orderColName = "";           //排序字段的名称
            string orderValue = "";             //排序字段的值
            string nextOrderValue = "";         //下一个排序字段的值
            string parentIDName = "";           //父节点字段名称
            string parentIDValue = "";          //父节点字段值
            string parentIDAllName = "";        //路径字段名
            string parentIDAllValue = "";       //路径值

            string err = "";

            orderValue = Request["order"];
            if (!Functions.IsInt(orderValue))
            {
                //排序字段值不正确，不能排序
                return ;
            }
            parentIDAllValue = Request["pidall"];
            if (!Functions.IsIDString(parentIDAllValue))
            {
                //排序字段值不正确，不能排序
                return;
            }

            var debugInfo2 = new NatureDebugInfo { Title = "创建表单元数据管理的实例，并且获取排序字段名称" };

            #region 创建表单元数据管理的实例
            var managerMeta = new ManagerFormMeta
            {
                DalCollection = Dal,
                PageViewID = MasterPageViewID
            };
            Dictionary<int, IColumn> dicBaseCols = managerMeta.GetMetaData(debugInfo2.DetailList);
            foreach (var dic in dicBaseCols)
            {
                if (orderColName.Length == 0)
                {
                    orderColName = ((ColumnMeta) dic.Value).ColSysName;
                    continue;
                }
                if (parentIDName.Length == 0){
                    parentIDName = ((ColumnMeta)dic.Value).ColSysName;
                    continue;
                }
                if (parentIDAllName.Length == 0){
                    parentIDAllName = ((ColumnMeta)dic.Value).ColSysName;
                    continue;
                }
                break;
            }

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);
            #endregion

            debugInfo2 = new NatureDebugInfo { Title = "获取页面视图元数据，并且获取表名和主键字段名" };

            #region 获取页面视图元数据
            GetPageViewMeta(MasterPageViewID, debugInfo2.DetailList);

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);
            #endregion

            debugInfo2 = new NatureDebugInfo { Title = "获取下一个同级节点的排序" };
            #region 获取下一个同级节点的排序

            //                                                                     tableName   ParentIDAll =     
            const string sqlGetParentIDAllFormat = " select top 1 [DisOrder] from [{0}] where {1}  like '{2},{3}%' ORDER BY DisOrder DESC";
            string sqlGetParentIDAll = string.Format(sqlGetParentIDAllFormat,
                                                       PageViewMeta.DataSourceTableName , parentIDAllName ,parentIDAllValue,DataID );

            debugInfo2.Remark = sqlGetParentIDAll + "<br/>";

            nextOrderValue = Dal.DalCustomer.ExecuteString(sqlGetParentIDAll);
            err = Dal.DalCustomer.ErrorMessage;
            if (err.Length > 2)
                err = "访问数据库出错！";

            #endregion

            if (string.IsNullOrEmpty(nextOrderValue))
            {
                debugInfo2.Remark += orderValue + "没有子节点，序号 + 10" ;
                nextOrderValue = (int.Parse(orderValue) + 10).ToString(CultureInfo.InvariantCulture);
                
            }else
            {
                debugInfo2.Remark += nextOrderValue + "取到子节点最大序号 + 10" ;
                nextOrderValue = (int.Parse(nextOrderValue) + 10).ToString(CultureInfo.InvariantCulture);
            }

            json.Append("\"err\":\"" + err + "\",");
            json.Append("\"order\":" + nextOrderValue );

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);


        }
        #endregion

        #region 获取指定的一条记录

        /// <summary>
        /// 获取指定的一条记录
        /// </summary>
        /// <param name="json">The json.</param>
        /// user:jyk
        /// time:2012/10/27 10:05
        private void GetDataOne(StringBuilder json)
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Service.Data.GetData.GetDataOne]获取指定的一条记录" };
            BaseDebug.DetailList.Add(debugInfo);

            var debugInfo2 = new NatureDebugInfo { Title = "获取页面视图元数据" };

            #region 获取页面视图元数据（包括分页信息）

            GetPageViewMeta(MasterPageViewID, debugInfo2.DetailList);

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);
            #endregion

            #region 获取需要提取的字段ID和字段名称
            debugInfo2 = new NatureDebugInfo { Title = "获取表单需要提取的字段ID和字段名称" };

            //获取需要提取的字段ID和字段名称　　
            string sql = @"SELECT   pvc.ColumnID, col.ColSysName 
                            FROM    Manage_PageViewCol AS pvc INNER JOIN
                                    Manage_Columns AS col ON pvc.ColumnID = col.ColumnID
                            WHERE   (pvc.PVID = {0})
                            ORDER BY pvc.ColumnID";

            DataTable dt = Dal.DalMetadata.ExecuteFillDataTable(string.Format(sql, MasterPageViewID));
            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            #endregion

            #region 创建表单元数据管理的实例
            debugInfo2 = new NatureDebugInfo { Title = "创建表单元数据管理的实例，并且获取字段信息" };
            var managerMeta = new ManagerFormMeta
            {
                DalCollection = Dal,
                PageViewID = MasterPageViewID
            };
            Dictionary<int, IColumn> dicBaseCols = managerMeta.GetMetaData(debugInfo2.DetailList);
            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);
            #endregion

            #region 拼接字段名称
            debugInfo2 = new NatureDebugInfo { Title = "拼接字段名称 select *** " };
            string showCols = "";
            foreach (DataRow dr in dt.Rows)
            {
                if (((ColumnMeta)dicBaseCols[(int)dr[0]]).IsSave != 3)      //1：保存；2：不保存但是加载信息；3：不保存也不加载信息
                    showCols += "[" + dr[1] + "] as [" + dr[0] + "],";
            }
            showCols = showCols.TrimEnd(',');
            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);
         
            #endregion


            debugInfo2 = new NatureDebugInfo { Title = "拼接提取数据用的SQL，并且提取数据" };
      
            sql = Functions.IsInt(DataID)
                      ? "select top 1 {3} from {0} where {1} = {2}"
                      : "select top 1 {3} from {0} where {1} = '{2}'";

            string tmpSql = string.Format(sql, PageViewMeta.DataSourceTableName , PageViewMeta.PKColumn, DataID, showCols);
            string data = Dal.DalCustomer.ManagerJson.ExecuteFillJsonByColName(tmpSql);

            json.Append(data);

            debugInfo.Remark = tmpSql;
            debugInfo2.Remark = tmpSql;
            
            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);
          
            debugInfo.Stop();
       
        }

        #endregion

        #region 获取多条记录 分页

        /// <summary>
        /// 获取多条记录
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="dal">访问数据库的实例，可能是元数据，可能是客户数据库 </param>
        /// <param name="useColID">是否使用字段编号作为字段名称 </param>
        /// <param name="useKeyValue">是否使用key-value形式，字段编号作为key </param>
        /// <param name="isPager">true:分页提取数据；false：不分页提取全部数据</param>
        /// user:jyk
        /// time:2012/10/27 10:02
        private void GetDataListSource(StringBuilder json, DataAccessLibrary dal, bool useColID, bool useKeyValue,bool isPager)
        {
            var debugInfo = new NatureDebugInfo
                {
                    Title = "[Nature.Service.Data.GetData.GetDataListSource]分页获取数据",
                    Remark = "useColID:" + useColID + ",useKeyValue:" + useKeyValue
                };

            var debugInfo2 = new NatureDebugInfo {Title = "判断访问权限"};

            #region 判断是否有权限访问

            if (!MyUser.UserPermission.CanUseModuleID(ModuleID.ToString(CultureInfo.InvariantCulture)))
            {
                debugInfo2.Remark = "没有权限";
                Response.Write("\"err\":\"没有权限访问！\"");
                return;
            }
            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            #endregion

            debugInfo2 = new NatureDebugInfo {Title = "获取页面视图的元数据 d"};

            #region 获取页面视图元数据（包括分页信息）

            //页号
            string pageIndex = Request.QueryString["pageno"];
            if (!Functions.IsInt(pageIndex)) pageIndex = "1";

            //客户端传递过来的总记录数
            string lcRecordCount = Request.QueryString["pagerc"];
            if (!Functions.IsInt(lcRecordCount)) lcRecordCount = "0";

            //客户端传递过来的每页记录数
            string lcPageSizeStr = Request.QueryString["pagesize"];
            if (!Functions.IsInt(lcPageSizeStr)) lcPageSizeStr = "0";
            int lcPageSize = Convert.ToInt32(lcPageSizeStr);
            if (lcPageSize > 500) lcPageSize = 500;  //每页记录数不能超过500。

            GetPageViewMeta(MasterPageViewID, debugInfo2.DetailList);

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            #endregion

            BaseDebug.Title = "获取【" + PageViewMeta.Title + "】记录集";

            debugInfo2 = new NatureDebugInfo {Title = "获取字段ID和字段名称 d"};

            #region 获取需要提取的字段ID和字段名称

            string sql = @"SELECT   pvc.ColumnID, col.ColSysName ,pvc.Kind
                            FROM    Manage_PageViewCol AS pvc INNER JOIN
                                     Manage_Columns AS col ON pvc.ColumnID = col.ColumnID
                            WHERE   (pvc.PVID = {0})
                            ORDER BY pvc.ColumnID";

            DataTable dt = Dal.DalMetadata.ExecuteFillDataTable(string.Format(sql, MasterPageViewID));
            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            #endregion

            string query = "";

            debugInfo2 = new NatureDebugInfo {Title = "获取并设置查询条件 d"};

            #region 获取并设置查询条件

            bool hasKey = false;
            string findKeyTableName = null; //查询条件里的，使用这个表名来查询

            string key = Request["hasKey"];
            if (!string.IsNullOrEmpty(key))
                hasKey = (key == "1");

            if (hasKey)
            {
                //获取查询的字段的字典
                var managerMeta = new ManagerFindMeta()
                    {
                        DalCollection = Dal,
                        PageViewID = FindPageViewID
                    };
                Dictionary<int, IColumn> dicBaseCols = managerMeta.GetMetaData(debugInfo2.DetailList);

                if (dicBaseCols != null)
                {
                    //接收数据，装入字典
                    var dicColumnsValue = new Dictionary<int, object>();
                    foreach (KeyValuePair<int, IColumn> a in dicBaseCols)
                    {
                        var column = (ColumnMeta) a.Value;
                        if (string.IsNullOrEmpty(Request.QueryString["c" + column.ColumnID]))
                            dicColumnsValue[column.ColumnID] = "";
                        else
                            dicColumnsValue[column.ColumnID] =
                                Context.Server.UrlDecode(Request.QueryString["c" + column.ColumnID]);

                        //数据验证
                        debugInfo2.Remark += column.ColName + ":" + dicColumnsValue[column.ColumnID] + "<br/>";
                        string errMsg = CheckData(column, dicColumnsValue);
                        if (errMsg.Length == 0)
                        {
                            //没问题
                        }
                        else
                        {
                            //数据不合格
                            debugInfo2.Remark += column.ColName + ":" + dicColumnsValue[column.ColumnID] + "<br/>";
                            dicColumnsValue[column.ColumnID] = "";
                        }

                    }

                    var sbQuery = new StringBuilder(1000);

                    ManagerFind.SetQuery(dicBaseCols, dicColumnsValue, sbQuery, Dal.DalCustomer, out findKeyTableName);

                    query = sbQuery.ToString();
                }
            }

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            #endregion

            debugInfo2 = new NatureDebugInfo {Title = "获取可以访问的字段 d"};

            #region 获取可以访问的字段

            sql =
                @"SELECT TOP 1 ColumnIDs FROM Role_RoleColumn WHERE RoleID in ({0}) AND ModuleID = {1}  AND PVID = {2}";

            //当前用户可以访问的列ID集合
            string canUseColIDs =
                Dal.DalRole.ExecuteString(string.Format(sql, MyUser.UserPermission.RoleIDs, ModuleID, MasterPageViewID));
            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            #endregion

            debugInfo2 = new NatureDebugInfo {Title = "把字段名换成字段编号 c"};

            #region 获取字段集合,设置分页用的信息，表名、排序字段等

            string showCols = "[" + PageViewMeta.PKColumn + "] as [_id] ";
            if (useColID) //把字段名变成字段ID
                foreach (DataRow dr in dt.Rows)
                {
                    bool isRead = false;

                    //加上权限判断，有权限的才读取
                    if (MyUser.BaseUser.UserID == "1")
                        isRead = true;
                    else
                        //没有限制或者允许访问
                        if (string.IsNullOrEmpty(canUseColIDs) || canUseColIDs.Contains(dr[0].ToString()))
                            isRead = true;

                    if (isRead)
                        //判断是否需要显示，
                        if (dr[2].ToString() == "1")
                            showCols += ",[" + dr[1] + "] as [" + dr[0] + "]";
                }
            else //直接使用字段名
                foreach (DataRow dr in dt.Rows)
                {
                    bool isRead = false;
                    //加上权限判断，有权限的才读取
                    if (MyUser.BaseUser.UserID == "1")
                        isRead = true;
                    else if (string.IsNullOrEmpty(canUseColIDs) || canUseColIDs.Contains(dr[0].ToString()))
                        isRead = true;

                    if (isRead)
                        //判断是否需要显示，
                        if (dr[2].ToString() == "1") showCols += ",[" + dr[1] + "]";
                }

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            #endregion

            debugInfo2 = new NatureDebugInfo {Title = "设置分页工厂的属性 f"};

            #region 设置分页工厂的属性

            var pageTurn = PageViewMeta.PageTurnMeta;

            //设置分页用的信息，表名、排序字段等
            var pagerSQL = new PagerSQL
                {
                    TableName = pageTurn.TableNameList, //表名或者视图名
                    TableShowColumns = showCols, //要显示的字段
                    TablePKColumn = pageTurn.PKColumn, //主键
                    TableOrderByColumns = pageTurn.OrderColumns, //排序字段
                    TableQuery = pageTurn.Query, //查询条件
                    TableQueryAlways = pageTurn.QueryAlways, //固定查询条件
                    PageSize = pageTurn.PageSize, //一页的记录数
                    SetPagerSQLKind = (PagerSQLKind) pageTurn.PageTurnType //分页算法
                };

            //判断客户端是否传递每页记录数  20140315 新增 by jyk
            if (lcPageSize != 0)
            {
                pagerSQL.PageSize = lcPageSize;
            }

            if (findKeyTableName != null)
                pagerSQL.TableName = findKeyTableName;

            if (PageViewMeta.ForeignColumn != "")
            {
                if (!string.IsNullOrEmpty(ForeignID) && ForeignID != "-2")
                {
                    string tmpForeignId = ForeignID;
                    if (Functions.IsGuid(tmpForeignId)) tmpForeignId = "'" + tmpForeignId + "'";

                    if (pagerSQL.TableQueryAlways.Length == 0)
                        pagerSQL.TableQueryAlways = PageViewMeta.ForeignColumn + "=" + tmpForeignId;
                    else
                        pagerSQL.TableQueryAlways += " and " + PageViewMeta.ForeignColumn + "=" + tmpForeignId;

                }
            }

            //设置用户输入的查询条件
            pagerSQL.TableQuery = query;

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            #endregion

            debugInfo2 = new NatureDebugInfo {Title = "获取角色规定的查询条件d"};

            #region 获取角色规定的查询条件。判断用户角色是否有记录的过滤方案

            string tmpQuery = MyUser.UserPermission.GetResourceListCastSQL(MasterPageViewID, Dal.DalMetadata);
            if (tmpQuery.Length > 0)
            {
                tmpQuery = tmpQuery.Replace("{userid}", MyUser.BaseUser.UserID);
                tmpQuery = tmpQuery.Replace("{personid}", MyUser.BaseUser.PersonID);
                tmpQuery = tmpQuery.Replace("{deptid}", MyUser.BaseUser.DepartmentID[0]);

                //有过滤方案，即查询语句。添加到分页控件的固定查询条件里。)
                if (pagerSQL.TableQueryAlways.Length == 0)
                    pagerSQL.TableQueryAlways = tmpQuery;
                else
                    pagerSQL.TableQueryAlways += " and " + tmpQuery;
            }

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            #endregion

            debugInfo2 = new NatureDebugInfo {Title = "拼接分页SQL f"};

            //判断是否分页 
            if (!isPager)
            {
                pagerSQL.PageSize = 999999;
            }

            #region 拼接SQL；

            pagerSQL.CreateSQL();

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);

            #endregion

            if (isPager)
            {
                //分页提取数据
                debugInfo2 = new NatureDebugInfo {Title = "统计总记录数 d"};

                #region 计算总记录数
                //如果客户端传递过来，那么直接用客户端的，如果没有，那么统计  20140315 增加 by jyk
                string recordCount = lcRecordCount == "0" ? dal.ExecuteString(pagerSQL.GetRecordCountSQL) : lcRecordCount;

                debugInfo2.Stop();
                debugInfo.DetailList.Add(debugInfo2);

                #endregion

                debugInfo2 = new NatureDebugInfo {Title = "获取记录 d"};

                #region 设置总记录数 获取数据

                //设置总记录数
                pagerSQL.RecordCount = Functions.IsInt(recordCount) ? int.Parse(recordCount) : 0;
                //计算页数
                pagerSQL.ComputePageCount();

                //提取指定页数据的 SQL
                debugInfo2.Remark = sql = pagerSQL.GetSQLByPageIndex(int.Parse(pageIndex));

                //返回记录集的json
                string data = useKeyValue
                                  ? dal.ManagerJson.ExecuteFillJsonByColNameKey(string.Format(sql, ModuleID))
                                  : dal.ManagerJson.ExecuteFillJsonByColName(string.Format(sql, ModuleID));


                #endregion

                #region 分页信息

                json.Append("\"pageTurn\":{");
                json.Append("\"pageSize\":");
                json.Append(pageTurn.PageSize);
                json.Append(",\"naviCount\":");
                json.Append(pageTurn.NaviCount);
                json.Append(",\"recordCount\":");
                json.Append(pagerSQL.RecordCount);
                json.Append(",\"pageCount\":");
                json.Append(pagerSQL.PageCount);
                json.Append(",\"pageIndex\":");
                json.Append(pageIndex);
                json.Append("},");

                #endregion

                if (data == null)
                    json[json.Length - 1] = ' ';
                else
                    //数据
                    json.Append(data);

                debugInfo2.Stop();
                debugInfo.DetailList.Add(debugInfo2);

            }
            else
            {
                //不分页，提取全部数据    
                debugInfo2 = new NatureDebugInfo {Title = "获取记录 d"};

                //提取指定页数据的 SQL
                debugInfo2.Remark = sql = pagerSQL.GetSQLByPageIndex(1);

                //返回记录集的json
                string data = useKeyValue
                                  ? dal.ManagerJson.ExecuteFillJsonByColNameKey(string.Format(sql, ModuleID))
                                  : dal.ManagerJson.ExecuteFillJsonByColName(string.Format(sql, ModuleID));

                if (data == null )
                    if (json.Length >1)
                        json[json.Length - 1] = '}';
                    else
                        json.Append("\"a\":1");
                else
                    //数据
                    json.Append(data);

                debugInfo2.Stop();
                debugInfo.DetailList.Add(debugInfo2);
            }

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);

        }

        #endregion

        #region 验证数据类型
        /// <summary>
        /// 验证提交过来的数据，进行类型验证
        /// </summary>
        /// <param name="column"></param>
        /// <param name="dicColumnsValue"></param>
        /// <returns></returns>
        private string  CheckData(ColumnMeta column, IDictionary<int, object> dicColumnsValue)
        {
            //查询关键字
            string key = dicColumnsValue[column.ColumnID].ToString();

            string[] arrKey = key.Split('`');     //如果是范围查询，那么会输入两个查询关键字，用 `连接，所以需要先拆分一下

            foreach (string tmpKey in arrKey)
            {
                if (tmpKey.Length == 0)
                {
                    //查询，没有值，不做验证
                    continue;
                }

                if (tmpKey == "-99999")
                {
                    //查询，下拉列表框，不做查询条件的标记
                    continue;
                }

                #region 根据字段类型，判断参数是否合格
                switch (column.ColType)
                {
                    case "bigint":
                        #region 验证 bigint

                        long tmplong;
                        if (!Int64.TryParse(tmpKey, out tmplong))
                            return InputError(column, tmpKey);

                        #endregion
                        break;

                    case "tinyint":
                        #region 验证 tinyint

                        Int16 tmpInt16;
                        if (!Int16.TryParse(tmpKey, out tmpInt16))
                            return InputError(column, tmpKey);

                        if (tmpInt16 < 0 || tmpInt16 > 255)
                            return InputError(column, tmpKey);

                        #endregion
                        break;

                    case "smallint":
                        #region 验证 smallint
                        //Int16 tmpInt16;
                        if (!Int16.TryParse(tmpKey, out tmpInt16))
                            return InputError(column, tmpKey);

                        #endregion
                        break;

                    case "int":
                        #region 验证 int
                        string[] tmpArrValue = tmpKey.Split('`');
                        foreach (string a in tmpArrValue)
                        {
                            if (!Functions.IsIDString(a))
                                return InputError(column, a);

                            //Int32 tmpInt32;
                            //if (!Int32.TryParse(a, out tmpInt32))
                            //    return InputError(column, a);
                        }
                        #endregion
                        break;

                    case "numeric":
                    case "smallmoney":
                    case "money":
                    case "decimal":
                        #region 验证 decimal

                        decimal tmpdecimal;
                        if (!decimal.TryParse(tmpKey, out tmpdecimal))
                            return InputError(column, tmpKey);

                        #endregion
                        break;

                    case "real":
                    case "float":
                        #region 验证 float

                        float tmpfloat;
                        if (!float.TryParse(tmpKey, out tmpfloat))
                            return InputError(column, tmpKey);

                        #endregion
                        break;

                    case "uniqueidentifier":
                    case "char":
                    case "nchar":
                    case "varchar":
                    case "nvarchar":
                    case "text":
                    case "ntext":
                        dicColumnsValue[column.ColumnID] = tmpKey.Replace("'", "''");
                        break;

                    case "smalldatetime":
                    case "datetime":
                        #region 验证时间
                        DateTime tmpDateTime;
                        if (!DateTime.TryParse(tmpKey, out tmpDateTime))
                            return InputError(column, tmpKey);
                                
                        #endregion
                        break;

                    case "bit":
                        #region 验证bool
                        switch (tmpKey.ToLower())
                        {
                            case "0":
                            case "false":
                                dicColumnsValue[column.ColumnID] = "0";
                                break;

                            case "1":
                            case "true":
                                dicColumnsValue[column.ColumnID] = "1";
                                break;

                            default:
                                dicColumnsValue[column.ColumnID] = "0";
                                break;

                        }
                        #endregion
                        break;

                }
                #endregion
            }

            return "";
        }

        #endregion

        private static string InputError(ColumnMeta bInfo, string tmpDataValue)
        {
            //坚持是否是查询控件

            //查询，判断是否未填，未填写不判断
            if (tmpDataValue.Length == 0)
                return "";


            //信息格式不正确，即和数据库的字段的格式不符合。
            string msg = "【" + bInfo.ColName + "】的格式不正确，请重新填写！" + tmpDataValue;

            //Functions.MsgBox(msg + tmpDataValue, false);

            return msg;

        }
    }

}