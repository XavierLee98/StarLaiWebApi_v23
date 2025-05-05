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
    [DefaultClassOptions]
    [XafDisplayName("Global Item Inquiry")]
    [NavigationItem("Reports")]

    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("SaveDocRecord", AppearanceItemType = "Action", TargetItems = "Save", Context = "Any", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("SaveAndCloseDocRecord", AppearanceItemType = "Action", TargetItems = "SaveAndClose", Context = "Any", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "Cancel", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    public class GlobalItemInquiry : XPObject
    {
        public GlobalItemInquiry(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        private string _Search;
        [XafDisplayName("Search")]
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public string Search
        {
            get { return _Search; }
            set
            {
                SetPropertyValue("Search", ref _Search, value);
            }
        }

        private string _Exclude;
        [XafDisplayName("Exclude")]
        [Index(1), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public string Exclude
        {
            get { return _Exclude; }
            set
            {
                SetPropertyValue("Exclude", ref _Exclude, value);
            }
        }

        private SearchMethod _Method;
        [XafDisplayName("Method")]
        [Index(15), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public SearchMethod Method
        {
            get { return _Method; }
            set
            {
                SetPropertyValue("Method", ref _Method, value);
            }
        }

        private string _OldCode;
        [XafDisplayName("Old Code")]
        [Index(18), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public string OldCode
        {
            get { return _OldCode; }
            set
            {
                SetPropertyValue("OldCode", ref _OldCode, value);
            }
        }

        private string _CatalogNumber;
        [XafDisplayName("Catalog Number")]
        [Index(22), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public string CatalogNumber
        {
            get { return _CatalogNumber; }
            set
            {
                SetPropertyValue("CatalogNumber", ref _CatalogNumber, value);
            }
        }

        [Association("GlobalItemInquiry-GlobalItemInquiryDetails")]
        [XafDisplayName("Items")]
        public XPCollection<GlobalItemInquiryDetails> GlobalItemInquiryDetails
        {
            get { return GetCollection<GlobalItemInquiryDetails>("GlobalItemInquiryDetails"); }
        }
    }
}