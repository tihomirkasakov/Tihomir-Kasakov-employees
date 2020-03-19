using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SirmaSolution.Web.Models
{
    public class EmployeeInfo
    {
        public int EmployeeId { get; set; }

        public int ProjectId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
