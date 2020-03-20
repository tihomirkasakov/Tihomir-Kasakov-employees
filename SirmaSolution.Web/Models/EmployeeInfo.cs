namespace SirmaSolution.Web.Models
{
    using System;

    public class EmployeeInfo
    {
        public int EmployeeId { get; set; }

        public int ProjectId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
