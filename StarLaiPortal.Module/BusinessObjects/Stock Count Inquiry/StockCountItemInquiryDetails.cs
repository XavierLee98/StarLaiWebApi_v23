using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
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

namespace StarLaiPortal.Module.BusinessObjects.Stock_Count_Inquiry
{
    [DefaultClassOptions]
    [DefaultProperty("StockCountNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("LinkDoc", AppearanceItemType = "Action", TargetItems = "Link", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("UnlinkDoc", AppearanceItemType = "Action", TargetItems = "Unlink", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]

    [XafDisplayName("Stock Count Item Details")]
    public class StockCountItemInquiryDetails : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public StockCountItemInquiryDetails(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        private string _StockCountNo;
        [XafDisplayName("Stock Count No")]
        [Index(0), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("StockCountNo", Enabled = false)]
        public string StockCountNo
        {
            get { return _StockCountNo; }
            set
            {
                SetPropertyValue("StockCountNo", ref _StockCountNo, value);
            }
        }

        private DateTime _StockCountDate;
        [XafDisplayName("Stock Count Date")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public DateTime StockCountDate
        {
            get { return _StockCountDate; }
            set
            {
                SetPropertyValue("StockCountDate", ref _StockCountDate, value);
            }
        }

        private string _Status;
        [XafDisplayName("Status")]
        [Index(5), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Status", Enabled = false)]
        public string Status
        {
            get { return _Status; }
            set
            {
                SetPropertyValue("Status", ref _Status, value);
            }
        }

        private string _Counter;
        [XafDisplayName("Counter")]
        [Index(8), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Counter", Enabled = false)]
        public string Counter
        {
            get { return _Counter; }
            set
            {
                SetPropertyValue("Counter", ref _Counter, value);
            }
        }

        private string _RecountRef;
        [XafDisplayName("Recount Ref.")]
        [Index(10), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("RecountRef", Enabled = false)]
        public string RecountRef
        {
            get { return _RecountRef; }
            set
            {
                SetPropertyValue("RecountRef", ref _RecountRef, value);
            }
        }

        private string _Warehouse;
        [XafDisplayName("Warehouse")]
        [Index(11), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Warehouse", Enabled = false)]
        public string Warehouse
        {
            get { return _Warehouse; }
            set
            {
                SetPropertyValue("Warehouse", ref _Warehouse, value);
            }
        }

        private string _Bins;
        [XafDisplayName("Bins")]
        [Index(12), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Bins", Enabled = false)]
        public string Bins
        {
            get { return _Bins; }
            set
            {
                SetPropertyValue("Bins", ref _Bins, value);
            }
        }

        private string _ItemCode;
        [XafDisplayName("Item Code")]
        [Index(13), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemCode", Enabled = false)]
        public string ItemCode
        {
            get { return _ItemCode; }
            set
            {
                SetPropertyValue("ItemCode", ref _ItemCode, value);
            }
        }

        private string _ItemBarcode;
        [XafDisplayName("Item Barcode")]
        [Index(15), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemBarcode", Enabled = false)]
        public string ItemBarcode

        {
            get { return _ItemBarcode; }
            set
            {
                SetPropertyValue("ItemBarcode", ref _ItemBarcode, value);
            }
        }

        private decimal _Counted;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "d")]
        [XafDisplayName("Counted")]
        [Index(23), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal Counted
        {
            get { return _Counted; }
            set
            {
                SetPropertyValue("Counted", ref _Counted, value);
            }
        }

        private StockCountItemInquiry _StockCountItemInquiry;
        [Association("StockCountItemInquiry-StockCountItemInquiryDetails")]
        [Index(99), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("StockCountItemInquiry", Enabled = false)]
        public StockCountItemInquiry StockCountItemInquiry
        {
            get { return _StockCountItemInquiry; }
            set { SetPropertyValue("StockCountItemInquiry", ref _StockCountItemInquiry, value); }
        }
    }
}