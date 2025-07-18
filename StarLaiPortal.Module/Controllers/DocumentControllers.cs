using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using StarLaiPortal.Module.BusinessObjects.Inquiry_View;
using StarLaiPortal.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;
using DevExpress.ExpressApp.Web.Templates.ActionContainers.Menu;
using DevExpress.Web;
using DevExpress.ExpressApp.Web.Templates.ActionContainers;
using StarLaiPortal.Module.BusinessObjects.Sales_Quotation;
using DevExpress.Xpo.DB;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using StarLaiPortal.Module.BusinessObjects.Pack_List;
using StarLaiPortal.Module.BusinessObjects.Delivery_Order;
using StarLaiPortal.Module.BusinessObjects.Load;
using DevExpress.ExpressApp.Xpo;
using StarLaiPortal.Module.BusinessObjects.Item_Inquiry;
using System.Runtime.InteropServices;
using DevExpress.Office.Utils;

// 2024-07-26 - sales history add date filter - ver 1.0.19
// 2025-07-16 - enhance saleshistory - ver 1.0.23

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class DocumentControllers : ViewController
    {
        private DateTime Fromdate;
        private DateTime Todate;
        public DocumentControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            this.DocumentDateFrom.Active.SetItemValue("Enabled", false);
            this.DocumentDateTo.Active.SetItemValue("Enabled", false);
            this.DocumentStatus.Active.SetItemValue("Enabled", false);
            this.DocumentFilter.Active.SetItemValue("Enabled", false);
            // Start ver 1.0.23
            this.SalesHistoryDTFrom.Active.SetItemValue("Enabled", false);
            this.SalesHistoryDTTo.Active.SetItemValue("Enabled", false);
            this.SalesHistoryDocFilter.Active.SetItemValue("Enabled", false);
            // End ver 1.0.23

            if (typeof(SalesOrder).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(SalesOrder))
                {
                    if (View.Id == "SalesOrder_ListView")
                    {
                        DocumentStatus.Items.Clear();

                        DocumentStatus.Items.Add(new ChoiceActionItem("All", "All", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Open", "Open", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Draft", "Draft", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Submitted", "Submitted", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Cancelled", "Cancelled", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Closed", "Closed", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Post", "Posted", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("PendPost", "Pending Post", null));

                        DocumentStatus.SelectedIndex = 1;

                        this.DocumentStatus.Active.SetItemValue("Enabled", true);
                        DocumentStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        DocumentStatus.CustomizeControl += action_CustomizeControl;

                        this.DocumentDateFrom.Active.SetItemValue("Enabled", true);
                        this.DocumentDateFrom.Value = DateTime.Today.AddDays(-14);
                        DocumentDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;

                        this.DocumentDateTo.Active.SetItemValue("Enabled", true);
                        this.DocumentDateTo.Value = DateTime.Today;
                        DocumentDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateTo.CustomizeControl += DateActionTo_CustomizeControl;

                        this.DocumentFilter.Active.SetItemValue("Enabled", true);
                    }
                }
            }

            if (typeof(SalesQuotation).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(SalesQuotation))
                {
                    if (View.Id == "SalesQuotation_ListView")
                    {
                        this.DocumentDateFrom.Active.SetItemValue("Enabled", true);
                        this.DocumentDateFrom.Value = DateTime.Today.AddDays(-14);
                        DocumentDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;
                        this.DocumentDateFrom.Execute += DocumentDateFrom_Execute;

                        this.DocumentDateTo.Active.SetItemValue("Enabled", true);
                        this.DocumentDateTo.Value = DateTime.Today;
                        DocumentDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateTo.CustomizeControl += DateActionTo_CustomizeControl;
                        this.DocumentDateTo.Execute += DocumentDateTo_Execute;

                        this.DocumentFilter.Active.SetItemValue("Enabled", true);
                    }
                }
            }

            // Start ver 1.0.15
            if (typeof(PickList).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(PickList))
                {
                    if (View.Id == "PickList_ListView_ByDate")
                    {
                        DocumentStatus.Items.Clear();

                        DocumentStatus.Items.Add(new ChoiceActionItem("All", "All", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Open", "Open", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Draft", "Draft", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Submitted", "Submitted", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Cancelled", "Cancelled", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Closed", "Closed", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Post", "Posted", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("PendPost", "Pending Post", null));

                        DocumentStatus.SelectedIndex = 0;

                        this.DocumentStatus.Active.SetItemValue("Enabled", true);
                        DocumentStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        DocumentStatus.CustomizeControl += action_CustomizeControl;

                        this.DocumentDateFrom.Active.SetItemValue("Enabled", true);
                        this.DocumentDateFrom.Value = DateTime.Today.AddDays(-14);
                        DocumentDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;
                        this.DocumentDateFrom.Execute += DocumentDateFrom_Execute;

                        this.DocumentDateTo.Active.SetItemValue("Enabled", true);
                        this.DocumentDateTo.Value = DateTime.Today;
                        DocumentDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateTo.CustomizeControl += DateActionTo_CustomizeControl;
                        this.DocumentDateTo.Execute += DocumentDateTo_Execute;

                        this.DocumentFilter.Active.SetItemValue("Enabled", true);
                    }
                }
            }

            if (typeof(PackList).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(PackList))
                {
                    if (View.Id == "PackList_ListView_ByDate")
                    {
                        DocumentStatus.Items.Clear();

                        DocumentStatus.Items.Add(new ChoiceActionItem("All", "All", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Open", "Open", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Draft", "Draft", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Submitted", "Submitted", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Cancelled", "Cancelled", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Closed", "Closed", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Post", "Posted", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("PendPost", "Pending Post", null));

                        DocumentStatus.SelectedIndex = 0;

                        this.DocumentStatus.Active.SetItemValue("Enabled", true);
                        DocumentStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        DocumentStatus.CustomizeControl += action_CustomizeControl;

                        this.DocumentDateFrom.Active.SetItemValue("Enabled", true);
                        this.DocumentDateFrom.Value = DateTime.Today.AddDays(-14);
                        DocumentDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;
                        this.DocumentDateFrom.Execute += DocumentDateFrom_Execute;

                        this.DocumentDateTo.Active.SetItemValue("Enabled", true);
                        this.DocumentDateTo.Value = DateTime.Today;
                        DocumentDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateTo.CustomizeControl += DateActionTo_CustomizeControl;
                        this.DocumentDateTo.Execute += DocumentDateTo_Execute;

                        this.DocumentFilter.Active.SetItemValue("Enabled", true);
                    }
                }
            }

            if (typeof(Load).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(Load))
                {
                    if (View.Id == "Load_ListView_ByDate")
                    {
                        DocumentStatus.Items.Clear();

                        DocumentStatus.Items.Add(new ChoiceActionItem("All", "All", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Open", "Open", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Draft", "Draft", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Submitted", "Submitted", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Cancelled", "Cancelled", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Closed", "Closed", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Post", "Posted", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("PendPost", "Pending Post", null));

                        DocumentStatus.SelectedIndex = 0;

                        this.DocumentStatus.Active.SetItemValue("Enabled", true);
                        DocumentStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        DocumentStatus.CustomizeControl += action_CustomizeControl;

                        this.DocumentDateFrom.Active.SetItemValue("Enabled", true);
                        this.DocumentDateFrom.Value = DateTime.Today.AddDays(-14);
                        DocumentDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;
                        this.DocumentDateFrom.Execute += DocumentDateFrom_Execute;

                        this.DocumentDateTo.Active.SetItemValue("Enabled", true);
                        this.DocumentDateTo.Value = DateTime.Today;
                        DocumentDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateTo.CustomizeControl += DateActionTo_CustomizeControl;
                        this.DocumentDateTo.Execute += DocumentDateTo_Execute;

                        this.DocumentFilter.Active.SetItemValue("Enabled", true);
                    }
                }
            }

            if (typeof(DeliveryOrder).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(DeliveryOrder))
                {
                    if (View.Id == "DeliveryOrder_ListView_ByDate")
                    {
                        DocumentStatus.Items.Clear();

                        DocumentStatus.Items.Add(new ChoiceActionItem("All", "All", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Open", "Open", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Draft", "Draft", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Submitted", "Submitted", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Cancelled", "Cancelled", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Closed", "Closed", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("Post", "Posted", null));
                        DocumentStatus.Items.Add(new ChoiceActionItem("PendPost", "Pending Post", null));

                        DocumentStatus.SelectedIndex = 0;

                        this.DocumentStatus.Active.SetItemValue("Enabled", true);
                        DocumentStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        DocumentStatus.CustomizeControl += action_CustomizeControl;

                        this.DocumentDateFrom.Active.SetItemValue("Enabled", true);
                        this.DocumentDateFrom.Value = DateTime.Today.AddDays(-14);
                        DocumentDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;
                        this.DocumentDateFrom.Execute += DocumentDateFrom_Execute;

                        this.DocumentDateTo.Active.SetItemValue("Enabled", true);
                        this.DocumentDateTo.Value = DateTime.Today;
                        DocumentDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                        this.DocumentDateTo.CustomizeControl += DateActionTo_CustomizeControl;
                        this.DocumentDateTo.Execute += DocumentDateTo_Execute;

                        this.DocumentFilter.Active.SetItemValue("Enabled", true);
                    }
                }
            }
            // End ver 1.0.15

            // Start ver 1.0.23
            // Start ver 1.0.19
            //if (typeof(SalesHistory).IsAssignableFrom(View.ObjectTypeInfo.Type))
            //{
            //    if (View.ObjectTypeInfo.Type == typeof(SalesHistory))
            //    {
            //        if (View.Id == "SalesHistoryList_Sales_ListView")
            //        {
            //            this.DocumentStatus.Active.SetItemValue("Enabled", true);
            //            DocumentStatus.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
            //            DocumentStatus.CustomizeControl += action_CustomizeControl;

            //            this.DocumentDateFrom.Active.SetItemValue("Enabled", true);
            //            this.DocumentDateFrom.Value = DateTime.Today.AddMonths(-3);
            //            DocumentDateFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
            //            this.DocumentDateFrom.CustomizeControl += DateActionFrom_CustomizeControl;

            //            this.DocumentDateTo.Active.SetItemValue("Enabled", true);
            //            this.DocumentDateTo.Value = DateTime.Today;
            //            DocumentDateTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
            //            this.DocumentDateTo.CustomizeControl += DateActionTo_CustomizeControl;

            //            this.DocumentFilter.Active.SetItemValue("Enabled", true);

            //            ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("SalesDate >= ? and SalesDate <= ?", DateTime.Today.AddMonths(-3), DateTime.Today.AddDays(1));
            //        }
            //    }
            //}
            // End ver 1.0.19

            if (typeof(SalesHistoryList).IsAssignableFrom(View.ObjectTypeInfo.Type))
            {
                if (View.ObjectTypeInfo.Type == typeof(SalesHistoryList))
                {
                    this.SalesHistoryDTFrom.Active.SetItemValue("Enabled", true);
                    this.SalesHistoryDTFrom.Value = DateTime.Today.AddMonths(-3);
                    SalesHistoryDTFrom.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                    this.SalesHistoryDTFrom.CustomizeControl += DateActionFrom_CustomizeControl;

                    this.SalesHistoryDTTo.Active.SetItemValue("Enabled", true);
                    this.SalesHistoryDTTo.Value = DateTime.Today;
                    SalesHistoryDTTo.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                    this.SalesHistoryDTTo.CustomizeControl += DateActionTo_CustomizeControl;

                    this.SalesHistoryDocFilter.Active.SetItemValue("Enabled", true);
                }
            }
            // End ver 1.0.23
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        private void DateActionFrom_CustomizeControl(object sender, CustomizeControlEventArgs e)
        {
            ParametrizedActionMenuActionItem actionItem = e.Control as ParametrizedActionMenuActionItem;

            if (actionItem != null)
            {
                ASPxDateEdit dateEdit = actionItem.Control.Editor as ASPxDateEdit;
                if (dateEdit != null)
                {
                    dateEdit.Width = 120;
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
                    dateEdit.Width = 120;
                    dateEdit.Buttons.Clear();
                    if (dateEdit.Text != "")
                    {
                        Todate = Convert.ToDateTime(dateEdit.Text);
                    }
                }
            }
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

        private void DocumentDateFrom_Execute(object sender, ParametrizedActionExecuteEventArgs e)
        {
            if (View.ObjectTypeInfo.Type == typeof(SalesOrder))
            {
                if (View.Id == "SalesOrder_ListView")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                        "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                            Fromdate, Todate.AddDays(1));
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(SalesQuotation))
            {
                if (View.Id == "SalesQuotation_ListView")
                {
                    ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?", Fromdate, Todate.AddDays(1));
                }
            }
        }

        private void DocumentDateTo_Execute(object sender, ParametrizedActionExecuteEventArgs e)
        {
            if (View.ObjectTypeInfo.Type == typeof(SalesOrder))
            {
                if (View.Id == "SalesOrder_ListView")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                        "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                            Fromdate, Todate.AddDays(1));
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(SalesQuotation))
            {
                if (View.Id == "SalesQuotation_ListView")
                {
                    ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?", Fromdate, Todate.AddDays(1));
                }
            }
        }

        private void DocumentStatus_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            if (View.ObjectTypeInfo.Type == typeof(SalesOrder))
            {
                if (View.Id == "SalesOrder_ListView")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                            Fromdate, Todate.AddDays(1));
                    }
                }
            }

            // Start ver 1.0.15
            if (View.ObjectTypeInfo.Type == typeof(PickList))
            {
                if (View.Id == "PickList_ListView_ByDate")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                            Fromdate, Todate.AddDays(1));
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(PackList))
            {
                if (View.Id == "PackList_ListView_ByDate")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                            Fromdate, Todate.AddDays(1));
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(Load))
            {
                if (View.Id == "Load_ListView_ByDate")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                            Fromdate, Todate.AddDays(1));
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(DeliveryOrder))
            {
                if (View.Id == "DeliveryOrder_ListView_ByDate")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                            Fromdate, Todate.AddDays(1));
                    }
                }
            }
            // End ver 1.0.15
        }

        private void DocumentFilter_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (View.ObjectTypeInfo.Type == typeof(SalesOrder))
            {
                if (View.Id == "SalesOrder_ListView")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?", 
                           Fromdate, Todate.AddDays(1));
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(SalesQuotation))
            {
                if (View.Id == "SalesQuotation_ListView")
                {
                    ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?", Fromdate, Todate.AddDays(1));
                }
            }

            // Start ver 1.0.15
            if (View.ObjectTypeInfo.Type == typeof(PickList))
            {
                if (View.Id == "PickList_ListView_ByDate")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                           Fromdate, Todate.AddDays(1));
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(PackList))
            {
                if (View.Id == "PackList_ListView_ByDate")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                           Fromdate, Todate.AddDays(1));
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(Load))
            {
                if (View.Id == "Load_ListView_ByDate")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                           Fromdate, Todate.AddDays(1));
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(DeliveryOrder))
            {
                if (View.Id == "DeliveryOrder_ListView_ByDate")
                {
                    if (DocumentStatus.SelectedItem.Id != "All")
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Status] = ? " +
                            "and DocDate >= ? and DocDate <= ?", DocumentStatus.SelectedItem.Id, Fromdate, Todate.AddDays(1));
                    }
                    else
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("DocDate >= ? and DocDate <= ?",
                           Fromdate, Todate.AddDays(1));
                    }
                }
            }
            // End ver 1.0.15

            // Start ver 1.0.23
            //// Start ver 1.0.19
            //if (View.ObjectTypeInfo.Type == typeof(SalesHistory))
            //{
            //    ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("SalesDate >= ? and SalesDate <= ?", Fromdate, Todate.AddDays(1));
            //}
            //// End ver 1.0.19
            // End ver 1.0.23
        }

        // Start ver 1.0.23
        private void SalesHistoryDocFilter_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (View.ObjectTypeInfo.Type == typeof(SalesHistoryList))
            {
                SalesHistoryList header = (SalesHistoryList)e.CurrentObject;

                header.Sales.Clear();

                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();

                header.Sales.Clear();

                SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("sp_GetSales", new OperandValue(header.Code),
                  new OperandValue(Fromdate), new OperandValue(Todate.AddDays(1)));

                int i = 1;

                if (sprocData.ResultSet.Count() > 0)
                {
                    if (sprocData.ResultSet[0].Rows.Count() > 0)
                    {
                        foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                        {
                            SalesHistory item = new SalesHistory();
                            item.Id = i;
                            item.No = i;
                            item.Customer = row.Values[1].ToString();
                            item.SalesDate = (DateTime)row.Values[2];
                            item.Quantity = (decimal)row.Values[3];
                            item.UnitPrice = (decimal)row.Values[4];
                            item.SAPInvoiceNo = row.Values[5].ToString();
                            item.Salesperson = row.Values[6].ToString();
                            item.Whse = row.Values[7].ToString();
                            header.Sales.Add(item);
                        }
                    }
                }

                persistentObjectSpace.Session.DropIdentityMap();
                persistentObjectSpace.Dispose();

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();
            }
        }
        // End ver 1.0.23
    }
}
