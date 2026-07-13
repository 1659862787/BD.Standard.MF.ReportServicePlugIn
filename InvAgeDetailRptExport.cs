
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.FIN.App.Core.Match.Object;
using Kingdee.K3.SCM.App.Stock.Report;
using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Text;

namespace BD.Standard.MF.ReportServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("库龄报表二开报表服务插件")]
    public class InvAgeDetailRptExport : InvAgeDetail
    {
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);

            DynamicObject dyFilter = filter.FilterParameter.CustomFilter;
            int day = Convert.ToBoolean(dyFilter["CurDateIsOne"]) ? 1 : 0;

            StringBuilder stringBuilder =new StringBuilder();

            //FCurDateIsOne
            stringBuilder.Append("/*dialect*/");
            stringBuilder.AppendLine($"IF EXISTS (SELECT 1  FROM sys.columns WHERE object_id = OBJECT_ID('{tableName}')  AND name = 'FLOTNO')");
            stringBuilder.AppendLine($"BEGIN");
            stringBuilder.AppendLine($"UPDATE {tableName} SET FAGEDAYS =");
            //检查是否为有效的 yyyy-mm-dd 格式 
            stringBuilder.AppendLine($"CASE WHEN ISDATE(SUBSTRING(FLOTNO, 1, 10)) = 1 AND SUBSTRING(FLOTNO, 1, 10) LIKE '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'");
            stringBuilder.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, 1, 10), 120), GETDATE())+{day})");
            //检查是否为有效的 yyyymmdd 格式 
            stringBuilder.AppendLine($"WHEN ISDATE(SUBSTRING(FLOTNO, 1, 8)) = 1  AND SUBSTRING(FLOTNO, 1, 8) LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
            stringBuilder.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, 1, 8), 112), GETDATE())+{day})");
            //检查是否为有效的 yyyy-mm-dd 格式
            stringBuilder.AppendLine($"WHEN ISDATE(SUBSTRING(FLOTNO, LEN(FLOTNO) -9, 10)) = 1 AND SUBSTRING(FLOTNO, LEN(FLOTNO) -9, 10) LIKE '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'");
            stringBuilder.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, LEN(FLOTNO) -9, 10), 120), GETDATE())+{day})");
            //检查是否为有效的 yyyymmdd 格式 
            stringBuilder.AppendLine($"WHEN ISDATE(SUBSTRING(FLOTNO, LEN(FLOTNO) -7, 8)) = 1 AND SUBSTRING(FLOTNO, LEN(FLOTNO) -7, 8) LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
            stringBuilder.AppendLine($"THEN CONVERT(nvarchar, DATEDIFF(day, CONVERT(date, SUBSTRING(FLOTNO, LEN(FLOTNO) -7, 8), 112), GETDATE())+{day})");
            stringBuilder.AppendLine($"ELSE FAGEDAYS");
            stringBuilder.AppendLine($"END");
            stringBuilder.AppendLine($"END");
            DBUtils.Execute(this.Context, stringBuilder.ToString());


        }

    }


}
