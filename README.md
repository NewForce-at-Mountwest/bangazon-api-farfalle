# Bangazon Platform API

## To test for each feature: 



### Products:
endpoint: https://localhost:5001/api/product
supported HTTP Methods: (GET All, GET Single (by ID), POST, PUT, DELETE)
Additional queries: none


### Product types:
endpoint: https://localhost:5001/api/producttype
supported HTTP Methods: (GET All, GET Single (by ID), POST, PUT, DELETE)
Additional queries: none

### Customers:




### Orders:


### Payment types:
endpoint: https://localhost:5001/api/paymenttype
supported HTTP Methods: (GET All, GET Single (by ID), POST, PUT, DELETE (archives the payment type))
Additional queries: ~paymenttype/?HardDelete=true will remove a payment type from the database instead of archiving it



### Employees:
endpoint: https://localhost:5001/api/employee
supported HTTP Methods: (GET All, GET Single (by ID), POST, and PUT)
Additional queries: ~employee/?PeteyDeletey=True  will actually delete an employee instead of archiving.


### Computers:
endpoint: https://localhost:5001/api/computer
supported HTTP Methods: (GET All, GET Single (by ID), POST, PUT, DELETE)
Additional queries: ~computer/?HardDelete=true will remove a computer from the database instead of archiving it


### Training programs:


### Employee Training
endpoint: https://localhost:5001/api/employeetraining
supported HTTP Methods: (POST, DELETE)
Additional queries: none

### Departments:
endpoint: https://localhost:5001/api/department
supported HTTP Methods: (GET All, GET Single (by ID), POST, PUT)
Additional queries:  ~department/?_include=employees, will show all the employees in a department
        ~department/?_filter=budget&_gt={number} will show all the departments with a budget greater than {number}
        ~department/q=delete_test_item will delete the department from the database (for test purposes only)



#### Client side app can be found here: https://github.com/NewForce-at-Mountwest/bangazon-farfalle-react-client
