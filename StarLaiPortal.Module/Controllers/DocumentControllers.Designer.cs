namespace StarLaiPortal.Module.Controllers
{
    partial class DocumentControllers
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.DocumentDateFrom = new DevExpress.ExpressApp.Actions.ParametrizedAction(this.components);
            this.DocumentDateTo = new DevExpress.ExpressApp.Actions.ParametrizedAction(this.components);
            this.DocumentStatus = new DevExpress.ExpressApp.Actions.SingleChoiceAction(this.components);
            this.DocumentFilter = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            // 
            // DocumentDateFrom
            // 
            this.DocumentDateFrom.Caption = "From";
            this.DocumentDateFrom.Category = "ObjectsCreation";
            this.DocumentDateFrom.ConfirmationMessage = null;
            this.DocumentDateFrom.Id = "DocumentDateFrom";
            this.DocumentDateFrom.NullValuePrompt = null;
            this.DocumentDateFrom.ShortCaption = null;
            this.DocumentDateFrom.ToolTip = null;
            this.DocumentDateFrom.ValueType = typeof(System.DateTime);
            this.DocumentDateFrom.Execute += new DevExpress.ExpressApp.Actions.ParametrizedActionExecuteEventHandler(this.DocumentDateFrom_Execute);
            // 
            // DocumentDateTo
            // 
            this.DocumentDateTo.Caption = "To";
            this.DocumentDateTo.Category = "ObjectsCreation";
            this.DocumentDateTo.ConfirmationMessage = null;
            this.DocumentDateTo.Id = "DocumentDateTo";
            this.DocumentDateTo.NullValuePrompt = null;
            this.DocumentDateTo.ShortCaption = null;
            this.DocumentDateTo.ToolTip = null;
            this.DocumentDateTo.ValueType = typeof(System.DateTime);
            this.DocumentDateTo.Execute += new DevExpress.ExpressApp.Actions.ParametrizedActionExecuteEventHandler(this.DocumentDateTo_Execute);
            // 
            // DocumentFilter
            // 
            this.DocumentFilter.Caption = "Filter";
            this.DocumentFilter.Category = "ObjectsCreation";
            this.DocumentFilter.ConfirmationMessage = null;
            this.DocumentFilter.Id = "DocumentFilter";
            this.DocumentFilter.ToolTip = null;
            this.DocumentFilter.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.DocumentFilter_Execute);
            // 
            // DocumentStatus
            // 
            this.DocumentStatus.Caption = "Status";
            this.DocumentStatus.Category = "ObjectsCreation";
            this.DocumentStatus.ConfirmationMessage = null;
            this.DocumentStatus.Id = "DocumentStatus";
            this.DocumentStatus.ToolTip = null;
            this.DocumentStatus.Execute += new DevExpress.ExpressApp.Actions.SingleChoiceActionExecuteEventHandler(this.DocumentStatus_Execute);
            // 
            // DocumentControllers
            // 
            this.Actions.Add(this.DocumentDateFrom);
            this.Actions.Add(this.DocumentDateTo);
            this.Actions.Add(this.DocumentFilter);
            this.Actions.Add(this.DocumentStatus);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.ParametrizedAction DocumentDateFrom;
        private DevExpress.ExpressApp.Actions.ParametrizedAction DocumentDateTo;
        private DevExpress.ExpressApp.Actions.SimpleAction DocumentFilter;
        private DevExpress.ExpressApp.Actions.SingleChoiceAction DocumentStatus;
    }
}
