using Admiral.ImportData;
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

// 2023-08-25 remove validation for qty ver 1.0.9
// 2023-09-25 add copyto qty ver 1.0.10
// 2023-12-04 add outstanding qty ver 1.0.13
// 2024-01-29 change column label ver 1.0.14
// 2024-04-01 add foreignname field ver 1.0.15
// 2024-05-16 enhance speed - ver 1.0.15

namespace StarLaiPortal.Module.BusinessObjects.Advanced_Shipment_Notice
{
    [DefaultClassOptions]
    [UpdateImport("OIDKey")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    //[Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("LinkDoc", AppearanceItemType = "Action", TargetItems = "Link", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("UnlinkDoc", AppearanceItemType = "Action", TargetItems = "Unlink", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [XafDisplayName("ASN Details")]

    public class ASNDetails : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public ASNDetails(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
            ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;
            if (user != null)
            {
                CreateUser = Session.GetObjectByKey<ApplicationUser>(user.Oid);
            }
            else
            {
                CreateUser = Session.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
            }
            CreateDate = DateTime.Now;
        }

        private ApplicationUser _CreateUser;
        [XafDisplayName("Create User")]
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
        [Index(300), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public ApplicationUser CreateUser
        {
            get { return _CreateUser; }
            set
            {
                SetPropertyValue("CreateUser", ref _CreateUser, value);
            }
        }

        private DateTime? _CreateDate;
        [Index(301), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public DateTime? CreateDate
        {
            get { return _CreateDate; }
            set
            {
                SetPropertyValue("CreateDate", ref _CreateDate, value);
            }
        }

        private ApplicationUser _UpdateUser;
        [XafDisplayName("Update User"), ToolTip("Enter Text")]
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
        [Index(302), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public ApplicationUser UpdateUser
        {
            get { return _UpdateUser; }
            set
            {
                SetPropertyValue("UpdateUser", ref _UpdateUser, value);
            }
        }

        private DateTime? _UpdateDate;
        [Index(303), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public DateTime? UpdateDate
        {
            get { return _UpdateDate; }
            set
            {
                SetPropertyValue("UpdateDate", ref _UpdateDate, value);
            }
        }

        private string _PONo;
        [XafDisplayName("PO No.")]
        [Appearance("PONo", Enabled = false)]
        [Index(0), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string PONo
        {
            get { return _PONo; }
            set
            {
                SetPropertyValue("PONo", ref _PONo, value);
            }
        }

        private vwItemMasters _ItemCode;
        [ImmediatePostData]
        [NoForeignKey]
        [XafDisplayName("Item Code")]
        // Start ver 1.0.15
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        // End ver 1.0.15
        [Index(3), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemCode", Enabled = false)]
        [RuleRequiredField(DefaultContexts.Save)]
        public vwItemMasters ItemCode
        {
            get { return _ItemCode; }
            set
            {
                SetPropertyValue("ItemCode", ref _ItemCode, value);
                if (!IsLoading && value != null)
                {
                    LegacyItemCode = ItemCode.LegacyItemCode;
                    // Start ver 1.0.15
                    ForeignName = ItemCode.FrgnName;
                    // End ver 1.0.15
                }
                else if (!IsLoading && value != null)
                {
                    LegacyItemCode = null;
                    // Start ver 1.0.15
                    ForeignName = null;
                    // End ver 1.0.15
                }
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

        private string _ItemDesc;
        [RuleRequiredField(DefaultContexts.Save)]
        [XafDisplayName("Item Description")]
        [Appearance("ItemDesc", Enabled = false)]
        [Index(5), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string ItemDesc
        {
            get { return _ItemDesc; }
            set
            {
                SetPropertyValue("ItemDesc", ref _ItemDesc, value);
            }
        }

        private string _CatalogNo;
        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(8), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string CatalogNo
        {
            get { return _CatalogNo; }
            set
            {
                SetPropertyValue("CatalogNo", ref _CatalogNo, value);
            }
        }

        // Start ver 1.0.15
        private string _ForeignName;
        [XafDisplayName("Foreign Name")]
        [Index(9), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("ForeignName", Enabled = false)]
        public string ForeignName
        {
            get { return _ForeignName; }
            set
            {
                SetPropertyValue("ForeignName", ref _ForeignName, value);
            }
        }
        // End ver 1.0.15

        private string _UOM;
        [XafDisplayName("UOM Group")]
        [Appearance("UOM", Enabled = false)]
        [Index(10), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string UOM
        {
            get { return _UOM; }
            set
            {
                SetPropertyValue("UOM", ref _UOM, value);
            }
        }

        private vwWarehouse _Location;
        [NoForeignKey]
        [RuleRequiredField(DefaultContexts.Save)]
        [XafDisplayName("Loc")]
        [Appearance("Location", Enabled = false)]
        [Index(13), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public vwWarehouse Location
        {
            get { return _Location; }
            set
            {
                SetPropertyValue("Location", ref _Location, value);
            }
        }

        private vwBin _DefBin;
        [NoForeignKey]
        [XafDisplayName("Def Bin")]
        [Appearance("DefBin", Enabled = false)]
        [Index(15), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public vwBin DefBin
        {
            get { return _DefBin; }
            set
            {
                SetPropertyValue("DefBin", ref _DefBin, value);
            }
        }

        private decimal _Quantity;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "d")]
        // Start ver 1.0.14
        //[XafDisplayName("Open Quantity")]
        [XafDisplayName("Open Qty")]
        // End ver 1.0.14
        [Appearance("Quantity", Enabled = false)]
        [Index(18), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal Quantity
        {
            get { return _Quantity; }
            set
            {
                SetPropertyValue("Quantity", ref _Quantity, value);
            }
        }

        private decimal _UnloadQty;
        // Start ver 1.0.9
        // Start ver 1.0.13
        [ImmediatePostData]
        // End ver 1.0.13
        // End ver 1.0.9
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "d")]
        // Start ver 1.0.14
        //[XafDisplayName("Unload Qty")]
        [XafDisplayName("ASN Qty")]
        // End ver 1.0.14
        [Index(20), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal UnloadQty
        {
            get { return _UnloadQty; }
            set
            {
                SetPropertyValue("UnloadQty", ref _UnloadQty, value);
                // Start ver 1.0.9
                //if (!IsLoading)
                //{
                //    if (UnloadQty > Quantity)
                //    {
                //        UnloadQty = Quantity;
                //    }
                //}
                // End ver 1.0.9
                // Start ver 1.0.13
                if (!IsLoading)
                {
                    OutstandingQty = UnloadQty;
                }
                // End ver 1.0.13
            }
        }

        // Start ver 1.0.13
        private decimal _OutstandingQty;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "d")]
        [XafDisplayName("Outstanding Qty")]
        [Appearance("OutstandingQty", Enabled = false)]
        [Index(21), VisibleInListView(true), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public decimal OutstandingQty
        {
            get { return _OutstandingQty; }
            set
            {
                SetPropertyValue("OutstandingQty", ref _OutstandingQty, value);
            }
        }
        // End ver 1.0.13

        private string _PORefNo;
        [XafDisplayName("PO Ref. No.")]
        [Appearance("PORefNo", Enabled = false)]
        [Index(23), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string PORefNo
        {
            get { return _PORefNo; }
            set
            {
                SetPropertyValue("PORefNo", ref _PORefNo, value);
            }
        }

        private string _BaseDoc;
        [XafDisplayName("BaseDoc")]
        [Index(25), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public string BaseDoc
        {
            get { return _BaseDoc; }
            set
            {
                SetPropertyValue("BaseDoc", ref _BaseDoc, value);
            }
        }

        private string _BaseId;
        [XafDisplayName("BaseId")]
        [Index(28), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public string BaseId
        {
            get { return _BaseId; }
            set
            {
                SetPropertyValue("BaseId", ref _BaseId, value);
            }
        }

        private bool _LineClosed;
        [XafDisplayName("LineClosed")]
        [Index(29), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public bool LineClosed
        {
            get { return _LineClosed; }
            set
            {
                SetPropertyValue("LineClosed", ref _LineClosed, value);
            }
        }

        private int _LabelPrintCount;
        [XafDisplayName("LabelPrintCount")]
        [Index(30), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public int LabelPrintCount
        {
            get { return _LabelPrintCount; }
            set
            {
                SetPropertyValue("LabelPrintCount", ref _LabelPrintCount, value);
            }
        }

        // Start ver 1.0.10
        private decimal _CopyToQty;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "d")]
        [XafDisplayName("Copy To Qty")]
        [Appearance("CopyToQty", Enabled = false)]
        [Index(33), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public decimal CopyToQty
        {
            get { return _CopyToQty; }
            set
            {
                SetPropertyValue("CopyToQty", ref _CopyToQty, value);
            }
        }

        private decimal _CopyTotalQty;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "d")]
        [XafDisplayName("Copy To Qty")]
        [Appearance("CopyTotalQty", Enabled = false)]
        [Index(33), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public decimal CopyTotalQty
        {
            get { return _CopyTotalQty; }
            set
            {
                SetPropertyValue("CopyTotalQty", ref _CopyTotalQty, value);
            }
        }
        // End ver 1.0.10

        private int _OIDKey;
        [Index(80), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [XafDisplayName("OIDKey")]
        [Appearance("OIDKey", Enabled = false)]
        public int OIDKey
        {
            get { return _OIDKey; }
            set
            {
                SetPropertyValue("OIDKey", ref _OIDKey, value);
            }
        }

        [Browsable(false)]
        public bool IsNew
        {
            get
            { return Session.IsNewObject(this); }
        }

        // Start ver 1.0.10
        [Browsable(false)]
        public bool IsValid
        {
            get
            {
                if (CopyToQty < UnloadQty)
                {
                    return true;
                }

                return false;
            }
        }
        // End ver 1.0.10

        private ASN _ASN;
        [Association("ASN-ASNDetails")]
        [Index(99), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("ASN", Enabled = false)]
        public ASN ASN
        {
            get { return _ASN; }
            set { SetPropertyValue("ASN", ref _ASN, value); }
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (!(Session is NestedUnitOfWork)
                && (Session.DataLayer != null)
                    && (Session.ObjectLayer is SimpleObjectLayer)
                        )
            {
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;
                if (user != null)
                {
                    UpdateUser = Session.GetObjectByKey<ApplicationUser>(user.Oid);
                }
                else
                {
                    UpdateUser = Session.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                }
                UpdateDate = DateTime.Now;

                if (Session.IsNewObject(this))
                {

                }
            }
        }
    }
}