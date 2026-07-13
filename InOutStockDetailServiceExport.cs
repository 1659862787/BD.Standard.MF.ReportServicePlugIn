using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.FIN.HS.App.Report;
using Kingdee.K3.SCM.App.Stock.Report;
using System;
using System.ComponentModel;
using System.Text;

namespace BD.Standard.MF.ReportServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("存货收发存明细报表服务插件")]
    public class InOutStockDetailServiceExport : InOutStockDetailService
    {
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);

            DynamicObject dyFilter = filter.FilterParameter.CustomFilter;
            string FLOTNO = Convert.ToString(dyFilter["FLOTNO"]);
            if (!string.IsNullOrWhiteSpace(FLOTNO))
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append("/*dialect*/");
                stringBuilder.AppendLine($"IF EXISTS (SELECT 1  FROM sys.columns WHERE object_id = OBJECT_ID('{tableName}')  AND name = 'FLOTNO')");
                stringBuilder.AppendLine($"BEGIN");
                stringBuilder.AppendLine($"delete from {tableName} where isnull(flotno,'') not like '%{FLOTNO}%'");
                stringBuilder.AppendLine($"END");
                stringBuilder.AppendLine($";WITH CTE AS ( SELECT fidentityid, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS New_Seq ");
                stringBuilder.AppendLine($" from {tableName})");
                stringBuilder.AppendLine($"UPDATE CTE SET fidentityid = New_Seq;");
                DBUtils.Execute(this.Context, stringBuilder.ToString());

            }
        }



    }


}
