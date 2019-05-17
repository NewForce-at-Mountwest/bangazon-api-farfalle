using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using TestBangazonAPI;

namespace TestStudentExerciseUsingAPI
{

    public class TestProductTypes
    {


        public async Task<ProductType> createTable(HttpClient client)
        {
            ProductType Table = new ProductType
            {
                Name = "Table",
                IsActive = true

            };
            string TableAsJSON = JsonConvert.SerializeObject(Table);


            HttpResponseMessage response = await client.PostAsync(
                "api/ProductType",
                new StringContent(TableAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            ProductType newTable = JsonConvert.DeserializeObject<ProductType>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newTable;

        }

        // Delete a ProductType in the database and make sure we get a no content status code back
        public async Task deleteTable(ProductType Table, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/ProductType/{Table.Id}?HardDelete=true");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_ProductTypes()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our ProductTypes; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/ProductType");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of ProductType instances
                List<ProductType> ProductTypeList = JsonConvert.DeserializeObject<List<ProductType>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any ProductTypes in the list?
                Assert.True(ProductTypeList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_ProductType()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new ProductType
                ProductType newTable = await createTable(client);

                // Try to get that ProductType from the database
                HttpResponseMessage response = await client.GetAsync($"api/ProductType/{newTable.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                ProductType ProductType = JsonConvert.DeserializeObject<ProductType>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Table", newTable.Name);

                // Clean up after ourselves- delete Table!
                deleteTable(newTable, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_ProductType_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a ProductType with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/ProductType/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_ProductType()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new Table
                ProductType newTable = await createTable(client);

                // Make sure his info checks out
                Assert.Equal("Table", newTable.Name);


                // Clean up after ourselves - delete Table!
                deleteTable(newTable, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_ProductType_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/ProductType/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_ProductType()
        {

            // We're going to change a ProductType's name! This is their new name.
            string newProductTypeName = "End table";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new ProductType
                ProductType newTable = await createTable(client);

                // Change their first name
                newTable.Name = newProductTypeName;

                // Convert them to JSON
                string modifiedTableAsJSON = JsonConvert.SerializeObject(newTable);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/ProductType/{newTable.Id}",
                    new StringContent(modifiedTableAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the ProductType we just edited
                HttpResponseMessage getTable = await client.GetAsync($"api/ProductType/{newTable.Id}");
                getTable.EnsureSuccessStatusCode();

                string getTableBody = await getTable.Content.ReadAsStringAsync();
                ProductType modifiedTable = JsonConvert.DeserializeObject<ProductType>(getTableBody);

                Assert.Equal(HttpStatusCode.OK, getTable.StatusCode);

                // Make sure his name was in fact updated
                Assert.Equal(newProductTypeName, modifiedTable.Name);

                // Clean up after ourselves- delete him
                deleteTable(modifiedTable, client);
            }
        }
    }
}
