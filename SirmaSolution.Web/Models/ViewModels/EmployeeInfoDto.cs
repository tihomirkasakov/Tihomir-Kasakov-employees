﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SirmaSolution.Web.Models.ViewModels
{
    public class EmployeeInfoDto
    {
        public int ProjectId { get; set; }

        public int FirstEmployeeId { get; set; }

        public int SecondEmployeeId { get; set; }

        public int DaysOnSameProject { get; set; }
    }
}
