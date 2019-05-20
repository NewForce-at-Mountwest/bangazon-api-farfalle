using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase

        {
            private readonly IConfiguration _config;
            public CustomerController(IConfiguration config)
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
            public async Task<IActionResult> Get(string include, string q)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                    //SqlCommands dependant on query strings

                        string command = "";
                        string customerColumns = @"SELECT Customer.Id, Customer.FirstName, Customer.LastName";

                        string customerTable = " FROM Customer";

                        string paymentColumns = ", PaymentType.Id AS PaymentTypeId, PaymentType.AcctNumber, PaymentType.Name AS PaymentName, PaymentType.CustomerId, PaymentType.IsActive";

                        string paymentTable = " JOIN PaymentType ON Customer.Id = PaymentType.CustomerId";

                        string productColumns = ", Product.Id as ProductId, Product.ProductTypeId, Product.CustomerId AS ProductCustomerId, Product.Price, Product.Title, Product.Description, Product.Quantity, Product.CustomerId";

                        string productTable = " JOIN Product ON Product.CustomerId = Customer.Id";

                        string searchTable = $" WHERE Customer.FirstName LIKE '%{q}%' OR Customer.LastName LIKE '%{q}%'";

                        //Conditionals for query strings

                        if (include == "paymentTypes")
                        {
                            command = $"{customerColumns}{paymentColumns}{customerTable}{paymentTable}";
                        
                        }
                        else if (include == "products")
                        {  
                            command = $"{customerColumns}{productColumns}{customerTable}{productTable}";
                                              
                        }
                        else
                        {
                            command = $"{customerColumns}{customerTable}";      
                        
                        }

                    if (q != null)
                    {
                        command = $"{command}{searchTable}";
                        
                    } else
                    {
                        command = command;
                    }

                    cmd.CommandText = command;


                    SqlDataReader reader = cmd.ExecuteReader();
                        List<Customer> customers = new List<Customer>();

                        while (reader.Read())
                        {
                            Customer customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                PaymentTypes = new List<PaymentType>()
                            };




                            if (include == "paymentTypes")
                            {
                                PaymentType paymentType = new PaymentType
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                                     AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                                    Name = reader.GetString(reader.GetOrdinal("PaymentName")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                                };

                                //If customer is already in the list, adds only the payment type, otherwise adds the payment type and the customer

                                if (customers.Any(x => x.Id == customer.Id))
                                {
                                    Customer currentCustomer = customers.Where(x => x.Id == customer.Id).FirstOrDefault();
                                    if (paymentType.IsActive == true)
                                    {
                                        currentCustomer.PaymentTypes.Add(paymentType);
                                    }
                                    
                                }

                                else
                                {

                                    if (paymentType.IsActive == true)
                                    {
                                    customer.PaymentTypes.Add(paymentType);
                                    customers.Add(customer);
                                    }
                                    
                                    
                                }
                            }

                        //If customer is already in the list, adds only the product, otherwise adds the product and the customer

                        if (include == "products")
                            {
                                Product product = new Product()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                    Price = reader.GetInt32(reader.GetOrdinal("Price")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                                };

                                if (customers.Any(x => x.Id == customer.Id))
                                {
                                    Customer currentCustomer = customers.Where(x => x.Id == customer.Id).FirstOrDefault();

                                    currentCustomer.Products.Add(product);
                            } else
                                {
                                    customer.Products.Add(product);
                                    customers.Add(customer);
                                }
                            }

                        //Adds customer to list if it is missing if an include is NotFiniteNumberException used

                        if(!customers.Any(x => x.Id == customer.Id))
                        {
                            customers.Add(customer);
                        }



                    }
                        reader.Close();
                        return Ok(customers);
                    }
                }

            }



            [HttpGet("{id}", Name = "GetCustomer")]
            public async Task<IActionResult> Get([FromRoute] int id)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT Id, FirstName, LastName FROM Customer WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();

                        Customer customer = null;

                        if (reader.Read())
                        {
                            customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName"))
                            };

                        }
                        reader.Close();
                        return Ok(customer);
                    }
                }
            }


            


            [HttpPost]
            public async Task<IActionResult> Post([FromBody] Customer customer)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO Customer (FirstName, LastName) OUTPUT INSERTED.Id VALUES (@FirstName, @LastName)";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));


                        int newId = (int)cmd.ExecuteScalar();
                        customer.Id = newId;
                        return CreatedAtRoute("GetCustomer", new { id = newId }, customer);
                    }
                }
            }


            [HttpPut("{id}")]
            public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Customer customer)
            {
                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"UPDATE Customer SET FirstName = @FirstName, LastName = @LastName WHERE Id = @id";


                            cmd.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));
                            cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));
                           
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
                    if (!CustomerExists(id))
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
                            cmd.CommandText = @"DELETE FROM Customer WHERE ID = @id";
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
                    if (!CustomerExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }


            private bool CustomerExists(int id)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT Id, FirstName, LastName FROM Customer WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        SqlDataReader reader = cmd.ExecuteReader();
                        return reader.Read();

                    }
                }
            }

        }

    }

   



            

            










        
