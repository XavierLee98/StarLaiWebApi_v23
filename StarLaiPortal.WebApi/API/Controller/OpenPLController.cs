using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Dapper;
using StarLaiPortal.WebApi.Model;
using DevExpress.ExpressApp.Security;
using StarLaiPortal.Module.BusinessObjects;
using DevExpress.Data.Filtering;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using StarLaiPortal.WebApi.Helper;
using System.Dynamic;
using DevExpress.Xpo;
using StarLaiPortal.Module.Controllers;
using StarLaiPortal.Module.BusinessObjects.Pack_List;
using StarLaiPortal.Module.BusinessObjects.Load;
using StarLaiPortal.Module.BusinessObjects.Delivery_Order;
using StarLaiPortal.Module.BusinessObjects.Setup;
using System.Collections.Concurrent;

namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OpenPLController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _soLocks = new();

        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public OpenPLController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            this.securityProvider = securityProvider;
            this.Configuration = configuration;
        }
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { userGuid = userId.ToString() });
                    var val = conn.Query($"exec sp_getdatalist 'OpenPL', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("ReasonCode")]
        public IActionResult GetReasonCode()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    var val = conn.Query("exec sp_getdatalist 'ReasonCode'").ToList();
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
                    string json = JsonConvert.SerializeObject(new { oid = oid });
                    var val = conn.Query($"exec sp_getdatalist 'OpenPL', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] ExpandoObject obj)
        {
            dynamic dynamicObj = obj;

            var lockKey = string.Join(",",
                ((IEnumerable<dynamic>)dynamicObj.PickListDetailsActuals)
                .Select(x => $"{x.PickList}_{x.PickListDetailOid}")
                .Distinct()
                .OrderBy(x => x));

            var gate = _soLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
            bool acquired = await gate.WaitAsync(TimeSpan.FromSeconds(60));
            if (!acquired)
                return Problem("Another submission for the same pick is in progress. Please try again.");
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string jsonString = JsonConvert.SerializeObject(obj);

                    jsonString = jsonString = jsonString.Replace("'", "''");

                    var validatejson = conn.Query<ValidateJson>($"exec ValidateJsonInput 'PickListDetailsActual', '{jsonString}'").FirstOrDefault();
                    if (validatejson.Error)
                    {
                        return Problem(validatejson.ErrorMessage);
                    }
                }

                var detailsObject = (IEnumerable<dynamic>)dynamicObj.PickListDetailsActuals;

                if (detailsObject != null)
                {
                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        foreach (var line in detailsObject)
                        {
                            if (string.IsNullOrEmpty(line.ToBin))
                            {
                                return Problem("The ToBin value is null. Please select Tobin. [JSON]");
                            }

                            if (line.FromBin == line.ToBin)
                            {
                                return Problem($"From Bin and To Bin cannot be same. [JSON]");
                            }

                            string json = JsonConvert.SerializeObject(new { itemcode = line.ItemCode, bincode = line.FromBin, quantity = line.PickQty });

                            var validateBalance = conn.Query<ValidateJson>($"exec sp_beforedatasave 'ValidateStockBalance', '{json}'").FirstOrDefault();
                            if (validateBalance.Error)
                            {
                                return Problem(validateBalance.ErrorMessage);
                            }
                        }
                    }
                }
                else
                {
                    return Problem("Pick List Actuals are not found.");
                }

                IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<PickListDetailsActual>();
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                IObjectSpace plOS = objectSpaceFactory.CreateObjectSpace<PickList>();
                PickList plobj = plOS.FindObject<PickList>(CriteriaOperator.Parse("Oid = ?", dynamicObj.PickOid));

                var userId = security.UserId;
                var userName = security.UserName;

                if (plobj.Status != DocStatus.Draft)
                {
                    return Problem($"Update Failed. Pick List No. {plobj.DocNum} already {plobj.Status}.");
                }

                foreach (var line in plobj.PickListDetails)
                {
                    var actualPickQty = plobj.PickListDetailsActual.Where(x => x.PickList == line.PickList && x.PickListDetailOid == line.Oid).Sum(y => y.PickQty);

                    var inputQty = detailsObject.Where(x => x.PickList == dynamicObj.PickOid && x.PickListDetailOid == line.Oid).Sum(y => (decimal)y.PickQty);

                    if (actualPickQty + inputQty > line.PlanQty) return Problem($"Actual pick quantity cannot more than plan quantity. Please check picklist in portal.");
                }

                plobj.Picker = plOS.GetObjectByKey<ApplicationUser>(userId);

                LogHelper.CreateLog(Configuration.GetConnectionString("ConnectionString"), userId.ToString(), "Picking", obj);

                List<PickListDetailsActual> objs = new List<PickListDetailsActual>();
                foreach (ExpandoObject exobj in dynamicObj.PickListDetailsActuals)
                {
                    PickListDetailsActual curobj = newObjectSpace.CreateObject<PickListDetailsActual>();
                    ExpandoParser.ParseExObjectXPO<PickListDetailsActual>(new Dictionary<string, object>(exobj), curobj, newObjectSpace);

                    if (curobj.ToBin == null)
                    {
                        return Problem("The ToBin value is null. Please select Tobin. [Detail]");
                    }

                    if (curobj.FromBin == curobj.ToBin)
                    {
                        return Problem($"From Bin and To Bin cannot be same. [Detail]");
                    }

                    curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);


                    curobj.Save();
                    objs.Add(curobj);
                }

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    foreach (var line in plobj.PickListDetails)
                    {
                        var actualPickQty = plobj.PickListDetailsActual.Where(x => x.PickList == line.PickList && x.PickListDetailOid == line.Oid).Sum(y => y.PickQty);

                        var inputQty = detailsObject.Where(x => x.PickList == dynamicObj.PickOid && x.PickListDetailOid == line.Oid).Sum(y => (decimal)y.PickQty);

                        if (actualPickQty + inputQty > line.PlanQty) return Problem($"Actual pick quantity cannot more than plan quantity. Please check picklist in portal.");

                        line.PickQty = actualPickQty + inputQty;

                        if (dynamicObj.PickListDetails != null && dynamicObj.PickListDetails.Count > 0)
                        {
                            var list = (IEnumerable<dynamic>)dynamicObj.PickListDetails;

                            var selectedLine = list.Where(x => x.Oid == line.Oid).FirstOrDefault();

                            if (selectedLine != null)
                            {
                                line.Reason = plOS.GetObjectByKey<DiscrepancyReason>((int)selectedLine.Reason);
                            }
                        }
                    }
                }

                plobj.Status = DocStatus.Submitted;

                plobj.Save();

                IObjectSpace packos = objectSpaceFactory.CreateObjectSpace<PackList>();
                IObjectSpace loados = objectSpaceFactory.CreateObjectSpace<Load>();
                IObjectSpace deliveryos = objectSpaceFactory.CreateObjectSpace<DeliveryOrder>();

                var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                GeneralControllers con = new GeneralControllers();
                var result = con.GenerateAutoDO(Configuration.GetConnectionString("ConnectionString"), objs.FirstOrDefault().PickList, newObjectSpace, packos, loados, deliveryos, companyPrefix);
                if (result == 0) throw new Exception($"Fail to generate Auto Delivery for Pick List ({plobj.DocNum}). Please retry again.");

                plOS.CommitChanges();
                newObjectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = objs.FirstOrDefault().PickList.Oid, username = userName });
                    conn.Query($"exec sp_afterdatasave 'PickListStatus', '{json}'");
                }

                return Ok(new { oid = plobj.Oid, docnum = plobj.DocNum, IsAutoDO = result });

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
            finally
            {
                gate.Release();
            }
        }
    }
}
