using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Delivery_Order;
using StarLaiPortal.Module.BusinessObjects.Item_Inquiry;
using StarLaiPortal.Module.BusinessObjects.Print_Module;
using StarLaiPortal.Module.BusinessObjects.Reports;
using StarLaiPortal.Module.BusinessObjects.Sales_Order_Collection;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.Stock_Count_Inquiry;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

// 2023-10-23 add stock count ver 1.0.12
// 2023-12-04 add daily delivery summary ver 1.0.13
// 2024-01-30 - add inventory movement table - ver 1.0.14
// 2024-04-05 - add inquiry view sp - ver 1.0.15
// 2024-06-01 - enlarge popout screen - ver 1.0.17
// 2024-02-04 - add global item inquiry - ver 1.0.22

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class NavigationControllers : WindowController
    {
        private ShowNavigationItemController showNavigationItemController;
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public NavigationControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
            TargetWindowType = WindowType.Main;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            showNavigationItemController = Frame.GetController<ShowNavigationItemController>();
            showNavigationItemController.CustomShowNavigationItem += showNavigationItemController_CustomShowNavigationItem;

            // Start ver 1.0.17
            ((WebApplication)Application).PopupWindowManager.PopupShowing += PopupWindowManager_PopupShowing;
            // End ver 1.0.17
        }
        //protected override void OnViewControlsCreated()
        //{
        //    base.OnViewControlsCreated();
        //    // Access and customize the target View control.
        //}
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
            showNavigationItemController.CustomShowNavigationItem -= showNavigationItemController_CustomShowNavigationItem;
        }

        void showNavigationItemController_CustomShowNavigationItem(object sender, CustomShowNavigationItemEventArgs e)
        {
            if (e.ActionArguments.SelectedChoiceActionItem.Id == "ItemInquiry_ListView")
            {
                IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(ItemInquiry));
                ItemInquiry iteminquiry = objectSpace.FindObject<ItemInquiry>(new BinaryOperator("Oid", 1));

                DocTypes number = objectSpace.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.SO));

                iteminquiry.Search = null;
                iteminquiry.Cart = number.NextDocNum.ToString();
                iteminquiry.PriceList1 = null;
                iteminquiry.PriceList2 = null;
                iteminquiry.Stock1 = null;
                iteminquiry.Stock2 = null;

                for (int i = 0; iteminquiry.ItemInquiryDetails.Count > i;)
                {
                    iteminquiry.ItemInquiryDetails.Remove(iteminquiry.ItemInquiryDetails[i]);
                }

                DetailView detailView = Application.CreateDetailView(objectSpace, iteminquiry);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
                e.ActionArguments.ShowViewParameters.CreatedView = detailView;

                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "PrintLabel_ListView")
            {
                IObjectSpace objectSpace = Application.CreateObjectSpace();
                PrintLabel newprintlabel = objectSpace.CreateObject<PrintLabel>();

                DetailView detailView = Application.CreateDetailView(objectSpace, newprintlabel);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
                e.ActionArguments.ShowViewParameters.CreatedView = detailView;

                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "SalesOrderCollectionReport_ListView")
            {
                IObjectSpace objectSpace = Application.CreateObjectSpace();
                SalesOrderCollectionReport newsalescollectionreport = objectSpace.CreateObject<SalesOrderCollectionReport>();

                DetailView detailView = Application.CreateDetailView(objectSpace, newsalescollectionreport);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
                e.ActionArguments.ShowViewParameters.CreatedView = detailView;

                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "StockReorderingReport_ListView")
            {
                IObjectSpace objectSpace = Application.CreateObjectSpace();
                StockReorderingReport newstockreorder = objectSpace.CreateObject<StockReorderingReport>();

                DetailView detailView = Application.CreateDetailView(objectSpace, newstockreorder);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
                e.ActionArguments.ShowViewParameters.CreatedView = detailView;

                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "ItemInquiry_ListView_Report")
            {
                IObjectSpace objectSpace = Application.CreateObjectSpace();
                ItemInquiry newiteminquiry = objectSpace.CreateObject<ItemInquiry>();

                DetailView detailView = Application.CreateDetailView(objectSpace, "ItemInquiry_DetailView_Report", true, newiteminquiry);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                ItemInquiryDefault defaultdata = objectSpace.FindObject<ItemInquiryDefault>(CriteriaOperator.Parse("DocType = ? and IsActive= ?",
                    DocTypeList.Reports, "True"));

                if (defaultdata != null)
                {
                    if (defaultdata.PriceList1 != null)
                    {
                        ((ItemInquiry)detailView.CurrentObject).PriceList1 = ((ItemInquiry)detailView.CurrentObject).Session.GetObjectByKey<vwPriceList>
                            (defaultdata.PriceList1.ListNum);
                    }
                    if (defaultdata.PriceList2 != null)
                    {
                        ((ItemInquiry)detailView.CurrentObject).PriceList2 = ((ItemInquiry)detailView.CurrentObject).Session.GetObjectByKey<vwPriceList>
                            (defaultdata.PriceList2.ListNum);
                    }
                    if (defaultdata.PriceList3 != null)
                    {
                        ((ItemInquiry)detailView.CurrentObject).PriceList3 = ((ItemInquiry)detailView.CurrentObject).Session.GetObjectByKey<vwPriceList>
                            (defaultdata.PriceList3.ListNum);
                    }
                    if (defaultdata.PriceList4 != null)
                    {
                        ((ItemInquiry)detailView.CurrentObject).PriceList4 = ((ItemInquiry)detailView.CurrentObject).Session.GetObjectByKey<vwPriceList>
                            (defaultdata.PriceList4.ListNum);
                    }
                    if (defaultdata.Stock1 != null)
                    {
                        ((ItemInquiry)detailView.CurrentObject).Stock1 = ((ItemInquiry)detailView.CurrentObject).Session.GetObjectByKey<vwWarehouse>
                            (defaultdata.Stock1.WarehouseCode);
                    }
                    if (defaultdata.Stock2 != null)
                    {
                        ((ItemInquiry)detailView.CurrentObject).Stock2 = ((ItemInquiry)detailView.CurrentObject).Session.GetObjectByKey<vwWarehouse>
                            (defaultdata.Stock2.WarehouseCode);
                    }
                    if (defaultdata.Stock3 != null)
                    {
                        ((ItemInquiry)detailView.CurrentObject).Stock3 = ((ItemInquiry)detailView.CurrentObject).Session.GetObjectByKey<vwWarehouse>
                            (defaultdata.Stock3.WarehouseCode);
                    }
                    if (defaultdata.Stock4 != null)
                    {
                        ((ItemInquiry)detailView.CurrentObject).Stock4 = ((ItemInquiry)detailView.CurrentObject).Session.GetObjectByKey<vwWarehouse>
                            (defaultdata.Stock4.WarehouseCode);
                    }
                }

                objectSpace.CommitChanges();
                objectSpace.Refresh();

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            // Start ver 1.0.12
            if (e.ActionArguments.SelectedChoiceActionItem.Id == "StockCountBinInquiry_ListView")
            {
                IObjectSpace objectSpace = Application.CreateObjectSpace();
                StockCountBinInquiry newstockcountbin = objectSpace.CreateObject<StockCountBinInquiry>();

                DetailView detailView = Application.CreateDetailView(objectSpace, "StockCountBinInquiry_DetailView", true, newstockcountbin);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                newstockcountbin.FromDate = DateTime.Today;
                newstockcountbin.ToDate = DateTime.Today;
                newstockcountbin.Round = 1;

                objectSpace.CommitChanges();
                objectSpace.Refresh();

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "StockCountItemInquiry_ListView")
            {
                IObjectSpace objectSpace = Application.CreateObjectSpace();
                StockCountItemInquiry newstockcountitem = objectSpace.CreateObject<StockCountItemInquiry>();

                DetailView detailView = Application.CreateDetailView(objectSpace, "StockCountItemInquiry_DetailView", true, newstockcountitem);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                newstockcountitem.FromDate = DateTime.Today;
                newstockcountitem.ToDate = DateTime.Today;
                newstockcountitem.Round = 1;

                objectSpace.CommitChanges();
                objectSpace.Refresh();

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "StockCountVarianceInquiry_ListView")
            {
                IObjectSpace objectSpace = Application.CreateObjectSpace();
                StockCountVarianceInquiry newstockcountvariance = objectSpace.CreateObject<StockCountVarianceInquiry>();

                DetailView detailView = Application.CreateDetailView(objectSpace, "StockCountVarianceInquiry_DetailView", true, newstockcountvariance);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                newstockcountvariance.StockCountDate = DateTime.Today;

                objectSpace.CommitChanges();
                objectSpace.Refresh();

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }
            // End ver 1.0.12

            // Start ver 1.0.13
            if (e.ActionArguments.SelectedChoiceActionItem.Id == "DailyDeliveryOrders_ListView")
            {
                IObjectSpace objectSpace = Application.CreateObjectSpace();
                DailyDeliveryOrders newdaily = objectSpace.CreateObject<DailyDeliveryOrders>();

                DetailView detailView = Application.CreateDetailView(objectSpace, "DailyDeliveryOrders_DetailView", true, newdaily);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                newdaily.DateFr = DateTime.Today;
                newdaily.DateTo = DateTime.Today;

                objectSpace.CommitChanges();
                objectSpace.Refresh();

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }
            // End ver 1.0.13

            // Start ver 1.0.14
            if (e.ActionArguments.SelectedChoiceActionItem.Id == "StockMovement_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(StockMovement));
                StockMovement list = nonPersistentOS.CreateObject<StockMovement>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }
            // End ver 1.0.14

            // Start ver 1.0.15
            if (e.ActionArguments.SelectedChoiceActionItem.Id == "SalesQuotationInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(SalesQuotationInquiry));
                SalesQuotationInquiry list = nonPersistentOS.CreateObject<SalesQuotationInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "ARDownpaymentInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(ARDownpaymentInquiry));
                ARDownpaymentInquiry list = nonPersistentOS.CreateObject<ARDownpaymentInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "PickListDetailsInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(PickListDetailsInquiry));
                PickListDetailsInquiry list = nonPersistentOS.CreateObject<PickListDetailsInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "BundleIDInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(BundleIDInquiry));
                BundleIDInquiry list = nonPersistentOS.CreateObject<BundleIDInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "PackListInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(PackListInquiry));
                PackListInquiry list = nonPersistentOS.CreateObject<PackListInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "LoadingInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(LoadingInquiry));
                LoadingInquiry list = nonPersistentOS.CreateObject<LoadingInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "DeliveryInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(DeliveryInquiry));
                DeliveryInquiry list = nonPersistentOS.CreateObject<DeliveryInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "InvoiceInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(InvoiceInquiry));
                InvoiceInquiry list = nonPersistentOS.CreateObject<InvoiceInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "PurchaseOrderInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(PurchaseOrderInquiry));
                PurchaseOrderInquiry list = nonPersistentOS.CreateObject<PurchaseOrderInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "GRPOInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(GRPOInquiry));
                GRPOInquiry list = nonPersistentOS.CreateObject<GRPOInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "ASNInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(ASNInquiry));
                ASNInquiry list = nonPersistentOS.CreateObject<ASNInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "PurchaseReturnInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(PurchaseReturnInquiry));
                PurchaseReturnInquiry list = nonPersistentOS.CreateObject<PurchaseReturnInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "SalesReturnRequestInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(SalesReturnRequestInquiry));
                SalesReturnRequestInquiry list = nonPersistentOS.CreateObject<SalesReturnRequestInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "CreditMemoInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(CreditMemoInquiry));
                CreditMemoInquiry list = nonPersistentOS.CreateObject<CreditMemoInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "WarehouseTransferDetailsInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(WarehouseTransferDetailsInquiry));
                WarehouseTransferDetailsInquiry list = nonPersistentOS.CreateObject<WarehouseTransferDetailsInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }

            if (e.ActionArguments.SelectedChoiceActionItem.Id == "StockAdjustmentDetailsInquiry_ListView")
            {
                XPObjectSpace persistentObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
                var nonPersistentOS = Application.CreateObjectSpace(typeof(StockAdjustmentDetailsInquiry));
                StockAdjustmentDetailsInquiry list = nonPersistentOS.CreateObject<StockAdjustmentDetailsInquiry>();

                DetailView detailView = Application.CreateDetailView(nonPersistentOS, list);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }
            // End ver 1.0.15

            // Start ver 1.0.22
            if (e.ActionArguments.SelectedChoiceActionItem.Id == "GlobalItemInquiry_ListView")
            {
                IObjectSpace objectSpace = Application.CreateObjectSpace();
                GlobalItemInquiry newglobaliteminquiry = objectSpace.CreateObject<GlobalItemInquiry>();

                DetailView detailView = Application.CreateDetailView(objectSpace, "GlobalItemInquiry_DetailView", true, newglobaliteminquiry);
                detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

                objectSpace.CommitChanges();
                objectSpace.Refresh();

                e.ActionArguments.ShowViewParameters.CreatedView = detailView;
                e.Handled = true;
            }
            // End ver 1.0.22

            // Start ver 1.0.15
            if (DateTime.Now.Minute.ToString("00").Substring(1, 1) == "0" ||
                DateTime.Now.Minute.ToString("00").Substring(1, 1) == "5")
            {
                MemoryManagement.FlushGCMemory();
            }
            // End ver 1.0.15
        }

        // Start ver 1.0.17
        private void PopupWindowManager_PopupShowing(object sender, PopupShowingEventArgs e)
        {
            e.PopupControl.CustomizePopupWindowSize += XafPopupWindowControl_CustomizePopupWindowSize;
        }

        private void XafPopupWindowControl_CustomizePopupWindowSize(object sender, DevExpress.ExpressApp.Web.Controls.CustomizePopupWindowSizeEventArgs e)
        {
            // only loopup listview
            if (e.ShowViewSource.SourceView == null)
            {
                e.Width = Unit.Percentage(70);
                e.Height = Unit.Percentage(80);
                e.Handled = true;
            }
        }
        // End ver 1.0.17
    }
}
