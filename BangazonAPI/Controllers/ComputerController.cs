﻿//**********************************************************************************************//
// This Controller gives the client access to getAll, getSingle, Post, Put, and Delete 
// (soft delete updates the decommission date to a non-null value)
// on the Computer Resource.  It does not support query functionality at this point.
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
    public class ComputerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputerController(IConfiguration config)
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


        //GET:Code for getting a list of Computers which are ACTIVE in the system
        [HttpGet]
        public async Task<IActionResult> GetAllComputers()
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string commandText = $"SELECT Id, PurchaseDate, Make, Manufacturer FROM Computer WHERE DecommissionDate IS NULL";

                    cmd.CommandText = commandText;

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Computer> computers = new List<Computer>();
                    Computer computer = null;


                    while (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecommissionDate = reader.GetDateTime(reader.GetOrdinal("DecommissionDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))

                        };


                        computers.Add(computer);
                    }


                    reader.Close();

                    return Ok(computers);
                }
            }
        }



        //GET: Code for getting a single computer (active or not)
        [HttpGet("{id}", Name = "Computer")]
        public async Task<IActionResult> GetSingleComputer([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT Id, PurchaseDate, Make, Manufacturer, DecommissionDate FROM Computer WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer computerToDisplay = null;

                    while (reader.Read())
                    {

                        {
                            computerToDisplay = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                DecommissionDate = reader.GetDateTime(reader.GetOrdinal("DecommissionDate")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))

                            };

                            //Check to see if the decommission date is null.  If not, add it to the object

                            //if (!reader.IsDBNull(reader.GetOrdinal("DecommissionDate")))
                            //{
                            //    computerToDisplay.DecommissionDate = reader.GetDateTime(reader.GetOrdinal("DecommissionDate"));
                            //}
                            //else
                            //    computerToDisplay.DecommissionDate = null;
                            //{

                            //}
                        };
                    };


                    reader.Close();

                    return Ok(computerToDisplay);
                }
            }
        }

        ////  POST: Code for creating a computer
        //[HttpPost]
        //public async Task<IActionResult> Computer([FromBody] Computer computer)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {

        //            cmd.CommandText = $@"INSERT INTO Computer (AcctNumber, [Name], CustomerId, IsActive)
        //                                            OUTPUT INSERTED.Id
        //                                            VALUES (@AcctNumber, @Name, @CustomerId, 1)";
        //            cmd.Parameters.Add(new SqlParameter("@AcctNumber", computer.AcctNumber));
        //            cmd.Parameters.Add(new SqlParameter("@Name", computer.Name));
        //            cmd.Parameters.Add(new SqlParameter("@CustomerId", computer.CustomerId));




        //            int newId = (int)cmd.ExecuteScalar();
        //            computer.Id = newId;
        //            return CreatedAtRoute("Computer", new { id = newId }, computer);
        //        }
        //    }
        //}

        //// PUT: Code for editing a computer
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Computer([FromRoute] int id, [FromBody] Computer computer)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"UPDATE Computer
        //                                                SET AcctNumber = @AcctNumber,
        //                                                Name = @Name,
        //                                                CustomerId=@CustomerId,
        //                                                isActive = 1
        //                                                WHERE id = @id";

        //                cmd.Parameters.Add(new SqlParameter("@AcctNumber", computer.AcctNumber));
        //                cmd.Parameters.Add(new SqlParameter("@Name", computer.Name));
        //                cmd.Parameters.Add(new SqlParameter("@CustomerId", computer.CustomerId));
        //                cmd.Parameters.Add(new SqlParameter("@id", id));

        //                int rowsAffected = cmd.ExecuteNonQuery();
        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }
        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (!ComputerExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        //// DELETE: Code for deleting a payment type--soft delete actually changes 'isActive' to 0 (false)
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Computer([FromRoute] int id, bool HardDelete)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {

        //                if (HardDelete == true)
        //                {
        //                    cmd.CommandText = @"DELETE Computer
        //                                      WHERE id = @id";
        //                }
        //                else
        //                {
        //                    cmd.CommandText = @"UPDATE Computer
        //                                    SET isActive = 0
        //                                    WHERE id = @id";
        //                }

        //                cmd.Parameters.Add(new SqlParameter("@id", id));
        //                int rowsAffected = cmd.ExecuteNonQuery();
        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }
        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (!ComputerExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}


        //private bool ComputerExists(int id)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"SELECT Id, name
        //                            FROM Computer
        //                            WHERE Id = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));

        //            SqlDataReader reader = cmd.ExecuteReader();
        //            return reader.Read();
        //        }
        //    }
        //}


    }
}




