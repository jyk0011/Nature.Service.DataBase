using System;
using System.Diagnostics;
using System.Text;
using System.Web;
using Nature.Common;
using Nature.Data;
using Nature.DebugWatch;
using Nature.MetaData.Manager;

namespace Nature.Service.Data
{
    /// <summary>
    /// 删除数据的服务
    /// </summary>
    public class DataDelete : BaseAshxCrud
    {
        /// <summary>
        /// 删除数据的服务
        /// </summary>
        public override void Process()
        {
            base.Process();

            //强制不缓存
            Response.Cache.SetNoStore();

            var json = new StringBuilder(3000);

            switch (base.Action )
            {
                case "physically": // 物理删除数据
                    BaseDebug.Title = "物理删除数据";
                    Physically(json, Dal);
                    break;

                case "logic": //逻辑删除
                    BaseDebug.Title = "逻辑删除数据";
                    Logic(json, Dal);
                    break;

                default: //没有
                    DefaultAction(json);
                    break;
            }

            Response.Write(json.ToString());

        }

        private void DefaultAction(StringBuilder json)
        {
            json.Append("\"err\":\"没有这个action\"");

        }

        #region 物理删除

        /// <summary>
        /// 物理删除
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="dal"> </param>
        /// user:jyk
        /// time:2012/10/27 10:05
        private void Physically(StringBuilder json, DalCollection dal)
        {
            var debugInfo = new NatureDebugInfo {Title = "判断是否有权限访问"};

            //定义操作日志
            var operateLog = new ManagerLogOperate
            {
                AddUserID = Int32.Parse(MyUser.BaseUser.UserID),
                Dal = Dal.DalCustomer,
                ModuleID = ModuleID,
                ButtonID = ButtonID,
                PageViewID = MasterPageViewID,
                OperateKind = 53
            };

            //定义数据变更日志
            var dataChangeLog = new ManagerLogDataChange
            {
                AddUserID = Int32.Parse(MyUser.BaseUser.UserID),
                Dal = Dal
            };


            #region 判断是否有权限访问
            if (MyUser.UserPermission.CheckCanUpdate(ModuleID, MasterPageViewID, DataID, dal.DalMetadata,debugInfo.DetailList).Length != 0)
            {
                #region 保存操作记录
                operateLog.State = 6;
                operateLog.WriteOperateLog(debugInfo.DetailList);
                #endregion

                Response.Write("{\"err\":\"没有权限访问！\"}");
                debugInfo.Stop();
                BaseDebug.DetailList.Add(debugInfo);
                return;
            }

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
            #endregion

            debugInfo = new NatureDebugInfo { Title = "获取页面视图元数据" };

            #region 获取页面视图元数据（包括分页信息）
            GetPageViewMeta(MasterPageViewID, debugInfo.DetailList);

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
            #endregion

            dataChangeLog.PageViewMeta = PageViewMeta;
            dataChangeLog.DataID = DataID;

            dataChangeLog.OperateLogID = DataID;
            //获取修改前记录
            dataChangeLog.OldDataJson = dataChangeLog.GetDataToJson(debugInfo.DetailList);

            debugInfo = new NatureDebugInfo {Title = "物理删除一条记录"};

            #region 物理删除一条记录
            string sql = "delete from {0} where {1} = {2}";
            sql = string.Format(sql, PageViewMeta.ModiflyTableName, PageViewMeta.PKColumn, DataID);

            dal.DalCustomer.ExecuteNonQuery(sql);
            debugInfo.Remark = sql;
            string msg = "\"msg\":\"\"";

            if (dal.DalCustomer.ErrorMessage.Length > 2)
            {
                #region 保存操作记录
                operateLog.State = 7;
                operateLog.WriteOperateLog(debugInfo.DetailList);
                #endregion

                msg = "\"msg\":\"删除记录的时候发生意外情况，请与管理员联系！\"";
                debugInfo.Remark += "<br/>" + dal.DalCustomer.ErrorMessage;
            }

            json.Append(msg);
            #endregion

            //获取删除后记录
            dataChangeLog.NewDataJson = "";
            //拼接提交的数据
            dataChangeLog.SubmitDataJson = "";

            //记录日志
            dataChangeLog.WriteDataChangeLog(debugInfo.DetailList);

            #region 保存操作记录
            operateLog.State = 1;
            operateLog.WriteOperateLog(debugInfo.DetailList);
            #endregion

            debugInfo.Stop();


            BaseDebug.DetailList.Add(debugInfo);
            

        }

        #endregion

        #region 逻辑删除

        /// <summary>
        /// 逻辑删除
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="dal">访问数据库的实例，可能是元数据，可能是客户数据库 </param>
        /// user:jyk
        /// time:2012/10/27 10:02
        private void Logic(StringBuilder json, DalCollection dal)
        {
            var debugInfo = new NatureDebugInfo { Title = "判断是否有权限访问" };

            //定义操作日志
            var operateLog = new ManagerLogOperate
            {
                AddUserID = Int32.Parse(MyUser.BaseUser.UserID),
                Dal = Dal.DalCustomer,
                ModuleID = ModuleID,
                ButtonID = ButtonID,
                PageViewID = MasterPageViewID,
                OperateKind = 54
            };

            //定义数据变更日志
            var dataChangeLog = new ManagerLogDataChange
            {
                AddUserID = Int32.Parse(MyUser.BaseUser.UserID),
                Dal = Dal
            };

            #region 判断是否有权限访问
            if (MyUser.UserPermission.CheckCanUpdate(ModuleID, MasterPageViewID, DataID, dal.DalMetadata,debugInfo.DetailList).Length != 0)
            {
                #region 保存操作记录
                operateLog.State = 6;
                operateLog.WriteOperateLog(debugInfo.DetailList);
                #endregion

                Response.Write("\"err\":\"没有权限访问！\"");
                debugInfo.Stop();
                BaseDebug.DetailList.Add(debugInfo);
                return;
            }
            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
            #endregion

            #region 获取页面视图元数据（包括分页信息）
            GetPageViewMeta(MasterPageViewID, debugInfo.DetailList);
            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
            #endregion

            dataChangeLog.PageViewMeta = PageViewMeta;
            dataChangeLog.DataID = DataID;

            //获取修改前记录
            dataChangeLog.OldDataJson = dataChangeLog.GetDataToJson(debugInfo.DetailList);


            debugInfo = new NatureDebugInfo {Title = "逻辑删除一条记录"};

            #region 逻辑删除一条记录
            string sql = "update {0} set isdel=1 where {1} = {2}";
            sql = string.Format(sql, PageViewMeta.ModiflyTableName, PageViewMeta.PKColumn, DataID);

            dal.DalCustomer.ExecuteNonQuery(sql);
            debugInfo.Remark = sql;
         
            string msg = "\"msg\":\"\"";

            if (dal.DalCustomer.ErrorMessage.Length > 2)
            {
                #region 保存操作记录
                operateLog.State = 7;
                operateLog.WriteOperateLog(debugInfo.DetailList);
                #endregion

                msg = "\"msg\":\"删除记录的时候发生意外情况，请与管理员联系！\"";
                debugInfo.Remark += "<br/>" + dal.DalCustomer.ErrorMessage;
            }

            json.Append(msg);
            #endregion

            #region 保存操作记录
            operateLog.State = 1;
            operateLog.WriteOperateLog(debugInfo.DetailList);
            #endregion

            dataChangeLog.OperateLogID = operateLog.OperateLogID; ;
           
            //获取逻辑删除后记录
            dataChangeLog.NewDataJson = dataChangeLog.GetDataToJson(debugInfo.DetailList);
            //拼接提交的数据
            dataChangeLog.SubmitDataJson = "";

            //记录日志
            dataChangeLog.WriteDataChangeLog(debugInfo.DetailList);


         

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
           

        }

        #endregion

    }
}