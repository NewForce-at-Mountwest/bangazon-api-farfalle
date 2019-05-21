using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

//This is the controller to allow the client to access read, update, and create productTypes. It has two delete functionalities; normal delete will archive the product by setting the isActive property to false, and the query string HardDelete=true will trigger a hard delete

//developed by: connor fitzgerald
namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypeController(IConfiguration config)
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

        [HttpGet]

        //get all productTypes
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = @"SELECT Id AS 'Product Type Id',
                    Name AS 'Product Type Name',
                    IsActive AS 'Active'
                    FROM ProductType WHERE IsActive = 1";
   
                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<ProductType> ProductTypes = new List<ProductType>();

                    while (reader.Read())
                    {
                        ProductType productType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Product Type Id")),
                            Name = reader.GetString(reader.GetOrdinal("Product Type Name")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("Active"))

                        };
      
                            ProductTypes.Add(productType);
                        }

                 
                    return Ok(ProductTypes);
                }
            }
        }

        [HttpGet("{id}", Name = "GetProductType")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, IsActive FROM ProductType 
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    ProductType ProductType = null;

                    if (reader.Read())
                    {
                        ProductType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                        };
                    }
                    reader.Close();

                    return Ok(ProductType);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductType ProductType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO ProductType (Name, IsActive)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Name, 1)";
                    cmd.Parameters.Add(new SqlParameter("@Name", ProductType.Name));
                  

                    int newId = (int)cmd.ExecuteScalar();
                    ProductType.Id = newId;
                    return CreatedAtRoute("GetProductType", new { id = newId }, ProductType);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ProductType ProductType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE ProductType
                                            SET Name = @Name,
                                            IsActive = 1
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Name", ProductType.Name));
                       
                        cmd.Parameters.Add(new SqlParameter("@id", ProductType.Id));


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
                if (!ProductTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, bool HardDelete)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        if (HardDelete == true)
                        {
                            cmd.CommandText = @"DELETE FROM ProductType WHERE id = @id";
                            
                        }

                        else { cmd.CommandText = @"UPDATE                                   ProductType                                SET IsActive = 0
                                            WHERE Id = @id"; }

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
                if (!ProductTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ProductTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, IsActive
                        FROM ProductType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}