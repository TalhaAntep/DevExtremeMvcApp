using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;



    public static class DbLogger
    {
        private static string _connectionString = @"Server=localhost,1433;Database=ShapePortalDb;User Id=Talha;Password=A123ebr123;";
        
    public static void Log(string apiName, string userID , string message , string level ="Info")
        {

        try
        {

            using (var context = new SqlConnection(_connectionString))
            {
                context.Open();

                var cmd = new SqlCommand("INSERT INTO log_Table (ApiName, UserId, Message, Level) VALUES (@ApiName, @UserId, @Message, @Level)", context);
                cmd.Parameters.AddWithValue("@ApiName", apiName);
                cmd.Parameters.AddWithValue("@UserId", userID?? "");
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@Level", level);
                cmd.ExecuteNonQuery();
            }
        }
        catch
        {

        }
           
    }

}   

