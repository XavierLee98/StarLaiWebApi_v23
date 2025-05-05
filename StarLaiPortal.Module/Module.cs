using System;
using System.Text;
using System.Linq;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using System.Collections.Generic;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.DomainLogics;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;

// 2024-01-30 - allow Show Persistent Objects in a Non-Persistent Object - ver 1.0.14

namespace StarLaiPortal.Module {
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ModuleBase.
    public sealed partial class StarLaiPortalModule : ModuleBase {
        public StarLaiPortalModule() {
            InitializeComponent();
        }
        public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB) {
            ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
            return new ModuleUpdater[] { updater };
        }

        // Start ver 1.0.14
        private void Application_ObjectSpaceCreated(object sender, ObjectSpaceCreatedEventArgs e)
        {
            CompositeObjectSpace compositeObjectSpace = e.ObjectSpace as CompositeObjectSpace;
            if (compositeObjectSpace != null)
            {
                if (!(compositeObjectSpace.Owner is CompositeObjectSpace))
                {
                    compositeObjectSpace.PopulateAdditionalObjectSpaces((XafApplication)sender);
                }
            }
        }
        // End ver 1.0.14
        public override void Setup(XafApplication application) {
            base.Setup(application);
            // Manage various aspects of the application UI and behavior at the module level.

            // Start ver 1.0.14
            #region Show Persistent Objects in a Non-Persistent Object
            application.ObjectSpaceCreated += Application_ObjectSpaceCreated;
            #endregion
            // End ver 1.0.14
        }
        public override void CustomizeTypesInfo(ITypesInfo typesInfo) {
            base.CustomizeTypesInfo(typesInfo);
            CalculatedPersistentAliasHelper.CustomizeTypesInfo(typesInfo);

            #region hide Oid
            var xpObjectTypeInfo = typesInfo.FindTypeInfo(typeof(DevExpress.Xpo.XPObject)) as TypeInfo;
            if (xpObjectTypeInfo != null)
            {
                var oidMemberInfo = xpObjectTypeInfo.FindMember(nameof(DevExpress.Xpo.XPObject.Oid));
                oidMemberInfo.AddAttribute(new VisibleInDetailViewAttribute(false));
                oidMemberInfo.AddAttribute(new VisibleInListViewAttribute(false));
                oidMemberInfo.AddAttribute(new VisibleInLookupListViewAttribute(false));
                ((XafMemberInfo)oidMemberInfo).Refresh();
            }
            #endregion
        }
    }
}
