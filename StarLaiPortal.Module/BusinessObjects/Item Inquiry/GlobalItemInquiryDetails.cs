using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;

namespace StarLaiPortal.Module.BusinessObjects.Item_Inquiry
{
    [DefaultProperty("ItemCode")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("LinkDoc", AppearanceItemType = "Action", TargetItems = "Link", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("UnlinkDoc", AppearanceItemType = "Action", TargetItems = "Unlink", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]

    [XafDisplayName("Global Item Inquiry Details")]
    public class GlobalItemInquiryDetails : XPObject
    { 
        public GlobalItemInquiryDetails(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        private string _ItemCode;
        [XafDisplayName("Item Code")]
        [Index(0), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemCode", Enabled = false)]
        public string ItemCode
        {
            get { return _ItemCode; }
            set
            {
                SetPropertyValue("ItemCode", ref _ItemCode, value);
            }
        }

        private string _ItemDesc;
        [XafDisplayName("Item Description")]
        [Index(3), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemDesc", Enabled = false)]
        public string ItemDesc

        {
            get { return _ItemDesc; }
            set
            {
                SetPropertyValue("ItemDesc", ref _ItemDesc, value);
            }
        }

        private string _LegacyItemCode;
        [XafDisplayName("Legacy Item Code")]
        [Index(4), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("LegacyItemCode", Enabled = false)]
        public string LegacyItemCode
        {
            get { return _LegacyItemCode; }
            set
            {
                SetPropertyValue("LegacyItemCode", ref _LegacyItemCode, value);
            }

        }

        private string _Model;
        [XafDisplayName("Model")]
        [Index(5), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Model", Enabled = false)]
        public string Model

        {
            get { return _Model; }
            set
            {
                SetPropertyValue("Model", ref _Model, value);
            }
        }

        private string _OriginalCatalog;
        [XafDisplayName("Original Catalog No.")]
        [Index(23), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("OriginalCatalog", Enabled = false)]
        public string OriginalCatalog

        {
            get { return _OriginalCatalog; }
            set
            {
                SetPropertyValue("OriginalCatalog", ref _OriginalCatalog, value);
            }
        }

        private string _HierarchyLevel1;
        [XafDisplayName("Hierarchy Level 1")]
        [Index(24), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("HierarchyLevel1", Enabled = false)]
        public string HierarchyLevel1

        {
            get { return _HierarchyLevel1; }
            set
            {
                SetPropertyValue("HierarchyLevel1", ref _HierarchyLevel1, value);
            }
        }

        private string _HierarchyLevel2;
        [XafDisplayName("Hierarchy Level 2")]
        [Index(25), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("HierarchyLevel2", Enabled = false)]
        public string HierarchyLevel2

        {
            get { return _HierarchyLevel2; }
            set
            {
                SetPropertyValue("HierarchyLevel2", ref _HierarchyLevel2, value);
            }
        }

        private string _HierarchyLevel3;
        [XafDisplayName("Hierarchy Level 3")]
        [Index(26), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("HierarchyLevel3", Enabled = false)]
        public string HierarchyLevel3

        {
            get { return _HierarchyLevel3; }
            set
            {
                SetPropertyValue("HierarchyLevel3", ref _HierarchyLevel3, value);
            }
        }

        private string _HierarchyLevel4;
        [XafDisplayName("Hierarchy Level 4")]
        [Index(28), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("HierarchyLevel4", Enabled = false)]
        public string HierarchyLevel4

        {
            get { return _HierarchyLevel4; }
            set
            {
                SetPropertyValue("HierarchyLevel4", ref _HierarchyLevel4, value);
            }
        }

        private string _HierarchyLevel5;
        [XafDisplayName("Hierarchy Level 5")]
        [Index(30), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("HierarchyLevel5", Enabled = false)]
        public string HierarchyLevel5

        {
            get { return _HierarchyLevel5; }
            set
            {
                SetPropertyValue("HierarchyLevel5", ref _HierarchyLevel5, value);
            }
        }

        private string _Remarks;
        [XafDisplayName("Remarks")]
        [Index(33), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("Remarks", Enabled = false)]
        public string Remarks

        {
            get { return _Remarks; }
            set
            {
                SetPropertyValue("Remarks", ref _Remarks, value);
            }
        }

        private string _Barcode;
        [XafDisplayName("Barcode")]
        [Index(35), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("Barcode", Enabled = false)]
        public string Barcode

        {
            get { return _Barcode; }
            set
            {
                SetPropertyValue("Barcode", ref _Barcode, value);
            }
        }

        private string _ForeignName;
        [XafDisplayName("Foreign Name")]
        [Index(38), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("ForeignName", Enabled = false)]
        public string ForeignName
        {
            get { return _ForeignName; }
            set
            {
                SetPropertyValue("ForeignName", ref _ForeignName, value);
            }
        }

        private string _BPCatalogNo;
        [XafDisplayName("BP Catalog No")]
        [Index(38), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("BPCatalogNo", Enabled = false)]
        public string BPCatalogNo
        {
            get { return _BPCatalogNo; }
            set
            {
                SetPropertyValue("BPCatalogNo", ref _BPCatalogNo, value);
            }
        }

        private string _Origin;
        [XafDisplayName("Origin")]
        [Index(39), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("Origin", Enabled = false)]
        public string Origin
        {
            get { return _Origin; }
            set
            {
                SetPropertyValue("Origin", ref _Origin, value);
            }
        }

        private string _ProdName;
        [XafDisplayName("Prod Name")]
        [Index(50), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("ProdName", Enabled = false)]
        [DbType("nvarchar(MAX)")]
        public string ProdName
        {
            get { return _ProdName; }
            set
            {
                SetPropertyValue("ProdName", ref _ProdName, value);
            }
        }

        private string _ProdModel;
        [XafDisplayName("Prod Model")]
        [Index(51), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("ProdModel", Enabled = false)]
        [DbType("nvarchar(MAX)")]
        public string ProdModel
        {
            get { return _ProdModel; }
            set
            {
                SetPropertyValue("ProdModel", ref _ProdModel, value);
            }
        }

        private string _ProdOrigin;
        [XafDisplayName("Prod Origin")]
        [Index(52), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("ProdOrigin", Enabled = false)]
        [DbType("nvarchar(MAX)")]
        public string ProdOrigin
        {
            get { return _ProdOrigin; }
            set
            {
                SetPropertyValue("ProdOrigin", ref _ProdOrigin, value);
            }
        }

        private string _TariffCode;
        [XafDisplayName("Tariff Code")]
        [Index(53), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("TariffCode", Enabled = false)]
        [DbType("nvarchar(MAX)")]
        public string TariffCode
        {
            get { return _TariffCode; }
            set
            {
                SetPropertyValue("TariffCode", ref _TariffCode, value);
            }
        }

        private string _PictureName;
        [XafDisplayName("Picture Name")]
        [Index(70), VisibleInListView(true), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("PictureName", Enabled = false)]
        public string PictureName
        {
            get { return _PictureName; }
            set
            {
                SetPropertyValue("PictureName", ref _PictureName, value);
            }
        }

        private GlobalItemInquiry _GlobalItemInquiry;
        [Association("GlobalItemInquiry-GlobalItemInquiryDetails")]
        [Index(99), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("GlobalItemInquiry", Enabled = false)]
        public GlobalItemInquiry GlobalItemInquiry
        {
            get { return _GlobalItemInquiry; }
            set { SetPropertyValue("GlobalItemInquiry", ref _GlobalItemInquiry, value); }
        }
    }
}