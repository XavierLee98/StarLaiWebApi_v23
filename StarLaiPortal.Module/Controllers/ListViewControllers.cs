using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.ExpressApp.Web.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Utils;
using DevExpress.Web;
using DevExpress.Xpo;
using DevExpress.Xpo.Helpers;
using DevExpress.XtraGrid.Columns;
using StarLaiPortal.Module.BusinessObjects.Advanced_Shipment_Notice;
using StarLaiPortal.Module.BusinessObjects.Credit_Notes_Cancellation;
using StarLaiPortal.Module.BusinessObjects.Dashboard;
using StarLaiPortal.Module.BusinessObjects.Delivery_Order;
using StarLaiPortal.Module.BusinessObjects.GRN;
using StarLaiPortal.Module.BusinessObjects.Inquiry_View;
using StarLaiPortal.Module.BusinessObjects.Item_Inquiry;
using StarLaiPortal.Module.BusinessObjects.Load;
using StarLaiPortal.Module.BusinessObjects.Pack_List;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using StarLaiPortal.Module.BusinessObjects.Purchase_Order;
using StarLaiPortal.Module.BusinessObjects.Purchase_Return;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;
using StarLaiPortal.Module.BusinessObjects.Sales_Order_Collection;
using StarLaiPortal.Module.BusinessObjects.Sales_Quotation;
using StarLaiPortal.Module.BusinessObjects.Sales_Refund;
using StarLaiPortal.Module.BusinessObjects.Sales_Return;
using StarLaiPortal.Module.BusinessObjects.Stock_Adjustment;
using StarLaiPortal.Module.BusinessObjects.Stock_Count;
using StarLaiPortal.Module.BusinessObjects.Warehouse_Transfer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using DevExpress.Xpo.DB.Helpers;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using StarLaiPortal.Module.BusinessObjects.Container_Tracking;
using DevExpress.Persistent.BaseImpl;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Reports;
using StarLaiPortal.Module.BusinessObjects.Print_Module;
using StarLaiPortal.Module.BusinessObjects.Stock_Count_Inquiry;

// 2023-07-28 add AR Downpayment cancalletion ver 1.0.7
// 2023-09-11 add dashboard sales/purchase/warehouse ver 1.0.9
// 2023-09-19 add disable detail view ver 1.0.9
// 2023-10-20 add stock count ver 1.0.11
// 2025-09-11 Hide Export by role ver 1.0.25
// 2026-03-02 Hide Export by different role group by module ver 1.0.27

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class ListViewControllers : ViewController<ListView>
    {
        private ListViewProcessCurrentObjectController processCurrentObjectController;
        ListViewController listViewController;
        public ListViewControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            if (View.ObjectTypeInfo.Type == typeof(Dashboards))
            {
                if (View.Id == "Dashboards_ListView")
                {
                    processCurrentObjectController = Frame.GetController<ListViewProcessCurrentObjectController>();
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem +=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }

            // Start ver 1.0.9
            if (View.ObjectTypeInfo.Type == typeof(DashboardsSales))
            {
                if (View.Id == "DashboardsSales_ListView")
                {
                    processCurrentObjectController = Frame.GetController<ListViewProcessCurrentObjectController>();
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem +=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(DashboardsPurchase))
            {
                if (View.Id == "DashboardsPurchase_ListView")
                {
                    processCurrentObjectController = Frame.GetController<ListViewProcessCurrentObjectController>();
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem +=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(DashboardsWarehouse))
            {
                if (View.Id == "DashboardsWarehouse_ListView")
                {
                    processCurrentObjectController = Frame.GetController<ListViewProcessCurrentObjectController>();
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem +=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(vwInquirySalesOrder))
            {
                if (View.Id == "vwInquirySalesOrder_ListView")
                {
                    processCurrentObjectController = Frame.GetController<ListViewProcessCurrentObjectController>();
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem +=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(vwInquiryPickList))
            {
                if (View.Id == "vwInquiryPickList_ListView")
                {
                    processCurrentObjectController = Frame.GetController<ListViewProcessCurrentObjectController>();
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem +=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }
            // End ver 1.0.9

            if (View.ObjectTypeInfo.Type == typeof(SalesQuotation) || View.ObjectTypeInfo.Type == typeof(SalesOrder) ||
                View.ObjectTypeInfo.Type == typeof(SalesOrderCollection) || View.ObjectTypeInfo.Type == typeof(PickList) ||
                 View.ObjectTypeInfo.Type == typeof(PackList) ||
                 View.ObjectTypeInfo.Type == typeof(Load) || View.ObjectTypeInfo.Type == typeof(PurchaseOrders) ||
                 View.ObjectTypeInfo.Type == typeof(ASN) || View.ObjectTypeInfo.Type == typeof(GRN) ||
                 View.ObjectTypeInfo.Type == typeof(PurchaseReturnRequests) || View.ObjectTypeInfo.Type == typeof(SalesReturnRequests) ||
                 View.ObjectTypeInfo.Type == typeof(SalesReturns) || View.ObjectTypeInfo.Type == typeof(PurchaseReturns) ||
                 View.ObjectTypeInfo.Type == typeof(WarehouseTransferReq) || View.ObjectTypeInfo.Type == typeof(WarehouseTransfers) ||
                 View.ObjectTypeInfo.Type == typeof(StockAdjustmentRequests) || View.ObjectTypeInfo.Type == typeof(StockAdjustments) ||
                 View.ObjectTypeInfo.Type == typeof(SalesRefundRequests) || View.ObjectTypeInfo.Type == typeof(SalesRefunds) ||
                 View.ObjectTypeInfo.Type == typeof(DeliveryOrder) ||
                 // Start ver 1.0.7
                 View.ObjectTypeInfo.Type == typeof(ARDownpaymentCancel) ||
                 // End ver 1.0.7
                 // Start ver 1.0.12
                 View.ObjectTypeInfo.Type == typeof(StockCountSheet) ||
                 View.ObjectTypeInfo.Type == typeof(StockCountConfirm) ||
                 // Start ver 1.0.12
                 // Start ver 1.0.25
                 View.ObjectTypeInfo.Type == typeof(ContainerTracking)
                 // End ver 1.0.25
                 )
            {
                listViewController = Frame.GetController<ListViewController>();
                if (listViewController != null)
                {
                    listViewController.EditAction.Active["123"] = false;
                }
            }

            // Start ver 1.0.15
            //if (DateTime.Now.Minute.ToString("00").Substring(1, 1) == "0" ||
            //    DateTime.Now.Minute.ToString("00").Substring(1, 1) == "3" ||
            //    DateTime.Now.Minute.ToString("00").Substring(1, 1) == "6")
            //if (DateTime.Now.Minute.ToString("00").Substring(1, 1) == "0" || 
            //    DateTime.Now.Minute.ToString("00").Substring(1, 1) == "5")
            //{
            bool upd = false;
            SqlConnection conn = new SqlConnection(getConnectionString());
            string getRMBool = "SELECT ReleaseMemory FROM [" + ConfigurationManager.AppSettings.Get("CommonTable").ToString() + "]..ODBC WHERE " +
                "DBName = '" + conn.Database + "'";
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn.Open();
            SqlCommand cmd = new SqlCommand(getRMBool, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetBoolean(0) == false)
                {
                    MemoryManagement.FlushGCMemory();
                    upd = true;

                }
            }
            cmd.Dispose();
            conn.Close();

            if (upd == true)
            {
                SqlCommand TransactionNotification = new SqlCommand("", conn);
                TransactionNotification.CommandTimeout = 600;

                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                conn.Open();

                TransactionNotification.CommandText = "UPDATE [" + ConfigurationManager.AppSettings.Get("CommonTable").ToString() + "]..ODBC " +
                    "SET ReleaseMemory = 1 WHERE DBName = '" + conn.Database + "'";

                SqlDataReader UpdReader = TransactionNotification.ExecuteReader();

                TransactionNotification.Dispose();
                conn.Close();
            }
            //}
            // End ver 1.0.15

            //// Start ver 1.0.25
            //ExportController exportController = Frame.GetController<ExportController>();
            //if (exportController != null)
            //{
            //    PermissionPolicyRole exportdaterole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportDataRole')"));

            //    if (exportdaterole == null)
            //    {
            //        // Deactivate the ExportAction
            //        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
            //    }    
            //}
            //// End ver 1.0.25

            // Start ver 1.0.27
            ExportController exportController = Frame.GetController<ExportController>();
            if (exportController != null)
            {
                // Dashboard
                if (View.ObjectTypeInfo.Type == typeof(Dashboards) || View.ObjectTypeInfo.Type == typeof(DashboardsSales) || 
                    View.ObjectTypeInfo.Type == typeof(DashboardsPurchase) || View.ObjectTypeInfo.Type == typeof(DashboardsWarehouse))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportDashboard')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Reports
                if (View.ObjectTypeInfo.Type == typeof(ReportDataV2) || View.ObjectTypeInfo.Type == typeof(vwInquiryStockBalance) ||
                    View.ObjectTypeInfo.Type == typeof(vwInquiryStockBalancebyWhs) || View.ObjectTypeInfo.Type == typeof(vwInquiryStockMovement) ||
                    View.ObjectTypeInfo.Type == typeof(StockMovementResult) || View.ObjectTypeInfo.Type == typeof(StockReorderingReport) ||
                    View.ObjectTypeInfo.Type == typeof(ItemInquiryDetails) || View.ObjectTypeInfo.Type == typeof(vwInquiryCurrentARAging) ||
                    View.ObjectTypeInfo.Type == typeof(vwInquiryCustomer) || View.ObjectTypeInfo.Type == typeof(vwInquiryOrderTemplateMapping) ||
                    View.ObjectTypeInfo.Type == typeof(vwInquiryBusinessPartner) || View.ObjectTypeInfo.Type == typeof(GlobalItemInquiryDetails) ||
                    View.ObjectTypeInfo.Type == typeof(ItemBinInquiryResult) || View.ObjectTypeInfo.Type == typeof(vwInquiryStockBalanceTransfer) ||
                    View.ObjectTypeInfo.Type == typeof(vwInquiryStockPendingPost))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportReports')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Print Module
                if (View.ObjectTypeInfo.Type == typeof(PrintLabelDetails))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportPrintModule')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Sales Quotation
                if (View.ObjectTypeInfo.Type == typeof(SalesQuotation) || View.ObjectTypeInfo.Type == typeof(SalesQuotationDetails) ||
                    View.ObjectTypeInfo.Type == typeof(SalesQuotationAppStatus) || View.ObjectTypeInfo.Type == typeof(SalesQuotationAppStage) ||
                    View.ObjectTypeInfo.Type == typeof(SalesQuotationDocTrail) || View.ObjectTypeInfo.Type == typeof(SalesQuotationAttachment) ||
                    View.ObjectTypeInfo.Type == typeof(vwInquirySalesQuotation) || View.ObjectTypeInfo.Type == typeof(SalesQuotationInquiryResult) ||
                    View.ObjectTypeInfo.Type == typeof(vwInquirySalesQuotationDetails))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportSQ')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Sales Order
                if (View.ObjectTypeInfo.Type == typeof(SalesOrder) || View.ObjectTypeInfo.Type == typeof(SalesOrderDetails) || 
                    View.ObjectTypeInfo.Type == typeof(SalesOrderDocStatus) || View.ObjectTypeInfo.Type == typeof(SalesOrderCollection) || 
                    View.ObjectTypeInfo.Type == typeof(SalesOrderCollectionDetails) || View.ObjectTypeInfo.Type == typeof(SalesOrderCollectionReturn) || 
                    View.ObjectTypeInfo.Type == typeof(SalesOrderCollectionDocStatus) || View.ObjectTypeInfo.Type == typeof(vwInquirySalesOrder) || 
                    View.ObjectTypeInfo.Type == typeof(vwInquiryARDownpayment) || View.ObjectTypeInfo.Type == typeof(ARDownpaymentInquiryResult) ||
                    View.ObjectTypeInfo.Type == typeof(vwInquirySalesOrderDetails))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportSO')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Manual Credit Note Request
                if (View.ObjectTypeInfo.Type == typeof(SalesRefundRequests) || View.ObjectTypeInfo.Type == typeof(SalesRefundReqAppStage) || 
                    View.ObjectTypeInfo.Type == typeof(SalesRefundReqAppStatus) || View.ObjectTypeInfo.Type == typeof(SalesRefundReqDetails) || 
                    View.ObjectTypeInfo.Type == typeof(SalesRefundReqDocTrail))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportCN')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }
                
                // AR Downpayment Cancellation
                if (View.ObjectTypeInfo.Type == typeof(ARDownpaymentCancel) || View.ObjectTypeInfo.Type == typeof(ARDownpaymentCancelDetails) || 
                    View.ObjectTypeInfo.Type == typeof(ARDownpaymentCancellationAppStage) || View.ObjectTypeInfo.Type == typeof(ARDownpaymentCancellationAppStatus) || 
                    View.ObjectTypeInfo.Type == typeof(ARDownpaymentCancellationDocTrail))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportARCNCL')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Pick List
                if (View.ObjectTypeInfo.Type == typeof(PickList) || View.ObjectTypeInfo.Type == typeof(PickListDetails) || 
                    View.ObjectTypeInfo.Type == typeof(PickListDetailsActual) || View.ObjectTypeInfo.Type == typeof(PickListDocTrail) || 
                    View.ObjectTypeInfo.Type == typeof(PickListAttachment) || View.ObjectTypeInfo.Type == typeof(vwInquiryOpenPickList) || 
                    View.ObjectTypeInfo.Type == typeof(vwInquiryPickList) || View.ObjectTypeInfo.Type == typeof(vwInquiryPickListDetails) || 
                    View.ObjectTypeInfo.Type == typeof(PickListDetailsInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportPickList')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Pack List
                if (View.ObjectTypeInfo.Type == typeof(PackList) || View.ObjectTypeInfo.Type == typeof(PackListDetails) ||
                   View.ObjectTypeInfo.Type == typeof(PackListDocTrail) || View.ObjectTypeInfo.Type == typeof(vwInquiryBundleID) ||
                   View.ObjectTypeInfo.Type == typeof(BundleIDInquiryResult) || View.ObjectTypeInfo.Type == typeof(vwInquiryPackList) ||
                   View.ObjectTypeInfo.Type == typeof(PackListInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportPackList')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Loading
                if (View.ObjectTypeInfo.Type == typeof(Load) || View.ObjectTypeInfo.Type == typeof(LoadDetails) ||
                View.ObjectTypeInfo.Type == typeof(LoadDocTrail) || View.ObjectTypeInfo.Type == typeof(vwInquiryLoadingDetails) ||
                View.ObjectTypeInfo.Type == typeof(vwInquiryLoading) || View.ObjectTypeInfo.Type == typeof(LoadingInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportLoading')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Delivery Order
                if (View.ObjectTypeInfo.Type == typeof(DeliveryOrder) || View.ObjectTypeInfo.Type == typeof(DeliveryOrderDetails) || 
                    View.ObjectTypeInfo.Type == typeof(DeliveryOrderDocTrail) || View.ObjectTypeInfo.Type == typeof(vwInquiryDelivery) || 
                    View.ObjectTypeInfo.Type == typeof(DeliveryInquiryResult) || View.ObjectTypeInfo.Type == typeof(vwInquiryInvoice) || 
                    View.ObjectTypeInfo.Type == typeof(InvoiceInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportDO')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Purchase Order
                if (View.ObjectTypeInfo.Type == typeof(PurchaseOrders) || View.ObjectTypeInfo.Type == typeof(PurchaseOrderDetails) || 
                    View.ObjectTypeInfo.Type == typeof(PurchaseOrderAppStage) || View.ObjectTypeInfo.Type == typeof(PurchaseOrderAppStatus) || 
                    View.ObjectTypeInfo.Type == typeof(PurchaseOrderAttachment) || View.ObjectTypeInfo.Type == typeof(PurchaseOrderDocTrail) || 
                    View.ObjectTypeInfo.Type == typeof(vwInquiryPurchaseOrders) || View.ObjectTypeInfo.Type == typeof(PurchaseOrderInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportPO')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // GRN
                if (View.ObjectTypeInfo.Type == typeof(GRN) || View.ObjectTypeInfo.Type == typeof(GRNDetails) || 
                    View.ObjectTypeInfo.Type == typeof(GRNDocTrail) || View.ObjectTypeInfo.Type == typeof(vwInquiryGRN) || 
                    View.ObjectTypeInfo.Type == typeof(GRPOInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportGRPO')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // ASN
                if (View.ObjectTypeInfo.Type == typeof(ASN) || View.ObjectTypeInfo.Type == typeof(ASNDetails) || 
                    View.ObjectTypeInfo.Type == typeof(ASNDocTrail) || View.ObjectTypeInfo.Type == typeof(vwInquiryASN) || 
                    View.ObjectTypeInfo.Type == typeof(ASNInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportASN')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Purchase Return
                if (View.ObjectTypeInfo.Type == typeof(PurchaseReturnRequests) || View.ObjectTypeInfo.Type == typeof(PurchaseReturnRequestDetails) || 
                    View.ObjectTypeInfo.Type == typeof(PurchaseReturnRequestDocTrail) || View.ObjectTypeInfo.Type == typeof(PurchaseReturnRequestAppStage) || 
                    View.ObjectTypeInfo.Type == typeof(PurchaseReturnRequestAppStatus) || View.ObjectTypeInfo.Type == typeof(PurchaseReturns) || 
                    View.ObjectTypeInfo.Type == typeof(PurchaseReturnDetails) || View.ObjectTypeInfo.Type == typeof(PurchaseReturnDocTrail) || 
                    View.ObjectTypeInfo.Type == typeof(vwInquiryPurchaseReturn) || View.ObjectTypeInfo.Type == typeof(PurchaseReturnInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportPurchaseReturn')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Sales Return
                if (View.ObjectTypeInfo.Type == typeof(SalesReturnRequests) || View.ObjectTypeInfo.Type == typeof(SalesReturnRequestDetails) || 
                    View.ObjectTypeInfo.Type == typeof(SalesReturnReqAttachment) || View.ObjectTypeInfo.Type == typeof(SalesReturnRequestAppStage) ||
                    View.ObjectTypeInfo.Type == typeof(SalesReturnRequestAppStatus) || View.ObjectTypeInfo.Type == typeof(SalesReturnRequestDocTrail) || 
                    View.ObjectTypeInfo.Type == typeof(SalesReturns) || View.ObjectTypeInfo.Type == typeof(SalesReturnDetails) ||
                    View.ObjectTypeInfo.Type == typeof(SalesReturnDocTrail) || View.ObjectTypeInfo.Type == typeof(vwInquirySalesReturnRequestDetails) || 
                    View.ObjectTypeInfo.Type == typeof(vwInquirySalesReturnRequestDetails) || View.ObjectTypeInfo.Type == typeof(SalesReturnRequestInquiryResult) || 
                    View.ObjectTypeInfo.Type == typeof(vwInquiryCreditMemo) || View.ObjectTypeInfo.Type == typeof(CreditMemoInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportSalesReturn')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Warehouse Transfer
                if (View.ObjectTypeInfo.Type == typeof(WarehouseTransferReq) || View.ObjectTypeInfo.Type == typeof(WarehouseTransferReqDetails) ||
                   View.ObjectTypeInfo.Type == typeof(WarehouseTransferReqAppStage) || View.ObjectTypeInfo.Type == typeof(WarehouseTransferReqAppStatus) ||
                   View.ObjectTypeInfo.Type == typeof(WarehouseTransferReqAttachment) || View.ObjectTypeInfo.Type == typeof(WarehouseTransferReqDocTrail) ||
                   View.ObjectTypeInfo.Type == typeof(WarehouseTransfers) || View.ObjectTypeInfo.Type == typeof(WarehouseTransferDetails) ||
                   View.ObjectTypeInfo.Type == typeof(WarehouseTransfersDocTrail) || View.ObjectTypeInfo.Type == typeof(WarehouseTransferAttachment) ||
                   View.ObjectTypeInfo.Type == typeof(vwInquiryWarehouseTransferDetails) || View.ObjectTypeInfo.Type == typeof(WarehouseTransferDetailsInquiryResult) ||
                   View.ObjectTypeInfo.Type == typeof(vwInquiryStockReordering) || View.ObjectTypeInfo.Type == typeof(vwInquiryBundleTransferDetails) ||
                   View.ObjectTypeInfo.Type == typeof(vwInquiryOpenBundleTransferDetails))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportWhsTransfer')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Stock Adjustment
                if (View.ObjectTypeInfo.Type == typeof(StockAdjustmentRequests) || View.ObjectTypeInfo.Type == typeof(StockAdjustmentReqDetails) ||
                   View.ObjectTypeInfo.Type == typeof(StockAdjustmentReqAppStage) || View.ObjectTypeInfo.Type == typeof(StockAdjustmentReqAppStatus) ||
                   View.ObjectTypeInfo.Type == typeof(StockAdjustmentReqAttachment) || View.ObjectTypeInfo.Type == typeof(StockAdjustmentReqDocTrail) ||
                   View.ObjectTypeInfo.Type == typeof(StockAdjustments) || View.ObjectTypeInfo.Type == typeof(StockAdjustmentDetails) ||
                   View.ObjectTypeInfo.Type == typeof(StockAdjustmentDocTrail) || View.ObjectTypeInfo.Type == typeof(StockAdjustmentAttactment) ||
                   View.ObjectTypeInfo.Type == typeof(vwInquiryStockAdjustmentDetails) || View.ObjectTypeInfo.Type == typeof(StockAdjustmentDetailsInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportStockAdj')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Stock Count
                if (View.ObjectTypeInfo.Type == typeof(StockCountSheet) || View.ObjectTypeInfo.Type == typeof(StockCountSheetCounted) ||
                  View.ObjectTypeInfo.Type == typeof(StockCountSheetTarget) || View.ObjectTypeInfo.Type == typeof(StockCountSheetDocTrail) ||
                  View.ObjectTypeInfo.Type == typeof(StockCountConfirm) || View.ObjectTypeInfo.Type == typeof(StockCountConfirmDetails) ||
                  View.ObjectTypeInfo.Type == typeof(StockCountConfirmDocTrail) || View.ObjectTypeInfo.Type == typeof(StockCountBinInquiryDetails) ||
                  View.ObjectTypeInfo.Type == typeof(StockCountVarianceInquiryDetails) || View.ObjectTypeInfo.Type == typeof(StockCountItemInquiryDetails))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportStockCount')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }

                // Container Tracking
                if (View.ObjectTypeInfo.Type == typeof(ContainerTracking) || View.ObjectTypeInfo.Type == typeof(ContainerTrackingInquiryResult))
                {
                    PermissionPolicyRole exportrole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ExportContainerTracking')"));
                    if (exportrole == null)
                    {
                        // Deactivate the ExportAction
                        exportController.ExportAction.Active.SetItemValue("HideExportButtonInListView", false);
                    }
                }
            }
            // End ver 1.0.27

        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
            ASPxGridListEditor gridListEditor = View.Editor as ASPxGridListEditor;
            if (gridListEditor != null)
            {
                ASPxGridView gridView = gridListEditor.Grid;
                //gridView.Settings.UseFixedTableLayout = true;
                //gridView.Width = Unit.Percentage(128);
                //gridView.SettingsResizing.ColumnResizeMode = ColumnResizeMode.Control;
                gridListEditor.Grid.Styles.Cell.Wrap = DevExpress.Utils.DefaultBoolean.False;
            }
        }
        protected override void OnDeactivated()
        {
            if (View.ObjectTypeInfo.Type == typeof(Dashboards))
            {
                if (View.Id == "Dashboards_ListView")
                {
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem -=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }

            // Start ver 1.0.9
            if (View.ObjectTypeInfo.Type == typeof(DashboardsSales))
            {
                if (View.Id == "DashboardsSales_ListView")
                {
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem -=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(DashboardsPurchase))
            {
                if (View.Id == "DashboardsPurchase_ListView")
                {
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem -=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(DashboardsWarehouse))
            {
                if (View.Id == "DashboardsWarehouse_ListView")
                {
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem -=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(vwInquirySalesOrder))
            {
                if (View.Id == "vwInquirySalesOrder_ListView")
                {
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem -=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(vwInquiryPickList))
            {
                if (View.Id == "vwInquiryPickList_ListView")
                {
                    if (processCurrentObjectController != null)
                    {
                        processCurrentObjectController.CustomProcessSelectedItem -=
                            processCurrentObjectController_CustomProcessSelectedItem;
                    }
                }
            }
            // End ver 1.0.9

            if (View.ObjectTypeInfo.Type == typeof(SalesQuotation) || View.ObjectTypeInfo.Type == typeof(SalesOrder) ||
                View.ObjectTypeInfo.Type == typeof(SalesOrderCollection) || View.ObjectTypeInfo.Type == typeof(PickList) ||
                View.ObjectTypeInfo.Type == typeof(PackList) ||
                 View.ObjectTypeInfo.Type == typeof(Load) || View.ObjectTypeInfo.Type == typeof(PurchaseOrders) ||
                 View.ObjectTypeInfo.Type == typeof(ASN) || View.ObjectTypeInfo.Type == typeof(GRN) ||
                 View.ObjectTypeInfo.Type == typeof(PurchaseReturnRequests) || View.ObjectTypeInfo.Type == typeof(SalesReturnRequests) ||
                 View.ObjectTypeInfo.Type == typeof(SalesReturns) || View.ObjectTypeInfo.Type == typeof(PurchaseReturns) ||
                 View.ObjectTypeInfo.Type == typeof(WarehouseTransferReq) || View.ObjectTypeInfo.Type == typeof(WarehouseTransfers) ||
                 View.ObjectTypeInfo.Type == typeof(StockAdjustmentRequests) || View.ObjectTypeInfo.Type == typeof(StockAdjustments) ||
                 View.ObjectTypeInfo.Type == typeof(SalesRefundRequests) || View.ObjectTypeInfo.Type == typeof(SalesRefunds) ||
                 View.ObjectTypeInfo.Type == typeof(DeliveryOrder) ||
                 // Start ver 1.0.7
                 View.ObjectTypeInfo.Type == typeof(ARDownpaymentCancel) ||
                 // End ver 1.0.7
                 // Start ver 1.0.12
                 View.ObjectTypeInfo.Type == typeof(StockCountSheet) ||
                 View.ObjectTypeInfo.Type == typeof(StockCountConfirm)
                 // Start ver 1.0.12
                 )
            {
                if (listViewController != null)
                {
                    listViewController.EditAction.Active.RemoveItem("123");
                    listViewController = null;
                }
            }

            //// Start ver 1.0.25
            //ExportController exportController = Frame.GetController<ExportController>();
            //if (exportController != null)
            //{
            //    exportController.ExportAction.Active.RemoveItem("HideExportButtonInListView");
            //}
            //// End ver 1.0.25

            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        private void processCurrentObjectController_CustomProcessSelectedItem(
            object sender, CustomProcessListViewSelectedItemEventArgs e)
        {
            e.Handled = true;
        }

        public string getConnectionString()
        {
            string connectionString = "";

            ConnectionStringParser helper = new ConnectionStringParser(Application.ConnectionString);
            helper.RemovePartByName("xpodatastorepool");
            connectionString = string.Format(helper.GetConnectionString());

            return connectionString;
        }
    }
}
