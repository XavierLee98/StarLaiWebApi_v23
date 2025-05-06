using Dapper;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Dynamic;

namespace StarLaiPortal.WebApi.Helper
{
    public static class LogHelper
    {
        public static void CreateLog(string connString, string userId,string module, ExpandoObject obj)
        {
            string jsonString = JsonConvert.SerializeObject(obj);
            jsonString = jsonString.Replace("'", "''");

            using (SqlConnection conn = new SqlConnection(connString))
            {
                var insertResult = conn.Execute($"exec sp_App_InsertAppPostLog '{module}','{userId}', '{jsonString}'");
                if (insertResult < 0)
                {
                    throw new Exception("Fail to insert Log.");
                }
            }
        }
    }
}
