using Dapper;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItemBinEnquiryController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public ItemBinEnquiryController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            this.securityProvider = securityProvider;
            this.Configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get(string whscode, string bincode, string itemcode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = "";
                    json = JsonConvert.SerializeObject(new { whscode, bincode, itemcode });
                    var val = conn.Query($"exec sp_GetInquiry 'ItemBinEnquiry', '{json}'").ToList();

                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
