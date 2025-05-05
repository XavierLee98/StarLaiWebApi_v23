using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo.DB;
using StarLaiPortal.Module.BusinessObjects.Item_Inquiry;
using StarLaiPortal.Module.BusinessObjects.Stock_Count_Inquiry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 2024-04-01 add status filter ver 1.0.15
// 2024-10-08 allow to search without warehouse ver 1.0.21

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class StockCountInquiryControllers : ViewController
    {
        GeneralControllers genCon;
        public StockCountInquiryControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            this.StockCountBinSearch.Active.SetItemValue("Enabled", false);
            this.StockCountItemSearch.Active.SetItemValue("Enabled", false);
            this.StockCountVarianceSearch.Active.SetItemValue("Enabled", false);

            if (typeof(StockCountBinInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(StockCountBinInquiry))
                {
                    this.StockCountBinSearch.Active.SetItemValue("Enabled", true);
                    this.StockCountBinSearch.ActionMeaning = ActionMeaning.Accept;
                }
            }

            if (typeof(StockCountItemInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(StockCountItemInquiry))
                {
                    this.StockCountItemSearch.Active.SetItemValue("Enabled", true);
                    this.StockCountItemSearch.ActionMeaning = ActionMeaning.Accept;
                }
            }

            if (typeof(StockCountVarianceInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(StockCountVarianceInquiry))
                {
                    this.StockCountVarianceSearch.Active.SetItemValue("Enabled", true);
                    this.StockCountVarianceSearch.ActionMeaning = ActionMeaning.Accept;
                }
            }
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
            genCon = Frame.GetController<GeneralControllers>();
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
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

        private void StockCountBinSearch_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            StockCountBinInquiry selectedObject = (StockCountBinInquiry)e.CurrentObject;

            XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
            SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_StockCountBin", new OperandValue(selectedObject.Oid), 
                new OperandValue(selectedObject.FromDate.Date),
                new OperandValue(selectedObject.ToDate.Date), new OperandValue(selectedObject.Round),
                // Start ver 1.0.15
                new OperandValue(selectedObject.Status));
                // End ver 1.0.15

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
            View.Refresh();

            persistentObjectSpace.Session.DropIdentityMap();
            persistentObjectSpace.Dispose();
        }

        private void StockCountItemSearch_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            StockCountItemInquiry selectedObject = (StockCountItemInquiry)e.CurrentObject;

            XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
            SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_StockCountItem", new OperandValue(selectedObject.Oid),
                new OperandValue(selectedObject.FromDate.Date),
                new OperandValue(selectedObject.ToDate.Date), new OperandValue(selectedObject.Round));

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
            View.Refresh();

            persistentObjectSpace.Session.DropIdentityMap();
            persistentObjectSpace.Dispose();
        }

        private void StockCountVarianceSearch_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            StockCountVarianceInquiry selectedObject = (StockCountVarianceInquiry)e.CurrentObject;

            // Start ver 1.0.21
            //if (selectedObject.Warehouse != null)
            //{
            // End ver 1.0.21
            XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
            SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_StockCountVariance", new OperandValue(selectedObject.Oid),
                new OperandValue(selectedObject.StockCountDate.Date),
                // Start ver 1.0.21
                //new OperandValue(selectedObject.Warehouse.WarehouseCode));
                new OperandValue(selectedObject.Warehouse == null ? "" : selectedObject.Warehouse.WarehouseCode));
                // End ver 1.0.21

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
            View.Refresh();

            persistentObjectSpace.Session.DropIdentityMap();
            persistentObjectSpace.Dispose();
            // Start ver 1.0.21
            //}
            //else
            //{
            //    showMsg("Error", "Please select warehouse.", InformationType.Error);
            //}
            // End ver 1.0.21
        }
    }
}
