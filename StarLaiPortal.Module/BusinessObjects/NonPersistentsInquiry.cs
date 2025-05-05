using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;

// 2024-08-20 - add EIV-Validated Status - ver 1.0.19

namespace StarLaiPortal.Module.BusinessObjects
{
    #region Sales Quotation Inquiry
    [DomainComponent]
    [DefaultProperty("DocNum")]
    [NavigationItem("Sales Quotation")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Sales Quoatation Inquiry (SP)")]
    public class SalesQuotationInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        public SalesQuotationInquiry()
        {
            _Results = new BindingList<SalesQuotationInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<SalesQuotationInquiryResult> _Results;

        public BindingList<SalesQuotationInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Sales Quotation Inquiry Result")]
    public class SalesQuotationInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Quotation No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("Posting Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(5)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Expected Delivery Date")]
        [Appearance("DueDate", Enabled = false)]
        [Index(8)]
        public DateTime DueDate
        {
            get; set;
        }

        [XafDisplayName("Create Date")]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [Appearance("CreateDate", Enabled = false)]
        [Index(9)]
        public DateTime CreateDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(10)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Exceed Credit Limit")]
        [Appearance("HitCreditLimit", Enabled = false)]
        [Index(13)]
        public string HitCreditLimit
        {
            get; set;
        }

        [XafDisplayName("Exceed Credit Term")]
        [Appearance("HitCreditTerm", Enabled = false)]
        [Index(15)]
        public string HitCreditTerm
        {
            get; set;
        }

        [XafDisplayName("Price Change")]
        [Appearance("HitPriceChange", Enabled = false)]
        [Index(18)]
        public string HitPriceChange
        {
            get; set;
        }

        [XafDisplayName("Customer Group")]
        [Appearance("CardGroup", Enabled = false)]
        [Index(20)]
        public string CardGroup
        {
            get; set;
        }

        [XafDisplayName("Customer Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(23)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(25)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("Customer Contact")]
        [Appearance("ContactNo", Enabled = false)]
        [Index(28)]
        public string ContactNo
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        [Appearance("Transporter", Enabled = false)]
        [Index(30)]
        public string Transporter
        {
            get; set;
        }

        [XafDisplayName("Salesperson")]
        [Appearance("Salesperson", Enabled = false)]
        [Index(33)]
        public string Salesperson
        {
            get; set;
        }

        [XafDisplayName("Priority")]
        [Appearance("Priority", Enabled = false)]
        [Index(35)]
        public string Priority
        {
            get; set;
        }

        [XafDisplayName("Series")]
        [Appearance("Series", Enabled = false)]
        [Index(36)]
        public string Series
        {
            get; set;
        }

        [XafDisplayName("Amount")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Amount", Enabled = false)]
        [Index(38)]
        public decimal Amount
        {
            get; set;
        }

        [XafDisplayName("Remarks")]
        [Appearance("Remarks", Enabled = false)]
        [Index(40)]
        [Size(254)]
        public string Remarks
        {
            get; set;
        }

        [XafDisplayName("Portal SO No.")]
        [Appearance("PortalSONo", Enabled = false)]
        [Index(43)]
        public string PortalSONo
        {
            get; set;
        }

        [XafDisplayName("SAP SO No.")]
        [Appearance("SONo", Enabled = false)]
        [Index(45)]
        public string SONo
        {
            get; set;
        }

        [XafDisplayName("Pick List No.")]
        [Appearance("PickListNo", Enabled = false)]
        [Index(48)]
        public string PickListNo
        {
            get; set;
        }

        [XafDisplayName("Pack List No.")]
        [Appearance("PackListNo", Enabled = false)]
        [Index(50)]
        public string PackListNo
        {
            get; set;
        }

        [XafDisplayName("Loading No.")]
        [Appearance("LoadingNo", Enabled = false)]
        [Index(53)]
        public string LoadingNo
        {
            get; set;
        }

        [XafDisplayName("Portal DO No.")]
        [Appearance("PortalDONo", Enabled = false)]
        [Index(58)]
        public string PortalDONo
        {
            get; set;
        }

        [XafDisplayName("SAP DO No.")]
        [Appearance("SAPDONo", Enabled = false)]
        [Index(60)]
        public string SAPDONo
        {
            get; set;
        }

        [XafDisplayName("SAP Invoice No.")]
        [Appearance("SAPInvNo", Enabled = false)]
        [Index(63)]
        public string SAPInvNo
        {
            get; set;
        }

        [XafDisplayName("Price Change")]
        [Appearance("PriceChange", Enabled = false)]
        [Index(65)]
        public bool PriceChange
        {
            get; set;
        }

        [XafDisplayName("Exceed Limit")]
        [Appearance("ExceedPrice", Enabled = false)]
        [Index(68)]
        public bool ExceedPrice
        {
            get; set;
        }

        [XafDisplayName("Exceed Credit Control")]
        [Appearance("ExceedCreditControl", Enabled = false)]
        [Index(70)]
        public bool ExceedCreditControl
        {
            get; set;
        }
    }
    #endregion

    #region AR Downpayment Inquiry
    [DomainComponent]
    [DefaultProperty("DocNum")]
    [NavigationItem("Sales Order")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("AR Downpayment Inquiry (SP)")]
    public class ARDownpaymentInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        public ARDownpaymentInquiry()
        {
            _Results = new BindingList<ARDownpaymentInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<ARDownpaymentInquiryResult> _Results;

        public BindingList<ARDownpaymentInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("AR Downpayment Inquiry Result")]
    public class ARDownpaymentInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Transaction No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("SAP Transaction No.")]
        [Appearance("SAPNo", Enabled = false)]
        [Index(5)]
        public string SAPNo
        {
            get; set;
        }

        [XafDisplayName("Create Date Time")]
        [Appearance("CreateDT", Enabled = false)]
        [Index(8)]
        public DateTime CreateDT
        {
            get; set;
        }

        [XafDisplayName("Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(10)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(13)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(15)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(18)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(20)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("SAP SO No.")]
        [Appearance("SAPSONo", Enabled = false)]
        [Index(23)]
        public string SAPSONo
        {
            get; set;
        }

        [XafDisplayName("Amount")]
        [Appearance("Amount", Enabled = false)]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Index(25)]
        public decimal Amount
        {
            get; set;
        }

        [XafDisplayName("Payment Type")]
        [Appearance("PaymentType", Enabled = false)]
        [Index(28)]
        public string PaymentType
        {
            get; set;
        }

        [XafDisplayName("Reference No.")]
        [Appearance("RefNo", Enabled = false)]
        [Index(30)]
        public string RefNo
        {
            get; set;
        }
    }
    #endregion

    #region Pick List Details Inquiry
    [DomainComponent]
    [DefaultProperty("PortalNo")]
    [NavigationItem("Pick List")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Pick List Details Inquiry (SP)")]
    public class PickListDetailsInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public PickListDetailsInquiry()
        {
            _Results = new BindingList<PickListDetailsInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<PickListDetailsInquiryResult> _Results;

        public BindingList<PickListDetailsInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Pick List Details Inquiry Result")]
    public class PickListDetailsInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Pick List No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("Series")]
        [Appearance("Series", Enabled = false)]
        [Index(5)]
        public string Series
        {
            get; set;
        }

        [XafDisplayName("Posting Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(8)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Expected Delivery Date")]
        [Appearance("DueDate", Enabled = false)]
        [Index(10)]
        public DateTime DueDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(13)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        [Appearance("Transporter", Enabled = false)]
        [Index(15)]
        public string Transporter
        {
            get; set;
        }

        [XafDisplayName("Priority")]
        [Appearance("Priority", Enabled = false)]
        [Index(16)]
        public string Priority
        {
            get; set;
        }

        [XafDisplayName("Picker")]
        [Appearance("Picker", Enabled = false)]
        [Index(18)]
        public string Picker
        {
            get; set;
        }

        [XafDisplayName("Customer Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(20)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(23)]
        [Size(200)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("Portal SO No.")]
        [Appearance("PortalSONo", Enabled = false)]
        [Index(25)]
        public string PortalSONo
        {
            get; set;
        }

        [XafDisplayName("SAP SO No.")]
        [Appearance("SAPSONo", Enabled = false)]
        [Index(28)]
        public string SAPSONo
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(30)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(33)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Legacy Item Code")]
        [Appearance("LegacyItemCode", Enabled = false)]
        [Index(35)]
        public string LegacyItemCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(38)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(40)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(43)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("PlanQty")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("PlanQty", Enabled = false)]
        [Index(45)]
        public decimal PlanQty
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(48)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("PickQty")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("PickQty", Enabled = false)]
        [Index(50)]
        public decimal PickQty
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(53)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Bin Location")]
        [Appearance("Bin", Enabled = false)]
        [Index(55)]
        public string Bin
        {
            get; set;
        }

        [XafDisplayName("Discrepancy Reason")]
        [Appearance("DiscreReason", Enabled = false)]
        [Index(58)]
        [Size(254)]
        public string DiscreReason
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(60)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Print Status")]
        [Appearance("PrintStatus", Enabled = false)]
        [Index(63)]
        public string PrintStatus
        {
            get; set;
        }

        [XafDisplayName("Print Count")]
        [Appearance("PrintCount", Enabled = false)]
        [Index(65)]
        public int PrintCount
        {
            get; set;
        }
    }
    #endregion

    #region Bundle ID Inquiry
    [DomainComponent]
    [NavigationItem("Pack List")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Bundle ID Inquiry (SP)")]
    public class BundleIDInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public BundleIDInquiry()
        {
            _Results = new BindingList<BundleIDInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<BundleIDInquiryResult> _Results;

        public BindingList<BundleIDInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Bundle ID Inquiry Result")]
    public class BundleIDInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Pack List No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("Loading Date")]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [Appearance("LoadingDate", Enabled = false)]
        [Index(5)]
        public DateTime LoadingDate
        {
            get; set;
        }

        [XafDisplayName("Bundle ID")]
        [Appearance("BundleID", Enabled = false)]
        [Index(8)]
        public string BundleID
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(10)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Legacy Item Code")]
        [Appearance("LegacyItemCode", Enabled = false)]
        [Index(11)]
        public string LegacyItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(13)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("SO No.")]
        [Appearance("SONo", Enabled = false)]
        [Index(15)]
        public string SONo
        {
            get; set;
        }

        [XafDisplayName("SO Date")]
        [Appearance("SODate", Enabled = false)]
        [Index(18)]
        public DateTime SODate
        {
            get; set;
        }

        [XafDisplayName("Customer Name")]
        [Appearance("CustomerName", Enabled = false)]
        [Index(20)]
        public string CustomerName
        {
            get; set;
        }

        [XafDisplayName("Transporter Name")]
        [Appearance("TransporterName", Enabled = false)]
        [Index(23)]
        public string TransporterName
        {
            get; set;
        }

        [XafDisplayName("Pick List Doc Num")]
        [Appearance("PickListDocNum", Enabled = false)]
        [Index(25)]
        public string PickListDocNum
        {
            get; set;
        }

        [XafDisplayName("Pack Date")]
        [Appearance("PackDate", Enabled = false)]
        [Index(28)]
        public DateTime PackDate
        {
            get; set;
        }

        [XafDisplayName("Pack Time")]
        [Appearance("PackTime", Enabled = false)]
        [Index(30)]
        public string PackTime
        {
            get; set;
        }
    }
    #endregion

    #region Pack List Inquiry
    [DomainComponent]
    [NavigationItem("Pack List")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Pack List Inquiry (SP)")]
    public class PackListInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public PackListInquiry()
        {
            _Results = new BindingList<PackListInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<PackListInquiryResult> _Results;

        public BindingList<PackListInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Pick List Details Inquiry Result")]
    public class PackListInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Pack List No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(5)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(8)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Customer Group")]
        [Appearance("CardGroup", Enabled = false)]
        [Index(10)]
        public string CardGroup
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(13)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(15)]
        [Size(200)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(18)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(20)]
        [Size(200)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Legacy Item Code")]
        [Appearance("LegacyItemCode", Enabled = false)]
        [Index(23)]
        public string LegacyItemCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(25)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(28)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(30)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Quantity", Enabled = false)]
        [Index(33)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(35)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(36)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Bin Location")]
        [Appearance("Bin", Enabled = false)]
        [Index(37)]
        public string Bin
        {
            get; set;
        }

        [XafDisplayName("Pack To Location")]
        [Appearance("ToBin", Enabled = false)]
        [Index(38)]
        public string ToBin
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(40)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Portal SO No.")]
        [Appearance("PortalSONo", Enabled = false)]
        [Index(43)]
        public string PortalSONo
        {
            get; set;
        }

        [XafDisplayName("SAP SO No.")]
        [Appearance("SAPSONo", Enabled = false)]
        [Index(45)]
        public string SAPSONo
        {
            get; set;
        }

        [XafDisplayName("Bundle ID")]
        [Appearance("BundleID", Enabled = false)]
        [Index(48)]
        public string BundleID
        {
            get; set;
        }

        [XafDisplayName("Pick List No.")]
        [Appearance("PickListNo", Enabled = false)]
        [Index(50)]
        public string PickListNo
        {
            get; set;
        }
    }
    #endregion

    #region Loading Inquiry
    [DomainComponent]
    [NavigationItem("Loading Bay")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Loading Inquiry (SP)")]
    public class LoadingInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        public LoadingInquiry()
        {
            _Results = new BindingList<LoadingInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<LoadingInquiryResult> _Results;

        public BindingList<LoadingInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Loading Inquiry Result")]
    public class LoadingInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Loading No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("Posting Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(5)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(8)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Pack List No.")]
        [Appearance("PackListNo", Enabled = false)]
        [Index(10)]
        public string PackListNo
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(13)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Driver Name")]
        [Appearance("Driver", Enabled = false)]
        [Index(15)]
        public string Driver
        {
            get; set;
        }

        [XafDisplayName("Vehicle No.")]
        [Appearance("VehicleNo", Enabled = false)]
        [Index(18)]
        public string VehicleNo
        {
            get; set;
        }

        [XafDisplayName("Bundle ID")]
        [Appearance("BundleID", Enabled = false)]
        [Index(20)]
        public string BundleID
        {
            get; set;
        }

        [XafDisplayName("Loading Time")]
        [Appearance("LoadingTime", Enabled = false)]
        [Index(23)]
        public string LoadingTime
        {
            get; set;
        }

        [XafDisplayName("SO Num")]
        [Appearance("SONum", Enabled = false)]
        [Index(25)]
        public string SONum
        {
            get; set;
        }

        [XafDisplayName("SO Date")]
        [Appearance("SODate", Enabled = false)]
        [Index(28)]
        public DateTime SODate
        {
            get; set;
        }

        [XafDisplayName("Item No")]
        [Appearance("ItemNo", Enabled = false)]
        [Index(30)]
        public string ItemNo
        {
            get; set;
        }

        [XafDisplayName("Item Desc")]
        [Appearance("ItemDesc ", Enabled = false)]
        [Size(200)]
        [Index(33)]
        public string ItemDesc
        {
            get; set;
        }

        [XafDisplayName("Legacy Code")]
        [Appearance("LegacyCode", Enabled = false)]
        [Index(35)]
        public string LegacyCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo ", Enabled = false)]
        [Index(38)]
        public string CatalogNo
        {
            get; set;
        }
    }
    #endregion

    #region Delivery Inquiry
    [DomainComponent]
    [NavigationItem("Delivery Order")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Delivery Inquiry (SP)")]
    public class DeliveryInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public DeliveryInquiry()
        {
            _Results = new BindingList<DeliveryInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddMonths(-1);
        }

        private BindingList<DeliveryInquiryResult> _Results;

        public BindingList<DeliveryInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Delivery Inquiry Result")]
    public class DeliveryInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Transaction No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("SAP Transaction No.")]
        [Appearance("SAPNo", Enabled = false)]
        [Index(5)]
        public string SAPNo
        {
            get; set;
        }

        [XafDisplayName("Create Date Time")]
        [Appearance("CreateDT", Enabled = false)]
        [Index(8)]
        public DateTime CreateDT
        {
            get; set;
        }

        [XafDisplayName("Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(10)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Delivery Date")]
        [Appearance("DueDate", Enabled = false)]
        [Index(13)]
        public DateTime DueDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(15)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(18)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(20)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(23)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(25)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Old Code")]
        [Appearance("OldCode", Enabled = false)]
        [Index(28)]
        public string OldCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(30)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(33)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(35)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [Appearance("Quantity", Enabled = false)]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Index(38)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(40)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(43)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(45)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("SAP SO No.")]
        [Appearance("SAPSONo", Enabled = false)]
        [Index(48)]
        public string SAPSONo
        {
            get; set;
        }

        [XafDisplayName("Loading No.")]
        [Appearance("LoadingNo", Enabled = false)]
        [Index(50)]
        public string LoadingNo
        {
            get; set;
        }

        [XafDisplayName("SAP Inv No.")]
        [Appearance("SAPInvNo", Enabled = false)]
        [Index(53)]
        public string SAPInvNo
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        [Appearance("Transporter", Enabled = false)]
        [Index(55)]
        public string Transporter
        {
            get; set;
        }
    }
    #endregion

    #region Invoice Inquiry
    [DomainComponent]
    [NavigationItem("Delivery Order")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Invoice Inquiry (SP)")]
    public class InvoiceInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public InvoiceInquiry()
        {
            _Results = new BindingList<InvoiceInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<InvoiceInquiryResult> _Results;

        public BindingList<InvoiceInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Invoice Inquiry Result")]
    public class InvoiceInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("SAP Transaction No.")]
        [Appearance("SAPNo", Enabled = false)]
        [Index(5)]
        public string SAPNo
        {
            get; set;
        }

        [XafDisplayName("Portal SO")]
        [Appearance("PortalSONo", Enabled = false)]
        [Index(6)]
        public string PortalSONo
        {
            get; set;
        }

        [XafDisplayName("SO Series")]
        [Appearance("SOSeries", Enabled = false)]
        [Index(7)]
        public string SOSeries
        {
            get; set;
        }

        [XafDisplayName("Create Date Time")]
        [Appearance("CreateDT", Enabled = false)]
        [Index(8)]
        public DateTime CreateDT
        {
            get; set;
        }

        [XafDisplayName("Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(10)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Delivery Date")]
        [Appearance("DueDate", Enabled = false)]
        [Index(13)]
        public DateTime DueDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(15)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(18)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(20)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(23)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(25)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Old Code")]
        [Appearance("OldCode", Enabled = false)]
        [Index(28)]
        public string OldCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(30)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(33)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(35)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [Appearance("Quantity", Enabled = false)]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Index(38)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(40)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(43)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(45)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("SAP SO No.")]
        [Appearance("SAPSONo", Enabled = false)]
        [Index(48)]
        public string SAPSONo
        {
            get; set;
        }

        [XafDisplayName("Loading No.")]
        [Appearance("LoadingNo", Enabled = false)]
        [Index(50)]
        public string LoadingNo
        {
            get; set;
        }

        [XafDisplayName("SAP Inv No.")]
        [Appearance("SAPInvNo", Enabled = false)]
        [Index(53)]
        public string SAPInvNo
        {
            get; set;
        }

        [XafDisplayName("Portal DO No.")]
        [Appearance("PortalDONo", Enabled = false)]
        [Index(55)]
        public string PortalDONo
        {
            get; set;
        }

        [XafDisplayName("SAP DO No.")]
        [Appearance("SAPDONo", Enabled = false)]
        [Index(58)]
        public string SAPDONo
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        [Appearance("Transporter", Enabled = false)]
        [Index(59)]
        public string Transporter
        {
            get; set;
        }

        [XafDisplayName("Doc Total")]
        [Appearance("DocTotal", Enabled = false)]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Index(60)]
        public decimal DocTotal
        {
            get; set;
        }

        // Start ver 1.0.19
        [XafDisplayName("EIV-Validated Status")]
        [Appearance("EIVValidatedStatus", Enabled = false)]
        [Index(61)]
        public string EIVValidatedStatus
        {
            get; set;
        }
        // End ver 1.0.19
    }
    #endregion

    #region Purchase Order Inquiry
    [DomainComponent]
    [NavigationItem("Purchase Order")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Purchase Order Inquiry (SP)")]
    public class PurchaseOrderInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        public PurchaseOrderInquiry()
        {
            _Results = new BindingList<PurchaseOrderInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<PurchaseOrderInquiryResult> _Results;

        public BindingList<PurchaseOrderInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Purchase Order Inquiry Result")]
    public class PurchaseOrderInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal PO No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("SAP PO No.")]
        [Appearance("SAPNo", Enabled = false)]
        [Index(5)]
        public string SAPNo
        {
            get; set;
        }

        [XafDisplayName("Posting Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(8)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Expected Delivery Date")]
        [Appearance("DueDate", Enabled = false)]
        [Index(10)]
        public DateTime DueDate
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(13)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(15)]
        [Size(200)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(18)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(20)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(23)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Legacy Item Code")]
        [Appearance("LegacyItemCode", Enabled = false)]
        [Index(25)]
        public string LegacyItemCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(28)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(30)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(33)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("Open Qty")]
        [Appearance("OpenQty", Enabled = false)]
        [Index(34)]
        public int OpenQty
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Quantity", Enabled = false)]
        [Index(35)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(38)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(40)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Unit Price")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Price", Enabled = false)]
        [Index(43)]
        public decimal Price
        {
            get; set;
        }

        [XafDisplayName("Amount")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Amount", Enabled = false)]
        [Index(45)]
        public decimal Amount
        {
            get; set;
        }

        [XafDisplayName("ASN No.")]
        [Appearance("ASNNo", Enabled = false)]
        [Index(48)]
        public string ASNNo
        {
            get; set;
        }

        [XafDisplayName("Portal GRPO No.")]
        [Appearance("GRPONo", Enabled = false)]
        [Index(50)]
        public string GRPONo
        {
            get; set;
        }

        [XafDisplayName("SAP GRPO No.")]
        [Appearance("SAPGRPONo", Enabled = false)]
        [Index(53)]
        public string SAPGRPONo
        {
            get; set;
        }

        [XafDisplayName("SAP Invoice No.")]
        [Appearance("SAPInvNo", Enabled = false)]
        [Index(55)]
        public string SAPInvNo
        {
            get; set;
        }

        [XafDisplayName("SAP PO Status")]
        [Appearance("SAPStatus", Enabled = false)]
        [Index(58)]
        public string SAPStatus
        {
            get; set;
        }

        [XafDisplayName("Label Print Count ")]
        [Appearance("LabelPrintCount ", Enabled = false)]
        [Index(60)]
        public int LabelPrintCount
        {
            get; set;
        }
    }
    #endregion

    #region GRPO Inquiry
    [DomainComponent]
    [NavigationItem("GRPO")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("GRPO Inquiry (SP)")]
    public class GRPOInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public GRPOInquiry()
        {
            _Results = new BindingList<GRPOInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<GRPOInquiryResult> _Results;

        public BindingList<GRPOInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("GRPO Inquiry Result")]
    public class GRPOInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Transaction No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("SAP GRPO No.")]
        [Appearance("SAPNo", Enabled = false)]
        [Index(5)]
        public string SAPNo
        {
            get; set;
        }

        [XafDisplayName("SAP PO No.")]
        [Appearance("SAPPONo", Enabled = false)]
        [Index(6)]
        public string SAPPONo
        {
            get; set;
        }

        [XafDisplayName("Portal PO No.")]
        [Appearance("PortalPONo", Enabled = false)]
        [Index(8)]
        public string PortalPONo
        {
            get; set;
        }

        [XafDisplayName("ASN No.")]
        [Appearance("ASNNo", Enabled = false)]
        [Index(10)]
        public string ASNNo
        {
            get; set;
        }

        [XafDisplayName("Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(13)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(15)]
        public string Status
        {
            get; set;
        }
        [XafDisplayName("Customer/Vendor Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(18)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(20)]
        [Size(200)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(23)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(25)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Legacy Item Code")]
        [Appearance("LegacyItemCode", Enabled = false)]
        [Index(25)]
        public string LegacyItemCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(28)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(30)]
        public string Model
        {
            get; set;
        }
        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(33)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("ASN Qty")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("ASNQty ", Enabled = false)]
        [Index(35)]
        public decimal ASNQty
        {
            get; set;
        }

        [XafDisplayName("Received Qty")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("ReceivedQty", Enabled = false)]
        [Index(35)]
        public decimal ReceivedQty
        {
            get; set;
        }

        [XafDisplayName("Discrepancy Qty")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("DiscrepancyQty", Enabled = false)]
        [Index(35)]
        public decimal DiscrepancyQty
        {
            get; set;
        }

        [XafDisplayName("Discrepancy Reason")]
        [Appearance("DiscrepancyReason", Enabled = false)]
        [Size(200)]
        [Index(38)]
        public string DiscrepancyReason
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(38)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(40)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Bin Location")]
        [Appearance("Bin", Enabled = false)]
        [Index(43)]
        public string Bin
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(45)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Reference No.")]
        [Appearance("Reference", Enabled = false)]
        [Index(48)]
        public string Reference
        {
            get; set;
        }
    }
    #endregion

    #region ASN Inquiry
    [DomainComponent]
    [NavigationItem("Advanced Shipment Notice")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("ASN Inquiry (SP)")]
    public class ASNInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public ASNInquiry()
        {
            _Results = new BindingList<ASNInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<ASNInquiryResult> _Results;

        public BindingList<ASNInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("ASN Inquiry Result")]
    public class ASNInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal ASN No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("Posting Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(5)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Document Date")]
        [Appearance("TaxDate", Enabled = false)]
        [Index(8)]
        public DateTime TaxDate
        {
            get; set;
        }

        [XafDisplayName("ETA Date")]
        [Appearance("ETADate", Enabled = false)]
        [Index(10)]
        public DateTime ETADate
        {
            get; set;
        }

        [XafDisplayName("ESR Date")]
        [Appearance("ESRDate", Enabled = false)]
        [Index(13)]
        public DateTime ESRDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(15)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Print Status")]
        [Appearance("PrintStatus", Enabled = false)]
        [Index(18)]
        public string PrintStatus
        {
            get; set;
        }

        [XafDisplayName("Label Print Count ")]
        [Appearance("LabelPrintCount ", Enabled = false)]
        [Index(19)]
        public int LabelPrintCount
        {
            get; set;
        }

        [XafDisplayName("Vehicle")]
        [Appearance("Vehicle", Enabled = false)]
        [Index(20)]
        public string Vehicle
        {
            get; set;
        }

        [XafDisplayName("Container")]
        [Appearance("Container", Enabled = false)]
        [Index(23)]
        public string Container
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(25)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(28)]
        [Size(200)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("SAP PO No.")]
        [Appearance("SAPPONo", Enabled = false)]
        [Index(30)]
        public string SAPPONo
        {
            get; set;
        }

        [XafDisplayName("Portal PO No.")]
        [Appearance("PortalPONo", Enabled = false)]
        [Index(33)]
        public string PortalPONo
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(35)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(38)]
        [Size(200)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Legacy Item Code")]
        [Appearance("LegacyItemCode", Enabled = false)]
        [Index(40)]
        public string LegacyItemCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(43)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(45)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(48)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("ASN Qty")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("PlanQty", Enabled = false)]
        [Index(50)]
        public decimal PlanQty
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(53)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Received Qty")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("LoadQty", Enabled = false)]
        [Index(55)]
        public decimal LoadQty
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(58)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(60)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Reference No.")]
        [Appearance("Reference", Enabled = false)]
        [Index(63)]
        public string Reference
        {
            get; set;
        }

        [XafDisplayName("Outstanding Qty")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("OpenQty", Enabled = false)]
        [Index(65)]
        public decimal OpenQty
        {
            get; set;
        }

        [XafDisplayName("Series")]
        [Appearance("Series", Enabled = false)]
        [Index(68)]
        public string Series
        {
            get; set;
        }

        [XafDisplayName("Sales Number")]
        [Appearance("SalesNumber", Enabled = false)]
        [Index(70)]
        public string SalesNumber
        {
            get; set;
        }
    }
    #endregion

    #region Purchase Return Inquiry
    [DomainComponent]
    [NavigationItem("Purchase Return")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Purchase Return Inquiry (SP)")]
    public class PurchaseReturnInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public PurchaseReturnInquiry()
        {
            _Results = new BindingList<PurchaseReturnInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<PurchaseReturnInquiryResult> _Results;

        public BindingList<PurchaseReturnInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Purchase Return Inquiry Result")]
    public class PurchaseReturnInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Transaction No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("SAP Transaction No.")]
        [Appearance("SAPNo", Enabled = false)]
        [Index(5)]
        public string SAPNo
        {
            get; set;
        }

        [XafDisplayName("SAP GRPO No.")]
        [Appearance("SAPGRPONo", Enabled = false)]
        [Index(8)]
        public string SAPGRPONo
        {
            get; set;
        }

        [XafDisplayName("Portal GRPO No.")]
        [Appearance("PortalGRPONo", Enabled = false)]
        [Index(10)]
        public string PortalGRPONo
        {
            get; set;
        }

        [XafDisplayName("Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(13)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(15)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(18)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer/Vendor Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(20)]
        [Size(200)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(23)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(25)]
        [Size(200)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Legacy Item Code")]
        [Appearance("LegacyItemCode", Enabled = false)]
        [Index(28)]
        public string LegacyItemCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(30)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(33)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(35)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Quantity", Enabled = false)]
        [Index(38)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(40)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(43)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Bin Location")]
        [Appearance("Bin", Enabled = false)]
        [Index(45)]
        public string Bin
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(48)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Unit Price")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Price", Enabled = false)]
        [Index(50)]
        public decimal Price
        {
            get; set;
        }

        [XafDisplayName("Amount")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Amount", Enabled = false)]
        [Index(53)]
        public decimal Amount
        {
            get; set;
        }

        [XafDisplayName("Return Reason")]
        [Appearance("ReturnReason", Enabled = false)]
        [Index(55)]
        public string ReturnReason
        {
            get; set;
        }

        [XafDisplayName("Reference")]
        [Appearance("Reference", Enabled = false)]
        [Index(58)]
        public string Reference
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        [Appearance("Transporter", Enabled = false)]
        [Index(60)]
        public string Transporter
        {
            get; set;
        }
    }
    #endregion

    #region Sales Return Request Inquiry
    [DomainComponent]
    [NavigationItem("Sales Return")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Sales Return Request Inquiry (SP)")]
    public class SalesReturnRequestInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public SalesReturnRequestInquiry()
        {
            _Results = new BindingList<SalesReturnRequestInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<SalesReturnRequestInquiryResult> _Results;

        public BindingList<SalesReturnRequestInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Sales Return Request Inquiry Result")]
    public class SalesReturnRequestInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal SRR No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("Portal Return No.")]
        [Appearance("PortalReturnNo", Enabled = false)]
        [Index(5)]
        public string PortalReturnNo
        {
            get; set;
        }

        [XafDisplayName("SAP Sales Return No.")]
        [Appearance("SAPReturnNo", Enabled = false)]
        [Index(8)]
        public string SAPReturnNo
        {
            get; set;
        }

        [XafDisplayName("SAP CN No.")]
        [Appearance("SAPCNNo", Enabled = false)]
        [Index(10)]
        public string SAPCNNo
        {
            get; set;
        }

        [XafDisplayName("Posting Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(13)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Document Date")]
        [Appearance("TaxDate", Enabled = false)]
        [Index(15)]
        public DateTime TaxDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(18)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Customer Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(20)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("Customer Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(23)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("Reference")]
        [Appearance("Reference", Enabled = false)]
        [Index(25)]
        public string Reference
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        [Appearance("Transporter", Enabled = false)]
        [Index(28)]
        public string Transporter
        {
            get; set;
        }

        [XafDisplayName("Remarks")]
        [Appearance("Remarks", Enabled = false)]
        [Index(30)]
        [Size(254)]
        public string Remarks
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(33)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(35)]
        [Size(200)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Legacy Item Code")]
        [Appearance("LegacyItemCode", Enabled = false)]
        [Index(38)]
        public string LegacyItemCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(40)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(43)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(45)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Quantity", Enabled = false)]
        [Index(48)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(50)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Return Reason")]
        [Appearance("ReturnReason", Enabled = false)]
        [Index(53)]
        public string ReturnReason
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(55)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Bin Location")]
        [Appearance("Bin", Enabled = false)]
        [Index(58)]
        public string Bin
        {
            get; set;
        }

        [XafDisplayName("Unit Price")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Price", Enabled = false)]
        [Index(60)]
        public decimal Price
        {
            get; set;
        }

        [XafDisplayName("Amount")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Amount", Enabled = false)]
        [Index(63)]
        public decimal Amount
        {
            get; set;
        }
    }
    #endregion

    #region A/R Credit Memo Inquiry
    [DomainComponent]
    [NavigationItem("Sales Return")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("A/R Credit Memo Inquiry (SP)")]
    public class CreditMemoInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public CreditMemoInquiry()
        {
            _Results = new BindingList<CreditMemoInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<CreditMemoInquiryResult> _Results;

        public BindingList<CreditMemoInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Credit Memo Inquiry Result")]
    public class CreditMemoInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("SAP No")]
        [Appearance("SAPNo", Enabled = false)]
        [Index(3)]
        public string SAPNo
        {
            get; set;
        }

        [XafDisplayName("Create DT")]
        [Appearance("CreateDT", Enabled = false)]
        [Index(5)]
        public DateTime CreateDT
        {
            get; set;
        }

        [XafDisplayName("Doc Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(8)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Due Date")]
        [Appearance("DueDate", Enabled = false)]
        [Index(10)]
        public DateTime DueDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(13)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Card Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(15)]
        public string CardCode
        {
            get; set;
        }


        [XafDisplayName("Card Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(18)]
        public string CardName
        {
            get; set;
        }


        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(20)]
        public string ItemCode
        {
            get; set;
        }


        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(23)]
        public string ItemName
        {
            get; set;
        }


        [XafDisplayName("Old Code")]
        [Appearance("OldCode", Enabled = false)]
        [Index(25)]
        public string OldCode
        {
            get; set;
        }


        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(28)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(30)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(33)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Quantity", Enabled = false)]
        [Index(35)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(38)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(40)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(43)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Portal SO No")]
        [Appearance("PortalSONo", Enabled = false)]
        [Index(45)]
        public string PortalSONo
        {
            get; set;
        }

        [XafDisplayName("SO Series")]
        [Appearance("SOSeries", Enabled = false)]
        [Index(48)]
        public string SOSeries
        {
            get; set;
        }

        [XafDisplayName("Loading No")]
        [Appearance("LoadingNo", Enabled = false)]
        [Index(50)]
        public string LoadingNo
        {
            get; set;
        }

        [XafDisplayName("Portal Inv No")]
        [Appearance("PortalInvNo", Enabled = false)]
        [Index(53)]
        public string PortalInvNo
        {
            get; set;
        }

        [XafDisplayName("SAP Inv No")]
        [Appearance("SAPInvNo", Enabled = false)]
        [Index(55)]
        public string SAPInvNo
        {
            get; set;
        }

        [XafDisplayName("Portal DO No")]
        [Appearance("PortalDONo", Enabled = false)]
        [Index(58)]
        public string PortalDONo
        {
            get; set;
        }

        [XafDisplayName("SAP DO No")]
        [Appearance("SAPDONo", Enabled = false)]
        [Index(60)]
        public string SAPDONo
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        [Appearance("Transporter", Enabled = false)]
        [Index(63)]
        public string Transporter
        {
            get; set;
        }

        [XafDisplayName("DocKey")]
        [Appearance("DocKey", Enabled = false)]
        [Index(65), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public string DocKey
        {
            get; set;
        }

        [XafDisplayName("Row Total")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("RowTotal", Enabled = false)]
        [Index(68)]
        public decimal RowTotal
        {
            get; set;
        }

        [XafDisplayName("Doc. Total")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("DocTotal", Enabled = false)]
        [Index(70)]
        public decimal DocTotal
        {
            get; set;
        }

        [XafDisplayName("Sales Return Reason")]
        [Appearance("SalesReturnReason", Enabled = false)]
        [Index(73)]
        public string SalesReturnReason
        {
            get; set;
        }
    }
    #endregion

    #region Warehouse Transfer Details Inquiry
    [DomainComponent]
    [NavigationItem("Warehouse Transfer")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Warehouse Transfer Details Inquiry (SP)")]
    public class WarehouseTransferDetailsInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public WarehouseTransferDetailsInquiry()
        {
            _Results = new BindingList<WarehouseTransferDetailsInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<WarehouseTransferDetailsInquiryResult> _Results;

        public BindingList<WarehouseTransferDetailsInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Warehouse Transfer Details Inquiry Result")]
    public class WarehouseTransferDetailsInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Transfer Req. No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("SAP Transfer No.")]
        [Appearance("SAPNo", Enabled = false)]
        [Index(5)]
        public string SAPNo
        {
            get; set;
        }

        [XafDisplayName("Posting Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(8)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(10)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Reference")]
        [Appearance("Reference", Enabled = false)]
        [Index(13)]
        public string Reference
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(15)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(18)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(20)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Legacy Item Code")]
        [Appearance("LegacyItemCode", Enabled = false)]
        [Index(23)]
        public string LegacyItemCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(25)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(28)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(30)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Quantity", Enabled = false)]
        [Index(33)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(35)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("From Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(38)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("From Bin")]
        [Appearance("Bin", Enabled = false)]
        [Index(40)]
        public string Bin
        {
            get; set;
        }

        [XafDisplayName("Transfer Type")]
        [Appearance("TransferType ", Enabled = false)]
        [Index(43)]
        public string TransferType
        {
            get; set;
        }

        [XafDisplayName("To Warehouse")]
        [Appearance("ToWarehouse", Enabled = false)]
        [Index(45)]
        public string ToWarehouse
        {
            get; set;
        }

        [XafDisplayName("To Bin")]
        [Appearance("ToBin", Enabled = false)]
        [Index(48)]
        public string ToBin
        {
            get; set;
        }
    }
    #endregion

    #region Stock Adjustment Details Inquiry
    [DomainComponent]
    [NavigationItem("Stock Adjustment")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&Close", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = ViewItemVisibility.Hide, Context = "Any")]

    [XafDisplayName("Stock Adjustment Details Inquiry (SP)")]
    public class StockAdjustmentDetailsInquiry
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Status")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public InquiryViewStatus Status { get; set; }

        [XafDisplayName("ItemCode")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        public StockAdjustmentDetailsInquiry()
        {
            _Results = new BindingList<StockAdjustmentDetailsInquiryResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddDays(-7);
        }

        private BindingList<StockAdjustmentDetailsInquiryResult> _Results;

        public BindingList<StockAdjustmentDetailsInquiryResult> Results { get { return _Results; } }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Stock Adjustment Details Inquiry Result")]
    public class StockAdjustmentDetailsInquiryResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public string PriKey;

        [XafDisplayName("Portal Transaction No.")]
        [Appearance("PortalNo", Enabled = false)]
        [Index(3)]
        public string PortalNo
        {
            get; set;
        }

        [XafDisplayName("SAP Transaction No.")]
        [Appearance("SAPNo", Enabled = false)]
        [Index(5)]
        public string SAPNo
        {
            get; set;
        }

        [XafDisplayName("Posting Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(8)]
        public DateTime DocDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(10)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(13)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(15)]
        [Size(200)]
        public string ItemName
        {
            get; set;
        }

        [XafDisplayName("Legacy Item Code")]
        [Appearance("LegacyItemCode", Enabled = false)]
        [Index(18)]
        public string LegacyItemCode
        {
            get; set;
        }

        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(20)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(23)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(25)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Quantity", Enabled = false)]
        [Index(28)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(30)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(33)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Bin Location")]
        [Appearance("Bin", Enabled = false)]
        [Index(35)]
        public string Bin
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(38)]
        [Size(254)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Unit Price")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Price", Enabled = false)]
        [Index(40)]
        public decimal Price
        {
            get; set;
        }


        [XafDisplayName("Amount")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Amount", Enabled = false)]
        [Index(43)]
        public decimal Amount
        {
            get; set;
        }

        [XafDisplayName("Reason Code")]
        [Appearance("ReasonCode", Enabled = false)]
        [Index(45)]
        public string ReasonCode
        {
            get; set;
        }

        [XafDisplayName("Cost Type")]
        [Appearance("CostType", Enabled = false)]
        [Index(48)]
        public string CostType
        {
            get; set;
        }
    }
    #endregion
}