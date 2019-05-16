using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class EmployeeTraining
    {

        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int TrainingProgram { get; set; }
        public List<Employee> employees { get; set; } = new List<Employee>();


    }
}
