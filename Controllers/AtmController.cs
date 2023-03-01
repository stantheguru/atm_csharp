using System.Text;
using atm.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;

namespace atm.Controllers
{
    public class AtmController : Controller
    {
        SqlConnection conn = new SqlConnection(@"Server=DESKTOP-OFRUC79;database=atm;integrated security=true");

        [Route("api/[controller]")]
     

        //Register Customer
        [Route("register")]
        [HttpPost]
        public async Task<string> Post([FromForm] Customer customer)
        {

            JObject success = new JObject();
            string hashPIN = "";
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(customer.PIN.ToString());
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                hashPIN = sb.ToString();
            }


            try
            {

                string query = "insert into Customers values(@ID,@Email, @KRAPIN, @AccountName, @AccountNumber, @PIN, @MobileNumber, @Limit, @AvailableBalance, @ActualBalance, @RegistrationDate)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add(new SqlParameter("@ID", customer.ID));
                cmd.Parameters.Add(new SqlParameter("@Email", customer.Email));
                cmd.Parameters.Add(new SqlParameter("@KRAPIN", customer.KRAPIN));
                cmd.Parameters.Add(new SqlParameter("@AccountName", customer.AccountName));
                cmd.Parameters.Add(new SqlParameter("@AccountNumber", customer.AccountNumber));
                cmd.Parameters.Add(new SqlParameter("@PIN", hashPIN));
                cmd.Parameters.Add(new SqlParameter("@MobileNumber", customer.MobileNumber));
                cmd.Parameters.Add(new SqlParameter("@Limit", customer.AvailableBalance));
                cmd.Parameters.Add(new SqlParameter("@AvailableBalance", customer.AvailableBalance));
                cmd.Parameters.Add(new SqlParameter("@ActualBalance", customer.ActualBalance));
                cmd.Parameters.Add(new SqlParameter("@RegistrationDate", DateTime.Now));


                conn.Open();
                int noOfRowsAffected = cmd.ExecuteNonQuery();
                conn.Close();
                if(noOfRowsAffected > 0)
                {
                    success.Add("success", "saved");
                }
                else
                {
                    success.Add("success", "failed");
                }
                return success.ToString();


               
                
            }catch(Exception e)
            {
                
                string error = e.Message.ToString();
                int index = error.IndexOf("The duplicate key value is");
                if (index>0)
                {
                    string substringError = error.Substring(index);
                    int indexStart =substringError.IndexOf("(");
                    int indexEnd = substringError.IndexOf(')');
                    Console.WriteLine(indexStart);
                    string duplicateError = substringError.Substring(indexStart, indexEnd-indexStart+1);
                    if (duplicateError.Equals("(" + customer.Email+")"))
                    {
                        success.Add("success", "Email exists");
                    }else if (duplicateError.Equals("(" + customer.AccountNumber.ToString() + ")")){
                        success.Add("success", "Account exists");
                    }
                }
                

               
                return success.ToString();
            }
            
        }

        //Login
        [Route("login")]
        [HttpPost]
        public async Task<string> Login([FromForm] Login user)
        {
            string sqlExists = "select Email, PIN, AccountName, MobileNumber from dbo.Customers where Email='" + user.Email + "'";

            SqlCommand sqlCommand = new SqlCommand(sqlExists, conn);
            conn.Open();
            SqlDataReader reader = sqlCommand.ExecuteReader();
            string exists = "NO";
            JObject job = new JObject();
            while (reader.Read())
            {

                var PIN = reader["PIN"].ToString();
                var mobile = reader["MobileNumber"].ToString();
                var Name = reader["AccountName"].ToString();
                
                job.Add("MobileNumber", mobile);
                job.Add("AccountName", Name);


                string hashPIN = "";
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(user.PIN.ToString());
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("X2"));
                    }
                    hashPIN = sb.ToString();
                }
                Console.WriteLine(job);
                if (PIN == hashPIN)
                {
                    exists = "YES";
                    job.Add("exists", "YES");
                }
                else
                {
                    exists = "NO";
                    job.Add("exists", "NO");
                }





            }



            conn.Close();
            if (exists == "YES")
            {

                return job.ToString();
            }
            else
            {
                return job.ToString();
            }
        }

    }
}
