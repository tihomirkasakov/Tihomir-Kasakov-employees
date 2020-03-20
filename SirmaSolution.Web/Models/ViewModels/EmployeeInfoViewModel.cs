namespace SirmaSolution.Web.Models.ViewModels
{
    using System.Collections.Generic;

    public class EmployeeInfoViewModel
    {
        public EmployeeInfoViewModel()
        {
            EmployeesInfo = new List<EmployeeInfoDto>();
        }

        public List<EmployeeInfoDto> EmployeesInfo { get; set; }
    }
}
