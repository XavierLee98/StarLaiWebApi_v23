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

namespace StarLaiPortal.Module.BusinessObjects.Inquiry_View
{
    [NavigationItem("Reports")]
    [XafDisplayName("Business Partner")]
    [DefaultProperty("BPCode")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("hideSave", AppearanceItemType = "Action", TargetItems = "Save", Context = "Any", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    //[Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    //[Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    public class vwInquiryBusinessPartner : XPLiteObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public vwInquiryBusinessPartner(Session session)
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
        [XafDisplayName("BP Code")]
        [Appearance("BPCode", Enabled = false)]
        [Index(0)]
        public string BPCode
        {
            get; set;
        }

        [XafDisplayName("BP Name")]
        [Appearance("BPName", Enabled = false)]
        [Index(3)]
        public string BPName
        {
            get; set;
        }

        [XafDisplayName("CardType")]
        [Appearance("CardType", Enabled = false)]
        [Index(5)]
        public string CardType
        {
            get; set;
        }

        [XafDisplayName("ValidFor")]
        [Appearance("ValidFor", Enabled = false)]
        [Index(8)]
        public string ValidFor
        {
            get; set;
        }

        [XafDisplayName("BillToDef")]
        [Appearance("BillToDef", Enabled = false)]
        [Index(10)]
        public string BillToDef
        {
            get; set;
        }

        [XafDisplayName("ShipToDef")]
        [Appearance("ShipToDef", Enabled = false)]
        [Index(12)]
        public string ShipToDef
        {
            get; set;

        }

        [XafDisplayName("Contact")]
        [Appearance("Contact", Enabled = false)]
        [Index(13)]
        public string Contact
        {
            get; set;

        }

        [XafDisplayName("PaymentTerm")]
        [NoForeignKey]
        [Appearance("PaymentTerm", Enabled = false)]
        [Index(15)]
        public vwPaymentTerm PaymentTerm
        {
            get; set;
        }

        [XafDisplayName("ListNum")]
        [Appearance("ListNum", Enabled = false)]
        [Index(18)]
        public int ListNum
        {
            get; set;
        }

        [XafDisplayName("Currency")]
        [Appearance("Currency", Enabled = false)]
        [Index(20)]
        public string Currency
        {
            get; set;
        }

        [XafDisplayName("GroupCode")]
        [Appearance("GroupCode", Enabled = false)]
        [Index(23)]
        public int GroupCode
        {
            get; set;
        }

        [XafDisplayName("SlpCode")]
        [Appearance("SlpCode", Enabled = false)]
        [Index(25)]
        [NoForeignKey]
        public vwSalesPerson SlpCode
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        [Appearance("Transporter", Enabled = false)]
        [Index(28)]
        [NoForeignKey]
        public vwTransporter Transporter
        {
            get; set;
        }

        [XafDisplayName("SalesOrderSeries")]
        [Appearance("SalesOrderSeries", Enabled = false)]
        [Index(30)]
        public string SalesOrderSeries
        {
            get; set;
        }

        [XafDisplayName("GroupName")]
        [Appearance("GroupName", Enabled = false)]
        [Index(33)]
        public string GroupName
        {
            get; set;
        }

        [XafDisplayName("LeadTime")]
        [Appearance("LeadTime", Enabled = false)]
        [Index(35)]
        public int LeadTime
        {
            get; set;
        }

        [XafDisplayName("Create Date")]
        [Appearance("CreateDate", Enabled = false)]
        [Index(38)]
        public string CreateDate
        {
            get; set;
        }

        [XafDisplayName("Territory")]
        [Appearance("Territory", Enabled = false)]
        [Index(40)]
        public string Territory
        {
            get; set;
        }

        [XafDisplayName("Price List")]
        [Appearance("PriceList", Enabled = false)]
        [Index(43)]
        public string PriceList
        {
            get; set;
        }

        [XafDisplayName("U_blockSales")]
        [Appearance("U_blockSales", Enabled = false)]
        [Index(45)]
        public string U_blockSales
        {
            get; set;
        }

        [Index(88), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(true)]
        public string BoFullName
        {
            get { return BPCode + "-" + BPName; }
        }
    }
}