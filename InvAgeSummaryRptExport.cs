
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.SCM.App.Stock.Report;
using System;
using System.ComponentModel;
using System.Text;

namespace BD.Standard.MF.ReportServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("库龄分析汇总报表服务插件")]
    public class InvAgeSummaryRptExport : InvAgeSummary
    {
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);

            DynamicObject dyFilter = filter.FilterParameter.CustomFilter;
            string QueryDate = DateTime.Parse(dyFilter["QueryDate"].ToString()).ToString("yyyy-MM-dd");
            int day = Convert.ToBoolean(dyFilter["CurDateIsOne"]) ? 1 : 0;
            //stringBuilder2:报表临时表通过批号计算库龄生成子表，stringBuilder2判断子表的库龄，动态生成新的区间。
            //stringBuilder3:将临时表与stringBuilder2生成的区间表匹配后，动态替换区间数据
            StringBuilder stringBuilder2 = new StringBuilder();
            StringBuilder stringBuilder3 = new StringBuilder();
            DynamicObjectCollection dy = dyFilter["InvAgeEntity"] as DynamicObjectCollection;
            if (dy.Count > 0)
            {
                foreach (var item in dy)    
                {
                    int DownDay = Convert.ToInt32(item["DownDay"].ToString());
                    int UpperDay = Convert.ToInt32(item["UpperDay"].ToString());

                    stringBuilder2.AppendLine($" ,case when  CONVERT(int, fageday)>={DownDay} AND  CONVERT(int, fageday)<={UpperDay} then CONVERT(decimal(18,6), fbaseqty) else 0.0 end fbaseqty_{DownDay}");
                    stringBuilder2.AppendLine($" ,case when  CONVERT(int, fageday)>={DownDay} AND  CONVERT(int, fageday)<={UpperDay} then CONVERT(decimal(18,6), fstockqty) else 0.0 end fstockqty_{DownDay}");

                    stringBuilder3.Append($" fbaseqty_{DownDay}=y.fbaseqty_{DownDay}, fstockqty_{DownDay}=y.fstockqty_{DownDay},");
                }
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("/*dialect*/");
                stringBuilder.AppendLine($"IF EXISTS (SELECT 1  FROM sys.columns WHERE object_id = OBJECT_ID('{tableName}')  AND name = 'FLOTNO')");
                stringBuilder.AppendLine($"BEGIN");
                stringBuilder.AppendLine($"EXEC('");

                stringBuilder.AppendLine($"update x  set {stringBuilder3.ToString().Remove(stringBuilder3.ToString().Length - 1)}  from {tableName} x inner join (");
                stringBuilder.AppendLine($"select *{stringBuilder2.ToString()}  from (");
                stringBuilder.AppendLine($"select fidentityid,fbaseqty,fqty as fstockqty,");
                //检查是否为有效的 yyyy-mm-dd 格式   FInstockDate
                stringBuilder.AppendLine($"CASE WHEN ISDATE(SUBSTRING(FLOTNO, 1, 10)) = 1 AND SUBSTRING(FLOTNO, 1, 10) LIKE ''[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'' and CONVERT(date, SUBSTRING(FLOTNO, 1, 10), 120)<CONVERT(date,''{QueryDate}'', 120) ");
                stringBuilder.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, 1, 10), 120),''{QueryDate}'')+{day})");
                //检查是否为有效的 yyyymmdd 格式 
                stringBuilder.AppendLine($"WHEN ISDATE(SUBSTRING(FLOTNO, 1, 8)) = 1  AND SUBSTRING(FLOTNO, 1, 8) LIKE ''[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'' and CONVERT(date, SUBSTRING(FLOTNO, 1, 8), 112)<CONVERT(date,''{QueryDate}'', 120) ");
                stringBuilder.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, 1, 8), 112), ''{QueryDate}'')+{day})");
                //检查是否为有效的 yyyy-mm-dd 格式
                stringBuilder.AppendLine($"WHEN ISDATE(SUBSTRING(FLOTNO, LEN(FLOTNO) -9, 10)) = 1 AND SUBSTRING(FLOTNO, LEN(FLOTNO) -9, 10) LIKE ''[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'' and CONVERT(date, SUBSTRING(FLOTNO, LEN(FLOTNO) -9, 10), 120)<CONVERT(date,''{QueryDate}'', 120) ");
                stringBuilder.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, LEN(FLOTNO) -9, 10), 120), ''{QueryDate}'')+{day})");
                //检查是否为有效的 yyyymmdd 格式 
                stringBuilder.AppendLine($"WHEN ISDATE(SUBSTRING(FLOTNO, LEN(FLOTNO) -7, 8)) = 1 AND SUBSTRING(FLOTNO, LEN(FLOTNO) -7, 8) LIKE ''[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'' and CONVERT(date, SUBSTRING(FLOTNO, LEN(FLOTNO) -7, 8), 112)<CONVERT(date,''{QueryDate}'', 120) ");
                stringBuilder.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, LEN(FLOTNO) -7, 8), 112), ''{QueryDate}'')+{day})");
                stringBuilder.AppendLine($"ELSE FAGEDAYS");
                stringBuilder.AppendLine($"END  FAGEDAY from {tableName})  a where a.FAGEDAY<>''3年以上'' ");
                stringBuilder.AppendLine($") y on x.fidentityid = y.fidentityid");

                stringBuilder.AppendLine($"');");
                stringBuilder.AppendLine($"END");
                DBUtils.Execute(this.Context, stringBuilder.ToString());
            }
            

        }

    }


}
