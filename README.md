# Bangazon Platform API

## To test for each feature:



### Products:
Endpoint: https://localhost:5001/api/product

supported HTTP Methods: (GET All, GET Single (by ID), POST, PUT, DELETE)
Additional queries: none


### Product types:
Endpoint: https://localhost:5001/api/producttype
supported HTTP Methods: (GET All, GET Single (by ID), POST, PUT, DELETE)
Additional queries: none

### Customers:
Endpoint - https://localhost:5001/api/customer
GET, POST, PUT, and DELETE methods supported.
Users may use the following query strings: /?include=products to display all the customer's products, /?include=paymentTypes to display all the customer's payment types, /?active=false to see a list of inactive customers, and /?q={your text here} to search for all customers with first or last names containing the sequence of letters specified in the query,.



### Orders:
Orders Controller includes full CRUD and testing for full CRUD.

To Test:
<ul>
<li>Press Ctrl+f5</li>
<li>Go to https://localhost:5001/api/Orders in the browser. You should see all Orders.</li>
<li>Go to https://localhost:5001/api/Orders/1 in the browser. You should see a single order.</li>
<li>Go to https://localhost:5001/api/Orders?include=customers in the browser. You should see all orders with the customers expanded.</li>
<li>Go to https://localhost:5001/api/Orders?include=products in the browser. You should see all orders with products expanded.</li>
<li>Open Postman. Post to https://localhost:5001/api/Orders with the header:
{
"customerId": 3,
"paymentTypeId": 3
}</li>
<li>You should see a new order at https://localhost:5001/api/Orders</li>
<li>Put to https://localhost:5001/api/Orders/1 with the header:
{
"customerId": 1,
"paymentTypeId": null
}</li>
<li>You should see that order 1 at https://localhost:5001/api/Orders/1 has changed.</li>
<li>Delete to https://localhost:5001/api/Orders/2. Order number 2 should be deleted.</li>
<li>Go to https://localhost:5001/api/Orders?completed=false. You should only see order 1.</li>
<li>Go to https://localhost:5001/api/Orders?completed=true. You shouldn't see order 1.</li>
</ul>
### Payment types:
Endpoint: https://localhost:5001/api/paymenttype
supported HTTP Methods: (GET All, GET Single (by ID), POST, PUT, DELETE (archives the payment type))
Additional queries: ~paymenttype/?HardDelete=true will remove a payment type from the database instead of archiving it



### Employees:
Endpoint: https://localhost:5001/api/employee
supported HTTP Methods: (GET All, GET Single (by ID), POST, and PUT)
Additional queries: ~employee/?PeteyDeletey=True  will actually delete an employee instead of archiving.


### Computers:
Endpoint: https://localhost:5001/api/computer
supported HTTP Methods: (GET All, GET Single (by ID), POST, PUT, DELETE)
Additional queries: ~computer/?HardDelete=true will remove a computer from the database instead of archiving it


### Training programs:
Endpoint - https://localhost:5001/api/trainingprogram
GET, POST, PUT, and DELETE methods supported. A "completed=false" query string may also be used to only see training programs that occur in the future or are currently ongoing.

### Employee Training
Endpoint: https://localhost:5001/api/employeetraining
supported HTTP Methods: (POST, DELETE)
Additional queries: none

### Departments:
Endpoint: https://localhost:5001/api/department
supported HTTP Methods: (GET All, GET Single (by ID), POST, PUT)
Additional queries:  ~department/?_include=employees, will show all the employees in a department
        ~department/?_filter=budget&_gt={number} will show all the departments with a budget greater than {number}
        ~department/q=delete_test_item will delete the department from the database (for test purposes only)



#### Client side app can be found here: https://github.com/NewForce-at-Mountwest/bangazon-farfalle-react-client
