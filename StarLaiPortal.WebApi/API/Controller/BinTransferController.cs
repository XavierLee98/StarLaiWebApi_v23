﻿using Dapper;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
using StarLaiPortal.Module.BusinessObjects.Warehouse_Transfer;
using StarLaiPortal.Module.Controllers;
using StarLaiPortal.WebApi.Helper;
using StarLaiPortal.WebApi.Model;
using System.Data.SqlClient;
using System.Dynamic;
namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BinTransferController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public BinTransferController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            this.securityProvider = securityProvider;
            this.Configuration = configuration;
        }

        [HttpPost("PostDraft")]
        public IActionResult PostDraft([FromBody] ExpandoObject obj)
        {
            try
            {
                dynamic dynamicObj = obj;

                var detailsObject = (IEnumerable<dynamic>)dynamicObj.WarehouseTransferDetails;
                if (detailsObject == null || detailsObject.Count() <= 0)
                    return Problem("Bin Transfer Details are null.");

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    foreach (var itemline in detailsObject)
                    {
                        string json = JsonConvert.SerializeObject(new { itemcode = itemline.ItemCode, bincode = itemline.FromBin, quantity = itemline.Quantity });

                        var validateBalance = conn.Query<ValidateJson>($"exec sp_beforedatasave 'ValidateStockBalance', '{json}'").FirstOrDefault();
                        if (validateBalance.Error)
                        {
                            return Problem(validateBalance.ErrorMessage);
                        }
                    }
                }

                IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<WarehouseTransfers>();
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                LogHelper.CreateLog(Configuration.GetConnectionString("ConnectionString"), userId.ToString(), "BinTransfer(From)", obj);

                WarehouseTransfers curobj = null;
                curobj = newObjectSpace.CreateObject<WarehouseTransfers>();
                ExpandoParser.ParseExObjectXPO<WarehouseTransfers>(obj, curobj, newObjectSpace);

                curobj.Picker = userName;
                curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                curobj.TransferType = TransferType.BT;

                foreach (var dtl in curobj.WarehouseTransferDetails)
                {
                    dtl.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    dtl.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                }

                curobj.Save();

                var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                GeneralControllers con = new GeneralControllers();
                curobj.DocNum = con.GenerateDocNum(DocTypeList.WT, objectSpaceFactory.CreateObjectSpace<DocTypes>(), TransferType.BT, 0, companyPrefix);
                newObjectSpace.CommitChanges();

                return Ok(new { oid = curobj.Oid, docnum = curobj.DocNum });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("startdate/enddate")]
        public IActionResult Get(DateTime startdate, DateTime enddate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    ISecurityStrategyBase security = securityProvider.GetSecurity();
                    var userId = security.UserId;
                    var userName = security.UserName;

                    string json = JsonConvert.SerializeObject(new { userName, startdate, enddate });

                    var val = conn.Query($"exec sp_getdatalist 'BTDraftList', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("oid")]
        public IActionResult Get(int oid)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid });

                    var val = conn.Query($"exec sp_getdatalist 'TransferDetails', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("Post")]
        public IActionResult Post([FromBody] ExpandoObject obj)
        {
            try
            {
                dynamic dynamicObj = obj;

                IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<WarehouseTransfers>();
                WarehouseTransfers transfers = objectSpace.FindObject<WarehouseTransfers>(CriteriaOperator.Parse("Oid = ?", dynamicObj.Oid));

                if (transfers.Status != DocStatus.Draft)
                {
                    return Problem($"Update Failed. Bin Transfer No.{transfers.DocNum} already {transfers.Status}.");
                }

                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                LogHelper.CreateLog(Configuration.GetConnectionString("ConnectionString"), userId.ToString(), "BinTransfer(To)", obj);

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    if (dynamicObj.WarehouseTransferDetails != null)
                    {

                        foreach (dynamic dtl in dynamicObj.WarehouseTransferDetails)
                        {
                            WarehouseTransferDetails detail = transfers.WarehouseTransferDetails.FirstOrDefault(line => line.Oid == dtl.Oid);
                            if (detail != null)
                            {
                                detail.ToBin = objectSpace.GetObjectByKey<vwBin>(dtl.ToBin);
                            }

                            string json = JsonConvert.SerializeObject(new { itemcode = detail.ItemCode.ItemCode, bincode = detail.FromBin.BinCode, quantity = detail.Quantity });

                            var validateBalance = conn.Query<ValidateJson>($"exec sp_beforedatasave 'ValidateStockBalance', '{json}'").FirstOrDefault();
                            if (validateBalance.Error)
                            {
                                return Problem(validateBalance.ErrorMessage);
                            }
                        }
                    }
                }

                objectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = transfers.Oid, username = userName });
                    conn.Query($"exec sp_afterdatasave 'WTPost', '{json}'");
                }

                return Ok(new { successful = 1 });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("CancelTransfer/oid")]
        public IActionResult CancelTransfer(int oid)
        {
            try
            {
                IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<WarehouseTransfers>();
                WarehouseTransfers doc = objectSpace.FindObject<WarehouseTransfers>(CriteriaOperator.Parse("Oid = ?", oid));

                if (doc.Status != DocStatus.Draft)
                {
                    return Problem($"Cancel Failed. Bin Transfer No.{doc.DocNum} already {doc.Status}.");
                }

                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                doc.Status = DocStatus.Cancelled;

                objectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = oid, username = userName });
                    conn.Query($"exec sp_afterdatasave 'CancelTransfer', '{json}'");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
