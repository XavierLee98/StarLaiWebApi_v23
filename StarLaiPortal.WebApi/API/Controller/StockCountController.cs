using Dapper;
using DevExpress.Data.Extensions;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
using DevExpress.Xpo.Metadata.Helpers;
using DevExpress.XtraPrinting.Native;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Load;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.Stock_Count;
using StarLaiPortal.Module.BusinessObjects.View;
using StarLaiPortal.Module.BusinessObjects.Warehouse_Transfer;
using StarLaiPortal.WebApi.Helper;
using StarLaiPortal.WebApi.Model;
using System.Data.SqlClient;
using System.Dynamic;
using System.Security.Cryptography;

namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockCountController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public StockCountController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            this.securityProvider = securityProvider;
            this.Configuration = configuration;
        }

        [HttpGet("startdate/enddate")]
        public IActionResult GetHeaderList(DateTime startdate, DateTime enddate)
        {
            try
            {
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { userId, startdate, enddate });

                    var val = conn.Query($"exec sp_getdatalist 'CountSheet', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("countdetail/oid")]
        public IActionResult GetDetailList(int oid)
        {
            try
            {
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid });

                    var val = conn.Query($"exec sp_getdatalist 'CountSheet', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("target/oid")]
        public IActionResult GetTargetList(int oid)
        {
            try
            {
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid });

                    var val = conn.Query($"exec sp_getdatalist 'CountTargetList', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("GetServerTime")]
        public IActionResult GetServerTime()
        {
            try
            {
                var serverTime = DateTime.Now;

                return Ok(new { serverTime = serverTime });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("DraftSave")]
        public IActionResult PostDraft([FromBody] ExpandoObject obj)
        {
            try
            {
                dynamic dynamicObj = obj;

                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId.ToString();
                var userName = security.UserName;

                LogHelper.CreateLog(Configuration.GetConnectionString("ConnectionString"), userId, "StockCount", obj);

                IObjectSpace sheetOS = objectSpaceFactory.CreateObjectSpace<StockCountSheet>();
                IObjectSpace stockCountedOS = objectSpaceFactory.CreateObjectSpace<StockCountSheetCounted>();

                StockCountSheet stockCountSheet = sheetOS.FindObject<StockCountSheet>(CriteriaOperator.Parse("Oid = ?", dynamicObj.Oid));

                if (stockCountSheet == null) throw new Exception($"Document not found.[{dynamicObj.Oid}]");

                if (stockCountSheet.Status != DocStatus.Draft)
                {
                    return Problem($"Update Failed. Stock Count Sheet No.{stockCountSheet.DocNum} already {stockCountSheet.Status}.");
                }

                if (dynamicObj.CountDetails != null && ((IEnumerable<dynamic>)dynamicObj.CountDetails).Count() > 0)
                {

                    foreach (ExpandoObject exobj in dynamicObj.CountDetails)
                    {
                        dynamic countDetail = exobj;

                        if (countDetail.Oid == -1)
                        {
                            StockCountSheetCounted curobj = stockCountedOS.CreateObject<StockCountSheetCounted>();
                            ExpandoParser.ParseExObjectXPO<StockCountSheetCounted>(new Dictionary<string, object>(exobj), curobj, stockCountedOS);

                            string itemjson = JsonConvert.SerializeObject(new { barcode = curobj.ItemBarCode });

                            string itemcode = string.Empty;
                            using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                            {
                                itemcode = conn.Query<string>($"exec sp_beforedatasave 'ValidateBarCode', '{itemjson}'").FirstOrDefault();
                            }

                            if (!string.IsNullOrEmpty(itemcode))
                            {
                                var item = stockCountedOS.GetObjectByKey<vwItemMasters>(itemcode);
                                if (item != null)
                                {
                                    curobj.ItemCode = item;
                                }
                            }

                            var bin = stockCountedOS.GetObjectByKey<vwBin>(curobj.BinBarCode);
                            if (bin != null)
                            {
                                curobj.Warehouse = stockCountedOS.GetObjectByKey<vwWarehouse>(bin.Warehouse);
                                curobj.Bin = bin;
                            }

                            curobj.CreateUser = stockCountedOS.GetObjectByKey<ApplicationUser>(security.UserId);
                            curobj.UpdateUser = stockCountedOS.GetObjectByKey<ApplicationUser>(security.UserId);
                            curobj.Save();
                        }
                        else if(countDetail.Oid > 0)
                        {
                            var currentLine = stockCountSheet.StockCountSheetCounted.Where(x => x.Oid == countDetail.Oid).FirstOrDefault();
                            if (currentLine != null)
                            {
                                if (countDetail.Isdelete)
                                {
                                    currentLine.Delete();
                                    continue;
                                }

                                currentLine.Quantity = (decimal)countDetail.Quantity;
                            }
                        }
                    }
                }
                stockCountedOS.CommitChanges();

                sheetOS.CommitChanges();

                string updatejson = JsonConvert.SerializeObject(new { oid = stockCountSheet.Oid, username = userName });

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    conn.Query($"exec sp_afterdatasave 'StockCountUpdate', '{updatejson}'");
                }

                return Ok(new { oid = stockCountSheet.Oid, docnum = stockCountSheet.DocNum });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        //[HttpPost("Post")]
        //public IActionResult Post([FromBody] ExpandoObject obj)
        //{
        //    try
        //    {
        //        dynamic dynamicObj = obj;

        //        IObjectSpace sheetOS = objectSpaceFactory.CreateObjectSpace<StockCountSheet>();
        //        IObjectSpace stockCountedOS = objectSpaceFactory.CreateObjectSpace<StockCountSheetCounted>();

        //        StockCountSheet stockCountSheet = sheetOS.FindObject<StockCountSheet>(CriteriaOperator.Parse("Oid = ?", dynamicObj.Oid));

        //        if (stockCountSheet.Status != DocStatus.Draft)
        //        {
        //            return Problem($"Update Failed. Stock Count Sheet No.{stockCountSheet.DocNum} already {stockCountSheet.Status}.");
        //        }

        //        ISecurityStrategyBase security = securityProvider.GetSecurity();
        //        var userId = security.UserId;
        //        var userName = security.UserName;

        //        //using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
        //        //{
        //        //    string json = JsonConvert.SerializeObject(new { oid = stockCountSheet.Oid });
        //        //    conn.Query($"exec sp_beforedatasave 'StockCountCountedDelete', '{json}'");
        //        //}

        //        if (dynamicObj.CountDetails != null && ((IEnumerable<dynamic>)dynamicObj.CountDetails).Count() > 0)
        //        {

        //            foreach (ExpandoObject exobj in dynamicObj.CountDetails)
        //            {
        //                //dynamic line = exobj;
        //                //bool isBarCodeFound = false;
        //                //foreach (var detail in stockCountSheet.StockCountSheetCounted)
        //                //{
        //                //    if (line.ItemBarCode == detail.ItemBarCode && line.BinBarCode == detail.BinBarCode)
        //                //    {
        //                //        detail.Quantity = (decimal)line.Quantity;
        //                //        isBarCodeFound = true;
        //                //        continue;
        //                //    }
        //                //}

        //                //if (isBarCodeFound) continue;

        //                StockCountSheetCounted curobj = stockCountedOS.CreateObject<StockCountSheetCounted>();
        //                ExpandoParser.ParseExObjectXPO<StockCountSheetCounted>(new Dictionary<string, object>(exobj), curobj, stockCountedOS);

        //                var item = stockCountedOS.GetObjectByKey<vwItemMasters>(curobj.ItemBarCode);
        //                if (item != null)
        //                {
        //                    curobj.ItemCode = item;
        //                }

        //                var bin = stockCountedOS.GetObjectByKey<vwBin>(curobj.BinBarCode);
        //                if (bin != null)
        //                {
        //                    curobj.Bin = bin;
        //                    curobj.Warehouse = stockCountedOS.GetObjectByKey<vwWarehouse>(bin.Warehouse);
        //                }

        //                curobj.CreateUser = stockCountedOS.GetObjectByKey<ApplicationUser>(userId);
        //                curobj.UpdateUser = stockCountedOS.GetObjectByKey<ApplicationUser>(userId);
        //                curobj.Save();
        //            }
        //        }

        //        stockCountSheet.Status = DocStatus.Submitted;
        //        stockCountedOS.CommitChanges();
        //        sheetOS.CommitChanges();

        //        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
        //        {
        //            string json = JsonConvert.SerializeObject(new { oid = stockCountSheet.Oid, username = userName });
        //            conn.Query($"exec sp_afterdatasave 'StockCountConfirm', '{json}'");
        //            return Ok(new { oid = stockCountSheet.Oid, docnum = stockCountSheet.DocNum });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.Message);
        //    }
        //}

    }
}
