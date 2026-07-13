
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Metadata.BusinessService;
using Kingdee.BOS.Core.Metadata.Util;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.App.Stock.Report.ClassStockDetail;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace BD.Standard.KY.ReportServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("库存账龄报表二开报表服务插件")]
    public class ReportPlugInTwo : SysReportBaseService
    {
        //当天入库的账龄按1天计算
        private int CurDateIsOne;
        //private string FIntervalTxt = "";
        private string FQueryDate = "";
        //显示标题
        //private string F_UJED_StockOrg = "";
        private string F_UJED_MaterialRange = "全部";
        private string F_UJED_StockRange = "全部";
        private string F_UJED_LotRange = "全部";
        //private string F_UJED_StockOrgWhere = "";
        private string F_UJED_MaterialWhere = "";
        private string F_UJED_StockWhere = "";
        private string F_UJED_LotWhere = "";
        private Boolean flag = true;
        private List<string> FMoBillno = new List<string>();

        private static readonly string Logpath = @"D:\KYLog\reportLog\" + DateTime.Now.ToString("yyyyMM");

        #region 设置报表
        public override void Initialize()
        {
            base.Initialize();
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            this.ReportProperty.ReportName = new LocaleValue(
                "库存账龄报表二开",
                base.Context.UserLocale.LCID
            );
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsGroupSummary = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.SimpleAllCols = false;
            //this.ReportProperty.IdentityFieldName = "FIDENTITYID";
            this.SetDecimalControl();
        }
        #endregion 设置报表

        #region 汇总
        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            result.Add(new SummaryField("Fbaseqty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("Fqty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("Famount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            for (int i = 0; i<Field.Length; i++)
            {

                if (Field[i].Contains("IntervalTxt"))
                {
                    result.Add(new SummaryField(Field[i], Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
                }

            }


            return result;
        }
        #endregion

        private void SetDecimalControl()
        {
            List<DecimalControlField> list = new List<DecimalControlField>();

            this.ReportProperty.DecimalControlFieldList = list;
         
        }

        private Dictionary<string, string> keyValues = null;

        string[] Field = { "FMATERIALID1", "FMATERIALID", "FMATERIALNUMBER", "FStockOrgId", "FMaterialTypeName", "FSTOCKID", "FSTOCKSTATUSNAME", "FStockLocId", "FSTOCKLOC", "FLOTNO", "FBASEUNITID", "FUnitID", "Fbilldatemin", "Fagedays", "Fbaseqty", "Fqty", "Famount" };
        string[] FieldType = { "numeric", "numeric", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "DECIMAL(23,10)", "DECIMAL(23,10)", "DECIMAL(23,10)" };


        #region 创建临时表
        public string CreateMainTempSql(string tmpTableName)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("/*dialect*/create table {0}", tmpTableName));
            builder.AppendLine(" ( ");

            for (int i = 0; i < Field.Length; i++)
            {
                if (i == Field.Length - 1)
                {
                    builder.AppendLine(Field[i] + " " + FieldType[i] + ")");
                }
                else
                {
                    builder.AppendLine(Field[i] + " " + FieldType[i] + ",");
                }
            }
            return builder.ToString();
        }
        #endregion 创建临时表

        #region  构建报表详细信息
        public void InventoryDetailSummarySql(string temp, IRptParams filter)
        {
            string midtable = " MiFei_GetReportDataTable ";
            Logger logger1 = new Logger(Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            
            StringBuilder where = new StringBuilder();
            where.Append(" where 1=1 ");
            if (!string.IsNullOrWhiteSpace(F_UJED_MaterialWhere))
            {
                where.AppendLine($" and FMATERIALNUMBER in {F_UJED_MaterialWhere}");
            }
            if (!string.IsNullOrWhiteSpace(F_UJED_StockWhere))
            {
                where.AppendLine($" and FStockNumber in {F_UJED_StockWhere}");
            }
            if (!string.IsNullOrWhiteSpace(F_UJED_LotWhere))
            {
                where.AppendLine($" and FLOTNO in {F_UJED_LotWhere}");
            }




        StringBuilder InsertBuilder1 = new StringBuilder();
            StringBuilder InsertBuilder2 = new StringBuilder();

            //FCurDateIsOne
            InsertBuilder1.Append("/*dialect*/");
            InsertBuilder1.AppendLine($"IF EXISTS (SELECT 1  FROM sys.columns WHERE object_id = OBJECT_ID('{midtable}')  AND name = 'FLOTNO')");
            InsertBuilder1.AppendLine($"BEGIN");
            InsertBuilder1.AppendLine($"UPDATE {midtable} SET FAGEDAYS =");
            //检查是否为有效的 yyyy-mm-dd 格式 
            InsertBuilder1.AppendLine($"CASE WHEN ISDATE(SUBSTRING(FLOTNO, 1, 10)) = 1 AND SUBSTRING(FLOTNO, 1, 10) LIKE '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'");
            InsertBuilder1.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, 1, 10), 120), GETDATE())+{CurDateIsOne})");
            //检查是否为有效的 yyyymmdd 格式 
            InsertBuilder1.AppendLine($"WHEN ISDATE(SUBSTRING(FLOTNO, 1, 8)) = 1  AND SUBSTRING(FLOTNO, 1, 8) LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
            InsertBuilder1.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, 1, 8), 112), GETDATE())+{CurDateIsOne})");
            //检查是否为有效的 yyyy-mm-dd 格式
            InsertBuilder1.AppendLine($"WHEN ISDATE(SUBSTRING(FLOTNO, LEN(FLOTNO) -9, 10)) = 1 AND SUBSTRING(FLOTNO, LEN(FLOTNO) -9, 10) LIKE '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'");
            InsertBuilder1  .AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, LEN(FLOTNO) -9, 10), 120), GETDATE())+{CurDateIsOne})");
            //检查是否为有效的 yyyymmdd 格式 
            InsertBuilder1.AppendLine($"WHEN ISDATE(SUBSTRING(FLOTNO, LEN(FLOTNO) -7, 8)) = 1 AND SUBSTRING(FLOTNO, LEN(FLOTNO) -7, 8) LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
            InsertBuilder1.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, LEN(FLOTNO) -7, 8), 112), GETDATE())+{CurDateIsOne})");
            InsertBuilder1.AppendLine($"ELSE FAGEDAYS");
            InsertBuilder1.AppendLine($"END");
            InsertBuilder1.AppendLine($"END");

            InsertBuilder1.AppendLine($"update t1 set t1.FOUTDATE=(SELECT MAX(t2.FOUTDATE) FROM MiFei_GetReportDataTable AS t2 WHERE t2.FMATERIALID = t1.FMATERIALID AND t2.FMATERIALNUMBER = t1.FMATERIALNUMBER AND t2.FMaterialTypeName = t1.FMaterialTypeName AND t2.FStockOrgId = t1.FStockOrgId AND t2.FSTOCKID = t1.FSTOCKID AND t2.FSTOCKSTATUSNAME = t1.FSTOCKSTATUSNAME AND t2.FStockLocId = t1.FStockLocId AND t2.FSTOCKLOC = t1.FSTOCKLOC AND t2.FLOTNO = t1.FLOTNO AND t2.FBASEUNITID = t1.FBASEUNITID AND t2.FUnitID = t1.FUnitID AND t2.fidentityid <> t1.fidentityid) FROM MiFei_GetReportDataTable AS t1 WHERE t1.FBILLTYPENAME LIKE '委外%' ");

            StringBuilder newcol = new StringBuilder();
            InsertBuilder2.AppendLine($" select FMATERIALID as FMATERIALID1 ,FMATERIALID,FMATERIALNUMBER,FMaterialTypeName,FStockOrgId,FSTOCKID,FStockNumber,FSTOCKSTATUSNAME,FStockLocId,FSTOCKLOC,FLOTNO,FBASEUNITID,FUnitID,FOUTDATE as Fbilldatemin,fagedays+{CurDateIsOne} AS fagedays,sum( CONVERT(decimal(18,6), fbaseqty)) Fbaseqty ,sum( CONVERT(decimal(18,6), fqty)) Fqty ,sum( CONVERT(decimal(18,6), famount)) Famount  from    MiFei_GetReportDataTable {where}  group by FMATERIALID,FMATERIALNUMBER,FMaterialTypeName,FStockOrgId,FSTOCKID,FStockNumber,FSTOCKSTATUSNAME,FStockLocId,FSTOCKLOC,FLOTNO,FBASEUNITID,FUnitID,FOUTDATE,fagedays  ");

            //动态列处理
            DynamicObject dyobject = filter.FilterParameter.CustomFilter;
            if (Convert.ToString(dyobject["FRadioGroup"]).Equals("A"))
            {
                StringBuilder InsertBuilder3 = new StringBuilder();
                InsertBuilder3.AppendLine(" select * ");


                DynamicObjectCollection InvAgeEntity = dyobject["InvAgeEntity"] as DynamicObjectCollection;
                int row = 1;
                foreach (DynamicObject dynamic in InvAgeEntity)
                {
                    string IntervalTxt = "IntervalTxt" + row++;


                    int DownDay = Convert.ToInt32(dynamic["DownDay"].ToString());
                    int UpperDay = Convert.ToInt32(dynamic["UpperDay"].ToString());
                    newcol.Append($",sum( CONVERT(decimal(18,6), {IntervalTxt}B)) as {IntervalTxt}B");
                    newcol.Append($",sum( CONVERT(decimal(18,6), {IntervalTxt}S)) as {IntervalTxt}S");
                    if (Convert.ToInt32(dynamic["IntervalDay"]) > 0)
                    {
                        InsertBuilder3.AppendLine($" ,case when  CONVERT(int, fagedays)>={DownDay} AND  CONVERT(int, fagedays)<={UpperDay} then CONVERT(decimal(18,6), fbaseqty) else 0.0 end {IntervalTxt}B");
                        InsertBuilder3.AppendLine($" ,case when  CONVERT(int, fagedays)>={DownDay} AND  CONVERT(int, fagedays)<={UpperDay} then CONVERT(decimal(18,6), fqty) else 0.0 end {IntervalTxt}S");
                    }
                    else
                    {
                        InsertBuilder3.AppendLine($" ,case when  CONVERT(int, fagedays)>={DownDay} then CONVERT(decimal(18,6), fbaseqty) else 0.0 end {IntervalTxt}B");
                        InsertBuilder3.AppendLine($" ,case when  CONVERT(int, fagedays)>={DownDay} then CONVERT(decimal(18,6), fqty) else 0.0 end {IntervalTxt}S");
                    }
                }
                InsertBuilder3.AppendLine($"  from  ({InsertBuilder2}) A" );
                InsertBuilder1.AppendLine(InsertBuilder3.ToString());



            }

            Logger logger = new Logger(Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            logger.WriteLog("执行sql：" + InsertBuilder1.ToString());

            StringBuilder InsertBuilder = new StringBuilder();
            InsertBuilder.AppendLine("/*dialect*/");

            //真正查询数据的sql
            DynamicObjectCollection dycon = DBUtils.ExecuteDynamicObject(
                this.Context,
                InsertBuilder1.ToString()
                );
            int count = 1;
            foreach (dynamic dyn in dycon)
            {
                InsertBuilder.AppendFormat("Insert into {0} ", temp);
                InsertBuilder.Append(" ( ");
                for (int i = 0; i < Field.Length; i++)
                {
                    if (i == Field.Length - 1)
                    {
                        InsertBuilder.Append(Field[i] + ")");
                    }
                    else
                    {
                        InsertBuilder.Append(Field[i] + ",");
                    }
                }//对应列名称
                InsertBuilder.Append(" values ");
                InsertBuilder.Append("(");
                for (int i = 0; i < Field.Length; i++)
                {
                   
                    var value = dyn[Field[i]];
                    if (i == 0)
                    {
                        InsertBuilder.Append("" + count++ + ",");
                    }
                    else if (i == Field.Length - 1)
                    {
                        InsertBuilder.Append("'" + value + "'");
                    }
                    else
                    {
                        InsertBuilder.Append("'" + value + "',");
                    }
                }
                InsertBuilder.AppendLine(")");
            }
            DBUtils.Execute(this.Context, InsertBuilder.ToString());
        }
        #endregion


        #region  构建报表核心
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            this.FilterParameter(filter); //给过滤条件赋值
            var orderBy = filter.FilterParameter.SortString;//排序字段
            if (orderBy.Length > 0)
            {
                base.KSQL_SEQ = string.Format(this.KSQL_SEQ, orderBy);

            }
            else
            {
                base.KSQL_SEQ = string.Format(this.KSQL_SEQ, "FMATERIALID desc");
            }
            using (new SessionScope())
            {
                //创建临时表-
                IDBService dbService =
                    Kingdee.BOS.App.ServiceHelper.GetService<Kingdee.BOS.Contracts.IDBService>();
                string[] arrTableName = null;
                string tmpTableName = String.Empty;
                arrTableName = dbService.CreateTemporaryTableName(this.Context, 1);
                tmpTableName = arrTableName[0];
                DBUtils.ExecuteDynamicObject(this.Context, this.CreateMainTempSql(tmpTableName));
                this.InventoryDetailSummarySql(tmpTableName, filter);

                List<Kingdee.BOS.Core.CommonFilter.ColumnField> columnInfo = filter.FilterParameter.ColumnInfo;
                StringBuilder selectField = new StringBuilder();

                selectField.Append("FMATERIALID,");

                Dictionary<string,  string> dictionary = new Dictionary<string, string>();
                dictionary.Add("FBASEUNIT", "FBASEUNITID");
                dictionary.Add("FUnit", "FUnitID");
                dictionary.Add("FStockOrgName", "FStockOrgId");
                dictionary.Add("FSTOCKNumber", "FSTOCKID");
                dictionary.Add("FSTOCKName", "FSTOCKID");
                //dictionary.Add("FSTOCKSTATUS", "FSTOCKSTATUSNAME");
                foreach (var item in columnInfo)
                {
                    if (dictionary.ContainsKey(item.FieldName) && !selectField.ToString().Contains(dictionary[item.FieldName]))
                    {
                        selectField.Append(dictionary[item.FieldName] + ",");   
                    }


                    if (Field.Contains(item.FieldName)&& !selectField.ToString().Contains(item.FieldName))
                    {
                        selectField.Append(item.FieldName + ",");
                    }
                }
                StringBuilder sql = new StringBuilder();
                sql.AppendLine(" SELECT  ");
                sql.AppendLine(selectField.ToString());
                sql.AppendLine(" sum(Fbaseqty) Fbaseqty,sum(Fqty) Fqty,sum(Famount) Famount");
                foreach (var keyValue in keyValues)
                {
                    sql.AppendLine(string.Format(" ,sum({0}) {0} ", keyValue.Key));
                }
                sql.AppendFormat(" ,{0} ", base.KSQL_SEQ);
                sql.AppendFormat(" INTO {0}  ", tableName);
                sql.AppendFormat(" FROM {0}  ", tmpTableName);
                sql.AppendFormat(" WHERE 1=1   ");
                if (!string.IsNullOrWhiteSpace(filter.FilterParameter.FilterString))
                {
                    sql.AppendFormat("  and {0}", filter.FilterParameter.FilterString);
                }
                sql.AppendFormat($"  group by {selectField.Remove(selectField.Length - 1, 1)} ");


                DBUtils.Execute(this.Context, sql.ToString());
                //删除表
                ITemporaryTableService service2 =
                    Kingdee.BOS.Contracts.ServiceFactory.GetService<Kingdee.BOS.Contracts.ITemporaryTableService>(
                        this.Context
                    );
                service2.DropTable(base.Context, new HashSet<string>(arrTableName));
            }
        }

        #endregion

        #region 设置列名
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();
            //header.AddChild("FIDENTITYID", new LocaleValue("序号")).ColIndex = 0;
            for (int i = 1; i < Field.Length; i++)
            {
                string value = Field[i];
                switch (Field[i])
                {
                    case "FMATERIALNUMBER":
                        value = "物料编码";
                        break;
                    case "FMaterialTypeName":
                        value = "存货类型";
                        break;
                    case "Fbilldatemin":
                        value = "最新出库日期";
                        break;
                    case "Fagedays":
                        value = "库龄天数";
                        break;
                    case "Fbaseqty":
                        value = "数量(基本)";
                        break;
                    case "Fqty":
                        value = "数量(库存)";
                        break;
                    case "Famount":
                        value = "总金额";
                        break;
                    case "FLOTNO":
                        value = "批号";
                        break;

                    case "FMATERIALID":
                        value = "物料内码";
                        break;
                    case "FStockOrgId":
                        value = "库存组织内码";
                        break;
                    case "FSTOCKID":
                        value = "仓库内码";
                        break;
                    case "FSTOCKSTATUSNAME":
                        value = "库存状态";
                        break;
                    case "FStockLocId":
                        value = "仓位内码";
                        continue;
                    case "FSTOCKLOC":
                        value = "仓位值集";
                        continue;    
                    case "FBASEUNITID":
                        value = "基本单位内码";
                        break;
                    case "FUnitID":
                        value = "库存单位内码";
                        break;

    
                }
                

                //设置列与类型，日期bos处理
                if ((value.Contains("数量") && !value.Contains("&"))|| value.Contains("总金额"))
                {
                    Kingdee.BOS.Core.List.ListHeader listHeader = header.AddChild(Field[i], new LocaleValue(value));
                    listHeader.ColIndex = i + 20;
                    listHeader.ColType = SqlStorageType.SqlNumeric;

                }
                else if (keyValues.TryGetValue(Field[i], out string value1))
                {
                    if (Field[i].EndsWith("B"))
                    {
                        value = value1 + "&数量(基本)";
                    }
                    else if (Field[i].EndsWith("S"))
                    {
                        value = value1 + "&数量(库存)";
                    }
                    Kingdee.BOS.Core.List.ListHeader listHeader = header.AddChild(Field[i], new LocaleValue(value));
                    listHeader.ColIndex = i + 40;
                    listHeader.ColType = SqlStorageType.SqlNumeric;
                }
                else
                {
                    header.AddChild(Field[i], new LocaleValue(value)).ColIndex = i + 20;
                }



            }
            return header;
        }
        #endregion 设置列名

        #region 过滤条件
        private void FilterParameter(IRptParams filter)
        {
            DynamicObject dyobject = filter.FilterParameter.CustomFilter;
            if (dyobject != null)
            {
                /*if (dyobject["StockOrgId"] != null)
                {
                    F_UJED_StockOrg = Convert.ToString(dyobject["StockOrgId"]);

                    string[] strings = F_UJED_StockOrg.Split(',');
                    StringBuilder sb = new StringBuilder();
                    sb.Append("(");
                    for (int i = 0; i < strings.Length; i++)
                    {
                        if (i == strings.Length - 1)
                        {
                            sb.Append("'" + strings[i] + "'");
                        }
                        else
                        {
                            sb.Append("'" + strings[i] + "',");
                        }

                    }
                    sb.Append(")");
                    F_UJED_StockOrgWhere = sb.ToString();




                }*/
                if (Convert.ToInt32(dyobject["MaterialFrom_Id"]) != 0)
                {
                    DynamicObject MaterialFrom = (DynamicObject)dyobject["MaterialFrom"];
                    DynamicObject MaterialTo = (DynamicObject)dyobject["MaterialTo"];
                    F_UJED_MaterialRange = MaterialFrom["Number"] + "(" + MaterialFrom["Name"] + ")--" + MaterialTo["Number"] + "(" + MaterialTo["Name"] + ")";

                    if (MaterialFrom["Number"].ToString().Equals(MaterialTo["Number"].ToString()))
                    {
                        F_UJED_MaterialWhere = "('" + MaterialFrom["Number"].ToString() + "')";
                    }
                    else
                    {
                        DynamicObjectCollection Materialdys = DBUtils.ExecuteDynamicObject(this.Context, $"SELECT Fnumber FROM  T_BD_MATERIAL where FNUMBER BETWEEN '{MaterialFrom["Number"].ToString()}' and  '{MaterialTo["Number"].ToString()}' ORDER  by FNUMBER");
                        StringBuilder sb = new StringBuilder();
                        sb.Append("(");
                        for (int i = 0; i < Materialdys.Count; i++)
                        {
                            if (i == Materialdys.Count - 1)
                            {
                                sb.Append("'" + Materialdys[i]["Fnumber"] + "'");
                            }
                            else
                            {
                                sb.Append("'" + Materialdys[i]["Fnumber"] + "',");
                            }
                        }
                        sb.Append(")");
                        F_UJED_MaterialWhere = sb.ToString();
                    }

                }
                if (Convert.ToInt32(dyobject["StockFrom_Id"]) != 0)
                {
                    DynamicObject StockFrom = (DynamicObject)dyobject["StockFrom"];
                    DynamicObject StockTo = (DynamicObject)dyobject["StockTo"];
                    F_UJED_StockRange = StockFrom["Number"] + "(" + StockFrom["Name"] + ")--" + StockTo["Number"] + "(" + StockTo["Name"] + ")";
                    if (StockFrom["Number"].ToString().Equals(StockTo["Number"].ToString()))
                    {
                        F_UJED_StockWhere = "('" + StockFrom["Number"].ToString() + "')";
                    }
                    else
                    {
                        DynamicObjectCollection Stockdys = DBUtils.ExecuteDynamicObject(this.Context, $"SELECT Fnumber FROM  T_BD_STOCK where FNUMBER BETWEEN '{StockFrom["Number"].ToString()}' and  '{StockTo["Number"].ToString()}' ORDER  by FNUMBER");
                        StringBuilder sb = new StringBuilder();
                        sb.Append("(");
                        for (int i = 0; i < Stockdys.Count; i++)
                        {
                            if (i == Stockdys.Count - 1)
                            {
                                sb.Append("'" + Stockdys[i]["Fnumber"] + "'");
                            }
                            else
                            {
                                sb.Append("'" + Stockdys[i]["Fnumber"] + "',");
                            }
                        }
                        sb.Append(")");
                        F_UJED_StockWhere = sb.ToString();
                    }
                }
                if (Convert.ToInt32(dyobject["LotFrom_Id"]) != 0)
                {
                    DynamicObject LotFrom = (DynamicObject)dyobject["LotFrom"];
                    DynamicObject LotTo = (DynamicObject)dyobject["LotTo"];
                    F_UJED_LotRange = LotFrom["Number"] + "(" + LotFrom["Name"] + ")--" + LotTo["Number"] + "(" + LotTo["Name"] + ")";

                    if (LotFrom["Number"].ToString().Equals(LotTo["Number"].ToString()))
                    {
                        F_UJED_LotWhere = "('" + LotFrom["Number"].ToString() + "')";
                    }
                    else
                    {
                        DynamicObjectCollection Lotdys = DBUtils.ExecuteDynamicObject(this.Context, $"SELECT Fnumber FROM  T_BD_LOTMASTER where FNUMBER BETWEEN '{LotFrom["Number"].ToString()}' and  '{LotTo["Number"].ToString()}' ORDER  by FNUMBER");
                        StringBuilder sb = new StringBuilder();
                        sb.Append("(");
                        for (int i = 0; i < Lotdys.Count; i++)
                        {
                            if (i == Lotdys.Count - 1)
                            {
                                sb.Append("'" + Lotdys[i]["Fnumber"] + "'");
                            }
                            else
                            {
                                sb.Append("'" + Lotdys[i]["Fnumber"] + "',");
                            }
                        }
                        sb.Append(")");
                        F_UJED_LotWhere = sb.ToString();
                    }


                }

                CurDateIsOne = Convert.ToBoolean(dyobject["CurDateIsOne"]) ? 1 : 0;
                Field = Field.Where(f => !f.Contains("IntervalTxt")).ToArray();
                if (Convert.ToString(dyobject["FRadioGroup"]).Equals("A"))
                {
                    DynamicObjectCollection InvAgeEntity = dyobject["InvAgeEntity"] as DynamicObjectCollection;
                    keyValues = new Dictionary<string, string>();
                    int row = 1;
                    foreach (DynamicObject dynamic in InvAgeEntity)
                    {
                        string IntervalTxt = "IntervalTxt" + row++;
                        string Interval = dynamic["IntervalTxt"].ToString();
                        keyValues.Add(IntervalTxt + "B", Interval);
                        keyValues.Add(IntervalTxt + "S", Interval);

                        //设置动态列数组
                        Array.Resize(ref Field, Field.Length + 2);
                        Array.Resize(ref FieldType, FieldType.Length + 2);
                        Field[Field.Length - 2] = IntervalTxt + "B";
                        Field[Field.Length - 1] = IntervalTxt + "S";
                        FieldType[FieldType.Length - 2] = "DECIMAL(23,10)";
                        FieldType[FieldType.Length - 1] = "DECIMAL(23,10)";


                    }
                }
                FQueryDate = dyobject["FQueryDate"].ToString();
            }
        }
        #endregion

        #region 过滤条件返回
        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            ReportTitles titles = new ReportTitles();
            //titles.AddTitle("F_UJED_StockOrg", F_UJED_StockOrg);
            titles.AddTitle("FMaterialRange", F_UJED_MaterialRange);
            titles.AddTitle("FStockRange", F_UJED_StockRange);
            titles.AddTitle("FLotRange", F_UJED_LotRange);

            return titles;
        }
        #endregion
    }
}
