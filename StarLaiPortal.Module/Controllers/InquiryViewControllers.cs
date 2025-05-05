using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web.Templates.ActionContainers.Menu;
using DevExpress.ExpressApp.Web.Templates.ActionContainers;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Dashboard;
using StarLaiPortal.Module.BusinessObjects.Inquiry_View;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Web;
using System.Web.UI.WebControls;
using DevExpress.Xpo.DB;
using DevExpress.ExpressApp.Web.Templates;
using DevExpress.ExpressApp.Web;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;
using DevExpress.ExpressApp.Xpo;
using StarLaiPortal.Module.BusinessObjects.Item_Inquiry;
using DevExpress.Utils.Filtering.Internal;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using StarLaiPortal.Module.BusinessObjects.Stock_Count_Inquiry;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using StarLaiPortal.Module.BusinessObjects.Delivery_Order;
using System.Web;
using DevExpress.XtraPrinting.Native;
using StarLaiPortal.Module.BusinessObjects.Pack_List;

// 2023-09-14 - add filter into inquiry - ver 1.0.9
// 2023-10-16 - sales order inquiry add "All" option for filter and view button - ver 1.0.11
// 2024-01-30 - add inventory movement search button - ver 1.0.14
// 2024-04-05 - add inquiry search button - ver 1.0.15
// 2024-05-29 - amend pist list inquiry - ver 1.0.16
// 2024-06-11 - add preview in sales order inquiry - ver 1.0.17
// 2024-07-18 - add view in pack list inquiry sp - ver 1.0.19
// 2024-08-20 - add EIVValidatedStatus - ver 1.0.19

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class InquiryViewControllers : ViewController
    {
        // Start ver 1.0.9
        private DateTime Fromdate;
        private DateTime Todate;
        // End ver 1.0.9

        GeneralControllers genCon;

        public InquiryViewControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            this.ViewOpenPickList.Active.SetItemValue("Enabled", false);
            this.ViewPickListDetailInquiry.Active.SetItemValue("Enabled", false);
            this.ViewPickListInquiry.Active.SetItemValue("Enabled", false);
            // Start ver 1.0.9
            this.InquiryStatus.Active.SetItemValue("Enabled", false);
            this.InquiryDateFrom.Active.SetItemValue("Enabled", false);
            this.InquiryDateTo.Active.SetItemValue("Enabled", false);
            this.InquiryFilter.Active.SetItemValue("Enabled", false);
            // End ver 1.0.9
            // Start ver 1.0.11
            this.ViewSalesOrderInquiry.Active.SetItemValue("Enabled", false);
            // End ver 1.0.11
            // Start ver 1.0.14
            this.StockMovementSPSearch.Active.SetItemValue("Enabled", false);
            // End ver 1.0.14
            // Start ver 1.0.15
            this.InquirySearch.Active.SetItemValue("Enabled", false);
            this.PrintDOInquiry.Active.SetItemValue("Enabled", false);
            this.PreviewInvInquiry.Active.SetItemValue("Enabled", false);
            // End ver 1.0.15
            // Start ver 1.0.17
            this.PreviewSOInquiry.Active.SetItemValue("Enabled", false);
            // End ver 1.0.17
            // Start ver 1.0.19
            this.PrintBundleInquiry.Active.SetItemValue("Enabled", false);
            // End ver 1.0.19

            if (typeof(vwInquiryOpenPickList).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(vwInquiryOpenPickList))
                {
                    this.ViewOpenPickList.Active.SetItemValue("Enabled", true);
                    this.ViewOpenPickList.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;
                }
            }

            // Start ver 1.0.15
            if (typeof(vwInquiryPickListDetails).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(vwInquiryPickListDetails))
                {
                    this.ViewPickListDetailInquiry.Active.SetItemValue("Enabled", true);
                    this.ViewPickListDetailInquiry.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;

                    if (View.Id == "vwInquiryPickListDetails_ListView")
                    {
                        InquiryStatus.Items.Clear();

                        // Start ver 1.0.16
                        InquiryStatus.Items.Add(new ChoiceActionItem("All", "All"));
                        // End ver 1.0.16
                        InquiryStatus.Items.Add(new ChoiceActionItem("Open", "Open"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Draft", "Draft"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Submitted", "Submitted"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Cancelled", "Cancelled"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Closed", "Closed"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Posted", "Posted"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Pending Post", "Pending Post"));

                        // Start ver 1.0.16
                        InquiryStatus.SelectedIndex = 0;
                        // End ver 1.0.16

                        // Start ver 1.0.16
                        //((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ?",
                        //    InquiryStatus.SelectedItem.Id);
                        // End ver 1.0.16

                        this.InquiryStatus.Active.SetItemValue("Enabled", true);
                        InquiryStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        InquiryStatus.CustomizeControl += action_CustomizeControl;

                        this.InquiryDateFrom.Active.SetItemValue("Enabled", true);
                        this.InquiryDateFrom.Value = DateTime.Today.AddDays(-7);
                        InquiryDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.InquiryDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;
                        this.InquiryDateTo.Active.SetItemValue("Enabled", true);
                        // Start ver 1.0.16
                        //this.InquiryDateTo.Value = DateTime.Today.AddDays(1);
                        // Start ver 1.0.19
                        //this.InquiryDateTo.Value = DateTime.Today;
                        this.InquiryDateTo.Value = DateTime.Today.AddDays(1);
                        // End ver 1.0.19
                        // End ver 1.0.16
                        InquiryDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.InquiryDateTo.CustomizeControl += DateActionTo_CustomizeControl;
                        this.InquiryFilter.Active.SetItemValue("Enabled", true);

                        // Start ver 1.0.16
                        if (InquiryStatus.SelectedItem.Id != "All")
                        {
                            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                                "and DocDate >= ? and DocDate <= ?", InquiryStatus.SelectedItem.Id, InquiryDateFrom.Value, InquiryDateTo.Value);
                        }
                        else
                        {
                            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?", 
                                InquiryDateFrom.Value, InquiryDateTo.Value);
                        }
                        // End ver 1.0.16
                        // Start ver 1.0.19
                        this.InquiryDateTo.Value = DateTime.Today;
                        // End ver 1.0.19
                    }
                }
            }


            //if (typeof(PickListDetailsInquiryResult).IsAssignableFrom(View.ObjectTypeInfo.Type))
            //{
            //    if (View.ObjectTypeInfo.Type == typeof(PickListDetailsInquiryResult))
            //    {
            //        this.ViewPickListDetailInquiry.Active.SetItemValue("Enabled", true);
            //        this.ViewPickListDetailInquiry.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;
            //    }
            //}
            // End ver 1.0.15

            if (typeof(vwInquiryPickList).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(vwInquiryPickList))
                {
                    this.ViewPickListInquiry.Active.SetItemValue("Enabled", true);
                    this.ViewPickListInquiry.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;
                }
            }

            // Start ver 1.0.9
            if (typeof(vwInquirySalesOrder).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(vwInquirySalesOrder))
                {
                    this.ViewSalesOrderInquiry.Active.SetItemValue("Enabled", true);
                    this.ViewSalesOrderInquiry.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;

                    if (View.Id == "vwInquirySalesOrder_ListView")
                    {
                        InquiryStatus.Items.Clear();

                        // Start ver 1.0.11
                        InquiryStatus.Items.Add(new ChoiceActionItem("All", "All"));
                        // End ver 1.0.11
                        InquiryStatus.Items.Add(new ChoiceActionItem("Open", "Open"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Draft", "Draft"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Submitted", "Submitted"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Cancelled", "Cancelled"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Closed", "Closed"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Posted", "Posted"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Pending Post", "Pending Post"));

                        InquiryStatus.SelectedIndex = 0;

                        this.InquiryStatus.Active.SetItemValue("Enabled", true);
                        InquiryStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        InquiryStatus.CustomizeControl += action_CustomizeControl;

                        this.InquiryDateFrom.Active.SetItemValue("Enabled", true);
                        this.InquiryDateFrom.Value = DateTime.Today.AddDays(-7);
                        InquiryDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.InquiryDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;
                        this.InquiryDateTo.Active.SetItemValue("Enabled", true);
                        this.InquiryDateTo.Value = DateTime.Today.AddDays(1);
                        InquiryDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.InquiryDateTo.CustomizeControl += DateActionTo_CustomizeControl;
                        this.InquiryFilter.Active.SetItemValue("Enabled", true);

                        // Start ver 1.0.11
                        if (InquiryStatus.SelectedItem.Id != "All")
                        {
                            // End ver 1.0.11
                            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?",
                            InquiryStatus.SelectedItem.Id, InquiryDateFrom.Value, InquiryDateTo.Value);
                        // Start ver 1.0.11
                        }
                        else
                        {
                            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                                InquiryDateFrom.Value, InquiryDateTo.Value);
                        }
                        // End ver 1.0.11

                        // Start ver 1.0.17
                        this.PreviewSOInquiry.Active.SetItemValue("Enabled", true);
                        // End ver 1.0.17

                        // Start ver 1.0.19
                        this.InquiryDateTo.Value = DateTime.Today;
                        // End ver 1.0.19
                    }
                }
            }

            if (typeof(vwInquiryPickList).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(vwInquiryPickList))
                {
                    if (View.Id == "vwInquiryPickList_ListView")
                    {
                        InquiryStatus.Items.Clear();

                        InquiryStatus.Items.Add(new ChoiceActionItem("Open", "Open"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Draft", "Draft"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Submitted", "Submitted"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Cancelled", "Cancelled"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Closed", "Closed"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Posted", "Posted"));
                        InquiryStatus.Items.Add(new ChoiceActionItem("Pending Post", "Pending Post"));

                        InquiryStatus.SelectedIndex = 1;

                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ?",
                            InquiryStatus.SelectedItem.Id);

                        this.InquiryStatus.Active.SetItemValue("Enabled", true);
                        InquiryStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        InquiryStatus.CustomizeControl += action_CustomizeControl;

                        this.InquiryDateFrom.Active.SetItemValue("Enabled", true);
                        this.InquiryDateFrom.Value = DateTime.Today.AddDays(-7);
                        InquiryDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.InquiryDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;
                        this.InquiryDateTo.Active.SetItemValue("Enabled", true);
                        this.InquiryDateTo.Value = DateTime.Today.AddDays(1);
                        InquiryDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.InquiryDateTo.CustomizeControl += DateActionTo_CustomizeControl;
                        this.InquiryFilter.Active.SetItemValue("Enabled", true);

                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                        "and DocDate >= ? and DocDate <= ?",
                        InquiryStatus.SelectedItem.Id, InquiryDateFrom.Value, InquiryDateTo.Value);

                        // Start ver 1.0.19
                        this.InquiryDateTo.Value = DateTime.Today;
                        // End ver 1.0.19
                    }
                }
            }
            // End ver 1.0.9

            // Start ver 1.0.14
            if (typeof(StockMovement).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(StockMovement))
                {
                    this.StockMovementSPSearch.Active.SetItemValue("Enabled", true);
                }
            }
            // End ver 1.0.14

            // Start ver 1.0.15
            if (typeof(SalesQuotationInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(SalesQuotationInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(ARDownpaymentInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(ARDownpaymentInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(PickListDetailsInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(PickListDetailsInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(BundleIDInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(BundleIDInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(PackListInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(PackListInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(LoadingInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(LoadingInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(DeliveryInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(DeliveryInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(InvoiceInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(InvoiceInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(PurchaseOrderInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(PurchaseOrderInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(GRPOInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(GRPOInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(ASNInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(ASNInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(PurchaseReturnInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(PurchaseReturnInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(SalesReturnRequestInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(SalesReturnRequestInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(CreditMemoInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(CreditMemoInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(WarehouseTransferDetailsInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(WarehouseTransferDetailsInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(StockAdjustmentDetailsInquiry).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(StockAdjustmentDetailsInquiry))
                {
                    this.InquirySearch.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(DeliveryInquiryResult).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(DeliveryInquiryResult))
                {
                    this.PrintDOInquiry.Active.SetItemValue("Enabled", true);
                }
            }

            if (typeof(InvoiceInquiryResult).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(InvoiceInquiryResult))
                {
                    this.PreviewInvInquiry.Active.SetItemValue("Enabled", true);
                }
            }
            // End ver 1.0.15

            // Start ver 1.0.19
            if (typeof(PackListInquiryResult).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(PackListInquiryResult))
                {
                    this.PrintBundleInquiry.Active.SetItemValue("Enabled", true);
                }
            }
            // End ver 1.0.19
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

        void action_CustomizeControl(object sender, CustomizeControlEventArgs e)
        {
            SingleChoiceActionAsModeMenuActionItem actionItem = e.Control as SingleChoiceActionAsModeMenuActionItem;
            if (actionItem != null && actionItem.Action.PaintStyle == DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption)
            {
                DropDownSingleChoiceActionControlBase control = (DropDownSingleChoiceActionControlBase)actionItem.Control;
                control.Label.Text = actionItem.Action.Caption;
                control.Label.Style["padding-right"] = "5px";
                control.ComboBox.Width = 100;
            }
        }

        private void DateActionFrom_CustomizeControl(object sender, CustomizeControlEventArgs e)
        {
            ParametrizedActionMenuActionItem actionItem = e.Control as ParametrizedActionMenuActionItem;

            if (actionItem != null)
            {
                ASPxDateEdit dateEdit = actionItem.Control.Editor as ASPxDateEdit;
                if (dateEdit != null)
                {
                    dateEdit.Width = 115;
                    dateEdit.Buttons.Clear();
                    if (dateEdit.Text != "")
                    {
                        Fromdate = Convert.ToDateTime(dateEdit.Text);
                    }
                }
            }
        }

        private void DateActionTo_CustomizeControl(object sender, CustomizeControlEventArgs e)
        {
            ParametrizedActionMenuActionItem actionItem = e.Control as ParametrizedActionMenuActionItem;

            if (actionItem != null)
            {
                ASPxDateEdit dateEdit = actionItem.Control.Editor as ASPxDateEdit;
                if (dateEdit != null)
                {
                    dateEdit.Width = 115;
                    dateEdit.Buttons.Clear();
                    if (dateEdit.Text != "")
                    {
                        Todate = Convert.ToDateTime(dateEdit.Text);
                    }
                }
            }
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

        private void ViewOpenPickList_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            vwInquiryOpenPickList selectedObject = (vwInquiryOpenPickList)e.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));
            openNewView(os, trx, ViewEditMode.View);
        }

        private void ViewOpenPickList_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            vwInquiryOpenPickList selectedObject = (vwInquiryOpenPickList)View.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));

            DetailView detailView = Application.CreateDetailView(os, "PickList_DetailView_Dashboard", true, trx);
            detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.View;
            e.View = detailView;
            e.DialogController.AcceptAction.Caption = "Go To Document";
            e.Maximized = true;
            //e.DialogController.CancelAction.Active["NothingToCancel"] = false;
        }

        private void ViewPickListDetailInquiry_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            vwInquiryPickListDetails selectedObject = (vwInquiryPickListDetails)e.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));
            openNewView(os, trx, ViewEditMode.View);
        }

        private void ViewPickListDetailInquiry_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            vwInquiryPickListDetails selectedObject = (vwInquiryPickListDetails)View.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));

            DetailView detailView = Application.CreateDetailView(os, "PickList_DetailView_Dashboard", true, trx);
            detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.View;
            e.View = detailView;
            e.DialogController.AcceptAction.Caption = "Go To Document";
            e.Maximized = true;
            //e.DialogController.CancelAction.Active["NothingToCancel"] = false;
        }

        private void ViewPickListInquiry_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            vwInquiryPickList selectedObject = (vwInquiryPickList)e.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));
            openNewView(os, trx, ViewEditMode.View);
        }

        private void ViewPickListInquiry_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            vwInquiryPickList selectedObject = (vwInquiryPickList)View.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            PickList trx = os.FindObject<PickList>(new BinaryOperator("DocNum", selectedObject.PortalNo));

            DetailView detailView = Application.CreateDetailView(os, "PickList_DetailView_Dashboard", true, trx);
            detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.View;
            e.View = detailView;
            e.DialogController.AcceptAction.Caption = "Go To Document";
            e.Maximized = true;
            //e.DialogController.CancelAction.Active["NothingToCancel"] = false;
        }

        // Start ver 1.0.9
        private void InquiryStatus_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            // Start ver 1.0.11
            if (InquiryStatus.SelectedItem.Id != "All")
            {
            // End ver 1.0.11
                ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                    "and DocDate >= ? and DocDate < ?",
                    InquiryStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
            // Start ver 1.0.11
            }
            else
            {
                ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate < ?",
                    Fromdate, Todate.AddDays(1));
            }
            // End ver 1.0.11
        }

        private void InquiryDateFrom_Execute(object sender, ParametrizedActionExecuteEventArgs e)
        {
            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                "and DocDate >= ? and DocDate < ?",
                InquiryStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
        }

        private void InquiryDateTo_Execute(object sender, ParametrizedActionExecuteEventArgs e)
        {
            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                "and DocDate >= ? and DocDate < ?",
                InquiryStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
        }

        private void InquiryFilter_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // Start ver 1.0.11
            if (InquiryStatus.SelectedItem.Id != "All")
            {
            // End ver 1.0.11
                ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                "and DocDate >= ? and DocDate < ?",
                InquiryStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
            // Start ver 1.0.11
            }
            else
            {
                ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate < ?",
                    Fromdate, Todate.AddDays(1));
            }
            // End ver 1.0.11
        }
        // End ver 1.0.9

        // Start ver 1.0.11
        private void ViewSalesOrderInquiry_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            vwInquirySalesOrder selectedObject = (vwInquirySalesOrder)e.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            SalesOrder trx = os.FindObject<SalesOrder>(new BinaryOperator("DocNum", selectedObject.PortalNo));
            openNewView(os, trx, ViewEditMode.View);
        }

        private void ViewSalesOrderInquiry_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            vwInquirySalesOrder selectedObject = (vwInquirySalesOrder)View.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            SalesOrder trx = os.FindObject<SalesOrder>(new BinaryOperator("DocNum", selectedObject.PortalNo));

            DetailView detailView = Application.CreateDetailView(os, "SalesOrder_DetailView_Dashboard", true, trx);
            detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.View;
            e.View = detailView;
            e.DialogController.AcceptAction.Caption = "Go To Document";
            e.Maximized = true;
            //e.DialogController.CancelAction.Active["NothingToCancel"] = false;
        }
        // End ver 1.0.11

        // Start ver 1.0.14
        private void StockMovementSPSearch_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string itemcode = "";
            string portalnum = "";
            StockMovement selectedObject = (StockMovement)e.CurrentObject;

            // Start ver 1.0.16
            if (selectedObject.ItemCode == null)
            {
                showMsg("Fail", "Please select item code.", InformationType.Error);
                return;
            }
            // End ver 1.0.16

            if (selectedObject.ItemCode != null)
            {
                itemcode = selectedObject.ItemCode.ItemCode;
            }

            if (selectedObject.PortalDocNum != null)
            {
                portalnum = selectedObject.PortalDocNum;
            }

            int cnt = 0;
            XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
            SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetStockMovement", 
                new OperandValue(selectedObject.DateFrom.Date),
                new OperandValue(selectedObject.DateTo.Date),
                new OperandValue(itemcode), new OperandValue(portalnum));

            if (sprocData.ResultSet.Count() > 0)
            {
                if (sprocData.ResultSet[0].Rows.Count() > 0)
                {
                    selectedObject.Results.Clear();
                    foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                    {
                        StockMovementResult stockmovement = new StockMovementResult();

                        stockmovement.Oid = ++cnt;
                        stockmovement.TransDate = DateTime.Parse(row.Values[0].ToString());
                        stockmovement.PortalNo = row.Values[1].ToString();
                        stockmovement.SAPNo = row.Values[2].ToString();
                        stockmovement.CardCode = row.Values[3].ToString();
                        stockmovement.CardName = row.Values[4].ToString();
                        stockmovement.ItemCode = row.Values[5].ToString();
                        stockmovement.ItemName = row.Values[6].ToString();
                        stockmovement.LegacyItemCode = row.Values[7].ToString();
                        stockmovement.CatalogNo = row.Values[8].ToString();
                        stockmovement.Model = row.Values[9].ToString();
                        stockmovement.Quantity = Convert.ToDecimal(row.Values[10].ToString());
                        stockmovement.UOM = row.Values[11].ToString();
                        stockmovement.Warehouse = row.Values[12].ToString();
                        stockmovement.BinLocation = row.Values[13].ToString();
                        stockmovement.TransType = row.Values[14].ToString();

                        selectedObject.Results.Add(stockmovement);
                    }
                }
            }

            ObjectSpace.Refresh();
            View.Refresh();

            persistentObjectSpace.Session.DropIdentityMap();
            persistentObjectSpace.Dispose();
        }
        // End ver 1.0.14

        // Start ver 1.0.15
        private void InquirySearch_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (View.ObjectTypeInfo.Type == typeof(SalesQuotationInquiry))
            {
                SalesQuotationInquiry currObject = (SalesQuotationInquiry)e.CurrentObject;
                currObject.Results.Clear();

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("SalesQuotationInquiry"),
                     new OperandValue(""));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            SalesQuotationInquiryResult result = new SalesQuotationInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.DocDate = DateTime.Parse(row.Values[2].ToString());
                            result.DueDate = DateTime.Parse(row.Values[3].ToString());
                            result.Status = row.Values[4].ToString();
                            result.HitCreditLimit = row.Values[5].ToString();
                            result.HitCreditTerm = row.Values[6].ToString();
                            result.HitPriceChange = row.Values[7].ToString();
                            result.CardGroup = row.Values[8].ToString();
                            result.CardCode = row.Values[9].ToString();
                            result.CardName = row.Values[10].ToString();
                            result.ContactNo = row.Values[11].ToString();
                            result.Transporter = row.Values[12].ToString();
                            result.Salesperson = row.Values[13].ToString();
                            result.Priority = row.Values[14].ToString();
                            result.Series = row.Values[15].ToString();
                            result.Amount = decimal.Parse(row.Values[16].ToString());
                            result.Remarks = row.Values[17].ToString();
                            result.PortalSONo = row.Values[18].ToString();
                            result.SONo = row.Values[19].ToString();
                            result.PickListNo = row.Values[20].ToString();
                            result.PackListNo = row.Values[21].ToString();
                            result.LoadingNo = row.Values[22].ToString();
                            result.PortalDONo = row.Values[23].ToString();
                            result.SAPDONo = row.Values[24].ToString();
                            result.SAPInvNo = row.Values[25].ToString();
                            result.CreateDate = DateTime.Parse(row.Values[26].ToString());
                            result.PriceChange = bool.Parse(row.Values[27].ToString());
                            result.ExceedPrice = bool.Parse(row.Values[28].ToString());
                            result.ExceedCreditControl = bool.Parse(row.Values[29].ToString());

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(ARDownpaymentInquiry))
            {
                ARDownpaymentInquiry currObject = (ARDownpaymentInquiry)e.CurrentObject;
                currObject.Results.Clear();

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("ARDownpaymentInquiry"),
                     new OperandValue(""));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            ARDownpaymentInquiryResult result = new ARDownpaymentInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.SAPNo = row.Values[2].ToString();
                            result.CreateDT = DateTime.Parse(row.Values[3].ToString());
                            result.DocDate = DateTime.Parse(row.Values[4].ToString());
                            result.Status = row.Values[5].ToString();
                            result.CardCode = row.Values[6].ToString();
                            result.CardName = row.Values[7].ToString();
                            result.Remark = row.Values[8].ToString();
                            result.SAPSONo = row.Values[9].ToString();
                            result.Amount = decimal.Parse(row.Values[10].ToString());
                            result.PaymentType = row.Values[11].ToString();
                            result.RefNo = row.Values[12].ToString();
                          
                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(PickListDetailsInquiry))
            {
                PickListDetailsInquiry currObject = (PickListDetailsInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("PickListDetailsInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            PickListDetailsInquiryResult result = new PickListDetailsInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.Series = row.Values[2].ToString();
                            result.DocDate = DateTime.Parse(row.Values[3].ToString());
                            result.DueDate = DateTime.Parse(row.Values[4].ToString());
                            result.Status = row.Values[5].ToString();
                            result.Transporter = row.Values[6].ToString();
                            result.Picker = row.Values[7].ToString();
                            result.CardCode = row.Values[8].ToString();
                            result.CardName = row.Values[9].ToString();
                            result.PortalSONo = row.Values[10].ToString();
                            result.SAPSONo = row.Values[11].ToString();
                            result.ItemCode = row.Values[12].ToString();
                            result.ItemName = row.Values[13].ToString();
                            result.LegacyItemCode = row.Values[14].ToString();
                            result.CatalogNo = row.Values[15].ToString();
                            result.Model = row.Values[16].ToString();
                            result.Brand = row.Values[17].ToString();
                            result.PlanQty = decimal.Parse(row.Values[18].ToString());
                            result.UOM = row.Values[19].ToString();
                            result.PickQty = decimal.Parse(row.Values[20].ToString());
                            result.Warehouse = row.Values[21].ToString();
                            result.Bin = row.Values[22].ToString();
                            result.DiscreReason = row.Values[23].ToString();
                            result.Remark = row.Values[24].ToString();
                            result.PrintStatus = row.Values[25].ToString();
                            result.PrintCount = Int32.Parse(row.Values[26].ToString());
                            result.Priority = row.Values[27].ToString();

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(BundleIDInquiry))
            {
                BundleIDInquiry currObject = (BundleIDInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(""), new OperandValue("BundleIDInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            BundleIDInquiryResult result = new BundleIDInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.LoadingDate = DateTime.Parse(row.Values[2].ToString());
                            result.BundleID = row.Values[3].ToString();
                            result.ItemCode = row.Values[4].ToString();
                            result.ItemName = row.Values[5].ToString();
                            result.SONo = row.Values[6].ToString();
                            result.SODate = DateTime.Parse(row.Values[7].ToString());
                            result.CustomerName = row.Values[8].ToString();
                            result.LegacyItemCode = row.Values[9].ToString();
                            result.TransporterName = row.Values[10].ToString();
                            result.PickListDocNum = row.Values[11].ToString();
                            result.PackDate = DateTime.Parse(row.Values[12].ToString());
                            result.PackTime = row.Values[13].ToString();
                        
                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(PackListInquiry))
            {
                PackListInquiry currObject = (PackListInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("PackListInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            PackListInquiryResult result = new PackListInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.DocDate = DateTime.Parse(row.Values[2].ToString());
                            result.Status = row.Values[3].ToString();
                            result.CardGroup = row.Values[4].ToString();
                            result.CardCode = row.Values[5].ToString();
                            result.CardName = row.Values[6].ToString();
                            result.ItemCode = row.Values[7].ToString();
                            result.ItemName = row.Values[8].ToString();
                            result.LegacyItemCode = row.Values[9].ToString();
                            result.CatalogNo = row.Values[10].ToString();
                            result.Model = row.Values[11].ToString();
                            result.Brand = row.Values[12].ToString();
                            result.Quantity = decimal.Parse(row.Values[13].ToString());
                            result.UOM = row.Values[14].ToString();
                            result.Warehouse = row.Values[15].ToString();
                            result.Bin = row.Values[16].ToString();
                            result.ToBin = row.Values[17].ToString();
                            result.Remark = row.Values[18].ToString();
                            result.PortalSONo = row.Values[19].ToString();
                            result.SAPSONo = row.Values[20].ToString();
                            result.BundleID = row.Values[21].ToString();
                            result.PickListNo = row.Values[22].ToString();
                            
                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(LoadingInquiry))
            {
                LoadingInquiry currObject = (LoadingInquiry)e.CurrentObject;
                currObject.Results.Clear();

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("LoadingInquiry"),
                     new OperandValue(""));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            LoadingInquiryResult result = new LoadingInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.DocDate = DateTime.Parse(row.Values[2].ToString());
                            result.Status = row.Values[3].ToString();
                            result.PackListNo = row.Values[4].ToString();
                            result.Remark = row.Values[5].ToString();
                            result.Driver = row.Values[6].ToString();
                            result.VehicleNo = row.Values[7].ToString();
                            result.BundleID = row.Values[8].ToString();
                            result.LoadingTime = row.Values[9].ToString();
                            result.SONum = row.Values[10].ToString();
                            result.SODate = DateTime.Parse(row.Values[11].ToString());
                            result.ItemNo = row.Values[12].ToString();
                            result.ItemDesc = row.Values[13].ToString();
                            result.LegacyCode = row.Values[14].ToString();
                            result.CatalogNo = row.Values[15].ToString();

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(DeliveryInquiry))
            {
                DeliveryInquiry currObject = (DeliveryInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("DeliveryInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            DeliveryInquiryResult result = new DeliveryInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.SAPNo = row.Values[2].ToString();
                            result.CreateDT = DateTime.Parse(row.Values[3].ToString());
                            result.DocDate = DateTime.Parse(row.Values[4].ToString());
                            result.DueDate = DateTime.Parse(row.Values[5].ToString());
                            result.Status = row.Values[6].ToString();
                            result.CardCode = row.Values[7].ToString();
                            result.CardName = row.Values[8].ToString();
                            result.ItemCode = row.Values[9].ToString();
                            result.ItemName = row.Values[10].ToString();
                            result.OldCode = row.Values[11].ToString();
                            result.CatalogNo = row.Values[12].ToString();
                            result.Model = row.Values[13].ToString();
                            result.Brand = row.Values[14].ToString();
                            result.Quantity = decimal.Parse(row.Values[15].ToString());
                            result.UOM = row.Values[16].ToString();
                            result.Warehouse = row.Values[17].ToString();
                            result.Remark = row.Values[18].ToString();
                            result.SAPSONo = row.Values[19].ToString();
                            result.LoadingNo = row.Values[20].ToString();
                            result.SAPInvNo = row.Values[21].ToString();
                            result.Transporter = row.Values[22].ToString();

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(InvoiceInquiry))
            {
                InvoiceInquiry currObject = (InvoiceInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("InvoiceInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            InvoiceInquiryResult result = new InvoiceInquiryResult();

                            result.PriKey = row.Values[0].ToString();  
                            result.SAPNo = row.Values[1].ToString();
                            result.CreateDT = DateTime.Parse(row.Values[2].ToString());
                            result.DocDate = DateTime.Parse(row.Values[3].ToString());
                            result.DueDate = DateTime.Parse(row.Values[4].ToString());
                            result.Status = row.Values[5].ToString();
                            result.CardCode = row.Values[6].ToString();
                            result.CardName = row.Values[7].ToString();
                            result.ItemCode = row.Values[8].ToString();
                            result.ItemName = row.Values[9].ToString();
                            result.OldCode = row.Values[10].ToString();
                            result.CatalogNo = row.Values[11].ToString();
                            result.Model = row.Values[12].ToString();
                            result.Brand = row.Values[13].ToString();
                            result.Quantity = decimal.Parse(row.Values[14].ToString());
                            result.UOM = row.Values[15].ToString();
                            result.Warehouse = row.Values[16].ToString();
                            result.Remark = row.Values[17].ToString();
                            result.PortalSONo = row.Values[18].ToString();
                            result.SAPSONo = row.Values[19].ToString();
                            result.SOSeries = row.Values[20].ToString();
                            result.LoadingNo = row.Values[21].ToString();
                            result.SAPInvNo = row.Values[22].ToString();
                            result.PortalDONo = row.Values[23].ToString();
                            result.SAPDONo = row.Values[24].ToString();
                            result.Transporter = row.Values[25].ToString();
                            result.DocTotal = decimal.Parse(row.Values[26].ToString());
                            // Start ver 1.0.19
                            result.EIVValidatedStatus = row.Values[27].ToString();
                            // End ver 1.0.19

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(PurchaseOrderInquiry))
            {
                PurchaseOrderInquiry currObject = (PurchaseOrderInquiry)e.CurrentObject;
                currObject.Results.Clear();
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("PurchaseOrderInquiry"),
                     new OperandValue(""));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            PurchaseOrderInquiryResult result = new PurchaseOrderInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.SAPNo = row.Values[2].ToString();
                            result.DocDate = DateTime.Parse(row.Values[3].ToString());
                            result.DueDate = DateTime.Parse(row.Values[4].ToString());
                            result.CardCode = row.Values[5].ToString();
                            result.CardName = row.Values[6].ToString();
                            result.Remark = row.Values[7].ToString();
                            result.ItemCode = row.Values[8].ToString();
                            result.ItemName = row.Values[9].ToString();
                            result.LegacyItemCode = row.Values[10].ToString();
                            result.CatalogNo = row.Values[11].ToString();
                            result.Model = row.Values[12].ToString();
                            result.Brand = row.Values[13].ToString();
                            result.Quantity = decimal.Parse(row.Values[14].ToString());
                            result.UOM = row.Values[15].ToString();
                            result.Warehouse = row.Values[16].ToString();
                            result.Price = decimal.Parse(row.Values[17].ToString());
                            result.Amount = decimal.Parse(row.Values[18].ToString());
                            result.ASNNo = row.Values[19].ToString();
                            result.GRPONo = row.Values[20].ToString();
                            result.SAPGRPONo = row.Values[21].ToString();
                            result.SAPInvNo = row.Values[22].ToString();
                            result.SAPStatus = row.Values[23].ToString();
                            result.OpenQty = Int32.Parse(row.Values[24].ToString());
                            result.LabelPrintCount = Int32.Parse(row.Values[25].ToString());

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(GRPOInquiry))
            {
                GRPOInquiry currObject = (GRPOInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("GRPOInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            GRPOInquiryResult result = new GRPOInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.SAPNo = row.Values[2].ToString();
                            result.PortalPONo = row.Values[3].ToString();
                            result.SAPPONo = row.Values[4].ToString();
                            result.ASNNo = row.Values[5].ToString();
                            result.DocDate = DateTime.Parse(row.Values[6].ToString());
                            result.Status = row.Values[7].ToString();
                            result.CardCode = row.Values[8].ToString();
                            result.CardName = row.Values[9].ToString();
                            result.ItemCode = row.Values[10].ToString();
                            result.ItemName = row.Values[11].ToString();
                            result.LegacyItemCode = row.Values[12].ToString();
                            result.CatalogNo = row.Values[13].ToString();
                            result.Model =row.Values[14].ToString();
                            result.Brand = row.Values[15].ToString();
                            result.ASNQty = decimal.Parse(row.Values[16].ToString());
                            result.ReceivedQty = decimal.Parse(row.Values[17].ToString());
                            result.DiscrepancyQty = decimal.Parse(row.Values[18].ToString());
                            result.DiscrepancyReason = row.Values[19].ToString();
                            result.UOM = row.Values[20].ToString();
                            result.Warehouse = row.Values[21].ToString();
                            result.Bin = row.Values[22].ToString();
                            result.Remark = row.Values[23].ToString();
                            result.Reference = row.Values[24].ToString();

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(ASNInquiry))
            {
                ASNInquiry currObject = (ASNInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("ASNInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            ASNInquiryResult result = new ASNInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.DocDate = DateTime.Parse(row.Values[2].ToString());
                            result.TaxDate = DateTime.Parse(row.Values[3].ToString());
                            result.ETADate = DateTime.Parse(row.Values[4].ToString());
                            result.ESRDate = DateTime.Parse(row.Values[5].ToString());
                            result.Status =row.Values[6].ToString();
                            result.PrintStatus = row.Values[7].ToString();
                            result.Vehicle = row.Values[8].ToString();
                            result.Container = row.Values[9].ToString();
                            result.CardCode = row.Values[10].ToString();
                            result.CardName = row.Values[11].ToString();
                            result.SAPPONo = row.Values[12].ToString();
                            result.PortalPONo = row.Values[13].ToString();
                            result.ItemCode = row.Values[14].ToString();
                            result.ItemName = row.Values[15].ToString();
                            result.LegacyItemCode = row.Values[16].ToString();
                            result.CatalogNo = row.Values[17].ToString();
                            result.Model = row.Values[18].ToString();
                            result.Brand = row.Values[19].ToString();
                            result.PlanQty = decimal.Parse(row.Values[20].ToString());
                            result.UOM = row.Values[21].ToString();
                            result.LoadQty = decimal.Parse(row.Values[22].ToString());
                            result.Warehouse = row.Values[23].ToString();
                            result.Remark = row.Values[24].ToString();
                            result.Reference = row.Values[25].ToString();
                            result.LabelPrintCount = Int32.Parse(row.Values[26].ToString());
                            result.OpenQty = decimal.Parse(row.Values[27].ToString());
                            result.Series = row.Values[28].ToString();
                            result.SalesNumber = row.Values[29].ToString();

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(PurchaseReturnInquiry))
            {
                PurchaseReturnInquiry currObject = (PurchaseReturnInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("PurchaseReturnInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            PurchaseReturnInquiryResult result = new PurchaseReturnInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.SAPNo = row.Values[2].ToString();
                            result.SAPGRPONo = row.Values[3].ToString();
                            result.PortalGRPONo = row.Values[4].ToString();
                            result.DocDate = DateTime.Parse(row.Values[5].ToString());
                            result.Status = row.Values[6].ToString();
                            result.CardCode = row.Values[7].ToString();
                            result.CardName = row.Values[8].ToString();
                            result.ItemCode = row.Values[9].ToString();
                            result.ItemName = row.Values[10].ToString();
                            result.LegacyItemCode = row.Values[11].ToString();
                            result.CatalogNo = row.Values[12].ToString();
                            result.Model = row.Values[13].ToString();
                            result.Brand = row.Values[14].ToString();
                            result.Quantity = decimal.Parse(row.Values[15].ToString());
                            result.UOM = row.Values[16].ToString();
                            result.Warehouse = row.Values[17].ToString();
                            result.Bin = row.Values[18].ToString();
                            result.Remark = row.Values[19].ToString();
                            result.Price = decimal.Parse(row.Values[20].ToString());
                            result.Amount = decimal.Parse(row.Values[21].ToString());
                            result.ReturnReason = row.Values[22].ToString();
                            result.Reference = row.Values[23].ToString();
                            result.Transporter = row.Values[24].ToString();

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(SalesReturnRequestInquiry))
            {
                SalesReturnRequestInquiry currObject = (SalesReturnRequestInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("SalesReturnRequestInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            SalesReturnRequestInquiryResult result = new SalesReturnRequestInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.PortalReturnNo = row.Values[2].ToString();
                            result.SAPReturnNo = row.Values[3].ToString();
                            result.SAPCNNo = row.Values[4].ToString();
                            result.DocDate = DateTime.Parse(row.Values[5].ToString());
                            result.TaxDate = DateTime.Parse(row.Values[6].ToString());
                            result.Status = row.Values[7].ToString();
                            result.CardCode = row.Values[8].ToString();
                            result.CardName = row.Values[9].ToString();
                            result.Reference = row.Values[10].ToString();
                            result.Transporter = row.Values[11].ToString();
                            result.Remarks = row.Values[12].ToString();
                            result.ItemCode = row.Values[13].ToString();
                            result.ItemName = row.Values[14].ToString();
                            result.LegacyItemCode = row.Values[15].ToString();
                            result.CatalogNo = row.Values[16].ToString();
                            result.Model = row.Values[17].ToString();
                            result.Brand = row.Values[18].ToString();
                            result.Quantity = decimal.Parse(row.Values[19].ToString());
                            result.UOM = row.Values[20].ToString();
                            result.ReturnReason = row.Values[21].ToString();
                            result.Warehouse = row.Values[22].ToString();
                            result.Bin = row.Values[23].ToString();
                            result.Price = decimal.Parse(row.Values[24].ToString());
                            result.Amount = decimal.Parse(row.Values[25].ToString());

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(CreditMemoInquiry))
            {
                CreditMemoInquiry currObject = (CreditMemoInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("CreditMemoInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            CreditMemoInquiryResult result = new CreditMemoInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.SAPNo = row.Values[1].ToString();
                            result.CreateDT = DateTime.Parse(row.Values[2].ToString());
                            result.DocDate = DateTime.Parse(row.Values[3].ToString());
                            result.DueDate = DateTime.Parse(row.Values[4].ToString());
                            result.Status = row.Values[5].ToString();
                            result.CardCode = row.Values[6].ToString();
                            result.CardName = row.Values[7].ToString();
                            result.ItemCode = row.Values[8].ToString();
                            result.ItemName = row.Values[9].ToString();
                            result.OldCode = row.Values[10].ToString();
                            result.CatalogNo = row.Values[11].ToString();
                            result.Model = row.Values[12].ToString();
                            result.Brand = row.Values[13].ToString();
                            result.Quantity = decimal.Parse(row.Values[14].ToString());
                            result.UOM = row.Values[15].ToString();
                            result.Warehouse = row.Values[16].ToString();
                            result.Remark = row.Values[17].ToString();
                            result.PortalSONo = row.Values[18].ToString();
                            result.SOSeries = row.Values[19].ToString();
                            result.LoadingNo = row.Values[20].ToString();
                            result.PortalInvNo = row.Values[21].ToString();
                            result.SAPInvNo = row.Values[22].ToString();
                            result.PortalDONo = row.Values[23].ToString();
                            result.SAPDONo = row.Values[24].ToString();
                            result.Transporter = row.Values[25].ToString();
                            result.DocKey = row.Values[26].ToString();
                            result.RowTotal = decimal.Parse(row.Values[27].ToString());
                            result.DocTotal = decimal.Parse(row.Values[28].ToString());
                            result.SalesReturnReason = row.Values[29].ToString();

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(WarehouseTransferDetailsInquiry))
            {
                WarehouseTransferDetailsInquiry currObject = (WarehouseTransferDetailsInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("WarehouseTransferDetailsInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            WarehouseTransferDetailsInquiryResult result = new WarehouseTransferDetailsInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.SAPNo = row.Values[2].ToString();
                            result.DocDate = DateTime.Parse(row.Values[3].ToString());
                            result.Status = row.Values[4].ToString();
                            result.Reference = row.Values[5].ToString();
                            result.Remark = row.Values[6].ToString();
                            result.ItemCode = row.Values[7].ToString();
                            result.ItemName = row.Values[8].ToString();
                            result.LegacyItemCode = row.Values[9].ToString();
                            result.CatalogNo = row.Values[10].ToString();
                            result.Model = row.Values[11].ToString();
                            result.Brand = row.Values[12].ToString();
                            result.Quantity = decimal.Parse(row.Values[13].ToString());
                            result.UOM = row.Values[14].ToString();
                            result.Warehouse = row.Values[15].ToString();
                            result.Bin = row.Values[16].ToString();
                            result.TransferType = row.Values[17].ToString();
                            result.ToWarehouse = row.Values[18].ToString();
                            result.ToBin = row.Values[19].ToString();

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }

            if (View.ObjectTypeInfo.Type == typeof(StockAdjustmentDetailsInquiry))
            {
                StockAdjustmentDetailsInquiry currObject = (StockAdjustmentDetailsInquiry)e.CurrentObject;
                currObject.Results.Clear();

                string itemcode = null;

                if (currObject.ItemCode != null)
                {
                    itemcode = currObject.ItemCode.ItemCode;
                }
                else
                {
                    itemcode = "All";
                }

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetInquiryView",
                    new OperandValue(currObject.DateFrom.Date),
                    new OperandValue(currObject.DateTo.AddDays(1).Date), new OperandValue(currObject.Status), new OperandValue("StockAdjustmentDetailsInquiry"),
                     new OperandValue(itemcode));

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            StockAdjustmentDetailsInquiryResult result = new StockAdjustmentDetailsInquiryResult();

                            result.PriKey = row.Values[0].ToString();
                            result.PortalNo = row.Values[1].ToString();
                            result.SAPNo = row.Values[2].ToString();
                            result.DocDate = DateTime.Parse(row.Values[3].ToString());
                            result.Status = row.Values[4].ToString();
                            result.ItemCode = row.Values[5].ToString();
                            result.ItemName = row.Values[6].ToString();
                            result.LegacyItemCode = row.Values[7].ToString();
                            result.CatalogNo = row.Values[8].ToString();
                            result.Model = row.Values[9].ToString();
                            result.Brand = row.Values[10].ToString();
                            result.Quantity = decimal.Parse(row.Values[11].ToString());
                            result.UOM = row.Values[12].ToString();
                            result.Warehouse = row.Values[13].ToString();
                            result.Bin = row.Values[14].ToString();
                            result.Remark = row.Values[15].ToString();
                            result.Price = decimal.Parse(row.Values[16].ToString());
                            result.Amount = decimal.Parse(row.Values[17].ToString());
                            result.ReasonCode = row.Values[18].ToString();
                            result.CostType = row.Values[19].ToString();

                            currObject.Results.Add(result);
                        }
                    }
                }

                ObjectSpace.Refresh();
                View.Refresh();

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();
            }
        }

        private void PrintDOInquiry_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count == 1)
            {
                string strServer;
                string strDatabase;
                string strUserID;
                string strPwd;
                string filename;

                SqlConnection conn = new SqlConnection(genCon.getConnectionString());
                DeliveryInquiryResult currobject = (DeliveryInquiryResult)View.CurrentObject;
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

                if (currobject.PortalNo == "")
                {
                    showMsg("Fail", "DO number not found.", InformationType.Error);
                    return;
                }

                IObjectSpace os = Application.CreateObjectSpace();
                DeliveryOrder delivery = os.FindObject<DeliveryOrder>(new BinaryOperator("DocNum", currobject.PortalNo));

                try
                {
                    ReportDocument doc = new ReportDocument();
                    strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                    doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\DeliveryOrder.rpt"));
                    strDatabase = conn.Database;
                    strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                    strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                    doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                    doc.Refresh();

                    doc.SetParameterValue("dockey@", delivery.Oid);
                    doc.SetParameterValue("dbName@", conn.Database);

                    filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                        + "_" + delivery.Oid + "_" + user.UserName + "_DO_"
                        + DateTime.Parse(delivery.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

                    doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                    doc.Close();
                    doc.Dispose();

                    string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                        ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                        + "_" + delivery.Oid + "_" + user.UserName + "_DO_"
                        + DateTime.Parse(delivery.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";
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
                showMsg("Fail", "Please select one DO only.", InformationType.Error);
            }
        }

        private void PreviewInvInquiry_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count == 1)
            {
                string strServer;
                string strDatabase;
                string strUserID;
                string strPwd;
                string filename;

                SqlConnection conn = new SqlConnection(genCon.getConnectionString());
                InvoiceInquiryResult currobject = (InvoiceInquiryResult)View.CurrentObject;
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

                if (currobject.PortalDONo == "")
                {
                    showMsg("Fail", "DO number not found.", InformationType.Error);
                    return;
                }

                IObjectSpace os = Application.CreateObjectSpace();
                DeliveryOrder delivery = os.FindObject<DeliveryOrder>(new BinaryOperator("DocNum", currobject.PortalDONo));
            

                if (delivery.SAPDocNum != null)
                {
                    try
                    {
                        ReportDocument doc = new ReportDocument();
                        strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                        doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\Invoice.rpt"));
                        strDatabase = conn.Database;
                        strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                        strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                        doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                        doc.Refresh();

                        doc.SetParameterValue("dockey@", delivery.Oid);
                        doc.SetParameterValue("dbName@", conn.Database);

                        filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                            + "_" + delivery.Oid + "_" + user.UserName + "_Inv_"
                            + DateTime.Parse(delivery.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

                        doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                        doc.Close();
                        doc.Dispose();

                        string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                            ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                            + "_" + delivery.Oid + "_" + user.UserName + "_Inv_"
                            + DateTime.Parse(delivery.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";
                        var script = "window.open('" + url + "');";

                        WebWindow.CurrentRequestWindow.RegisterStartupScript("DownloadFile", script);

                        IObjectSpace updos = Application.CreateObjectSpace();
                        DeliveryOrder trx = updos.FindObject<DeliveryOrder>(new BinaryOperator("Oid", delivery.Oid));

                        trx.INVPrintCount = trx.INVPrintCount + 1;
                        trx.INVPrintDate = DateTime.Now;

                        updos.CommitChanges();
                        updos.Refresh();
                    }
                    catch (Exception ex)
                    {
                        showMsg("Fail", ex.Message, InformationType.Error);
                    }
                }
                else
                {
                    showMsg("Fail", "Invoice not found.", InformationType.Error);
                }
            }
            else
            {
                showMsg("Fail", "Please select one Invoice only.", InformationType.Error);
            }
        }
        // End ver 1.0.15

        // Start ver 1.0.17
        private void PreviewSOInquiry_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count == 1)
            {
                string strServer;
                string strDatabase;
                string strUserID;
                string strPwd;
                string filename;

                SqlConnection conn = new SqlConnection(genCon.getConnectionString());
                vwInquirySalesOrder currobject = (vwInquirySalesOrder)View.CurrentObject;
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

                if (currobject.PortalNo == "")
                {
                    showMsg("Fail", "SO number not found.", InformationType.Error);
                    return;
                }

                IObjectSpace os = Application.CreateObjectSpace();
                SalesOrder so = os.FindObject<SalesOrder>(new BinaryOperator("DocNum", currobject.PortalNo));

                try
                {
                    ReportDocument doc = new ReportDocument();
                    strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                    doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\SalesOrder.rpt"));
                    strDatabase = conn.Database;
                    strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                    strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                    doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                    doc.Refresh();

                    doc.SetParameterValue("dockey@", so.Oid);
                    doc.SetParameterValue("dbName@", conn.Database);

                    filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                        + "_" + so.Oid + "_" + user.UserName + "_SO_"
                        + DateTime.Parse(so.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

                    doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                    doc.Close();
                    doc.Dispose();

                    string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                        ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                        + "_" + so.Oid + "_" + user.UserName + "_SO_"
                        + DateTime.Parse(so.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";
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
                showMsg("Fail", "Please select one SO only.", InformationType.Error);
            }
        }
        // End ver 1.0.17

        // Start ver 1.0.19
        private void PrintBundleInquiry_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count == 1)
            {
                string strServer;
                string strDatabase;
                string strUserID;
                string strPwd;
                string filename;

                SqlConnection conn = new SqlConnection(genCon.getConnectionString());
                PackListInquiryResult currobject = (PackListInquiryResult)View.CurrentObject;
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

                if (currobject.PortalNo == "")
                {
                    showMsg("Fail", "Pack list number not found.", InformationType.Error);
                    return;
                }

                IObjectSpace os = Application.CreateObjectSpace();
                PackList pal = os.FindObject<PackList>(new BinaryOperator("DocNum", currobject.PortalNo));

                try
                {
                    ReportDocument doc = new ReportDocument();
                    strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                    doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\Bundle.rpt"));
                    strDatabase = conn.Database;
                    strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                    strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                    doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                    doc.Refresh();

                    doc.SetParameterValue("dockey@", pal.Oid);
                    doc.SetParameterValue("dbName@", conn.Database);

                    filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                        + "_" + pal.Oid + "_" + user.UserName + "_PAL_"
                        + DateTime.Parse(pal.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

                    doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                    doc.Close();
                    doc.Dispose();

                    string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                        ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                        + "_" + pal.Oid + "_" + user.UserName + "_PAL_"
                        + DateTime.Parse(pal.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";
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
                showMsg("Fail", "Please select one pack list to print.", InformationType.Error);
            }
        }
        // End ver 1.0.19
    }
}
