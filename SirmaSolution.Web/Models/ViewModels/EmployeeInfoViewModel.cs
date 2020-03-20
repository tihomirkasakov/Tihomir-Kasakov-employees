using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SirmaSolution.Web.Models.ViewModels
{
    public class EmployeeInfoViewModel
    {
        public EmployeeInfoViewModel()
        {
            EmployeesInfo = new List<EmployeeInfoDto>();
        }

        public List<EmployeeInfoDto> EmployeesInfo { get; set; }
    }
}
