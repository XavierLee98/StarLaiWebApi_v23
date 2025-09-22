using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Web;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Container_Tracking;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System;

namespace StarLaiPortal.Module.Controllers
{
    public partial class ContainerControllers : ViewController
    {
        GeneralControllers genCon;
        public ContainerControllers()
        {
            InitializeComponent();
        }
        protected override void OnActivated()
        {
            base.OnActivated();

            this.CancelContainer.Active.SetItemValue("Enabled", false);
            this.PrintContainer.Active.SetItemValue("Enabled", false);
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();

            genCon = Frame.GetController<GeneralControllers>();

            if (View.Id == "ContainerTracking_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.View)
                {
                    this.CancelContainer.Active.SetItemValue("Enabled", true);
                    this.PrintContainer.Active.SetItemValue("Enabled", true);
                }
                else
                {
                    this.CancelContainer.Active.SetItemValue("Enabled", false);
                    this.PrintContainer.Active.SetItemValue("Enabled", false);
                }
            }
            else
            {
                this.CancelContainer.Active.SetItemValue("Enabled", false);
                this.PrintContainer.Active.SetItemValue("Enabled", false);
            }
        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
        }

        public void openNewView(IObjectSpace os, object target, ViewEditMode viewmode)
        {
            ShowViewParameters svp = new ShowViewParameters();
            DetailView dv = Application.CreateDetailView(os, target);
            dv.ViewEditMode = viewmode;
            dv.IsRoot = true;
            svp.CreatedView = dv;

            Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));

        }
        public void showMsg(string caption, string msg, InformationType msgtype)
        {
            MessageOptions options = new MessageOptions();
            options.Duration = 3000;
            //options.Message = string.Format("{0} task(s) have been successfully updated!", e.SelectedObjects.Count);
            options.Message = string.Format("{0}", msg);
            options.Type = msgtype;
            options.Web.Position = InformationPosition.Right;
            options.Win.Caption = caption;
            options.Win.Type = WinMessageType.Flyout;
            Application.ShowViewStrategy.ShowMessage(options);
        }

        private void CancelContainer_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ContainerTracking selectedObject = (ContainerTracking)e.CurrentObject;

            selectedObject.Status = DocStatus.Cancelled;

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            IObjectSpace os = Application.CreateObjectSpace();
            ContainerTracking trx = os.FindObject<ContainerTracking>(new BinaryOperator("Oid", selectedObject.Oid));
            openNewView(os, trx, ViewEditMode.View);
            showMsg("Successful", "Cancel Done.", InformationType.Success);
        }

        private void PrintContainer_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count >= 1)
            {
                ArrayList docentry = new ArrayList();

                SqlConnection conn = new SqlConnection(genCon.getConnectionString());
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

                foreach (ContainerTracking dtl in e.SelectedObjects)
                {
                    docentry.Add(dtl.Oid);
                }

                if (docentry.Count == 0)
                {
                    docentry.Add("0");
                }

                string strServer;
                string strDatabase;
                string strUserID;
                string strPwd;
                string filename;

                try
                {
                    ReportDocument doc = new ReportDocument();
                    strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                    doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\ContainerTracking.rpt"));
                    strDatabase = conn.Database;
                    strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                    strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                    doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                    doc.Refresh();

                    doc.SetParameterValue("dockey@", docentry.ToArray());
                    doc.SetParameterValue("dbName@", conn.Database);

                    filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                        + "_" + user.UserName + "_CT_"
                        + DateTime.Today.Date.ToString("yyyyMMdd") + ".pdf";

                    doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                    doc.Close();
                    doc.Dispose();

                    string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                        ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                        + "_" + user.UserName + "_CT_"
                        + DateTime.Today.Date.ToString("yyyyMMdd") + ".pdf";
                    var script = "window.open('" + url + "');";

                    WebWindow.CurrentRequestWindow.RegisterStartupScript("DownloadFile", script); 
                }
                catch (Exception ex)
                {
                    showMsg("Fail", ex.Message, InformationType.Error);
                }
            }
            else
            {
                showMsg("Fail", "Please select container tracking to print.", InformationType.Error);
            }
        }
    }
}
