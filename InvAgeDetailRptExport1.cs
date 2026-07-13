
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.FIN.App.Core.Match.Object;
using Kingdee.K3.SCM.App.Stock.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Text;

namespace BD.Standard.MF.ReportServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("库龄汇总报表二开报表服务插件")]
    public class InvAgeDetailRptExport1 : InvAgeSummary
    {
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
        }


        protected override void AfterCreateTempTable(string tablename)
        {
            base.AfterCreateTempTable(tablename);
        }

        protected override string AnalyzeDspCloumn(IRptParams filter, string tablename)
        {
            return base.AnalyzeDspCloumn(filter, tablename);
        }

        protected override string CreateGroupSummaryData(IRptParams filter, string tablename)
        {
            return base.CreateGroupSummaryData(filter, tablename);
        }

        protected override void CreateTempTable(string sSQL)
        {
            base.CreateTempTable(sSQL);
        }

        protected override string GetIdentityFieldIndexSQL(string tableName)
        {
            return base.GetIdentityFieldIndexSQL(tableName);
        }

        protected override DataTable GetListData(string sSQL)
        {
            return base.GetListData(sSQL);
        }

        protected override DataTable GetReportData(string tablename, IRptParams filter)
        {
            return base.GetReportData(tablename, filter);
        }

        protected override object InvokePluginMethod(string name)
        {
            return base.InvokePluginMethod(name);
        }

        protected override object InvokePluginMethod(string name, object args)
        {
            return base.InvokePluginMethod(name, args);
        }

        protected override object InvokePluginMethod(string name, object args1, object args2)
        {
            return base.InvokePluginMethod(name, args1, args2);
        }
    }


}
