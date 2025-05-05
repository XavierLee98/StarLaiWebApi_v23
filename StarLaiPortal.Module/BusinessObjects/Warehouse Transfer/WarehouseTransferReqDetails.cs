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

// 2023-08-25 - export and import function - ver 1.0.9
// 2023-10-16 - add legacyitemcode - ver 1.0.11
// 2024-01-29 - add available qty and variance - ver 1.0.14
// 2024-04-04 - remove stockbalance view - ver 1.0.15
// 2024-06-01 - remove AllItems 1.0.17

namespace StarLaiPortal.Module.BusinessObjects.Warehouse_Transfer
{
    [DefaultClassOptions]
    //[Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    //[Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("LinkDoc", AppearanceItemType = "Action", TargetItems = "Link", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("UnlinkDoc", AppearanceItemType = "Action", TargetItems = "Unlink", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [XafDisplayName("Warehouse Transfer Request Details")]
    public class WarehouseTransferReqDetails : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public WarehouseTransferReqDetails(Session session)
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
            Quantity = 1;
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

        private vwItemMasters _ItemCode;
        [ImmediatePostData]
        [NoForeignKey]
        // Start ver 1.0.17
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        // End ver 1.0.17
        [XafDisplayName("ItemCode")]
        [DataSourceCriteria("frozenFor = 'N'")]
        [Index(0), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ItemCode", Enabled = false, Criteria ="Not IsNew")]
        [RuleRequiredField(DefaultContexts.Save)]
        public vwItemMasters ItemCode

        {
            get { return _ItemCode; }
            set
            {
                SetPropertyValue("ItemCode", ref _ItemCode, value);
                if (!IsLoading && value != null)
                {
                    // Start ver 1.0.9
                    if (this.FromWarehouse != null)
                    {
                        FromBin = Session.FindObject<vwBinStockBalance>(CriteriaOperator.Parse("BinAbs = ? and ItemCode = ? and Warehouse = ?",
                            FromWarehouse.DftBinAbs, ItemCode.ItemCode, FromWarehouse.WarehouseCode));

                        // Start ver 1.0.14
                        // Start ver 1.0.15
                        //Available = Session.FindObject<vwStockBalance>(CriteriaOperator.Parse("ItemCode = ? and WhsCode = ?",
                        //    ItemCode, FromWarehouse.WarehouseCode));

                        vwStockBalance avail = Session.FindObject<vwStockBalance>(CriteriaOperator.Parse("ItemCode = ? and WhsCode = ?",
                            ItemCode, FromWarehouse.WarehouseCode));

                        if (avail != null)
                        {
                            Available = (decimal)avail.InStock;
                        }
                        else
                        {
                            Available = 0;
                        }
                        // End ver 1.0.15
                        // End ver 1.0.14
                    }
                    // End ver 1.0.9

                    ItemDesc = ItemCode.ItemName;
                    // Start ver 1.0.11
                    LegacyItemCode = ItemCode.LegacyItemCode;
                    // End ver 1.0.11
                    CatalogNo = ItemCode.CatalogNo;
                    UOM = ItemCode.UOM;


                }
                else if (!IsLoading && value == null)
                {
                    ItemDesc = null;
                    // Start ver 1.0.11
                    LegacyItemCode = null;
                    // End ver 1.0.11
                    CatalogNo = null;
                    UOM = null;
                    // Start ver 1.0.15
                    Available = 0;
                    // End ver 1.0.15
                }
            }
        }

        // Start ver 1.0.11
        private string _LegacyItemCode;
        [XafDisplayName("Legacy Item Code")]
        [Index(2), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("LegacyItemCode", Enabled = false)]
        public string LegacyItemCode
        {
            get { return _LegacyItemCode; }
            set
            {
                SetPropertyValue("LegacyItemCode", ref _LegacyItemCode, value);
            }
        }
        // End ver 1.0.11

        private string _ItemDesc;
        [RuleRequiredField(DefaultContexts.Save)]
        [XafDisplayName("Item Description")]
        [Appearance("ItemDesc", Enabled = false)]
        [Index(3), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string ItemDesc
        {
            get { return _ItemDesc; }
            set
            {
                SetPropertyValue("ItemDesc", ref _ItemDesc, value);
            }
        }

        private string _UOM;
        [XafDisplayName("UOM Group")]
        [Appearance("UOM", Enabled = false)]
        [Index(5), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string UOM
        {
            get { return _UOM; }
            set
            {
                SetPropertyValue("UOM", ref _UOM, value);
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

        private vwBinStockBalance _FromBin;
        [NoForeignKey]
        // Start ver 1.0.17
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        // End ver 1.0.17
        [XafDisplayName("From Bin")]
        [DataSourceCriteria("Warehouse = '@this.FromWarehouse.WarehouseCode' and ItemCode = '@this.ItemCode.ItemCode'")]
        [Index(10), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public vwBinStockBalance FromBin
        {
            get { return _FromBin; }
            set
            {
                SetPropertyValue("FromBin", ref _FromBin, value);
            }
        }

        private vwBin _ToBin;
        [NoForeignKey]
        [ImmediatePostData]
        // Start ver 1.0.17
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        // End ver 1.0.17
        [XafDisplayName("To Bin")]
        [DataSourceCriteria("Warehouse = '@this.ToWarehouse.WarehouseCode'")]
        [Index(13), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public vwBin ToBin
        {
            get { return _ToBin; }
            set
            {
                SetPropertyValue("ToBin", ref _ToBin, value);
            }
        }

        private decimal _Quantity;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "d")]
        [XafDisplayName("Quantity")]
        [Index(15), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal Quantity
        {
            get { return _Quantity; }
            set
            {
                SetPropertyValue("Quantity", ref _Quantity, value);
            }
        }

        // Start ver 1.0.14
        // Start ver 1.0.15
        //private vwStockBalance _Available;
        private decimal _Available;
        [ImmediatePostData]
        //[NoForeignKey]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "{0:N0}")]
        // End ver 1.0.15
        [XafDisplayName("Available")]
        [Appearance("Available", Enabled = false)]
        [Index(16), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        // Start ver 1.0.15
        //public vwStockBalance Available
        public decimal Available
        // End ver 1.0.15
        {
            get { return _Available; }
            set
            {
                SetPropertyValue("Available", ref _Available, value);
                // Start ver 1.0.15
                //if (!IsLoading && value != null)
                //{
                //    Variance = (decimal)Available.InStock - Quantity;
                //}
                //else if (!IsLoading && value == null)
                //{
                //    Variance = 0 - Quantity;
                //}

                if (!IsLoading && value != 0)
                {
                    Variance = Available - Quantity;
                }
                else if (!IsLoading && value == 0)
                {
                    Variance = 0 - Quantity;
                }
                // End ver 1.0.15
            }
        }

        private decimal _Variance;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "d")]
        [XafDisplayName("Variance")]
        [Appearance("Variance", Enabled = false)]
        [Index(17), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal Variance
        {
            get { return _Variance; }
            set
            {
                SetPropertyValue("Variance", ref _Variance, value);
            }
        }
        // End ver 1.0.14

        private vwWarehouse _FromWarehouse;
        [NoForeignKey]
        [ImmediatePostData]
        // Start ver 1.0.17
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        // End ver 1.0.17
        // Start ver 1.0.9
        //[RuleRequiredField(DefaultContexts.Save)]
        // End ver 1.0.9
        [XafDisplayName("From Warehouse")]
        [Index(18), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public vwWarehouse FromWarehouse
        {
            get { return _FromWarehouse; }
            set
            {
                SetPropertyValue("FromWarehouse", ref _FromWarehouse, value);
                if (!IsLoading && value != null)
                {
                    if (ItemCode != null)
                    {
                        FromBin = Session.FindObject<vwBinStockBalance>(CriteriaOperator.Parse("BinAbs = ? and ItemCode = ? and Warehouse = ?", 
                            FromWarehouse.DftBinAbs, ItemCode.ItemCode, FromWarehouse.WarehouseCode));
                    }
                }
            }
        }

        private vwWarehouse _ToWarehouse;
        [NoForeignKey]
        [ImmediatePostData]
        // Start ver 1.0.17
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        // End ver 1.0.17
        // Start ver 1.0.9
        //[RuleRequiredField(DefaultContexts.Save)]
        // End ver 1.0.9
        [XafDisplayName("To Warehouse")]
        [Index(20), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public vwWarehouse ToWarehouse
        {
            get { return _ToWarehouse; }
            set
            {
                SetPropertyValue("ToWarehouse", ref _ToWarehouse, value);
                if (!IsLoading && value != null)
                {
                    ToBin = Session.FindObject<vwBin>(CriteriaOperator.Parse("AbsEntry = ?", ToWarehouse.DftBinAbs));
                }
            }
        }

        [Browsable(false)]
        public bool IsNew
        {
            get
            { return Session.IsNewObject(this); }
        }

        private WarehouseTransferReq _WarehouseTransferReq;
        [Association("WarehouseTransferReq-WarehouseTransferReqDetails")]
        [Index(99), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("WarehouseTransferReq", Enabled = false)]
        public WarehouseTransferReq WarehouseTransferReq
        {
            get { return _WarehouseTransferReq; }
            set { SetPropertyValue("WarehouseTransferReq", ref _WarehouseTransferReq, value); }
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