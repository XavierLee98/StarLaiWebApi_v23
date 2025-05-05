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
using System.Text;

// 2024-06-12 e-invoice - ver 1.0.18

namespace StarLaiPortal.Module.BusinessObjects.View
{
    [DefaultClassOptions]
    [NavigationItem("SAP")]
    [XafDisplayName("Billing Address")]
    [DefaultProperty("AddressKey")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("hideSave", AppearanceItemType = "Action", TargetItems = "Save", Context = "Any", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    //[Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    //[Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    public class vwBillingAddress : XPLiteObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public vwBillingAddress(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        [Key]
        [Browsable(true)]
        [XafDisplayName("PriKey")]
        [Appearance("PriKey", Enabled = false)]
        [Index(0)]
        public string PriKey
        {
            get; set;
        }

        [XafDisplayName("CardCode")]
        [Appearance("CardCode", Enabled = false)]
        [Index(3)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("AddressKey")]
        [Appearance("AddressKey", Enabled = false)]
        [Index(5)]
        public string AddressKey
        {
            get; set;
        }

        [XafDisplayName("Address")]
        [Appearance("Address", Enabled = false)]
        [Index(8)]
        public string Address
        {
            get; set;
        }

        // Start ver 1.0.18
        [XafDisplayName("Street")]
        [Appearance("Street", Enabled = false)]
        [Index(10)]
        public string Street
        {
            get; set;
        }

        [XafDisplayName("Block")]
        [Appearance("Block", Enabled = false)]
        [Index(13)]
        public string Block
        {
            get; set;
        }

        [XafDisplayName("City")]
        [Appearance("City", Enabled = false)]
        [Index(15)]
        public string City
        {
            get; set;
        }

        [XafDisplayName("County")]
        [Appearance("County", Enabled = false)]
        [Index(16)]
        public string County
        {
            get; set;
        }

        [XafDisplayName("State")]
        [Appearance("State", Enabled = false)]
        [Index(18)]
        public string State
        {
            get; set;
        }

        [XafDisplayName("Country")]
        [Appearance("Country", Enabled = false)]
        [Index(20)]
        public string Country
        {
            get; set;
        }

        [XafDisplayName("ZipCode")]
        [Appearance("ZipCode", Enabled = false)]
        [Index(23)]
        public string ZipCode
        {
            get; set;
        }
        //End ver 1.0.18
    }
}