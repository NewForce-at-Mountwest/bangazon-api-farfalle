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

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                //set the connection
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        //get all of the products in the database
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                //open our sql connection
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "SELECT p.Id AS 'Product Id', p.ProductTypeId, p.CustomerId, p.Price, p.Title, p.Description, p.Quantity, p.CustomerId, p.IsActive FROM Product p WHERE p.IsActive = 1";
                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Product> Products = new List<Product>();

                    while (reader.Read())
                    {
        
                        //make a new product for each product we get from our server
                        Product Product = new Product
                        {
                            
                            Id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Price = reader.GetInt32(reader.GetOrdinal("Price")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                        };
                        //add each product to our list of products
                        Products.Add(Product);
                    }
                    reader.Close();

                    //return the whole list of products
                    return Ok(Products);
                }
            }
        }
        //get single product based on id
        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                       SELECT Id,ProductTypeId, CustomerId, Price, Title, Description, Quantity, CustomerId, IsActive FROM Product WHERE Id = @id";
                    //add a parameter to have a specific id
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Product Product = null;

                    if (reader.Read())
                    {
                        //build ze product
                        Product = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Price = reader.GetInt32(reader.GetOrdinal("Price")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                        };
                    }
                    reader.Close();
                    //give us back the product we found at that id
                    return Ok(Product);
                }
            }
        }

        //post a product
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product Product)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Product (ProductTypeId, CustomerId, Price, Title, Description, Quantity, isActive)
                                        OUTPUT INSERTED.Id
                                        VALUES (@ProductTypeId, @CustomerId, @Price, @Title, @Description, @Quantity, 1)";
                    //HOPEFULLY prevent people from doing funky stuff to our db when they post a product
                    cmd.Parameters.Add(new SqlParameter("@ProductTypeId", Product.ProductTypeId));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", Product.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@Price", Product.Price));
                    cmd.Parameters.Add(new SqlParameter("@Title", Product.Title));
                    cmd.Parameters.Add(new SqlParameter("@Description", Product.Description));
                    cmd.Parameters.Add(new SqlParameter("@Quantity", Product.Quantity));
                  

                    int newId = (int)cmd.ExecuteScalar();
                    Product.Id = newId;
                    return CreatedAtRoute("GetProduct", new { id = newId }, Product);
                }
            }
        }
        //change an existing product
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Product Product)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Product
                                            SET ProductTypeId = @ProductTypeId,
                                                CustomerId = @CustomerId,
                                                Price = @Price,
                                                Title = @Title,
                                                Description = @Description,
                                                Quantity = @Quantity,
                                                IsActive = @IsActive
                                            WHERE Id = @id";
                        //make sure we can't put in weird stuff
                        cmd.Parameters.Add(new SqlParameter("@id", Product.Id));
                        cmd.Parameters.Add(new SqlParameter("@ProductTypeId", Product.ProductTypeId));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", Product.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@Price", Product.Price));
                        cmd.Parameters.Add(new SqlParameter("@Title", Product.Title));
                        cmd.Parameters.Add(new SqlParameter("@Description", Product.Description));
                        cmd.Parameters.Add(new SqlParameter("@Quantity", Product.Quantity));
                        cmd.Parameters.Add(new SqlParameter("@IsActive", Product.IsActive));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            //this is what we want to see; it means it updated
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        //this means that it didn't update
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        //delete the whole product
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
                            cmd.CommandText = @"DELETE Product Where Id = @id";
                        }
                        else
                        {
                            cmd.CommandText = @"UPDATE Product SET IsActive = 0 WHERE Id = @id";
                            
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
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ProductExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT ProductTypeId, CustomerId, Price, Title, Description, Quantity, CustomerId, IsActive
                        FROM Product
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}