namespace StarLaiPortal.Module.Controllers
{
    partial class InquiryViewControllers
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ViewOpenPickList = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ViewPickListDetailInquiry = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ViewPickListInquiry = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.InquiryStatus = new DevExpress.ExpressApp.Actions.SingleChoiceAction(this.components);
            this.InquiryDateFrom = new DevExpress.ExpressApp.Actions.ParametrizedAction(this.components);
            this.InquiryDateTo = new DevExpress.ExpressApp.Actions.ParametrizedAction(this.components);
            this.InquiryFilter = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.ViewSalesOrderInquiry = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.StockMovementSPSearch = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.InquirySearch = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.PrintDOInquiry = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.PreviewInvInquiry = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.PreviewSOInquiry = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.PrintBundleInquiry = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            // 
            // ViewOpenPickList
            // 
            this.ViewOpenPickList.AcceptButtonCaption = null;
            this.ViewOpenPickList.CancelButtonCaption = null;
            this.ViewOpenPickList.Caption = "View";
            this.ViewOpenPickList.Category = "ListView";
            this.ViewOpenPickList.ConfirmationMessage = null;
            this.ViewOpenPickList.Id = "ViewOpenPickList";
            this.ViewOpenPickList.ToolTip = null;
            this.ViewOpenPickList.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ViewOpenPickList_CustomizePopupWindowParams);
            this.ViewOpenPickList.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ViewOpenPickList_Execute);
            // 
            // ViewPickListDetailInquiry
            // 
            this.ViewPickListDetailInquiry.AcceptButtonCaption = null;
            this.ViewPickListDetailInquiry.CancelButtonCaption = null;
            this.ViewPickListDetailInquiry.Caption = "View";
            this.ViewPickListDetailInquiry.Category = "ListView";
            this.ViewPickListDetailInquiry.ConfirmationMessage = null;
            this.ViewPickListDetailInquiry.Id = "ViewPickListDetailInquiry";
            this.ViewPickListDetailInquiry.ToolTip = null;
            this.ViewPickListDetailInquiry.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ViewPickListDetailInquiry_CustomizePopupWindowParams);
            this.ViewPickListDetailInquiry.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ViewPickListDetailInquiry_Execute);
            // 
            // ViewPickListInquiry
            // 
            this.ViewPickListInquiry.AcceptButtonCaption = null;
            this.ViewPickListInquiry.CancelButtonCaption = null;
            this.ViewPickListInquiry.Caption = "View";
            this.ViewPickListInquiry.Category = "ListView";
            this.ViewPickListInquiry.ConfirmationMessage = null;
            this.ViewPickListInquiry.Id = "ViewPickListInquiry";
            this.ViewPickListInquiry.ToolTip = null;
            this.ViewPickListInquiry.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ViewPickListInquiry_CustomizePopupWindowParams);
            this.ViewPickListInquiry.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ViewPickListInquiry_Execute);
            // 
            // InquiryStatus
            // 
            this.InquiryStatus.Caption = "Status";
            this.InquiryStatus.Category = "ObjectsCreation";
            this.InquiryStatus.ConfirmationMessage = null;
            this.InquiryStatus.Id = "InquiryStatus";
            this.InquiryStatus.ToolTip = null;
            this.InquiryStatus.Execute += new DevExpress.ExpressApp.Actions.SingleChoiceActionExecuteEventHandler(this.InquiryStatus_Execute);
            // 
            // InquiryDateFrom
            // 
            this.InquiryDateFrom.Caption = "From";
            this.InquiryDateFrom.Category = "ObjectsCreation";
            this.InquiryDateFrom.ConfirmationMessage = null;
            this.InquiryDateFrom.Id = "InquiryDateFrom";
            this.InquiryDateFrom.NullValuePrompt = null;
            this.InquiryDateFrom.ShortCaption = null;
            this.InquiryDateFrom.ToolTip = null;
            this.InquiryDateFrom.ValueType = typeof(System.DateTime);
            this.InquiryDateFrom.Execute += new DevExpress.ExpressApp.Actions.ParametrizedActionExecuteEventHandler(this.InquiryDateFrom_Execute);
            // 
            // InquiryDateTo
            // 
            this.InquiryDateTo.Caption = "To";
            this.InquiryDateTo.Category = "ObjectsCreation";
            this.InquiryDateTo.ConfirmationMessage = null;
            this.InquiryDateTo.Id = "InquiryDateTo";
            this.InquiryDateTo.NullValuePrompt = null;
            this.InquiryDateTo.ShortCaption = null;
            this.InquiryDateTo.ToolTip = null;
            this.InquiryDateTo.ValueType = typeof(System.DateTime);
            this.InquiryDateTo.Execute += new DevExpress.ExpressApp.Actions.ParametrizedActionExecuteEventHandler(this.InquiryDateTo_Execute);
            // 
            // InquiryFilter
            // 
            this.InquiryFilter.Caption = "Filter";
            this.InquiryFilter.Category = "ObjectsCreation";
            this.InquiryFilter.ConfirmationMessage = null;
            this.InquiryFilter.Id = "InquiryFilter";
            this.InquiryFilter.ToolTip = null;
            this.InquiryFilter.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.InquiryFilter_Execute);
            // 
            // ViewSalesOrderInquiry
            // 
            this.ViewSalesOrderInquiry.AcceptButtonCaption = null;
            this.ViewSalesOrderInquiry.CancelButtonCaption = null;
            this.ViewSalesOrderInquiry.Caption = "View";
            this.ViewSalesOrderInquiry.Category = "ListView";
            this.ViewSalesOrderInquiry.ConfirmationMessage = null;
            this.ViewSalesOrderInquiry.Id = "ViewSalesOrderInquiry";
            this.ViewSalesOrderInquiry.ToolTip = null;
            this.ViewSalesOrderInquiry.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ViewSalesOrderInquiry_CustomizePopupWindowParams);
            this.ViewSalesOrderInquiry.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ViewSalesOrderInquiry_Execute);
            // 
            // StockMovementSPSearch
            // 
            this.StockMovementSPSearch.Caption = "Search";
            this.StockMovementSPSearch.Category = "ObjectsCreation";
            this.StockMovementSPSearch.ConfirmationMessage = null;
            this.StockMovementSPSearch.Id = "StockMovementSPSearch";
            this.StockMovementSPSearch.ToolTip = null;
            this.StockMovementSPSearch.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.StockMovementSPSearch_Execute);
            // 
            // InquirySearch
            // 
            this.InquirySearch.Caption = "Search";
            this.InquirySearch.Category = "ObjectsCreation";
            this.InquirySearch.ConfirmationMessage = null;
            this.InquirySearch.Id = "InquirySearch";
            this.InquirySearch.ToolTip = null;
            this.InquirySearch.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.InquirySearch_Execute);
            // 
            // PrintDOInquiry
            // 
            this.PrintDOInquiry.Caption = "View DO";
            this.PrintDOInquiry.Category = "ObjectsCreation";
            this.PrintDOInquiry.ConfirmationMessage = null;
            this.PrintDOInquiry.Id = "PrintDOInquiry";
            this.PrintDOInquiry.ToolTip = null;
            this.PrintDOInquiry.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.PrintDOInquiry_Execute);
            // 
            // PreviewInvInquiry
            // 
            this.PreviewInvInquiry.Caption = "Print Invoice";
            this.PreviewInvInquiry.Category = "ObjectsCreation";
            this.PreviewInvInquiry.ConfirmationMessage = null;
            this.PreviewInvInquiry.Id = "PreviewInvInquiry";
            this.PreviewInvInquiry.ToolTip = null;
            this.PreviewInvInquiry.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.PreviewInvInquiry_Execute);
            // 
            // PreviewSOInquiry
            // 
            this.PreviewSOInquiry.Caption = "Preview SO";
            this.PreviewSOInquiry.Category = "ObjectsCreation";
            this.PreviewSOInquiry.ConfirmationMessage = null;
            this.PreviewSOInquiry.Id = "PreviewSOInquiry";
            this.PreviewSOInquiry.ToolTip = null;
            this.PreviewSOInquiry.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.PreviewSOInquiry_Execute);
            // 
            // PrintBundleInquiry
            // 
            this.PrintBundleInquiry.Caption = "Print Bundle";
            this.PrintBundleInquiry.Category = "ObjectsCreation";
            this.PrintBundleInquiry.ConfirmationMessage = null;
            this.PrintBundleInquiry.Id = "PrintBundleInquiry";
            this.PrintBundleInquiry.ToolTip = null;
            this.PrintBundleInquiry.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.PrintBundleInquiry_Execute);
            // 
            // InquiryViewControllers
            // 
            this.Actions.Add(this.ViewOpenPickList);
            this.Actions.Add(this.ViewPickListDetailInquiry);
            this.Actions.Add(this.ViewPickListInquiry);
            this.Actions.Add(this.InquiryStatus);
            this.Actions.Add(this.InquiryDateFrom);
            this.Actions.Add(this.InquiryDateTo);
            this.Actions.Add(this.InquiryFilter);
            this.Actions.Add(this.ViewSalesOrderInquiry);
            this.Actions.Add(this.StockMovementSPSearch);
            this.Actions.Add(this.InquirySearch);
            this.Actions.Add(this.PrintDOInquiry);
            this.Actions.Add(this.PreviewInvInquiry);
            this.Actions.Add(this.PreviewSOInquiry);
            this.Actions.Add(this.PrintBundleInquiry);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ViewOpenPickList;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ViewPickListDetailInquiry;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ViewPickListInquiry;
        private DevExpress.ExpressApp.Actions.SingleChoiceAction InquiryStatus;
        private DevExpress.ExpressApp.Actions.ParametrizedAction InquiryDateFrom;
        private DevExpress.ExpressApp.Actions.ParametrizedAction InquiryDateTo;
        private DevExpress.ExpressApp.Actions.SimpleAction InquiryFilter;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ViewSalesOrderInquiry;
        private DevExpress.ExpressApp.Actions.SimpleAction StockMovementSPSearch;
        private DevExpress.ExpressApp.Actions.SimpleAction InquirySearch;
        private DevExpress.ExpressApp.Actions.SimpleAction PrintDOInquiry;
        private DevExpress.ExpressApp.Actions.SimpleAction PreviewInvInquiry;
        private DevExpress.ExpressApp.Actions.SimpleAction PreviewSOInquiry;
        private DevExpress.ExpressApp.Actions.SimpleAction PrintBundleInquiry;
    }
}
