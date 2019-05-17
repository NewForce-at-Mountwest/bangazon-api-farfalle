//**********************************************************************************************//
// This Controller gives the client access to getAll, getSingle, Post, Put, and Delete 
// on the Payment Types Resource.  It does not support query functionality at this point.
// Created by Sydney Wait
//*********************************************************************************************//



using BangazonAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace BangazonAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentTypeController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }


        //GET:Code for getting a list of PaymentTypes which are ACTIVE in the system
        [HttpGet]
        public async Task<IActionResult> GetAllPaymentTypes()
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string commandText = $"SELECT Id, AcctNumber, [Name], CustomerId, IsActive FROM PaymentType WHERE IsActive = 1";



                    cmd.CommandText = commandText;



                    SqlDataReader reader = cmd.ExecuteReader();
                    List<PaymentType> paymentTypes = new List<PaymentType>();
                    PaymentType paymentType = null;


                    while (reader.Read())
                    {
                        paymentType = new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))

                        };



                        paymentTypes.Add(paymentType);
                    }


                    reader.Close();

                    return Ok(paymentTypes);
                }
            }
        }



        //GET: Code for getting a single paymentType (active or not)
        [HttpGet("{id}", Name = "PaymentType")]
        public async Task<IActionResult> GetSinglePaymentType([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT Id, AcctNumber, [Name], CustomerId, IsActive from PaymentType WHERE Id=@id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    PaymentType paymentTypeToDisplay = null;

                    while (reader.Read())
                    {

                        {
                            paymentTypeToDisplay = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))

                            };
                        };
                    };


                    reader.Close();

                    return Ok(paymentTypeToDisplay);
                }
            }
        }

        //  POST: Code for creating a paymentType
        [HttpPost]
        public async Task<IActionResult> PostPaymentType([FromBody] PaymentType paymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    
                    cmd.CommandText = $@"INSERT INTO PaymentType (AcctNumber, [Name], CustomerId, IsActive)
                                                    OUTPUT INSERTED.Id
                                                    VALUES (@AcctNumber, @Name, @CustomerId, 1)";
                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@Name", paymentType.Name));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", paymentType.CustomerId));
                    



                    int newId = (int)cmd.ExecuteScalar();
                    paymentType.Id = newId;
                    return CreatedAtRoute("PaymentType", new { id = newId }, paymentType);
                }
            }
        }

        // PUT: Code for editing a paymentType
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentType([FromRoute] int id, [FromBody] PaymentType paymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE PaymentType
                                                        SET AcctNumber = @AcctNumber,
                                                        Name = @Name,
                                                        CustomerId=@CustomerId,
                                                        isActive = 1
                                                        WHERE id = @id";

                        cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.AcctNumber));
                        cmd.Parameters.Add(new SqlParameter("@Name", paymentType.Name));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", paymentType.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: Code for deleting a payment type--soft delete actually changes 'isActive' to 0 (false)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentType([FromRoute] int id, bool HardDelete )
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                        if(HardDelete == true)
                        {
                        cmd.CommandText = @"DELETE PaymentType
                                              WHERE id = @id";
                        }                    
                        else { 
                        cmd.CommandText = @"UPDATE PaymentType
                                            SET isActive = 0
                                            WHERE id = @id";
                        }

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        private bool PaymentTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, name
                                    FROM PaymentType
                                    WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }


    }
}




