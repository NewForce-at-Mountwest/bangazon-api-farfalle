using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace TestBangazonAPI
{

    public class TestEmployees
    {


        public async Task<Employee> createEmployee(HttpClient client)
        {
            //make a new Employee
            Employee employee = new Employee
            {
                FirstName = "Bob",
                LastName = "Bobberson",
                IsSuperVisor = true,
                DepartmentId = 1, 
            
            };
            //turn the Employee into json
            string employeeAsJSON = JsonConvert.SerializeObject(employee);

            //post bob
            HttpResponseMessage response = await client.PostAsync(
                "api/Employee",
                new StringContent(employeeAsJSON, Encoding.UTF8, "application/json")
            );
            //make sure we were successful
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            //convert back into c#
            Employee newEmployee = JsonConvert.DeserializeObject<Employee>(responseBody);

            //make sure our status code is gud
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newEmployee;

        }

        // Delete bob in the database and make sure we get a no content status code back
        public async Task deleteEmployee(Employee employee, HttpClient client)
        {
            //this is our super secret delete method so that our tests don't flood the database with an army of bobs
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/Employee/{employee.Id}?PeteyDeletey=true");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Employees()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // get all our Employees and wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/Employee");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of Employee instances
                List<Employee> EmployeeList = JsonConvert.DeserializeObject<List<Employee>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any Employees in the list?
                Assert.True(EmployeeList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Employee()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Employee
                Employee newEmployee = await createEmployee(client);

                // Then we're going to try to get that Employee from the database
                HttpResponseMessage response = await client.GetAsync($"api/Employee/{newEmployee.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON back into C#
                Employee Employee = JsonConvert.DeserializeObject<Employee>(responseBody);

                // did we get back the right stuff? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Bob", newEmployee.FirstName);

                // delete Bob
                deleteEmployee(newEmployee, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistent_Employee_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a Employee that doesn't exist
                HttpResponseMessage response = await client.GetAsync("api/Employee/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {

                // hire bob
                Employee newEmployee = await createEmployee(client);

                // Make sure sure our Bob is not an imposter (no bad response)
                Assert.Equal("Bob", newEmployee.FirstName);


                // fire bob
                deleteEmployee(newEmployee, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Employee_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Attempt to delete a nonexistant Employee
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/Employee/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Employee()
        {

            // Change bob's name to robert because he's all grown up now, go get em champ
            string newName = "Robert";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new instance of bob
                Employee newEmployee = await createEmployee(client);

                // Set a new title
                newEmployee.FirstName = newName;

                // Convert it to JSON
                string modifiedEmployeeAsJSON = JsonConvert.SerializeObject(newEmployee);

                // PUT the new employee
                HttpResponseMessage response = await client.PutAsync(
                    $"api/Employee/{newEmployee.Id}",
                    new StringContent(modifiedEmployeeAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Check that there's a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                HttpResponseMessage getEmployee = await client.GetAsync($"api/Employee/{newEmployee.Id}");
                getEmployee.EnsureSuccessStatusCode();

                string getEmployeeBody = await getEmployee.Content.ReadAsStringAsync();
                Employee modifiedEmployee = JsonConvert.DeserializeObject<Employee>(getEmployeeBody);

                Assert.Equal(HttpStatusCode.OK, getEmployee.StatusCode);


                Assert.Equal(newName, modifiedEmployee.FirstName);

                // DELETE BOB
                deleteEmployee(modifiedEmployee, client);
            }
        }
    }
}

