using System.Text;
using System.Transactions;
using atm.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Npgsql;
using NuGet.Protocol.Plugins;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace atm.Controllers
{
    public class AtmController : Controller
    {

        NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=atm;User Id=postgres;Password=;Integrated Security=true;Pooling=true;");
        
        [Route("api/[controller]")]


        //Test 
        [Route("/test")]
        [HttpGet]
        public async Task<string> Test()
        {
            
            JObject job = new JObject();
           

           job.Add("TransactionID", 9);
                job.Add("TransactionAmount", 1000);
            
            return job.ToString();
            
        }

        //Register Customer
        [Route("register")]
        [HttpPost]
        public async Task<string> Post([FromForm] Customer customer)
        {
            Console.WriteLine(customer.ID);
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

                string query = "insert into customers(id_number,email, kra_pin, account_name, account_number, pin, mobile_number, withdrawal_limit, available_balance, actual_balance, registration_date) values(@ID,@Email, @KRAPIN, @AccountName, @AccountNumber, @PIN, @MobileNumber, @WithdrawalLimit, @AvailableBalance, @ActualBalance, @RegistrationDate)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.Add(new NpgsqlParameter("@ID", customer.ID));
                cmd.Parameters.Add(new NpgsqlParameter("@Email", customer.Email));
                cmd.Parameters.Add(new NpgsqlParameter("@KRAPIN", customer.KRAPIN));
                cmd.Parameters.Add(new NpgsqlParameter("@AccountName", customer.AccountName));
                cmd.Parameters.Add(new NpgsqlParameter("@AccountNumber", customer.AccountNumber));
                cmd.Parameters.Add(new NpgsqlParameter("@PIN", hashPIN));
                cmd.Parameters.Add(new NpgsqlParameter("@MobileNumber", customer.MobileNumber));
                cmd.Parameters.Add(new NpgsqlParameter("@WithdrawalLimit", customer.WithdrawalLimit));
                cmd.Parameters.Add(new NpgsqlParameter("@AvailableBalance", customer.AvailableBalance));
                cmd.Parameters.Add(new NpgsqlParameter("@ActualBalance", customer.ActualBalance));
                cmd.Parameters.Add(new NpgsqlParameter("@RegistrationDate", DateTime.Now));

                string queryString = cmd.CommandText;
                foreach (NpgsqlParameter param in cmd.Parameters)
                {
                    queryString = queryString.Replace(param.ParameterName, param.Value.ToString());
                }

                Console.WriteLine("Executing SQL query: " + queryString);

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
                success.Add("error", error);
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
            string sqlExists = "select CustomerID, Email, PIN, AccountName, MobileNumber, WithdrawalLimit from dbo.Customers where Email='" + user.Email + "'";
            
            NpgsqlCommand NpgsqlCommand = new NpgsqlCommand(sqlExists, conn);
            conn.Open();
            NpgsqlDataReader reader = NpgsqlCommand.ExecuteReader();
            string exists = "NO";
            JObject job = new JObject();
            while (reader.Read())
            {
                var CustomerID = reader["CustomerID"].ToString();
                var PIN = reader["PIN"].ToString();
                var mobile = reader["MobileNumber"].ToString();
                var Name = reader["AccountName"].ToString();
                var WithdrawalLimit = reader["WithdrawalLimit"].ToString();

                job.Add("MobileNumber", mobile);
                job.Add("AccountName", Name);
                job.Add("CustomerID", CustomerID);
                job.Add("WithdrawalLimit", WithdrawalLimit);


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

        //withdraw
        [Route("withdraw")]
        [HttpPost]
        public async Task<string> Withdraw([FromForm] Withdrawal withdrawal)
        {
            //Check balance
            JObject success = new JObject();
            var Balance = 0.0f;
            var AvailableBalance = 0.0f;
            try
            {
                string sqlBalance = "select ActualBalance, AvailableBalance from dbo.Customers where CustomerID='" + withdrawal.CustomerID + "'";
                NpgsqlCommand NpgsqlCommand = new NpgsqlCommand(sqlBalance, conn);
                conn.Open();
                NpgsqlDataReader reader = NpgsqlCommand.ExecuteReader();
                int noOfRowsAffected = 0;
                while (reader.Read())
                {
                    Balance = float.Parse(reader["ActualBalance"].ToString());
                    AvailableBalance = float.Parse(reader["AvailableBalance"].ToString());
                    
                    
                }
                conn.Close();
                //Save transaction
                if (withdrawal.Amount <= Balance)
                {
                    
                    conn.Open();
                   
                    string query = "insert into Transactions values(@TransactionTypeID, @TransactionAmount, @TransactionDate)";
                    NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                    cmd.Parameters.Add(new NpgsqlParameter("@TransactionTypeID", 1));
                    cmd.Parameters.Add(new NpgsqlParameter("@TransactionAmount", withdrawal.Amount));
                    cmd.Parameters.Add(new NpgsqlParameter("@TransactionDate", DateTime.Now));


                    noOfRowsAffected = cmd.ExecuteNonQuery();

                    conn.Close();

                }
                else
                {
                    success.Add("success", "No enough funds");
                }
                int rowsChanged = 0;

                //Save withdrawal
                Console.WriteLine("start with");
                var TransactionID = "";
                if (noOfRowsAffected > 0)
                {
                    string sqlTransaction = "select MAX(TransactionID) as id from dbo.Transactions";
                    NpgsqlCommand sqlId = new NpgsqlCommand(sqlTransaction, conn);
                    conn.Open();
                    NpgsqlDataReader sqlDataReader = sqlId.ExecuteReader();
                    
                    while (sqlDataReader.Read())
                    {
                       TransactionID = sqlDataReader["id"].ToString();
                        Console.WriteLine(TransactionID);

                    }
                    conn.Close();

                    //Save withdrawal
                    string query = "insert into Withdrawals values(@TransactionID,@CustomerID, @Amount, @WithdrawalDate)";
                    NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                    cmd.Parameters.Add(new NpgsqlParameter("@TransactionID", TransactionID));
                    cmd.Parameters.Add(new NpgsqlParameter("@CustomerID", withdrawal.CustomerID));
                    cmd.Parameters.Add(new NpgsqlParameter("@Amount", withdrawal.Amount));
                    cmd.Parameters.Add(new NpgsqlParameter("@WithdrawalDate", DateTime.Now));

                    conn.Open();
                    rowsChanged = cmd.ExecuteNonQuery();
                    conn.Close();
                }

                //Update balance
                if (rowsChanged > 0)
                {
                    string sqUpdate = "UPDATE Customers SET ActualBalance=@ActualBalance, AvailableBalance=@AvailableBalance where CustomerID = @CustomerID";
                    NpgsqlCommand sq = new NpgsqlCommand(sqUpdate, conn);

                    sq.Parameters.Add(new NpgsqlParameter("@ActualBalance", Balance-withdrawal.Amount));
                    sq.Parameters.Add(new NpgsqlParameter("@AvailableBalance", AvailableBalance-withdrawal.Amount));
                    sq.Parameters.Add(new NpgsqlParameter("@CustomerID", withdrawal.CustomerID));
                    conn.Open();
                    int rowsAffected = sq.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        success.Add("success", "Request success");
                    }
                    else
                    {
                        success.Add("success", "Request failed");
                    }
                    conn.Close();

                   
                }


                

                return success.ToString();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return success.ToString();
            }
        }

        //select balance
        [Route("balance")]
        [HttpPost]
        public async Task<string> Balance([FromForm] Balance balance)
        {
            string query = "select * from dbo.Customers where Email='" + balance.Email + "'";

            NpgsqlCommand NpgsqlCommand = new NpgsqlCommand(query, conn);
            conn.Open();
            NpgsqlDataReader reader = NpgsqlCommand.ExecuteReader();
            
            JObject job = new JObject();
            while (reader.Read())
            {
                var AccountName = reader["AccountName"].ToString().ToUpper();
                var AccountNumber = reader["AccountNumber"].ToString();
                var AvailableBalance = float.Parse(reader["AvailableBalance"].ToString());
                var ActualBalance = float.Parse(reader["ActualBalance"].ToString());
                

                job.Add("AccountName", AccountName);
                job.Add("AccountNumber", AccountNumber);
                job.Add("ActualBalance", ActualBalance);
                job.Add("AvailableBalance", AvailableBalance);


            }



             return job.ToString();
            
        }
        //Transfer funds
        [Route("transfer")]
        [HttpPost]
        public async Task<string> Transfer([FromForm] Transfer transfer)
        {
            //Check balance
            JObject success = new JObject();
            var Balance = 0.0f;
            var AvailableBalance = 0.0f;
            try
            {
                string sqlBalance = "select ActualBalance, AvailableBalance from dbo.Customers where Email='" + transfer.SenderEmail + "'";
                NpgsqlCommand NpgsqlCommand = new NpgsqlCommand(sqlBalance, conn);
                conn.Open();
                Console.WriteLine(sqlBalance);
                NpgsqlDataReader reader = NpgsqlCommand.ExecuteReader();
                int noOfRowsAffected = 0;
                while (reader.Read())
                {
                    Balance = float.Parse(reader["ActualBalance"].ToString());
                    AvailableBalance = float.Parse(reader["AvailableBalance"].ToString());
                    Console.WriteLine(Balance);
                    Console.WriteLine(AvailableBalance);



                }
                conn.Close();
                //Save transaction
                if (transfer.Amount < Balance)
                {

                    conn.Open();

                    string query = "insert into Transactions values(@TransactionTypeID, @TransactionAmount, @TransactionDate)";
                    NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                    cmd.Parameters.Add(new NpgsqlParameter("@TransactionTypeID", 2));
                    cmd.Parameters.Add(new NpgsqlParameter("@TransactionAmount", transfer.Amount));
                    cmd.Parameters.Add(new NpgsqlParameter("@TransactionDate", DateTime.Now));


                    noOfRowsAffected = cmd.ExecuteNonQuery();

                    conn.Close();

                }
                else
                {
                    success.Add("success", "No enough funds");
                }
                int rowsChanged = 0;

                //Save withdrawal
                
               
                var TransactionID = "";
                if (noOfRowsAffected > 0)
                {
                    string sqlTransaction = "select MAX(TransactionID) as id from dbo.Transactions";
                    NpgsqlCommand sqlId = new NpgsqlCommand(sqlTransaction, conn);
                    conn.Open();
                    NpgsqlDataReader sqlDataReader = sqlId.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        TransactionID = sqlDataReader["id"].ToString();
                        Console.WriteLine(TransactionID);

                    }
                    conn.Close();

                    

                    //Save withdrawal
                    string query = "insert into Transfers values(@TransactionID,@SenderEmail, @RecipientAccount, @Amount, @TransferDate)";
                    NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                    cmd.Parameters.Add(new NpgsqlParameter("@TransactionID", TransactionID));
                    cmd.Parameters.Add(new NpgsqlParameter("@SenderEmail", transfer.SenderEmail));
                    cmd.Parameters.Add(new NpgsqlParameter("@RecipientAccount", transfer.RecipientAccount));
                    cmd.Parameters.Add(new NpgsqlParameter("@Amount", transfer.Amount));
                    cmd.Parameters.Add(new NpgsqlParameter("@TransferDate", DateTime.Now));

                    conn.Open();
                    rowsChanged = cmd.ExecuteNonQuery();
                    conn.Close();
                }

                //Update balance
                int senderRows = 0;
                if (rowsChanged > 0)
                {
                    string sqUpdate = "UPDATE Customers SET ActualBalance=@ActualBalance, AvailableBalance=@AvailableBalance where Email = @SenderEmail";
                    NpgsqlCommand sq = new NpgsqlCommand(sqUpdate, conn);

                    sq.Parameters.Add(new NpgsqlParameter("@ActualBalance", Balance - transfer.Amount));
                    sq.Parameters.Add(new NpgsqlParameter("@AvailableBalance", AvailableBalance - transfer.Amount));
                    sq.Parameters.Add(new NpgsqlParameter("@SenderEmail", transfer.SenderEmail));
                    conn.Open();
                    senderRows = sq.ExecuteNonQuery();
                    conn.Close();


                }

                //update reciepient account
                var ActualBalanceRecipient = 0.0f;
                var AvailableBalanceRecipient = 0.0f;
                var AccountName = "";
                if (senderRows > 0)
                {
                    string sqlRecipient = "select ActualBalance, AvailableBalance, AccountName from dbo.Customers where AccountNumber = '"+transfer.RecipientAccount + "'";
                    NpgsqlCommand recipientCommand = new NpgsqlCommand(sqlRecipient, conn);
                    conn.Open();
                    NpgsqlDataReader sqlDataReader = recipientCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        ActualBalanceRecipient = float.Parse(sqlDataReader["ActualBalance"].ToString());
                        AvailableBalanceRecipient = float.Parse(sqlDataReader["AvailableBalance"].ToString());
                        AccountName = sqlDataReader["AccountName"].ToString().ToUpper();

                    }
                    conn.Close();

                    string sqUpdate = "UPDATE Customers SET ActualBalance=@ActualBalance, AvailableBalance=@AvailableBalance where AccountNumber = @RecipientAccount";
                    NpgsqlCommand sq = new NpgsqlCommand(sqUpdate, conn);

                    sq.Parameters.Add(new NpgsqlParameter("@ActualBalance", Balance + transfer.Amount));
                    sq.Parameters.Add(new NpgsqlParameter("@AvailableBalance", AvailableBalance + transfer.Amount));
                    sq.Parameters.Add(new NpgsqlParameter("@RecipientAccount", transfer.RecipientAccount));
                    conn.Open();
                    int recieverRows = sq.ExecuteNonQuery();
                    if (recieverRows > 0)
                    {
                        success.Add("success", "Request success");
                        success.Add("account", AccountName);
                    }
                    else
                    {
                        success.Add("success", "Request failed");
                    }

                    conn.Close();
                }




                return success.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return success.ToString();
            }
        }


    }
}
