using System;
using System.Configuration;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;
using Dapper;
using Microsoft.IdentityModel.Tokens;
using System.Transactions;

namespace Bank
{
    public partial class Form1 : Form
    {
        

        public Form1()
        {
            
            InitializeComponent();
        }
        bool InsertError = false;

        private void LoadErrorInsert(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Enter name again");
               
            }
            else if (String.IsNullOrEmpty(textBox2.Text) || !decimal.TryParse(textBox2.Text, out decimal dec))
            {
                MessageBox.Show("Enter Balance again");
            }   
            else
            {
                InsertError= true;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddJsonFile("C:\\Users\\Zyad Elfakharany\\source\\repos\\Bank\\Bank\\appsetting.json").Build();
            //MessageBox.Show(configuration.GetSection("constr").Value);
            var WalletToInsert = new Wallet();
            try
            {
                WalletToInsert = new Wallet
                {

                    WalletHolder = textBox1.Text,
                    WalletBalance = Convert.ToDecimal(textBox2.Text)
                };
                LoadErrorInsert(sender, e);
            }
            catch
            {
                LoadErrorInsert(sender, e);
            }
            if (InsertError == true)
            {
                var conn = new SqlConnection(configuration.GetSection("constr").Value);
                var HolderParameter = new SqlParameter
                {
                    ParameterName = "@Holder",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = WalletToInsert.WalletHolder
                };
                var BalanceParameter = new SqlParameter
                {
                    ParameterName = "@Balance",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = ParameterDirection.Input,
                    Value = WalletToInsert.WalletBalance
                };
                var comm = new SqlCommand("AddWallets", conn);
                comm.Parameters.Add(HolderParameter);
                comm.Parameters.Add(BalanceParameter);
                comm.CommandType = CommandType.StoredProcedure;
                conn.Open();
                WalletToInsert.WalletID = Convert.ToInt32(comm.ExecuteScalar());
                MessageBox.Show($"{WalletToInsert.ToString()}\n added successfully");
              
                conn.Close();
            }
            //textBox1.Text = textBox2.Text = String.Empty;   

        }
        bool DeleteError = false;

        private void LoadErrorDelete(object sender, EventArgs e)
        {
          
            if (String.IsNullOrEmpty(textBox3.Text) || !int.TryParse(textBox3.Text, out int res))
            {
                MessageBox.Show("Enter ID again");
            }
            else
            {
                DeleteError= true;
            }
        }
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddJsonFile("C:\\Users\\Zyad Elfakharany\\source\\repos\\Bank\\Bank\\appsetting.json").Build();
            LoadErrorDelete(sender, e);
            if (DeleteError == true)
            {
                var query = "IF EXISTS(SELECT id FROM Wallets WHERE id = @id)" +
                             "BEGIN\n" +
                                "DELETE FROM Wallets WHERE id = @id\n" +
                             "END";

                var idParamter = new SqlParameter
                {
                    ParameterName = "@id",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = ParameterDirection.Input,
                    Value = Convert.ToInt32(textBox3.Text)
                };

                var conn = new SqlConnection(configuration.GetSection("constr").Value);
                var comm = new SqlCommand(query, conn);
                comm.Parameters.Add(idParamter);
                comm.CommandType = CommandType.Text;
                conn.Open();
                if (comm.ExecuteNonQuery() > 0)
                {
                    MessageBox.Show($" Wallet deleted Succesfully ");
                }
                else
                {
                    MessageBox.Show($" {textBox3.Text} can't be deleted ");
                }
                conn.Close();
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void MinAndMax_Click(object sender, EventArgs e)
        {
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddJsonFile("C:\\Users\\Zyad Elfakharany\\source\\repos\\Bank\\Bank\\appsetting.json").Build();

            var sql = "SELECT MIN(Balance) FROM Wallets;" +
                       "SELECT MAX(Balance) FROM Wallets;";

            var db = new SqlConnection(configuration.GetSection("constr").Value);
            var multi = db.QueryMultiple(sql);

            MessageBox.Show(
          $"Min = {multi.Read<decimal>().Single()}" +
          $"\nMax = {multi.Read<decimal>().Single()}");

        }

        private void Update_Click(object sender, EventArgs e)
        {
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddJsonFile("C:\\Users\\Zyad Elfakharany\\source\\repos\\Bank\\Bank\\appsetting.json").Build();
            var sql = "UPDATE Wallets SET Holder = @Holder , Balance = @Balance Where id = @id";
            var dp = new SqlConnection(configuration.GetSection("constr").Value);
            var Prams =
                new
            {
                id=Convert.ToInt32(textBox3.Text),
                Holder=textBox1.Text,
                Balance=Convert.ToDecimal(textBox2.Text)
            };
            int nOfRows =  dp.Execute(sql, Prams);
            if(nOfRows>0)
            {
                MessageBox.Show("Updated successfully");
            }


        }

        private void Display_Click(object sender, EventArgs e)
        {
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddJsonFile("C:\\Users\\Zyad Elfakharany\\source\\repos\\Bank\\Bank\\appsetting.json").Build();
            var sql = "SELECT * FROM Wallets";
            var dp = new SqlConnection(configuration.GetSection("constr").Value);
            SqlCommand comm = new SqlCommand(sql, dp);
            comm.CommandType = CommandType.Text;
            dp.Open();
            SqlDataReader reader = comm.ExecuteReader();
            Wallet wallet = null;
            while (reader.Read())
            {
                wallet = new Wallet
                {
                    WalletID = reader.GetInt32(0),
                    WalletHolder = reader.GetString(1),
                    WalletBalance = reader.GetDecimal(2)
                };

                MessageBox.Show($"{wallet}");
            }
            //By Dapper in Latest version of C#
            /*using var db = new SqlConnection(configuration.GetSection("constr").Value);
            db.Open();

            var wallets = db.Query<Wallet>("SELECT * FROM Wallets").ToList();

            wallets.ForEach(wallet => MessageBox.Show(wallet));*/

        }

        private void TransactButton_Click(object sender, EventArgs e)
        {
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddJsonFile("C:\\Users\\Zyad Elfakharany\\source\\repos\\Bank\\Bank\\appsetting.json").Build();
            var db = new SqlConnection(configuration.GetSection("constr").Value);
            decimal amountToTranfer = Convert.ToDecimal(textBox5.Text);
            /*using (var transactionScope = new TransactionScope())
            {
                var walletFrom = db.QuerySingle<Wallet>
                      ("SELECT * FROM Wallets Where id = @id", new { id = Convert.ToInt32(textBox3.Text) });
                var walletTo = db.QuerySingle<Wallet>
                     ("SELECT * FROM Wallets Where id = @id", new { id = Convert.ToInt32(textBox4.Text) });
                db.Execute("UPDATE Wallets Set Balance = @Balance Where id = @id",
                       new
                       {
                           id = walletFrom.WalletID,
                           Balance = walletFrom.WalletBalance - amountToTranfer
                       }
                   );
                db.Execute("UPDATE Wallets Set Balance = @Balance Where id = @id",
                       new
                       {
                           id = walletTo.WalletID,
                           Balance = walletTo.WalletBalance + amountToTranfer
                       }
                   );
                transactionScope.Complete();*/


                SqlCommand command = db.CreateCommand();

                command.CommandType = CommandType.Text;

                

                

                
                    var idParamter = new SqlParameter
                    {
                        ParameterName = "@id",
                        SqlDbType = System.Data.SqlDbType.Int,
                        Direction = ParameterDirection.Input,
                        Value = Convert.ToInt32(textBox3.Text)
                    };
                    var amountToTranferPar = new SqlParameter
                    {
                        ParameterName = "@amountToTranfer",
                        SqlDbType = System.Data.SqlDbType.Decimal,
                        Direction = ParameterDirection.Input,
                        Value = Convert.ToInt32(textBox5.Text)
                    };
                    command.CommandText = "UPDATE Wallets Set Balance = Balance - @amountToTranfer Where id = @id";
                    command.Parameters.Add(amountToTranferPar);
                    command.Parameters.Add(idParamter);
                    db.Open();
                    SqlTransaction transaction = db.BeginTransaction();

                    command.Transaction = transaction;
                    command.ExecuteNonQuery();

                    var idToTrasactParamter = new SqlParameter
                    {
                        ParameterName = "@idtotransact",
                        SqlDbType = System.Data.SqlDbType.Int,
                        Direction = ParameterDirection.Input,
                        Value = Convert.ToInt32(textBox4.Text)
                    };
                    

                    command.CommandText = "UPDATE Wallets Set Balance = Balance + @amountToTranfer Where id = @idtotransact";
                    //command.Parameters.Add(amountToTranferPar);
                    command.Parameters.Add(idToTrasactParamter);
                    command.ExecuteNonQuery();

                    transaction.Commit();

                    Console.WriteLine("Transaction of transfer completed successfully");

                
               
            }
    }
    
}
