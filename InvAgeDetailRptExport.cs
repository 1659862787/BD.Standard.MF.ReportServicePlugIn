
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
    [Description("库龄分析明细报表服务插件")]
    public class InvAgeDetailRptExport : InvAgeDetail
    {
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);

            DynamicObject dyFilter = filter.FilterParameter.CustomFilter;
            string QueryDate = DateTime.Parse(dyFilter["QueryDate"].ToString()).ToString("yyyy-MM-dd");
            DateTime dateTime = Convert.ToDateTime(dyFilter["QueryDate"]);
            int day = Convert.ToBoolean(dyFilter["CurDateIsOne"]) ? 1 : 0;
            StringBuilder stringBuilder =new StringBuilder();
            //FCurDateIsOne
            stringBuilder.Append("/*dialect*/");
            stringBuilder.AppendLine($"IF EXISTS (SELECT 1  FROM sys.columns WHERE object_id = OBJECT_ID('{tableName}')  AND name = 'FLOTNO')");
            stringBuilder.AppendLine($"BEGIN");
            stringBuilder.AppendLine($"EXEC('");
            stringBuilder.AppendLine($"UPDATE {tableName} SET FAGEDAYS =");
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
            stringBuilder.AppendLine($"END");
            stringBuilder.AppendLine($"');");
            stringBuilder.AppendLine($"END");
            stringBuilder.AppendLine($"IF EXISTS (SELECT 1  FROM sys.columns WHERE object_id = OBJECT_ID('{tableName}'))");

            //加权平均价
            //stringBuilder.AppendLine($"BEGIN");
            //stringBuilder.AppendLine($"EXEC('");
            //stringBuilder.AppendLine($"update x set fprice=aveprice,fbaseprice=aveprice,famount=fqty*aveprice from {tableName} x inner join (select   d.FYEAR 年份 , FPERIOD 期间 ,k.FNUMBER 会计核算体系,a.FMATERIALID FMATERIALID\r\n,case when sum(isnull(c.FQTY,0)+isnull(c1.FCURRENTINQTY,0))=0 then 0 \r\nelse sum(isnull(c.FAMOUNT,0)+isnull(c1.FCURRENTINAMOUNT,0))/sum(isnull(c.FQTY,0)+isnull(c1.FCURRENTINQTY,0))end aveprice\r\nfROM T_BD_MATERIal  a  join  T_HS_inivSTOCKDIMENSION  b   on  a.FMATERIALID  =b.FMATERIALID   \r\njoin   T_HS_inivBALANCE_h  c1  on      b.FENTRYID =c1.FDIMEENTRYID   and   c1.FENDINITKEY =1  \r\nleft  join  T_HS_INIVBALANCE_h  c  on  b.FENTRYID =c.FDIMEENTRYID   and   c.FENDINITKEY =0  and   c.fid=c1.fid  \r\nleft  join   T_BD_STOCK  g   on  b.fstockid=g.fstockid left  join   T_BD_STOCK_L  gl   on  gl.fstockid=g.fstockid \r\nleft    join   T_BD_LOTMASTER  f  on   b.flot=f.flotid  left   join  T_ORG_ORGANIZATIONS  h   on  b.FCARGOOWNERID=h.FORGID \r\nleft   join  T_ORG_ORGANIZATIONS  h1   on  b.FSTOCKORGID=h1.FORGID  \r\nleft   join   T_HS_CALDIMENSIONS  j  on   c1.fdimensionid=j.fdimensionid  \r\nleft   join  T_ORG_ACCountsystem  k   on   j.FACCTSYSTEMID =k.FACCTSYSTEMID     \r\njoin  T_HS_OUTACCTG  d  on   d.fid=c1.fid  where  d.FYEAR=''{dateTime.Year}''\r\nand  FPERIOD=''{dateTime.Month}''\r\nand  k.FNUMBER =''KJHSTX01_SYS''\r\nand  h.fnumber=''100''\r\nand (isnull(c1.fqty ,0)>0  \r\nor isnull(c1.FAMOUNT ,0)<>0)\r\ngroup by d.FYEAR , FPERIOD ,k.FNUMBER,a.FMATERIALID) y on x.FMATERIALID=y.FMATERIALID  where aveprice<>0");
            //stringBuilder.AppendLine($"');");
            //stringBuilder.AppendLine($"END");

            //期末结存单价
            stringBuilder.AppendLine($"BEGIN");
            stringBuilder.AppendLine($"EXEC('");
            stringBuilder.AppendLine($"update x set fprice=aveprice,fbaseprice=aveprice,famount=fqty*aveprice from {tableName} x inner join (select   d.FYEAR 年份 , FPERIOD 期间 ,k.FNUMBER 会计核算体系,a.FMATERIALID FMATERIALID\r\n,case when  sum(isnull(c1.FQTY,0))=0 then 0 else sum(isnull(c1.FAMOUNT,0))/sum(isnull(c1.FQTY,0)) end aveprice\r\nfROM T_BD_MATERIal  a  join  T_HS_inivSTOCKDIMENSION  b   on  a.FMATERIALID  =b.FMATERIALID   \r\njoin   T_HS_inivBALANCE_h  c1  on      b.FENTRYID =c1.FDIMEENTRYID   and   c1.FENDINITKEY =1  \r\nleft  join  T_HS_INIVBALANCE_h  c  on  b.FENTRYID =c.FDIMEENTRYID   and   c.FENDINITKEY =0  and   c.fid=c1.fid  \r\nleft  join   T_BD_STOCK  g   on  b.fstockid=g.fstockid left  join   T_BD_STOCK_L  gl   on  gl.fstockid=g.fstockid \r\nleft    join   T_BD_LOTMASTER  f  on   b.flot=f.flotid  left   join  T_ORG_ORGANIZATIONS  h   on  b.FCARGOOWNERID=h.FORGID \r\nleft   join  T_ORG_ORGANIZATIONS  h1   on  b.FSTOCKORGID=h1.FORGID  \r\nleft   join   T_HS_CALDIMENSIONS  j  on   c1.fdimensionid=j.fdimensionid  \r\nleft   join  T_ORG_ACCountsystem  k   on   j.FACCTSYSTEMID =k.FACCTSYSTEMID     \r\njoin  T_HS_OUTACCTG  d  on   d.fid=c1.fid  where  d.FYEAR=''{dateTime.Year}''\r\nand  FPERIOD=''{dateTime.Month}''\r\nand  k.FNUMBER =''KJHSTX01_SYS''\r\nand  h.fnumber=''100''\r\nand (isnull(c1.fqty ,0)>0  \r\nor isnull(c1.FAMOUNT ,0)<>0)\r\ngroup by d.FYEAR , FPERIOD ,k.FNUMBER,a.FMATERIALID) y on x.FMATERIALID=y.FMATERIALID  where aveprice>=0");
            stringBuilder.AppendLine($"');");
            stringBuilder.AppendLine($"END");
            DBUtils.Execute(this.Context, stringBuilder.ToString());



        }

    }


}
