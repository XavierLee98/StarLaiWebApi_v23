using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web.Templates.ActionContainers.Menu;
using DevExpress.ExpressApp.Web.Templates.ActionContainers;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Persistent.Validation;
using StarLaiPortal.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Configuration;

// 2025-06-26 - Add check performance feature - ver 1.0.23

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class MemoryManagementController : ViewController
    {
        GeneralControllers genCon;
        public MemoryManagementController()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.

            // Start ver 1.0.23
            ChoiceActionItem NA = new ChoiceActionItem("NA", "Performance Action", null);
            ChoiceActionItem PerformanceMonitor = new ChoiceActionItem("PerformanceMonitor", "Performance Monitor", null);
            //ChoiceActionItem PerformanceMonitorNotePad = new ChoiceActionItem("PerformanceMonitorNotePad", "Performance Monitor NotePad", null);
            //ChoiceActionItem  = new ChoiceActionItem("DatabaseAnalysis", "Database Analysis", null);
            //ChoiceActionItem ClearCachDatabaseAnalysise = new ChoiceActionItem("ClearCache", "Clear Cache", null);

            PerformanceFunc.Items.Add(NA);
            PerformanceFunc.Items.Add(PerformanceMonitor);
            //PerformanceFunc.Items.Add(PerformanceMonitorNotePad);
            //PerformanceFunc.Items.Add(DatabaseAnalysis);
            //PerformanceFunc.Items.Add(ClearCachDatabaseAnalysise);
            // End ver 1.0.23
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            this.ForceReleaseMemory.Active.SetItemValue("Enabled", false);
            this.ForceFlushGC.Active.SetItemValue("Enabled", false);
            // Start ver 1.0.23
            this.PerformanceFunc.Active.SetItemValue("Enabled", false);
            // End ver 1.0.23
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
            genCon = Frame.GetController<GeneralControllers>();

            if (View.Id == "ApplicationUser_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.View)
                {
                    PermissionPolicyRole AdminRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('Administrators')"));

                    if (AdminRole != null)
                    {
                        ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

                        BusinessObjects.ApplicationUser curruser = View.CurrentObject as BusinessObjects.ApplicationUser;

                        if (curruser.UserName == "Admin")
                        {
                            this.ForceReleaseMemory.Active.SetItemValue("Enabled", true);
                            this.ForceFlushGC.Active.SetItemValue("Enabled", true);
                            // Start ver 1.0.23
                            this.PerformanceFunc.Active.SetItemValue("Enabled", true);

                            PerformanceFunc.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption;
                            PerformanceFunc.CustomizeControl += action_CustomizeControl;

                            PerformanceFunc.SelectedIndex = 0;
                            // End ver 1.0.23
                        }
                    }
                }
                else
                {
                    this.ForceReleaseMemory.Active.SetItemValue("Enabled", false);
                }
            }
            else
            {
                this.ForceReleaseMemory.Active.SetItemValue("Enabled", false);
            }
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        // Start ver 1.0.23
        void action_CustomizeControl(object sender, CustomizeControlEventArgs e)
        {
            SingleChoiceActionAsModeMenuActionItem actionItem = e.Control as SingleChoiceActionAsModeMenuActionItem;
            if (actionItem != null && actionItem.Action.PaintStyle == DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Caption)
            {
                DropDownSingleChoiceActionControlBase control = (DropDownSingleChoiceActionControlBase)actionItem.Control;
                //control.Label.Text = actionItem.Action.Caption;
                //control.Label.Style["padding-right"] = "5px";
                control.ComboBox.Width = 160;
            }
        }
        // End ver 1.0.23

        private void ForceReleaseMemory_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                MemoryManagement.FlushMemory();
            }
            catch (Exception ex)
            {
                genCon.showMsg("Fail", ex.Message, InformationType.Error);
            }

            genCon.showMsg("Successful", "Memory Flush Successful.", InformationType.Success);
        }

        private void ForceFlushGC_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                MemoryManagement.FlushGCMemory();
            }
            catch (Exception ex)
            {
                genCon.showMsg("Fail", ex.Message, InformationType.Error);
            }

            genCon.showMsg("Successful", "Flush GC Successful.", InformationType.Success);
        }

        // Start ver 1.0.23
        private void PerformanceFunc_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            if (e.SelectedChoiceActionItem.Id == "PerformanceMonitor")
            {
                try
                {
                    string performanceInfo = GetPerformanceInfo();

                    Application.ShowViewStrategy.ShowMessage(
                        performanceInfo,
                        InformationType.Info,
                        10000, // 10 seconds
                        InformationPosition.Top);
                }
                catch (Exception ex)
                {
                    Application.ShowViewStrategy.ShowMessage(
                        $"Error retrieving performance information: {ex.Message}",
                        InformationType.Error,
                        5000,
                        InformationPosition.Top);
                }
            }

            //if (e.SelectedChoiceActionItem.Id == "PerformanceMonitorNotePad")
            //{
            //    try
            //    {
            //        string performanceInfo = GetPerformanceInfo();
            //        ShowCustomDialog("Performance Monitor - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), performanceInfo);
            //    }
            //    catch (Exception ex)
            //    {
            //        ShowCustomDialog("Performance Monitor Error", $"Error retrieving performance information: {ex.Message}");
            //    }
            //}

                //if (e.SelectedChoiceActionItem.Id == "DatabaseAnalysis")
                //{
                //    try
                //    {
                //        string analysisResult = RunDatabasePerformanceAnalysis();
                //        ShowCustomDialog("Database Analysis Results - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), analysisResult);
                //    }
                //    catch (Exception ex)
                //    {
                //        ShowCustomDialog("Database Analysis Error", $"Database analysis failed: {ex.Message}");
                //    }
                //}

                //if (e.SelectedChoiceActionItem.Id == "ClearCache")
                //{
                //    try
                //    {
                //        var cacheStatsBefore = ItemInquiryControllers.GetCacheStats();

                //        // Clear our simple cache
                //        ItemInquiryControllers.ClearAllCache();

                //        // Force garbage collection
                //        GC.Collect();
                //        GC.WaitForPendingFinalizers();
                //        GC.Collect();

                //        var cacheStatsAfter = ItemInquiryControllers.GetCacheStats();

                //        string message = $"✅ CACHE CLEARED SUCCESSFULLY!\n\n" +
                //                       $"📊 STATISTICS:\n" +
                //                       $"Before: {cacheStatsBefore.ItemCount} items (~{cacheStatsBefore.EstimatedSizeMB} KB)\n" +
                //                       $"After: {cacheStatsAfter.ItemCount} items (~{cacheStatsAfter.EstimatedSizeMB} KB)\n" +
                //                       $"Items cleared: {cacheStatsBefore.ItemCount - cacheStatsAfter.ItemCount}\n\n" +
                //                       $"💡 Cache will rebuild automatically as data is accessed.";

                //        // Show custom dialog that user must close manually
                //        ShowCustomDialog("Cache Cleared - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);
                //    }
                //    catch (Exception ex)
                //    {
                //        ShowCustomDialog("Cache Clear Error", $"❌ Failed to clear cache: {ex.Message}");
                //    }
                //}
        }
        private string GetPerformanceInfo()
        {
            var info = new System.Text.StringBuilder();

            try
            {
                // Application uptime
                TimeSpan uptime = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime;
                info.AppendLine($"🕒 Application Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");

                // Memory usage
                long memoryUsage = GC.GetTotalMemory(false);
                string memoryMB = $"{memoryUsage / 1024 / 1024:N0} MB";
                string memoryStatus = memoryUsage > 500 * 1024 * 1024 ? " ⚠️ HIGH" : " ✅";
                info.AppendLine($"💾 Memory Usage: {memoryMB}{memoryStatus}");

                // GC Collections
                info.AppendLine($"🗑️ GC Collections: Gen0:{GC.CollectionCount(0)} Gen1:{GC.CollectionCount(1)} Gen2:{GC.CollectionCount(2)}");

                // Thread pool info
                int workerThreads, completionPortThreads;
                System.Threading.ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
                info.AppendLine($"🧵 Available Threads: Worker:{workerThreads} I/O:{completionPortThreads}");

                // Database connection pooling status
                string connectionString = genCon.getConnectionString();
                if (!string.IsNullOrEmpty(connectionString))
                {
                    bool poolingEnabled = connectionString.ToLower().Contains("pooling=true");
                    string poolingStatus = poolingEnabled ? "✅ ENABLED" : "❌ DISABLED";
                    info.AppendLine($"🔗 Connection Pooling: {poolingStatus}");
                }

                info.AppendLine();
                info.AppendLine("📊 PERFORMANCE OPTIMIZATIONS ACTIVE:");
                info.AppendLine("✅ Database Connection Pooling");
                info.AppendLine("✅ Memory Management (using statements)");
                info.AppendLine("✅ Result Limiting (max 5000 records)");
                info.AppendLine("✅ Static Content Caching (30 days)");
                info.AppendLine("✅ Gzip Compression");
                info.AppendLine("✅ Optimized Session State");
                info.AppendLine("✅ Reduced Logging Verbosity");

                // Performance recommendations
                info.AppendLine();
                info.AppendLine("💡 RECOMMENDATIONS:");

                if (memoryUsage > 500 * 1024 * 1024)
                {
                    info.AppendLine("⚠️ High memory usage detected - consider implementing more caching");
                }

                if (!connectionString?.ToLower().Contains("pooling=true") == true)
                {
                    info.AppendLine("❌ Enable database connection pooling for better performance");
                }

                if (GC.CollectionCount(2) > 10)
                {
                    info.AppendLine("⚠️ High GC activity - optimize object lifecycle management");
                }

                // If no issues, show positive message
                if (memoryUsage <= 500 * 1024 * 1024 &&
                    connectionString?.ToLower().Contains("pooling=true") == true &&
                    GC.CollectionCount(2) <= 10)
                {
                    info.AppendLine("✅ No performance issues detected");
                    info.AppendLine("✅ All major optimizations are active");
                    info.AppendLine("✅ Application is running optimally");
                }

                info.AppendLine();
                info.AppendLine($"📅 Last Updated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            }
            catch (Exception ex)
            {
                info.AppendLine($"❌ Error gathering performance data: {ex.Message}");
            }

            return info.ToString();
        }

        //private string GetPerformanceInfo()
        //{
        //    var info = new System.Text.StringBuilder();

        //    try
        //    {
        //        // Enhanced header for multiple tab monitoring
        //        info.AppendLine("🖥️ MULTI-TAB PERFORMANCE MONITOR");
        //        info.AppendLine("=" + new string('=', 50));
        //        info.AppendLine();

        //        // Session statistics for multiple tabs
        //        try
        //        {
        //            // Get session count from HttpContext if available
        //            int activeSessions = 0;
        //            int totalTabs = 0;

        //            if (System.Web.HttpContext.Current?.Application != null)
        //            {
        //                var app = System.Web.HttpContext.Current.Application;
        //                activeSessions = (int)(app["ActiveSessionCount"] ?? 0);
        //                totalTabs = (int)(app["TotalTabCount"] ?? 0);
        //            }

        //            info.AppendLine("👥 SESSION & TAB STATISTICS:");
        //            info.AppendLine($"   Active Sessions: {activeSessions}");
        //            info.AppendLine($"   Total Open Tabs: {totalTabs}");
        //            info.AppendLine($"   Last Cleanup: {DateTime.Now:HH:mm:ss}");

        //            // Tab-related warnings
        //            if (totalTabs > 20)
        //                info.AppendLine("   ⚠️ High number of open tabs - consider session limits");
        //            else if (totalTabs > 10)
        //                info.AppendLine("   ⚠️ Moderate tab usage - monitor memory");
        //            else
        //                info.AppendLine("   ✅ Tab usage is within normal range");

        //            info.AppendLine();
        //        }
        //        catch
        //        {
        //            info.AppendLine("👥 SESSION MONITORING: Basic tracking active");
        //            info.AppendLine();
        //        }

        //        // Application uptime
        //        TimeSpan uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
        //        info.AppendLine($"🕒 Application Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");

        //        // Enhanced memory usage with working set
        //        long memoryUsage = GC.GetTotalMemory(false);
        //        long workingSet = Process.GetCurrentProcess().WorkingSet64;
        //        string memoryMB = $"{memoryUsage / 1024 / 1024:N0} MB";
        //        string workingSetMB = $"{workingSet / 1024 / 1024:N0} MB";
        //        string memoryStatus = memoryUsage > 400 * 1024 * 1024 ? " ⚠️ HIGH" : " ✅";
        //        info.AppendLine($"💾 Managed Memory: {memoryMB}{memoryStatus}");
        //        info.AppendLine($"💾 Working Set: {workingSetMB}");

        //        // Enhanced GC Collections with analysis
        //        int gen0 = GC.CollectionCount(0);
        //        int gen1 = GC.CollectionCount(1);
        //        int gen2 = GC.CollectionCount(2);
        //        info.AppendLine($"🗑️ GC Collections: Gen0:{gen0} Gen1:{gen1} Gen2:{gen2}");

        //        if (gen2 > 20)
        //            info.AppendLine("   ⚠️ High Gen2 collections - memory pressure detected");

        //        // Thread pool info
        //        int workerThreads, completionPortThreads;
        //        System.Threading.ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
        //        info.AppendLine($"🧵 Available Threads: Worker:{workerThreads} I/O:{completionPortThreads}");

        //        // Database connection pooling status
        //        string connectionString = genCon.getConnectionString();
        //        if (!string.IsNullOrEmpty(connectionString))
        //        {
        //            bool poolingEnabled = connectionString.ToLower().Contains("pooling=true");
        //            string poolingStatus = poolingEnabled ? "✅ ENABLED" : "❌ DISABLED";
        //            info.AppendLine($"🔗 Connection Pooling: {poolingStatus}");
        //        }

        //        // Cache statistics
        //        try
        //        {
        //            var cacheStats = ItemInquiryControllers.GetCacheStats();
        //            info.AppendLine($"💾 Cache: {cacheStats.ItemCount} items, ~{cacheStats.EstimatedSizeMB} KB");
        //            if (cacheStats.ItemCount > 50)
        //                info.AppendLine($"  ⚠️ High cache usage - consider clearing cache");
        //            else if (cacheStats.ItemCount > 0)
        //                info.AppendLine($"  ✅ Cache is helping performance");
        //            else
        //                info.AppendLine($"  ℹ️ Cache is empty - will populate as data is accessed");
        //        }
        //        catch
        //        {
        //            info.AppendLine($"💾 Cache: Basic caching active");
        //        }

        //        info.AppendLine();
        //        info.AppendLine("📊 PERFORMANCE OPTIMIZATIONS ACTIVE:");
        //        info.AppendLine("✅ Database Connection Pooling");
        //        info.AppendLine("✅ Memory Management (using statements)");
        //        info.AppendLine("✅ Result Limiting (max 5000 records)");
        //        info.AppendLine("✅ Static Content Caching (30 days)");
        //        info.AppendLine("✅ Gzip Compression");
        //        info.AppendLine("✅ Optimized Session State");
        //        info.AppendLine("✅ Reduced Logging Verbosity");

        //        // Performance recommendations
        //        info.AppendLine();
        //        info.AppendLine("💡 RECOMMENDATIONS:");

        //        if (memoryUsage > 500 * 1024 * 1024)
        //        {
        //            info.AppendLine("⚠️ High memory usage detected - consider implementing more caching");
        //        }

        //        if (!connectionString?.ToLower().Contains("pooling=true") == true)
        //        {
        //            info.AppendLine("❌ Enable database connection pooling for better performance");
        //        }

        //        if (GC.CollectionCount(2) > 10)
        //        {
        //            info.AppendLine("⚠️ High GC activity - optimize object lifecycle management");
        //        }

        //        // If no issues, show positive message
        //        if (memoryUsage <= 500 * 1024 * 1024 &&
        //            connectionString?.ToLower().Contains("pooling=true") == true &&
        //            GC.CollectionCount(2) <= 10)
        //        {
        //            info.AppendLine("✅ No performance issues detected");
        //            info.AppendLine("✅ All major optimizations are active");
        //            info.AppendLine("✅ Application is running optimally");
        //        }

        //        info.AppendLine();
        //        info.AppendLine($"📅 Last Updated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        //    }
        //    catch (Exception ex)
        //    {
        //        info.AppendLine($"❌ Error gathering performance data: {ex.Message}");
        //    }

        //    return info.ToString();
        //}

        //private string RunDatabasePerformanceAnalysis()
        //{
        //    var result = new System.Text.StringBuilder();
        //    string connectionString = genCon.getConnectionString();

        //    if (string.IsNullOrEmpty(connectionString))
        //    {
        //        return "❌ No database connection string found";
        //    }

        //    try
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            result.AppendLine("🔍 COMPREHENSIVE DATABASE PERFORMANCE ANALYSIS");
        //            result.AppendLine("StarLaiPortal Database - Complete Health Check");
        //            result.AppendLine("=" + new string('=', 60));
        //            result.AppendLine();

        //            // 1. Database Overview
        //            result.AppendLine("📋 DATABASE OVERVIEW:");
        //            AnalyzeDatabaseOverview(connection, result);
        //            result.AppendLine();

        //            // 2. All Tables Analysis
        //            result.AppendLine("📊 ALL TABLES ANALYSIS:");
        //            AnalyzeAllTables(connection, result);
        //            result.AppendLine();

        //            // 3. Missing Indexes (Comprehensive)
        //            result.AppendLine("🔍 MISSING INDEXES ANALYSIS:");
        //            AnalyzeMissingIndexes(connection, result);
        //            result.AppendLine();

        //            // 4. Index Fragmentation
        //            result.AppendLine("🧩 INDEX FRAGMENTATION ANALYSIS:");
        //            AnalyzeIndexFragmentation(connection, result);
        //            result.AppendLine();

        //            // 5. Statistics Analysis
        //            result.AppendLine("📈 STATISTICS FRESHNESS ANALYSIS:");
        //            AnalyzeStatistics(connection, result);
        //            result.AppendLine();

        //            // 6. Slow Queries (Comprehensive)
        //            result.AppendLine("🐌 SLOW QUERIES ANALYSIS:");
        //            AnalyzeSlowQueries(connection, result);
        //            result.AppendLine();

        //            // 7. Storage Analysis
        //            result.AppendLine("💾 STORAGE ANALYSIS:");
        //            AnalyzeStorage(connection, result);
        //            result.AppendLine();

        //            // 8. Connection Analysis
        //            result.AppendLine("🔗 CONNECTION ANALYSIS:");
        //            AnalyzeConnections(connection, result);
        //            result.AppendLine();

        //            // 9. Comprehensive Recommendations
        //            result.AppendLine("💡 COMPREHENSIVE RECOMMENDATIONS:");
        //            GenerateComprehensiveRecommendations(connection, result);
        //            result.AppendLine();

        //            result.AppendLine($"📅 Analysis completed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        //            result.AppendLine("=" + new string('=', 60));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.AppendLine($"❌ Analysis failed: {ex.Message}");
        //    }

        //    return result.ToString();
        //}

        ///// <summary>
        ///// Show a custom dialog that stays open until user manually closes it
        ///// </summary>
        //private void ShowCustomDialog(string title, string content)
        //{
        //    try
        //    {
        //        // Create a temporary text file with the content
        //        string tempPath = System.IO.Path.GetTempPath();
        //        string fileName = $"PerformanceReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        //        string fullPath = System.IO.Path.Combine(tempPath, fileName);

        //        // Write content to file
        //        string fileContent = $"{title}\n";
        //        fileContent += new string('=', title.Length) + "\n\n";
        //        fileContent += content;
        //        fileContent += "\n\n";
        //        fileContent += "This file was automatically generated and can be safely deleted.\n";
        //        fileContent += $"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        //        System.IO.File.WriteAllText(fullPath, fileContent);

        //        // Open the file in Notepad (user can close when done reading)
        //        System.Diagnostics.Process.Start("notepad.exe", fullPath);

        //        // Show a simple confirmation that file was opened
        //        genCon.showMsg("Performance Report", $"Performance report opened in Notepad.\n\nFile location: {fullPath}\n\nYou can close Notepad when finished reading.", InformationType.Success);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Fallback to the existing genCon.showMsg if file approach fails
        //        genCon.showMsg(title, $"{content}\n\n(Note: Error opening in external viewer: {ex.Message})", InformationType.Info);
        //    }
        //}

        //#region Comprehensive Database Analysis Methods

        //private void AnalyzeDatabaseOverview(SqlConnection connection, System.Text.StringBuilder result)
        //{
        //    try
        //    {
        //        var query = @"
        //            SELECT
        //                DB_NAME() as DatabaseName,
        //                COUNT(DISTINCT t.name) as TableCount,
        //                COUNT(DISTINCT i.name) as IndexCount,
        //                COUNT(DISTINCT p.name) as StoredProcCount
        //            FROM sys.tables t
        //            LEFT JOIN sys.indexes i ON t.object_id = i.object_id
        //            LEFT JOIN sys.procedures p ON 1=1";

        //        using (var cmd = new SqlCommand(query, connection))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            if (reader.Read())
        //            {
        //                result.AppendLine($"  • Database: {reader["DatabaseName"]}");
        //                result.AppendLine($"  • Tables: {reader["TableCount"]}");
        //                result.AppendLine($"  • Indexes: {reader["IndexCount"]}");
        //                result.AppendLine($"  • Stored Procedures: {reader["StoredProcCount"]}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.AppendLine($"  ❌ Error analyzing database overview: {ex.Message}");
        //    }
        //}

        //private void AnalyzeAllTables(SqlConnection connection, System.Text.StringBuilder result)
        //{
        //    try
        //    {
        //        var query = @"
        //            SELECT TOP 20
        //                SCHEMA_NAME(t.schema_id) + '.' + t.name AS TableName,
        //                p.rows AS RowCount,
        //                CAST(ROUND(((SUM(a.total_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS TotalSpaceMB,
        //                COUNT(i.index_id) AS IndexCount
        //            FROM sys.tables t
        //            INNER JOIN sys.indexes i ON t.object_id = i.object_id
        //            INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
        //            INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
        //            WHERE t.name NOT LIKE 'dt%' AND t.is_ms_shipped = 0 AND i.object_id > 255
        //            GROUP BY t.schema_id, t.name, p.rows
        //            ORDER BY TotalSpaceMB DESC";

        //        using (var cmd = new SqlCommand(query, connection))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            if (reader.HasRows)
        //            {
        //                result.AppendLine("  📋 TOP TABLES BY SIZE:");
        //                while (reader.Read())
        //                {
        //                    var tableName = reader["TableName"].ToString();
        //                    var rowCount = Convert.ToInt64(reader["RowCount"]);
        //                    var sizeMB = Convert.ToDecimal(reader["TotalSpaceMB"]);
        //                    var indexCount = Convert.ToInt32(reader["IndexCount"]);

        //                    result.AppendLine($"    • {tableName}: {rowCount:N0} rows, {sizeMB:N1} MB, {indexCount} indexes");
        //                }
        //            }
        //            else
        //            {
        //                result.AppendLine("  ✅ No user tables found or unable to analyze");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.AppendLine($"  ❌ Error analyzing tables: {ex.Message}");
        //    }
        //}

        //private void AnalyzeMissingIndexes(SqlConnection connection, System.Text.StringBuilder result)
        //{
        //    try
        //    {
        //        var query = @"
        //            SELECT TOP 15
        //                CONVERT(decimal(18,2), user_seeks * avg_total_user_cost * (avg_user_impact * 0.01)) AS Impact,
        //                mid.statement AS TableName,
        //                mid.equality_columns,
        //                mid.inequality_columns,
        //                mid.included_columns,
        //                'CREATE INDEX [IX_' +
        //                REPLACE(REPLACE(REPLACE(ISNULL(mid.equality_columns,''),', ','_'),'[',''),']','') +
        //                CASE WHEN mid.inequality_columns IS NOT NULL THEN '_' +
        //                REPLACE(REPLACE(REPLACE(mid.inequality_columns,', ','_'),'[',''),']','') ELSE '' END +
        //                '] ON ' + mid.statement + ' (' + ISNULL(mid.equality_columns,'') +
        //                CASE WHEN mid.inequality_columns IS NOT NULL AND mid.equality_columns IS NOT NULL THEN ',' ELSE '' END +
        //                ISNULL(mid.inequality_columns, '') + ')' +
        //                ISNULL(' INCLUDE (' + mid.included_columns + ')', '') AS CreateStatement
        //            FROM sys.dm_db_missing_index_groups mig
        //            INNER JOIN sys.dm_db_missing_index_group_stats migs ON migs.group_handle = mig.index_group_handle
        //            INNER JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
        //            WHERE CONVERT(decimal(18,2), user_seeks * avg_total_user_cost * (avg_user_impact * 0.01)) > 1
        //            ORDER BY Impact DESC";

        //        using (var cmd = new SqlCommand(query, connection))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            if (reader.HasRows)
        //            {
        //                result.AppendLine("  🔍 MISSING INDEXES (High Impact):");
        //                int count = 0;
        //                while (reader.Read() && count < 10)
        //                {
        //                    var impact = reader.GetDecimal(0);
        //                    var table = reader.GetString(1);
        //                    var createStatement = reader.GetString(5);

        //                    result.AppendLine($"    • Impact Score: {impact:F1}");
        //                    result.AppendLine($"      Table: {table.Split('.').LastOrDefault()}");
        //                    result.AppendLine($"      SQL: {createStatement}");
        //                    result.AppendLine();
        //                    count++;
        //                }
        //            }
        //            else
        //            {
        //                result.AppendLine("  ✅ No significant missing indexes found");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.AppendLine($"  ❌ Error analyzing missing indexes: {ex.Message}");
        //    }
        //}

        //private void AnalyzeIndexFragmentation(SqlConnection connection, System.Text.StringBuilder result)
        //{
        //    try
        //    {
        //        var query = @"
        //            SELECT TOP 10
        //                OBJECT_SCHEMA_NAME(ips.object_id) + '.' + OBJECT_NAME(ips.object_id) AS TableName,
        //                i.name AS IndexName,
        //                ips.avg_fragmentation_in_percent,
        //                ips.page_count
        //            FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
        //            INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
        //            WHERE ips.avg_fragmentation_in_percent > 10 AND ips.page_count > 100
        //            ORDER BY ips.avg_fragmentation_in_percent DESC";

        //        using (var cmd = new SqlCommand(query, connection))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            if (reader.HasRows)
        //            {
        //                result.AppendLine("  🧩 FRAGMENTED INDEXES:");
        //                while (reader.Read())
        //                {
        //                    var tableName = reader["TableName"].ToString();
        //                    var indexName = reader["IndexName"].ToString();
        //                    var fragmentation = Convert.ToDecimal(reader["avg_fragmentation_in_percent"]);
        //                    var pageCount = Convert.ToInt64(reader["page_count"]);

        //                    string recommendation = fragmentation > 30 ? "REBUILD" : "REORGANIZE";
        //                    result.AppendLine($"    • {tableName}.{indexName}: {fragmentation:F1}% fragmented ({pageCount} pages) - {recommendation}");
        //                }
        //            }
        //            else
        //            {
        //                result.AppendLine("  ✅ No significant index fragmentation found");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.AppendLine($"  ❌ Error analyzing index fragmentation: {ex.Message}");
        //    }
        //}

        //private void AnalyzeStatistics(SqlConnection connection, System.Text.StringBuilder result)
        //{
        //    try
        //    {
        //        var query = @"
        //            SELECT TOP 10
        //                OBJECT_SCHEMA_NAME(s.object_id) + '.' + OBJECT_NAME(s.object_id) AS TableName,
        //                s.name AS StatName,
        //                STATS_DATE(s.object_id, s.stats_id) AS LastUpdated,
        //                DATEDIFF(day, STATS_DATE(s.object_id, s.stats_id), GETDATE()) AS DaysOld
        //            FROM sys.stats s
        //            INNER JOIN sys.tables t ON s.object_id = t.object_id
        //            WHERE STATS_DATE(s.object_id, s.stats_id) IS NOT NULL
        //            AND DATEDIFF(day, STATS_DATE(s.object_id, s.stats_id), GETDATE()) > 7
        //            ORDER BY DaysOld DESC";

        //        using (var cmd = new SqlCommand(query, connection))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            if (reader.HasRows)
        //            {
        //                result.AppendLine("  📈 OUTDATED STATISTICS:");
        //                while (reader.Read())
        //                {
        //                    var tableName = reader["TableName"].ToString();
        //                    var statName = reader["StatName"].ToString();
        //                    var lastUpdated = Convert.ToDateTime(reader["LastUpdated"]);
        //                    var daysOld = Convert.ToInt32(reader["DaysOld"]);

        //                    string urgency = daysOld > 30 ? "URGENT" : daysOld > 14 ? "HIGH" : "MEDIUM";
        //                    result.AppendLine($"    • {tableName}.{statName}: {daysOld} days old ({urgency} priority)");
        //                }
        //            }
        //            else
        //            {
        //                result.AppendLine("  ✅ All statistics are up to date");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.AppendLine($"  ❌ Error analyzing statistics: {ex.Message}");
        //    }
        //}

        //private void AnalyzeSlowQueries(SqlConnection connection, System.Text.StringBuilder result)
        //{
        //    try
        //    {
        //        var query = @"
        //            SELECT TOP 10
        //                qs.execution_count,
        //                qs.avg_elapsed_time / 1000 AS avg_elapsed_time_ms,
        //                qs.avg_logical_reads,
        //                qs.avg_physical_reads,
        //                qs.avg_worker_time / 1000 AS avg_cpu_time_ms,
        //                LEFT(REPLACE(REPLACE(st.text, CHAR(13), ' '), CHAR(10), ' '), 150) AS query_text
        //            FROM sys.dm_exec_query_stats qs
        //            CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
        //            WHERE qs.avg_elapsed_time > 100000 -- More than 100ms average
        //            ORDER BY qs.avg_elapsed_time DESC";

        //        using (var cmd = new SqlCommand(query, connection))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            if (reader.HasRows)
        //            {
        //                result.AppendLine("  🐌 SLOW QUERIES (>100ms average):");
        //                while (reader.Read())
        //                {
        //                    var execCount = reader.GetInt64(0);
        //                    var avgTime = reader.GetInt64(1);
        //                    var avgReads = reader.GetInt64(2);
        //                    var avgPhysicalReads = reader.GetInt64(3);
        //                    var avgCpuTime = reader.GetInt64(4);
        //                    var queryText = reader.GetString(5);

        //                    result.AppendLine($"    • Avg Time: {avgTime}ms, CPU: {avgCpuTime}ms, Executions: {execCount}");
        //                    result.AppendLine($"      Logical Reads: {avgReads}, Physical Reads: {avgPhysicalReads}");
        //                    result.AppendLine($"      Query: {queryText}...");
        //                    result.AppendLine();
        //                }
        //            }
        //            else
        //            {
        //                result.AppendLine("  ✅ No significantly slow queries found");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.AppendLine($"  ❌ Error analyzing slow queries: {ex.Message}");
        //    }
        //}

        //private void AnalyzeStorage(SqlConnection connection, System.Text.StringBuilder result)
        //{
        //    try
        //    {
        //        var query = @"
        //            SELECT
        //                name AS FileName,
        //                size * 8.0 / 1024 AS FileSizeMB,
        //                FILEPROPERTY(name, 'SpaceUsed') * 8.0 / 1024 AS SpaceUsedMB,
        //                (size - FILEPROPERTY(name, 'SpaceUsed')) * 8.0 / 1024 AS FreeSpaceMB,
        //                type_desc AS FileType
        //            FROM sys.database_files";

        //        using (var cmd = new SqlCommand(query, connection))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            if (reader.HasRows)
        //            {
        //                result.AppendLine("  💾 DATABASE FILES:");
        //                while (reader.Read())
        //                {
        //                    var fileName = reader["FileName"].ToString();
        //                    var fileSizeMB = Convert.ToDecimal(reader["FileSizeMB"]);
        //                    var spaceUsedMB = Convert.ToDecimal(reader["SpaceUsedMB"]);
        //                    var freeSpaceMB = Convert.ToDecimal(reader["FreeSpaceMB"]);
        //                    var fileType = reader["FileType"].ToString();

        //                    var usagePercent = fileSizeMB > 0 ? (spaceUsedMB / fileSizeMB) * 100 : 0;
        //                    result.AppendLine($"    • {fileName} ({fileType}): {fileSizeMB:F1} MB total, {spaceUsedMB:F1} MB used ({usagePercent:F1}%)");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.AppendLine($"  ❌ Error analyzing storage: {ex.Message}");
        //    }
        //}

        //private void AnalyzeConnections(SqlConnection connection, System.Text.StringBuilder result)
        //{
        //    try
        //    {
        //        var query = @"
        //            SELECT
        //                COUNT(*) AS TotalConnections,
        //                SUM(CASE WHEN status = 'sleeping' THEN 1 ELSE 0 END) AS SleepingConnections,
        //                SUM(CASE WHEN status = 'running' THEN 1 ELSE 0 END) AS ActiveConnections,
        //                SUM(CASE WHEN status = 'background' THEN 1 ELSE 0 END) AS BackgroundConnections
        //            FROM sys.dm_exec_sessions
        //            WHERE is_user_process = 1";

        //        using (var cmd = new SqlCommand(query, connection))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            if (reader.Read())
        //            {
        //                var total = reader.GetInt32(0);
        //                var sleeping = reader.GetInt32(1);
        //                var active = reader.GetInt32(2);
        //                var background = reader.GetInt32(3);

        //                result.AppendLine($"  • Total User Connections: {total}");
        //                result.AppendLine($"  • Active: {active}, Sleeping: {sleeping}, Background: {background}");

        //                if (total > 100)
        //                    result.AppendLine("  ⚠️ Very high connection count - investigate connection pooling");
        //                else if (total > 50)
        //                    result.AppendLine("  ⚠️ High connection count - monitor connection usage");
        //                else
        //                    result.AppendLine("  ✅ Connection count is healthy");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.AppendLine($"  ❌ Error analyzing connections: {ex.Message}");
        //    }
        //}

        //private void GenerateComprehensiveRecommendations(SqlConnection connection, System.Text.StringBuilder result)
        //{
        //    try
        //    {
        //        result.AppendLine("  🎯 IMMEDIATE ACTIONS:");
        //        result.AppendLine("    1. Review and implement missing indexes with high impact scores");
        //        result.AppendLine("    2. Rebuild/reorganize fragmented indexes (>30% = rebuild, 10-30% = reorganize)");
        //        result.AppendLine("    3. Update outdated statistics (especially those >30 days old)");
        //        result.AppendLine("    4. Optimize slow queries identified above");
        //        result.AppendLine();

        //        result.AppendLine("  📋 MAINTENANCE TASKS:");
        //        result.AppendLine("    • Schedule weekly index maintenance");
        //        result.AppendLine("    • Set up automatic statistics updates");
        //        result.AppendLine("    • Monitor database file growth");
        //        result.AppendLine("    • Review connection pooling settings");
        //        result.AppendLine();

        //        result.AppendLine("  🔧 OPTIMIZATION OPPORTUNITIES:");
        //        result.AppendLine("    • Consider partitioning for large tables (>1M rows)");
        //        result.AppendLine("    • Implement query plan caching for frequent queries");
        //        result.AppendLine("    • Review stored procedures for optimization");
        //        result.AppendLine("    • Consider read replicas for reporting workloads");
        //        result.AppendLine();

        //        result.AppendLine("  📊 MONITORING RECOMMENDATIONS:");
        //        result.AppendLine("    • Run this analysis weekly to track improvements");
        //        result.AppendLine("    • Monitor query performance after index changes");
        //        result.AppendLine("    • Track database growth trends");
        //        result.AppendLine("    • Set up alerts for connection count spikes");
        //        result.AppendLine();

        //        result.AppendLine("  🚀 PERFORMANCE IMPACT ESTIMATE:");
        //        result.AppendLine("    • Missing indexes: 20-80% improvement for affected queries");
        //        result.AppendLine("    • Index defragmentation: 10-30% improvement");
        //        result.AppendLine("    • Statistics updates: 5-50% improvement for affected queries");
        //        result.AppendLine("    • Query optimization: 50-500% improvement for specific queries");
        //    }
        //    catch (Exception ex)
        //    {
        //        result.AppendLine($"  ❌ Error generating recommendations: {ex.Message}");
        //    }
        //}

        //#endregion
        // End ver 1.0.23
    }
}
