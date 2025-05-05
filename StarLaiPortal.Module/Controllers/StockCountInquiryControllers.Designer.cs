namespace StarLaiPortal.Module.Controllers
{
    partial class StockCountInquiryControllers
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
            this.StockCountBinSearch = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.StockCountItemSearch = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.StockCountVarianceSearch = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            // 
            // StockCountBinSearch
            // 
            this.StockCountBinSearch.Caption = "Search";
            this.StockCountBinSearch.Category = "ListView";
            this.StockCountBinSearch.ConfirmationMessage = null;
            this.StockCountBinSearch.Id = "StockCountBinSearch";
            this.StockCountBinSearch.ToolTip = null;
            this.StockCountBinSearch.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.StockCountBinSearch_Execute);
            // 
            // StockCountItemSearch
            // 
            this.StockCountItemSearch.Caption = "Search";
            this.StockCountItemSearch.Category = "ListView";
            this.StockCountItemSearch.ConfirmationMessage = null;
            this.StockCountItemSearch.Id = "StockCountItemSearch";
            this.StockCountItemSearch.ToolTip = null;
            this.StockCountItemSearch.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.StockCountItemSearch_Execute);
            // 
            // StockCountVarianceSearch
            // 
            this.StockCountVarianceSearch.Caption = "Search";
            this.StockCountVarianceSearch.Category = "ListView";
            this.StockCountVarianceSearch.ConfirmationMessage = null;
            this.StockCountVarianceSearch.Id = "StockCountVarianceSearch";
            this.StockCountVarianceSearch.ToolTip = null;
            this.StockCountVarianceSearch.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.StockCountVarianceSearch_Execute);
            // 
            // StockCountInquiryControllers
            // 
            this.Actions.Add(this.StockCountBinSearch);
            this.Actions.Add(this.StockCountItemSearch);
            this.Actions.Add(this.StockCountVarianceSearch);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.SimpleAction StockCountBinSearch;
        private DevExpress.ExpressApp.Actions.SimpleAction StockCountItemSearch;
        private DevExpress.ExpressApp.Actions.SimpleAction StockCountVarianceSearch;
    }
}
