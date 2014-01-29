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
            ExcludeFiles = (@"*\bitrix\cache;*\backup;" + (!chbArchivePublicPart.Checked? @"*\bitrix;":string.Empty) + txbExcludeFiles.Text).Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries),
            IncludeCorePart = chbArchiveCore.Checked,
            ExcludeFileSize = excludeFileSize*1024,
            ExcludeStandardTypesCompressing = chbExcludeStandardTypesCompressing.Checked,
            DisableCompresing = chbDisableCompressing.Checked,
            CheckPackage = chbCheckPackage.Checked,
            IncludeDatabase = chbIncludeDataBase.Checked,
            StepDuration = stepDuration * 1000,
            Sleep = sleep * 1000,
            ConnectionString = WebConfigurationManager.ConnectionStrings["BXConnectionString"].ConnectionString
        };

        BuckupMeneger.Buckup(option);
        dataSource = GetFiles();
        BackupsGridView.MarkAsChanged();
        //FileListDataBind();
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

        e.Data = new DataView(dataSource, "ID>="+startRowIndex, "", DataViewRowState.CurrentRows);
    }

    protected void AuthBackupsGridView_PopupMenuClick(object sender, Bitrix.UI.BXPopupMenuClickEventArgs e)
    {
        
    }

    protected void BackupsGridView_Delete(object sender, Bitrix.UI.BXDeleteEventArgs e)
    {
        BXGridView grid = (BXGridView)sender;
        try
        {
            File.Delete((string)e.Keys["FullName"]);
            dataSource = GetFiles();
            grid.MarkAsChanged();
            
        }
        catch (PublicException ex)
        {
           
        }

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

}