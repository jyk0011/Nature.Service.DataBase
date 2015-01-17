using System;
using System.Collections.Generic;
using System.Text;
using Nature.Common;
using Nature.DebugWatch;
using Nature.MetaData.Entity;
using Nature.MetaData.Entity.MetaControl;
using Nature.MetaData.Enum;
using Nature.MetaData.Manager;
using Nature.MetaData.ManagerMeta;

namespace Nature.Service.Data
{
    /// <summary>
    /// 服务之保存数据
    /// </summary>
    public class PostData : BaseAshxCrud
    {
        public override void Process()
        {
            base.Process();
            //强制不缓存
            Response.Cache.SetNoStore();
           
            string re = "";
            switch (Request.QueryString["action"])
            {
                case "savedata":       //保存数据
                    BaseDebug.Title = "保存数据";
                    re = SaveData();
                    break;
          
                case "getdebug":
                    BaseDebug.Title = "获取保存数据参数的debug信息";
                    re = GetDebug();
                    break;

                case "disorder+":
                    BaseDebug.Title = "排序 + 10";
                    re = LevelAdd();
                    break;
            }

            Response.Write(re);
        }

        #region 获取debug信息
        private string GetDebug()
        {
            Response.Write("\"err1\":\"dengdai\""); 

            string guid = Request["guid"];
            if (string.IsNullOrEmpty(guid))
            {
                //没有传递参数
                return "";
            }

            if (Context.Cache[guid] != null)
            {
                Response.Write(",");
                Response.Write(Context.Cache[guid]); 
            }
            else
            {
                Response.Write(",\"err\":\"nothavedebug\""); 
            }
            return "";
        }
        #endregion

        #region 排序 ++ 
        private string LevelAdd()
        {
            string orderColName = "";   //排序字段的名称
            string orderValue = "";     //排序字段的值
            string err = "";

            orderValue = Request["order"];
            if (!Functions.IsInt(orderValue))
            {
                //排序字段值不正确，不能排序
                return "";
            }

            var debugInfo = new NatureDebugInfo { Title = "只修改排序，不修改其他字段" };

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
                orderColName = ((ColumnMeta) dic.Value).ColSysName;
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

            debugInfo2 = new NatureDebugInfo { Title = "判断有没有重复的排序值" };
            #region 修改后面的排序

            //                                                           tableName    orderName <> order  id =     
            const string sqlGetDisOrderValueFormat = " select top 1 0 from [{0}] where {3} <> {4} and {1} = {2}";
            string sqlGetDisOrderValue = string.Format(sqlGetDisOrderValueFormat,
                                                       PageViewMeta.ModiflyTableName, orderColName, orderValue,PageViewMeta.PKColumn, DataID);

            bool hasDiffValue = Dal.DalCustomer.ExecuteExists(sqlGetDisOrderValue);
            err = Dal.DalCustomer.ErrorMessage;

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);
            #endregion

            if (hasDiffValue)
            {
                debugInfo2 = new NatureDebugInfo {Title = "修改后面的排序"};
                #region 修改后面的排序
                const string sqlUpdateDisorderAfterFormat = " update [{0}] set [{1}] = [{1}] + 10 where [{1}] >= {2}";
                string sqlUpdateDisorderAfter = string.Format(sqlUpdateDisorderAfterFormat,
                                                              PageViewMeta.ModiflyTableName, orderColName, orderValue);
                Dal.DalCustomer.ExecuteNonQuery(sqlUpdateDisorderAfter);
                err += Dal.DalCustomer.ErrorMessage;

                debugInfo2.Stop();
                debugInfo.DetailList.Add(debugInfo2);

                #endregion
            }

            /*
            debugInfo2 = new NatureDebugInfo { Title = "修改自己的排序" };
            #region 修改自己的排序
            const string sqlUpdateDisorderFormat = " update {0} set {1} = {2} where {3} = {4}";
            string sqlUpdateDisorder = string.Format(sqlUpdateDisorderFormat, PageViewMeta.ModiflyTableName, orderColName, orderValue,PageViewMeta.PKColumn ,DataID );

            Dal.DalCustomer.ExecuteNonQuery(sqlUpdateDisorder);
            err += Dal.DalCustomer.ErrorMessage;

            debugInfo2.Stop();
            debugInfo.DetailList.Add(debugInfo2);
            #endregion
            */

            string re ="\"err\":\"" + err + "\"";

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);

            return re;
        }
        #endregion


        #region 保存数据
        private string SaveData()
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Service.Data.PostData.SaveData] 保存数据" };
            BaseDebug.DetailList.Add(debugInfo);

            var debugInfo2 = new NatureDebugInfo { Title = "定义操作日志 ManagerLogOperate、ManagerLogDataChange" };
            debugInfo.DetailList.Add(debugInfo2);
          
            #region 定义操作日志
            var  operateLog = new ManagerLogOperate
                                  {
                                      AddUserID = Int32.Parse(MyUser.BaseUser.UserID),
                                      Dal = Dal.DalCustomer,
                                      ModuleID = ModuleID,
                                      ButtonID = ButtonID,
                                      PageViewID = MasterPageViewID
                                  };

            //定义数据变更日志
            var dataChangeLog = new ManagerLogDataChange
            {
                AddUserID = Int32.Parse(MyUser.BaseUser.UserID),
                Dal = Dal 
            };

            debugInfo2.Stop();
            #endregion

            //主键字段值
            string idValue = "";
 
            debugInfo2 = new NatureDebugInfo { Title = "创建表单元数据管理的实例，获取表单的元数据" };
            debugInfo.DetailList.Add(debugInfo2);
            
            #region 创建表单元数据管理的实例
            var managerMeta = new ManagerFormMeta
            {
                DalCollection = Dal,
                PageViewID = MasterPageViewID
            };
            Dictionary<int, IColumn> dicBaseCols = managerMeta.GetMetaData(debugInfo2.DetailList);

            debugInfo2.Stop();
            #endregion

            debugInfo2 = new NatureDebugInfo { Title = "接收数据，装入字典，并且验证数据" };
            debugInfo.DetailList.Add(debugInfo2);

            bool isErr = false;

            #region 接收数据，装入字典
            var dicColumnsValue = new Dictionary<int, object>();

            foreach (KeyValuePair<int, IColumn> a in dicBaseCols)
            {
                var column = (ColumnMeta)a.Value;

                string tmpColId = Request["c" + column.ColumnID];
                dicColumnsValue[column.ColumnID] = string.IsNullOrEmpty(tmpColId)
                                                       ? ""
                                                       : Context.Server.UrlDecode(tmpColId);

                if (column.IsSave != 1) continue;


                var sb = new StringBuilder(200);

                if (column.ColumnKind == 21)
                {
                    //获取登录人的ID
                    dicColumnsValue[column.ColumnID] = MyUser.BaseUser.UserID;
                    sb.Append("登录人的ID ");
                    sb.Append(column.ColumnID);
                    sb.Append(":\"");
                    sb.Append(MyUser.BaseUser.UserID);
                    sb.Append("\"");
                    continue;
                }
                
                if  (column.ColumnKind ==22){
                        //获取登录人的所在部门
                        dicColumnsValue[column.ColumnID] = MyUser.BaseUser.DepartmentID[0];
                        sb.Append("所在部门的ID ");
                        sb.Append(column.ColumnID);
                        sb.Append(":\"");
                        sb.Append(MyUser.BaseUser.DepartmentID[0]);
                        sb.Append("\"");
                        continue;
                }

                sb.Append("验证数据 ");
                sb.Append(column.ColumnID);
                sb.Append(":\\");
                Json.StringToJson(dicColumnsValue[column.ColumnID].ToString(),sb);
                sb[sb.Length - 1] = '\\';
                sb.Append("\"");
                
                //数据验证
                var debugInfo3 = new NatureDebugInfo { Title = sb.ToString()};
                debugInfo2.DetailList.Add(debugInfo3);
                string errMsg = CheckData(column, dicColumnsValue);
                if (errMsg.Length != 0)
                {
                    operateLog.State = 3;
                    //数据不合格
                    ResourceUrlMessage = errMsg + '`' + column.ColumnID;
                    BaseDebug.ErrorMessage += errMsg + "<br/>";
                    dicColumnsValue[column.ColumnID] = "";
                    isErr = true;

                    debugInfo3.Title += ResourceUrlMessage;
                }
                debugInfo3.Stop();
                
            }
            debugInfo2.Stop();
            
            #endregion
             
            if (isErr)
            {
                debugInfo2 = new NatureDebugInfo { Title = "保存操作记录，数据验证未通过" };
                debugInfo.DetailList.Add(debugInfo2);
                #region 保存操作记录
                operateLog.State = 3;
                operateLog.WriteOperateLog(debugInfo2.DetailList);
                debugInfo2.Stop();
                #endregion

                //数据输入错误，不能继续
                return "\"err\":\"数据输入错误\"";
            }

            debugInfo2 = new NatureDebugInfo { Title = "获取页面视图元数据" };
            debugInfo.DetailList.Add(debugInfo2);

            #region 获取页面视图元数据
            GetPageViewMeta(MasterPageViewID, debugInfo2.DetailList);
            debugInfo2.Stop();
            #endregion

            dataChangeLog.PageViewMeta = PageViewMeta;
            dataChangeLog.DataID = DataID;

            debugInfo2 = new NatureDebugInfo { Title = "数据管理类实例化 ManagerData" };
            debugInfo.DetailList.Add(debugInfo2);
            #region 数据管理类实例化
            var managerData = new ManagerData
            {
                Dal = Dal.DalCustomer,
                DictFormColumnMeta = dicBaseCols,
                PageViewMeta = PageViewMeta,
                DataID = DataID,
                ManagerLogDataChange = dataChangeLog
            };

            debugInfo2.Stop();
            #endregion

            //看看有没有提交主键字段值
            if (string.IsNullOrEmpty(Request["c" + PageViewMeta.PKColumnID]) == false)
            {
                //提交了主键字段值
                idValue = Request["c" + PageViewMeta.PKColumnID];

            }

            debugInfo2 = new NatureDebugInfo { Title = "获取按钮类型，判断是添加还是修改" + ButtonID };
            debugInfo.DetailList.Add(debugInfo2);
            debugInfo2.Remark = "";
            #region 获取按钮类型
            const string sql = @"SELECT  BtnTypeID FROM Manage_ButtonBar WHERE (ButtonID = {0})";

            var butonType = (ButonType)Dal.DalMetadata.ExecuteScalar<int>(string.Format(sql, ButtonID));

            if (butonType == ButonType.AddData )
            {
                debugInfo2.Remark += "添加状态<br>";
                operateLog.OperateKind = 51;
                if (idValue.Length > 0)
                    dataChangeLog.DataID = idValue;

                //判断是否有 添加人 字段，有的话直接把当前登录人加上去
                if (dicColumnsValue.ContainsKey(1000150))
                {
                    //有添加人字段
                    dicColumnsValue[1000150] = MyUser.BaseUser.UserID;
                    debugInfo2.Remark += "有添加人字段1000150 " + MyUser.BaseUser.UserID + "<br>";
                
                }

                else if (dicColumnsValue.ContainsKey(5300120))
                {
                    //有添加人字段
                    dicColumnsValue[5300120] = MyUser.BaseUser.UserID;
                    debugInfo2.Remark += "有添加人字段5300120 " + MyUser.BaseUser.UserID + "<br>";
                }

               

            }
            else
            {
                debugInfo2.Remark += "修改状态<br>";
            
                operateLog.OperateKind = 52;

                //不是添加，登录人不被保存

                if (dicBaseCols.ContainsKey(1000150))
                {
                    debugInfo2.Remark += "不是添加，登录人不被保存<br>";
                    ((ColumnMeta) dicBaseCols[1000150]).IsSave = 2;
                }

                //保存最后修改人
                if (dicBaseCols.ContainsKey(1000190))
                {
                    //有修改人字段
                    dicColumnsValue[1000190] = MyUser.BaseUser.UserID;
                    debugInfo2.Remark += "有修改人字段1000190 " + MyUser.BaseUser.UserID + "<br>";
                }

            }
            debugInfo2.Stop();
            #endregion

            #region 如果是密码字段，按照md5处理

            foreach (var dic in dicBaseCols)
            {
                var column = ((FormColumnMeta )dic.Value);
                if (column.ControlKind == ControlType.PasswordTextBox)
                {
                    //密码框，md5处理
                    dicColumnsValue[column.ColumnID] = Functions.ToMD5(dicColumnsValue[column.ColumnID].ToString());
                }
                
            }

            #endregion

            debugInfo2 = new NatureDebugInfo { Title = "保存操作记录" };
            debugInfo.DetailList.Add(debugInfo2);
            #region 保存操作记录
            operateLog.WriteOperateLog(debugInfo2.DetailList);
            debugInfo2.Stop();

            #endregion

            dataChangeLog.OperateLogID = operateLog.OperateLogID;

            debugInfo2 = new NatureDebugInfo { Title = "保存到数据库" };
            debugInfo.DetailList.Add(debugInfo2);
            #region 保存到数据库
            managerData.DictFormColumnMeta = dicBaseCols;
            managerData.DictColumnsValue = dicColumnsValue;
            managerData.TypeOperationData = butonType;

            string err = managerData.SaveData(operateLog, debugInfo2.DetailList);
            debugInfo2.Stop();
            #endregion

            string re = "\"err\":\"" + err + "\",\"id\":\"" + managerData.DataID + "\"";

            ResourceUrlMessage = err;
            NewDataID = managerData.DataID;

            BaseDebug.ErrorMessage = err;
            debugInfo.Stop();
          
            
            return re;
        }
        #endregion

        #region 验证数据类型
        /// <summary>
        /// 验证提交过来的数据，进行类型验证
        /// </summary>
        /// <param name="column"></param>
        /// <param name="dicColumnsValue"></param>
        /// <returns></returns>
        private string CheckData(ColumnMeta column, IDictionary<int, object> dicColumnsValue)
        {
            string tmpDataValue = dicColumnsValue[column.ColumnID].ToString();
            switch (column.ColType)
            {
                case "bigint":
                    #region 验证 long
                    long tmplong;
                    if (!Int64.TryParse(tmpDataValue, out tmplong))
                        return InputError(column, tmpDataValue);
                    #endregion
                    break;

                case "tinyint":
                    #region 验证 tinyint
                    Int16 tmpInt16;
                    if (!Int16.TryParse(tmpDataValue, out tmpInt16))
                        return InputError(column, tmpDataValue);

                    if (tmpInt16 < 0 || tmpInt16 > 255)
                        return InputError(column, tmpDataValue);

                    #endregion

                    break;

                case "smallint":
                    #region 验证 smallint
                    //Int16 tmpInt16;
                    if (!Int16.TryParse(tmpDataValue, out tmpInt16))
                        return InputError(column, tmpDataValue);

                    #endregion

                    break;

                case "int":
                    #region 验证 int,

                    string[] tmpArrValue = tmpDataValue.Split('`');
                    foreach (string a in tmpArrValue)
                    {
                        Int32 tmpInt32;
                        if (!Int32.TryParse(a, out tmpInt32))
                            return InputError(column, a);
                    }

                    #endregion
                    break;

                case "ids":
                    #region 验证 ints，就是id集合，“1,2,3”的形式，变成nvarchar处理。

                    string[] tmpArrValue2 = tmpDataValue.Split('`');
                    foreach (string a in tmpArrValue2)
                    {
                        if (!Functions.IsIDString(a))
                            return InputError(column, a);
                    }

                    #endregion
                    break;

                case "numeric":
                case "smallmoney":
                case "money":
                case "decimal":
                    #region 验证 decimal
                    decimal tmpdecimal;
                    if (!decimal.TryParse(tmpDataValue, out tmpdecimal))
                        return InputError(column, tmpDataValue);

                    #endregion
                    break;

                case "real":
                case "float":
                    #region 验证 float
                    float tmpfloat;
                    if (!float.TryParse(tmpDataValue, out tmpfloat))
                        return InputError(column, tmpDataValue);

                    #endregion
                    break;

                case "char":
                case "varchar":
                case "nchar":
                case "nvarchar":
                    //判断长度
                    if (tmpDataValue.Length > column.ColSize)
                    {
                        return InputError(column, tmpDataValue);
                    }
                    dicColumnsValue[column.ColumnID] = tmpDataValue;
                    break;

                case "uniqueidentifier":
                case "text":
                case "ntext":
                    //dicColumnsValue[column.ColumnID] = tmpDataValue.Replace("'", "");
                    dicColumnsValue[column.ColumnID] = tmpDataValue;
                    break;

                case "smalldatetime":
                case "datetime":
                    if (tmpDataValue.Length == 0)
                    {
                        //没有值，不做验证。设置为 1900-1-1 表示为 null
                        dicColumnsValue[column.ColumnID] = "1900-01-01";
                    }
                    else
                    {
                        //有值
                        DateTime tmpDateTime;
                        tmpArrValue = tmpDataValue.Split('`');
                        foreach (string a in tmpArrValue)
                        {
                            if (a.Length > 0)
                            {
                                if (!DateTime.TryParse(a, out tmpDateTime))
                                    return InputError(column, a);
                            }
                        }
                    }
                    break;

                case "bit":
                    switch (tmpDataValue.ToLower())
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
                    break;

            }

            return "";
        }

        #endregion

        private static string InputError(ColumnMeta bInfo, string tmpDataValue)
        {
            //只添加修改，没有查询的情况，所以不判断是不是查询了。

            //查询，判断是否未填，未填写不判断
            //if (tmpDataValue.Length == 0)
            //    return "";


            //信息格式不正确，即和数据库的字段的格式不符合。
            string msg = "【" + bInfo.ColumnID + "-" + bInfo.ColName + "】的格式不正确，请重新填写！[" + tmpDataValue + "]";

            //Functions.MsgBox(msg + tmpDataValue, false);

            return msg;

        }
    }
}