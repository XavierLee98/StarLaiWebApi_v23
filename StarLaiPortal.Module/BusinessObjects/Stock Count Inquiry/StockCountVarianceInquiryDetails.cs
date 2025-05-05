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

    [XafDisplayName("Stock Count Variance Details")]

    public class StockCountVarianceInquiryDetails : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public StockCountVarianceInquiryDetails(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        private DateTime _StockCountDate;
        [XafDisplayName("Stock Count Date")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public DateTime StockCountDate
        {
            get { return _StockCountDate; }
            set
            {
                SetPropertyValue("StockCountDate", ref _StockCountDate, value);
            }
        }   

        private string _WhsCode;
        [XafDisplayName("Whs Code")]
        [Index(3), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("WhsCode", Enabled = false)]
        public string WhsCode
        {
            get { return _WhsCode; }
            set
            {
                SetPropertyValue("WhsCode", ref _WhsCode, value);
            }
        }

        private string _ItemCode;
        [XafDisplayName("Item Code")]
        [Index(5), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemCode", Enabled = false)]
        public string ItemCode
        {
            get { return _ItemCode; }
            set
            {
                SetPropertyValue("ItemCode", ref _ItemCode, value);
            }
        }

        private string _LegacyItemCode;
        [XafDisplayName("Legacy Item Code")]
        [Index(8), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("LegacyItemCode", Enabled = false)]
        public string LegacyItemCode
        {
            get { return _LegacyItemCode; }
            set
            {
                SetPropertyValue("LegacyItemCode", ref _LegacyItemCode, value);
            }
        }

        private string _ItemName;
        [XafDisplayName("Item Name")]
        [Index(10), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemName", Enabled = false)]
        public string ItemName
        {
            get { return _ItemName; }
            set
            {
                SetPropertyValue("ItemName", ref _ItemName, value);
            }
        }

        private string _SysQtyBins;
        [XafDisplayName("Sys Qty Bins")]
        [Index(13), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("SysQtyBins", Enabled = false)]
        public string SysQtyBins
        {
            get { return _SysQtyBins; }
            set
            {
                SetPropertyValue("SysQtyBins", ref _SysQtyBins, value);
            }
        }

        private int _SysQty;
        [ImmediatePostData]
        [XafDisplayName("Sys Qty")]
        [Index(15), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int SysQty
        {
            get { return _SysQty; }
            set
            {
                SetPropertyValue("SysQty", ref _SysQty, value);
            }
        }

        private decimal _AverageCost;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "d")]
        [XafDisplayName("Average Cost")]
        [Index(18), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal AverageCost
        {
            get { return _AverageCost; }
            set
            {
                SetPropertyValue("AverageCost", ref _AverageCost, value);
            }
        }

        private int _CountedRound;
        [ImmediatePostData]
        [XafDisplayName("Counted Round")]
        [Index(20), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int CountedRound
        {
            get { return _CountedRound; }
            set
            {
                SetPropertyValue("CountedRound", ref _CountedRound, value);
            }
        }

        private int _FinalCounted;
        [ImmediatePostData]
        [XafDisplayName("Final Counted")]
        [Index(23), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int FinalCounted
        {
            get { return _FinalCounted; }
            set
            {
                SetPropertyValue("FinalCounted", ref _FinalCounted, value);
            }
        }


        private int _FinalVar;
        [ImmediatePostData]
        [XafDisplayName("Final Var.")]
        [Index(25), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int FinalVar
        {
            get { return _FinalVar; }
            set
            {
                SetPropertyValue("FinalVar", ref _FinalVar, value);
            }
        }

        private string _Count1Bin;
        [XafDisplayName("Count 1 Bin")]
        [Index(28), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Count1Bin", Enabled = false)]
        public string Count1Bin
        {
            get { return _Count1Bin; }
            set
            {
                SetPropertyValue("Count1Bin", ref _Count1Bin, value);
            }
        }

        private int _Count1Qty;
        [ImmediatePostData]
        [XafDisplayName("Count 1 Qty")]
        [Index(30), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int Count1Qty
        {
            get { return _Count1Qty; }
            set
            {
                SetPropertyValue("Count1Qty", ref _Count1Qty, value);
            }
        }

        private int _Count1Var;
        [ImmediatePostData]
        [XafDisplayName("Count 1 Var")]
        [Index(33), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int Count1Var
        {
            get { return _Count1Var; }
            set
            {
                SetPropertyValue("Count1Var", ref _Count1Var, value);
            }
        }

        private string _Count2Bin;
        [XafDisplayName("Count 2 Bin")]
        [Index(35), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Count2Bin", Enabled = false)]
        public string Count2Bin
        {
            get { return _Count2Bin; }
            set
            {
                SetPropertyValue("Count2Bin", ref _Count2Bin, value);
            }
        }

        private int _Count2Qty;
        [ImmediatePostData]
        [XafDisplayName("Count 2 Qty")]
        [Index(38), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int Count2Qty
        {
            get { return _Count2Qty; }
            set
            {
                SetPropertyValue("Count2Qty", ref _Count2Qty, value);
            }
        }

        private int _Count2Var;
        [ImmediatePostData]
        [XafDisplayName("Count 2 Var")]
        [Index(40), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int Count2Var
        {
            get { return _Count2Var; }
            set
            {
                SetPropertyValue("Count2Var", ref _Count2Var, value);
            }
        }

        private string _Count3Bin;
        [XafDisplayName("Count 3 Bin")]
        [Index(43), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Count3Bin", Enabled = false)]
        public string Count3Bin
        {
            get { return _Count3Bin; }
            set
            {
                SetPropertyValue("Count3Bin", ref _Count3Bin, value);
            }
        }

        private int _Count3Qty;
        [ImmediatePostData]
        [XafDisplayName("Count 3 Qty")]
        [Index(45), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int Count3Qty
        {
            get { return _Count3Qty; }
            set
            {
                SetPropertyValue("Count3Qty", ref _Count3Qty, value);
            }
        }

        private int _Count3Var;
        [ImmediatePostData]
        [XafDisplayName("Count 3 Var")]
        [Index(48), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int Count3Var
        {
            get { return _Count3Var; }
            set
            {
                SetPropertyValue("Count3Var", ref _Count3Var, value);
            }
        }

        private string _Count4Bin;
        [XafDisplayName("Count 4 Bin")]
        [Index(50), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Count4Bin", Enabled = false)]
        public string Count4Bin
        {
            get { return _Count4Bin; }
            set
            {
                SetPropertyValue("Count4Bin", ref _Count4Bin, value);
            }
        }

        private int _Count4Qty;
        [ImmediatePostData]
        [XafDisplayName("Count 4 Qty")]
        [Index(53), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int Count4Qty
        {
            get { return _Count4Qty; }
            set
            {
                SetPropertyValue("Count4Qty", ref _Count4Qty, value);
            }
        }

        private int _Count4Var;
        [ImmediatePostData]
        [XafDisplayName("Count 4 Var")]
        [Index(55), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int Count4Var
        {
            get { return _Count4Var; }
            set
            {
                SetPropertyValue("Count4Var", ref _Count4Var, value);
            }
        }

        private string _Count5Bin;
        [XafDisplayName("Count 5 Bin")]
        [Index(58), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Count5Bin", Enabled = false)]
        public string Count5Bin
        {
            get { return _Count5Bin; }
            set
            {
                SetPropertyValue("Count5Bin", ref _Count5Bin, value);
            }
        }

        private int _Count5Qty;
        [ImmediatePostData]
        [XafDisplayName("Count 5 Qty")]
        [Index(60), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int Count5Qty
        {
            get { return _Count5Qty; }
            set
            {
                SetPropertyValue("Count5Qty", ref _Count5Qty, value);
            }
        }

        private int _Count5Var;
        [ImmediatePostData]
        [XafDisplayName("Count 5 Var")]
        [Index(63), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public int Count5Var
        {
            get { return _Count5Var; }
            set
            {
                SetPropertyValue("Count5Var", ref _Count5Var, value);
            }
        }

        private StockCountVarianceInquiry _StockCountVarianceInquiry;
        [Association("StockCountVarianceInquiry-StockCountVarianceInquiryDetails")]
        [Index(99), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("StockCountVarianceInquiry", Enabled = false)]
        public StockCountVarianceInquiry StockCountVarianceInquiry
        {
            get { return _StockCountVarianceInquiry; }
            set { SetPropertyValue("StockCountVarianceInquiry", ref _StockCountVarianceInquiry, value); }
        }
    }
}