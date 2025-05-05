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
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

// 2023-12-04 - add order status - ver 1.0.13
// 2024-01-30 - add inventory movement table - ver 1.0.14
// 2024-01-30 - add PODate and PODelivery - ver 1.0.14
// 2024-04-01 - add paymentso and paymentgroup - ver 1.0.15
// 2024-06-01 - add Salesperson - ver 1.0.17
// 2024-10-08 - add whse - ver 1.0.21

namespace StarLaiPortal.Module.BusinessObjects
{
    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Sales History")]
    public class SalesHistory
    {
        [Browsable(false), DevExpress.ExpressApp.Data.Key]
        public int Id;

        [XafDisplayName("No")]
        [Index(0), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("No", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("No1", Enabled = false)]
        public int No { get; set; }

        [XafDisplayName("Customer")]
        [Index(1), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Customer", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("Customer1", Enabled = false)]
        public string Customer { get; set; }

        [XafDisplayName("Salesperson")]
        [Index(2), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Salesperson", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("Salesperson1", Enabled = false)]
        public string Salesperson { get; set; }

        [XafDisplayName("Sales Date")]
        [Index(3), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("SalesDate", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("SalesDate1", Enabled = false)]
        public DateTime SalesDate { get; set; }

        [XafDisplayName("Quantity")]
        [Index(4), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Quantity", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("Quantity1", Enabled = false)]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal Quantity { get; set; }

        [XafDisplayName("Unit Price")]
        [Index(5), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("UnitPrice", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("UnitPrice1", Enabled = false)]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal UnitPrice { get; set; }

        [XafDisplayName("SAP Invoice No")]
        [Index(6), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("SAPInvoiceNo", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("SAPInvoiceNo1", Enabled = false)]
        public string SAPInvoiceNo { get; set; }

        // Start ver 1.0.21
        [XafDisplayName("Whse")]
        [Index(7), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Whse", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("Whse1", Enabled = false)]
        public string Whse { get; set; }
        // End ver 1.0.21

        [Browsable(false)]
        public bool IsErr { get; set; }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    public class SalesHistoryList
    {
        [XafDisplayName("Item Code")]
        [Index(0), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemCode", Enabled = false)]
        public string ItemCode { get; set; }

        private BindingList<SalesHistory> saleshistory;
        public SalesHistoryList()
        {
            saleshistory = new BindingList<SalesHistory>();
        }
        public BindingList<SalesHistory> Sales { get { return saleshistory; } }
    }

    [DomainComponent]
    [NonPersistent]
    // Start ver 1.0.14
    [XafDisplayName("Message")]
    // End ver 1.0.14
    public class Confirmation
    { 
        [XafDisplayName("Message")]
        [Appearance("Message", Enabled = false)]
        public string Message { get; set; }
    }

    [DomainComponent]
    [NonPersistent]
    [XafDisplayName("Confirmation")]
    public class StringParameters
    {
        // Add this property as the key member in the CustomizeTypesInfo event
        [XafDisplayName("Remarks")]
        [Appearance("ParamString", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [NonPersistentDc]
        public string ParamString { get; set; }

        //[XafDisplayName("Important")]
        [Appearance("ActionMessage", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("ActionMessage2", Enabled = false, FontColor = "Red")]
        [NonPersistentDc]
        public string ActionMessage { get; set; }

        [Browsable(false)]
        //[NonPersistentDc]
        public bool IsErr { get; set; }
    }

    [DomainComponent]
    [NonPersistent]
    [XafDisplayName("Approval")]
    [RuleCriteria("ApprovalRemarks", DefaultContexts.Save, "IsValid = 0", "Please fill in reason.")]
    public class ApprovalParameters : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public ApprovalParameters(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }
        // Add this property as the key member in the CustomizeTypesInfo event
        [XafDisplayName("Approval Status")]
        [Appearance("Approved", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        public ApprovalActions AppStatus { get; set; }

        [XafDisplayName("Remarks")]
        [Appearance("ParamString", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        public string ParamString { get; set; }

        //[XafDisplayName("Important")]
        [Appearance("ActionMessage", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("ActionMessage2", Enabled = false, FontColor = "Red")]
        public string ActionMessage { get; set; }

        [Browsable(false)]
        public bool IsErr { get; set; }

        [Browsable(false)]
        public bool IsValid
        {
            get
            {
                if ((ParamString == null || ParamString == "") && AppStatus == ApprovalActions.No)
                {
                    return true;
                }

                return false;
            }
        }
    }

    //[DomainComponent]
    //public class ObjectSpaceClass : IObjectSpaceLink
    //{
    //    //...
    //    IObjectSpace objectSpace;
    //    IObjectSpace IObjectSpaceLink.ObjectSpace
    //    {
    //        get { return objectSpace; }
    //        set { objectSpace = value; }
    //    }
    //}

    // Start ver 1.0.13
    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [XafDisplayName("Order Status")]
    public class OrderStatus
    {
        [Browsable(false), DevExpress.ExpressApp.Data.Key]
        public int Id;

        [XafDisplayName("No")]
        [Index(0), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("No", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("No1", Enabled = false)]
        public int No { get; set; }

        [XafDisplayName("Item Code")]
        [Index(1), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemCode", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("ItemCode1", Enabled = false)]
        public string ItemCode { get; set; }

        [XafDisplayName("Legacy Code")]
        [Index(2), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("LegacyCode", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("LegacyCode1", Enabled = false)]
        public string LegacyCode { get; set; }

        [XafDisplayName("Item Name")]
        [Index(3), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemName", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("ItemName1", Enabled = false)]
        public string ItemName { get; set; }

        [XafDisplayName("Origin")]
        [Index(4), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Origin", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("Origin1", Enabled = false)]
        public string Origin { get; set; }

        [XafDisplayName("Warehouse")]
        [Index(5), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Warehouse", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("Warehouse1", Enabled = false)]
        public string Warehouse { get; set; }

        [XafDisplayName("Doc No")]
        [Index(6), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("DocNo", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("DocNo1", Enabled = false)]
        public string DocNo { get; set; }

        [XafDisplayName("Quantity")]
        [Index(7), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Quantity", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("Quantity1", Enabled = false)]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal Quantity { get; set; }

        [XafDisplayName("ESR Date")]
        [Index(8), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ESRDate", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("ESRDate1", Enabled = false)]
        public DateTime ESRDate { get; set; }

        // Start ver 1.0.14
        [XafDisplayName("PO Date")]
        [Index(9), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("PODate", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("PODate1", Enabled = false)]
        public DateTime PODate { get; set; }

        [XafDisplayName("PO Delivery")]
        [Index(10), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("PODelivery", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "IsErr")]
        [Appearance("PODelivery1", Enabled = false)]
        public DateTime PODelivery { get; set; }
        // End ver 1.0.14

        [Browsable(false)]
        public bool IsErr { get; set; }
    }

    [DomainComponent]
    [NonPersistent]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    public class OrderStatusList
    {
        [XafDisplayName("Item Code")]
        [Index(0), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemCode", Enabled = false)]
        public string ItemCode { get; set; }

        private BindingList<OrderStatus> orderstatus;
        public OrderStatusList()
        {
            orderstatus = new BindingList<OrderStatus>();
        }
        public BindingList<OrderStatus> Orderstatus { get { return orderstatus; } }
    }
    // End ver 1.0.13

    // Start ver 1.0.14
    [DomainComponent]
    [DefaultProperty("DocNum")]
    [NavigationItem("Reports")]
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

    [XafDisplayName("Stock Movement (SP)")]
    public class StockMovement
    {
        [Key(AutoGenerate = true), Browsable(false)]
        public int Oid;

        [XafDisplayName("Date From")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateFrom { get; set; }

        [XafDisplayName("Date To")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DateTime DateTo { get; set; }

        [XafDisplayName("Item Code")]
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        [Index(2), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemMasters ItemCode { get; set; }

        [XafDisplayName("Portal Doc Num")]
        [NoForeignKey]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public string PortalDocNum { get; set; }

        public StockMovement()
        {
            _Results = new BindingList<StockMovementResult>();

            DateTo = DateTime.Today;
            DateFrom = DateTo.AddMonths(-3);
        }

        private BindingList<StockMovementResult> _Results;

        public BindingList<StockMovementResult> Results { get { return _Results; } }
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
    [XafDisplayName("Stock Movement Result")]
    public class StockMovementResult
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public int Oid;

        [XafDisplayName("Date")]
        [Index(0), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("TransDate", Enabled = false)]
        public DateTime TransDate { get; set; }

        [XafDisplayName("Portal Transaction No.")]
        [Index(1), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("PortalNo", Enabled = false)]
        public string PortalNo { get; set; }

        [XafDisplayName("SAP Transaction No.")]
        [Index(2), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("SAPNo", Enabled = false)]
        public string SAPNo { get; set; }

        [XafDisplayName("Customer/Vendor Code")]
        [Index(3), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("CardCode", Enabled = false)]
        public string CardCode { get; set; }

        [XafDisplayName("Customer/Vendor Name")]
        [Index(4), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("CardName", Enabled = false)]
        public string CardName { get; set; }

        [XafDisplayName("Item Code")]
        [Index(5), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemCode", Enabled = false)]
        public string ItemCode { get; set; }

        [XafDisplayName("Item Name")]
        [Index(6), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemName", Enabled = false)]
        public string ItemName { get; set; }

        [XafDisplayName("Legacy Item Code")]
        [Index(7), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("LegacyItemCode", Enabled = false)]
        public string LegacyItemCode { get; set; }

        [XafDisplayName("Catalog No")]
        [Index(8), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("CatalogNo", Enabled = false)]
        public string CatalogNo { get; set; }

        [XafDisplayName("Model")]
        [Index(9), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("Model", Enabled = false)]
        public string Model { get; set; }

        [XafDisplayName("Quantity")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Index(10), VisibleInListView(true), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("Quantity", Enabled = false)]
        public decimal Quantity { get; set; }

        [XafDisplayName("UOM")]
        [Index(11), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("UOM", Enabled = false)]
        public string UOM { get; set; }

        [XafDisplayName("Warehouse")]
        [Index(12), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Warehouse", Enabled = false)]
        public string Warehouse { get; set; }

        [XafDisplayName("Bin Location")]
        [Index(13), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("BinLocation", Enabled = false)]
        public string BinLocation { get; set; }

        [XafDisplayName("Trans. Type")]
        [Index(14), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("TransType", Enabled = false)]
        public string TransType { get; set; }
    }
    // End ver 1.0.14

    // Start ver 1.0.15
    [DomainComponent]
    [NonPersistent]
    [XafDisplayName("Sales Order Items")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    public class PickListSO
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public int PriKey;

        [XafDisplayName("Series")]
        //[NoForeignKey]
        [Appearance("Series", Enabled = false)]
        [Appearance("Series1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(3)]
        public string Series
        {
            get; set;
        }

        [XafDisplayName("DocNum")]
        [Appearance("DocNum", Enabled = false)]
        [Appearance("DocNum1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(5)]
        public string DocNum
        {
            get; set;
        }

        [XafDisplayName("Posting Date")]
        [Appearance("PostingDateStr", Enabled = false)]
        [Appearance("PostingDateStr1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(6)]
        public string PostingDateStr
        {
            get; set;
        }

        [XafDisplayName("Delivery Date")]
        [Appearance("DeliveryDateStr", Enabled = false)]
        [Appearance("DeliveryDateStr1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(7)]
        public string DeliveryDateStr
        {
            get; set;
        }

        [XafDisplayName("PostingDate")]
        [Appearance("PostingDate", Enabled = false)]
        [Appearance("PostingDate1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(8)]
        public DateTime PostingDate
        {
            get; set;
        }

        [XafDisplayName("DeliveryDate")]
        [Appearance("DeliveryDate", Enabled = false)]
        [Appearance("DeliveryDate1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(10)]
        public DateTime DeliveryDate
        {
            get; set;
        }

        [XafDisplayName("Priority")]
        [Appearance("Priority", Enabled = false)]
        [Appearance("Priority1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(13)]
        public PriorityType Priority
        {
            get; set;
        }

        [XafDisplayName("Customer")]
        [Appearance("Customer", Enabled = false)]
        [Appearance("Customer1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(15), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public string Customer
        {
            get; set;
        }

        [XafDisplayName("CustomerName")]
        [Appearance("CustomerName", Enabled = false)]
        [Appearance("CustomerName1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(18)]
        public string CustomerName
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        //[NoForeignKey]
        [Appearance("Transporter", Enabled = false)]
        [Appearance("Transporter1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(20)]
        public string Transporter
        {
            get; set;
        }

        [XafDisplayName("Salesperson")]
        [Appearance("Salesperson", Enabled = false)]
        [Appearance("Salesperson1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(23)]
        public string Salesperson
        {
            get; set;
        }

        [XafDisplayName("Remarks")]
        [Appearance("Remarks", Enabled = false)]
        [Appearance("Remarks1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(25)]
        public string Remarks
        {
            get; set;
        }

        [XafDisplayName("ItemCode")]
        [Appearance("ItemCode", Enabled = false)]
        [Appearance("ItemCode1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(28)]
        public string ItemCode
        {
            get; set;
        }

        [XafDisplayName("ItemDesc")]
        [Appearance("ItemDesc", Enabled = false)]
        [Appearance("ItemDesc1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(30)]
        public string ItemDesc
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [DbType("numeric(19,6)")]
        [ModelDefault("DisplayFormat", "{0:n4}")]
        [ModelDefault("EditMask", "{0:n4}")]
        [Appearance("Quantity", Enabled = false)]
        [Appearance("Quantity1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(33), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Appearance("Warehouse1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(35)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Mfg Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Appearance("CatalogNo1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(38)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Oid")]
        [Appearance("Oid", Enabled = false)]
        [Appearance("Oid1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(40), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public string Oid
        {
            get; set;
        }

        [XafDisplayName("CreateDate")]
        [Appearance("CreateDate", Enabled = false)]
        [Appearance("CreateDate1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(43), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public DateTime CreateDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Appearance("Status1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(45)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("SAP Doc No")]
        [Appearance("SAPDocNum", Enabled = false)]
        [Appearance("SAPDocNum1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(48)]
        public string SAPDocNum
        {
            get; set;
        }

        [XafDisplayName("InStock")]
        [DbType("numeric(19,6)")]
        [ModelDefault("DisplayFormat", "{0:n4}")]
        [ModelDefault("EditMask", "{0:n4}")]
        [Appearance("InStock", Enabled = false)]
        [Appearance("InStock1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(33), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal InStock
        {
            get; set;
        }

        [XafDisplayName("PartialPicked ")]
        [Appearance("PartialPicked ", Enabled = false)]
        [Appearance("PartialPicked1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(35)]
        public int PartialPicked
        {
            get; set;
        }

        // Start ver 1.0.9
        [XafDisplayName("Zone")]
        [Appearance("FirstBinZone", Enabled = false)]
        [Appearance("FirstBinZone1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(38)]
        public string FirstBinZone
        {
            get; set;
        }
    }

    [DomainComponent]
    [NonPersistent]
    [XafDisplayName("Sales Order")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave", AppearanceItemType.Action, "True", TargetItems = "Save", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSave&New", AppearanceItemType.Action, "True", TargetItems = "SaveAndNew", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideValidate", AppearanceItemType.Action, "True", TargetItems = "ShowAllContexts", Visibility = ViewItemVisibility.Hide, Context = "Any")]
    public class PickListSOGroup
    {
        [DevExpress.ExpressApp.Data.Key, Browsable(false)]
        public int PriKey;

        [Key]
        [Browsable(true)]
        [XafDisplayName("DocNum")]
        [Appearance("DocNum", Enabled = false)]
        [Appearance("DocNum1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(0)]
        public string DocNum
        {
            get; set;
        }

        [XafDisplayName("Series")]
        //[NoForeignKey]
        [Appearance("Series", Enabled = false)]
        [Appearance("Series1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(3)]
        public string Series
        {
            get; set;
        }

        [XafDisplayName("Posting Date")]
        [Appearance("PostingDateStr", Enabled = false)]
        [Appearance("PostingDateStr1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(4)]
        public string PostingDateStr
        {
            get; set;
        }

        [XafDisplayName("Delivery Date")]
        [Appearance("DeliveryDateStr", Enabled = false)]
        [Appearance("DeliveryDateStr1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(5)]
        public string DeliveryDateStr
        {
            get; set;
        }

        [XafDisplayName("PostingDate")]
        [Appearance("PostingDate", Enabled = false)]
        [Appearance("PostingDate1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(6)]
        public DateTime PostingDate
        {
            get; set;
        }

        [XafDisplayName("DeliveryDate")]
        [Appearance("DeliveryDate", Enabled = false)]
        [Appearance("DeliveryDate1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(7)]
        public DateTime DeliveryDate
        {
            get; set;
        }

        [XafDisplayName("Priority")]
        [Appearance("Priority", Enabled = false)]
        [Appearance("Priority1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(10)]
        public PriorityType Priority
        {
            get; set;
        }

        [XafDisplayName("Customer")]
        [Appearance("Customer", Enabled = false)]
        [Appearance("Customer1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(13), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public string Customer
        {
            get; set;
        }

        [XafDisplayName("CustomerName")]
        [Appearance("CustomerName", Enabled = false)]
        [Appearance("CustomerName1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(15)]
        public string CustomerName
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        //[NoForeignKey]
        [Appearance("Transporter", Enabled = false)]
        [Appearance("Transporter1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(18)]
        public string Transporter
        {
            get; set;
        }

        [XafDisplayName("Salesperson")]
        [Appearance("Salesperson", Enabled = false)]
        [Appearance("Salesperson1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(20)]
        public string Salesperson
        {
            get; set;
        }

        [XafDisplayName("Remarks")]
        [Appearance("Remarks", Enabled = false)]
        [Appearance("Remarks1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(23)]
        public string Remarks
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Appearance("Status1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(25)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("SAP Doc No")]
        [Appearance("SAPDocNum", Enabled = false)]
        [Appearance("SAPDocNum1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(28)]
        public string SAPDocNum
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Appearance("Warehouse1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(30)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("PartialPicked ")]
        [Appearance("PartialPicked ", Enabled = false)]
        [Appearance("PartialPicked1", BackColor = "#FF7C80", FontColor = "Black", Criteria = "PartialPicked > 0")]
        [Index(33)]
        public int PartialPicked
        {
            get; set;
        }
    }
    // End ver 1.0.15
}