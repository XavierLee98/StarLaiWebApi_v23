namespace StarLaiPortal.Module.Controllers
{
    partial class ContainerControllers
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
            this.CancelContainer = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.PrintContainer = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            // 
            // CancelContainer
            // 
            this.CancelContainer.Caption = "Cancel";
            this.CancelContainer.Category = "ObjectsCreation";
            this.CancelContainer.ConfirmationMessage = null;
            this.CancelContainer.Id = "CancelContainer";
            this.CancelContainer.ToolTip = null;
            this.CancelContainer.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.CancelContainer_Execute);
            // 
            // PrintContainer
            // 
            this.PrintContainer.Caption = "Print";
            this.PrintContainer.Category = "ObjectsCreation";
            this.PrintContainer.ConfirmationMessage = null;
            this.PrintContainer.Id = "PrintContainer";
            this.PrintContainer.ToolTip = null;
            this.PrintContainer.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.PrintContainer_Execute);
            // 
            // ContainerControllers
            // 
            this.Actions.Add(this.CancelContainer);
            this.Actions.Add(this.PrintContainer);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.SimpleAction CancelContainer;
        private DevExpress.ExpressApp.Actions.SimpleAction PrintContainer;
    }
}
