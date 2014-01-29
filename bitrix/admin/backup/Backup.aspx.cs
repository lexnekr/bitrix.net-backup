using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Web.Configuration;
using Bitrix.Modules;
using Bitrix.UI;
using System.Data;
using Bitrix.DataLayer;
using Bitrix.Security;
using System.IO;
using System.Threading;
using Bitrix.Services;

public partial class Backup : BXAdminPage
{
    private DataTable dataSource = null;

    protected void Page_Init(object sender, EventArgs e)
    {

    }

    protected override void OnLoad(EventArgs e)
    {
        dataSource = GetFiles();
    }

    private DataTable GetFiles()
    {
        BuckupOptions option = new BuckupOptions
        {
            BuckupFolder = HttpContext.Current.Request.PhysicalApplicationPath + "backup\\",
            DestinationFolder = HttpContext.Current.Request.PhysicalApplicationPath,
            ConnectionString = WebConfigurationManager.ConnectionStrings["BXConnectionString"].ConnectionString
        };
        chbIncludeDataBase.Text = string.Format("({0})", BuckupMeneger.GetDBSize(option));

        DataTable result = new DataTable();

        result.Columns.Add("Name", typeof(string));
        result.Columns.Add("FullName", typeof(string));
        result.Columns.Add("Size", typeof(int));
        result.Columns.Add("Created", typeof(DateTime));
        result.Columns.Add("ID", typeof(int));
        int i = 0;
        foreach (var file in BuckupMeneger.GetFiles(option))
        {
            DataRow dr = result.NewRow();
            dr["Name"] = file.Name;
            dr["FullName"] = string.Format("{0}", file.FullName);
            dr["Size"] = file.Length / 1024 / 1024;
            dr["Created"] = file.CreationTime;
            dr["ID"] = i;
            i++;
            result.Rows.Add(dr);
        }

        return result;
    }

    protected void btnArchive_Click(object sender, EventArgs e)
    {

        BuckupOptions option = GetOption();
        BuckupMeneger meneger = new BuckupMeneger();
        meneger.Buckup(option);

        dataSource = GetFiles();
        BackupsGridView.MarkAsChanged();
        //FileListDataBind();
    }

    public BuckupOptions GetOption()
    {
        long excludeFileSize = 0;
        long.TryParse(txbExcludeFileSize.Text, out excludeFileSize);

        DirectoryInfo dir = new DirectoryInfo(HttpContext.Current.Request.PhysicalApplicationPath + "backup\\");
        if (!dir.Exists)
            dir.Create();

        int stepDuration = 0;
        int.TryParse(txbStepDuration.Text, out stepDuration);

        int sleep = 0;
        int.TryParse(txbSleep.Text, out sleep);

        BuckupOptions option = new BuckupOptions
        {
            BuckupFolder = HttpContext.Current.Request.PhysicalApplicationPath,
            DestinationFolder = dir.FullName,
            ExcludeFiles = (@"*\bitrix\cache;*\backup;" + (!chbArchivePublicPart.Checked ? @"*\bitrix;" : string.Empty) + txbExcludeFiles.Text).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
            IncludeCorePart = chbArchiveCore.Checked,
            ExcludeFileSize = excludeFileSize * 1024,
            ExcludeStandardTypesCompressing = chbExcludeStandardTypesCompressing.Checked,
            DisableCompresing = chbDisableCompressing.Checked,
            CheckPackage = chbCheckPackage.Checked,
            IncludeDatabase = chbIncludeDataBase.Checked,
            StepDuration = stepDuration * 1000,
            Sleep = sleep * 1000,
            ConnectionString = WebConfigurationManager.ConnectionStrings["BXConnectionString"].ConnectionString
        };

        return option;
    }

    protected void BackupsGridView_Select(object sender, Bitrix.UI.BXSelectEventArgs e)
    {
        int startRowIndex = 0;
        int maximumRows = 0;
        if (e.PagingOptions != null)
        {
            startRowIndex = e.PagingOptions.startRowIndex;
            maximumRows = e.PagingOptions.maximumRows;
        }

        e.Data = new DataView(dataSource, "ID>=" + startRowIndex, "", DataViewRowState.CurrentRows);
    }

    protected void BackupsGridView_Delete(object sender, Bitrix.UI.BXDeleteEventArgs e)
    {
        BXGridView grid = (BXGridView)sender;
        try
        {
            File.Delete((string)e.Keys["FullName"]);
        }
        catch (Exception ex){ }

        dataSource = GetFiles();
        grid.MarkAsChanged();

    }

    protected void BackupsGridView_SelectCount(object sender, BXSelectCountEventArgs e)
    {
        if (dataSource == null)
            dataSource = GetFiles();
        e.Count = dataSource.Rows.Count;
    }

    protected void BXTabControl1_Command(object sender, Bitrix.UI.BXTabControlCommandEventArgs e)
    {

    }

    protected void btnCreateAgent_Click(object sender, EventArgs e)
    {
        string agentName = "BackupAgent_" + txbAgentName.Text;
        double hour = 0;
        double.TryParse(txbHour.Text, out hour);

        DateTime startDate = DateTime.Now.Date;
        

        BXSchedulerAgent agent = BXSchedulerAgent.GetByName(agentName);
        if (agent != null)
        {
            lblError.Text = string.Format("Агент с именем \"{0}\" уже существует", agentName);
            return;
        }
        else
            lblError.Text = "";

        if (string.IsNullOrEmpty(txbAgentName.Text))
        {
            lblError.Text = string.Format("Введите правильный имя агента", hour);
            return;
        }

        if(hour == 0)
        {
            lblError.Text = string.Format("Введите правильный интервал", hour);
            return;
        }

        try
        {
            int h = int.Parse(txbStartTime.Text.Split(':')[0].TrimStart('0'));
            int m = int.Parse(txbStartTime.Text.Split(':')[1].TrimStart('0'));
            if (h >= 0 && h < 24 && m >= 0 && m < 60)
            {
                startDate = startDate.AddHours((double)h);
                startDate = startDate.AddMinutes((double)m);
            }
            else
            {
                lblError.Text = string.Format("Введите правильный начало", hour);
                return;
            }
        }
        catch
        {
            lblError.Text = string.Format("Введите правильный начало", hour);
            return;
        }

        BuckupOptions option = GetOption();

        agent = new BXSchedulerAgent(agentName);
        agent.Parameters["BackupOptions"] = ToBase64(option);
        agent.Period = TimeSpan.FromHours(hour);
        agent.Periodic = true;
        agent.StartTime = startDate;
        agent.SetClassNameAndAssembly(typeof(Bitrix.Modules.BackupAgentExecutor));
        agent.Save();

        AgentGridView.DataBind();

    }

    public static string ToBase64(BuckupOptions obj)
    {
        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(BuckupOptions));

        using (StringWriter writer = new StringWriter())
        {
            serializer.Serialize(writer, obj);

            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(writer.ToString());
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }
    }


    #region Agent

    protected void AgentGridView_Select(object sender, Bitrix.UI.BXSelectEventArgs e)
    {
        int startRowIndex = 0;
        int maximumRows = 0;
        if (e.PagingOptions != null)
        {
            startRowIndex = e.PagingOptions.startRowIndex;
            maximumRows = e.PagingOptions.maximumRows;
        }

        e.Data = GetBackupAgents(e);
    }

    protected void AgentGridView_Delete(object sender, Bitrix.UI.BXDeleteEventArgs e)
    {
        BXGridView grid = (BXGridView)sender;
        try
        {
            
            BXSchedulerAgent.Delete(e.Keys["ID"]);

            grid.MarkAsChanged();

        }
        catch (Exception ex)
        {

        }

    }

    protected void AgentGridView_SelectCount(object sender, BXSelectCountEventArgs e)
    {
        var filter = new BXFormFilter(new BXFormFilterItem("Name", "BackupAgent", BXSqlFilterOperators.StartsLike));

        e.Count = BXSchedulerAgent.Count(new BXFilter(filter, BXSchedulerAgent.Fields));
    }

    private BXSchedulerAgentCollection GetBackupAgents(BXSelectEventArgs e)
    {
        var filter = new BXFormFilter(new BXFormFilterItem("Name", "BackupAgent", BXSqlFilterOperators.StartsLike));
        return BXSchedulerAgent.GetList(new BXFilter(filter, BXSchedulerAgent.Fields),
            new BXOrderBy(BXSchedulerAgent.Fields, e.SortExpression),
            null,
            new BXQueryParams(e.PagingOptions)
        );

    }

    #endregion
}