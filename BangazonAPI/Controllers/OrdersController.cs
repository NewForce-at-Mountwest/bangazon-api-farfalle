using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrdersController(IConfiguration config)
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
        public async Task<IActionResult> Get(string include, string completed)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";
                    string selectOrders = @"SELECT o.Id, o.PaymentTypeId, o.CustomerId";

                    string fromOrder = " FROM [Order] o";

                    string customerString = ", c.Id, c.FirstName, c.LastName";

                    string join = " JOIN PaymentType pt ON o.Id = pt.CustomerId JOIN Customer c ON pt.CustomerId = c.Id JOIN OrderProduct op ON o.Id = op.OrderId JOIN Product p ON op.ProductId = p.Id";

                    string productString = ", p.Id AS 'ProductId', p.ProductTypeId, p.CustomerId, p.Price, p.Title, p.Description, p.Quantity, p.CustomerId";

                    string completedTrue = "WHERE PaymentTypeId > 1";
                    string completedFalse = "WHERE PaymentTypeId = 1";

                    //Conditionals for query strings

                    if (include == "customers")
                    {
                        command = $"{selectOrders}{customerString}{fromOrder}{join}";

                    }
                    else if (include == "products")
                    {
                        command = $"{selectOrders}{productString}{fromOrder}{join}";

                    }
                    else
                    {
                        command = $"{selectOrders}{fromOrder}";

                    }
                    if (completed == "false")
                    {
                        command += $"{completedFalse}";

                    }
                    if (completed == "true")
                    {
                        command += $"{completedTrue}";

                    }
                    cmd.CommandText = command;

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Order> orders = new List<Order>();

                    while (reader.Read())
                    {
                        Order order = new Order
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))
                        };
                        orders.Add(order);
                        if (include == "customers")
                        {
                            Customer customer = new Customer()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName"))
                            };
                            order.customer = customer;
                        }
                        if (include == "products")
                        {
                            Product products = new Product()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Price = reader.GetInt32(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                            };
                            order.Products.Add(products);
                        }
                        else
                        { }
                    }
                    reader.Close();
                    return Ok(orders);
                }
            }
        }

        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, PaymentTypeId, CustomerId FROM [Order] WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Order order = null;

                    if (reader.Read())
                    {
                        order = new Order
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))
                        };

                    }
                    reader.Close();
                    return Ok(order);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO [Order] (PaymentTypeId, CustomerId) OUTPUT INSERTED.Id VALUES (@PaymentTypeId, @CustomerId)";
                    cmd.Parameters.Add(new SqlParameter("@PaymentTypeId", order.PaymentTypeId));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", order.CustomerId));


                    int newId = (int)cmd.ExecuteScalar();
                    order.Id = newId;
                    return CreatedAtRoute("GetOrder", new { id = newId }, order);
                }
            }
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Order order)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE [Order] SET PaymentTypeId = @PaymentTypeId, CustomerId = @CustomerId WHERE Id = @id";


                        cmd.Parameters.Add(new SqlParameter("@PaymentTypeId", order.PaymentTypeId));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", order.CustomerId));
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
                if (!OrderExists(id))
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
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM [Order] WHERE ID = @id";
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
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool OrderExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, PaymentTypeId, CustomerId FROM Order WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

    }
}