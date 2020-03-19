using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SirmaSolution.Web.Models;
using SirmaSolution.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SirmaSolution.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            int row = 0;
            Stream fileStream = file.OpenReadStream();
            List<int> errorOnRow = new List<int>();
            List<EmployeeInfo> employeesInfo = new List<EmployeeInfo>();
            Dictionary<int, (int EmployeeId, int DaysWorked)> result = new Dictionary<int, (int EmployeeId, int DaysWorked)>();

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    row++;
                    string line = reader.ReadLine();
                    if (!line.ToLower().Contains("empid"))
                    {
                        string[] data = line.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                        if (data.Length != 4)
                        {
                            errorOnRow.Add(row);
                            continue;
                        }

                        EmployeeInfo employeeInfo = new EmployeeInfo()
                        {
                            EmployeeId = String.IsNullOrWhiteSpace(data[0]) ? default(int) : int.Parse(data[0]),
                            ProjectId = String.IsNullOrWhiteSpace(data[1]) ? default(int) : int.Parse(data[1]),
                            FromDate = String.IsNullOrWhiteSpace(data[2]) ? DateTime.Today : DateTime.Parse(data[2], CultureInfo.InvariantCulture),
                            ToDate = String.IsNullOrWhiteSpace(data[3]) || data[3].ToLower() == "null" ? DateTime.Today : DateTime.Parse(data[3], CultureInfo.InvariantCulture)
                        };

                        employeesInfo.Add(employeeInfo);
                    }
                }
            }

            List<int> projects = employeesInfo.Select(x => x.ProjectId).Distinct().ToList();

            EmployeeInfoViewModel model = new EmployeeInfoViewModel();

            foreach (int project in projects)
            {
                EmployeeInfoDto dto = new EmployeeInfoDto()
                {
                    ProjectId = project
                };
                double maxDays = 0;
                List<EmployeeInfo> employeesWorkedOnSameProject = employeesInfo.Where(x => x.ProjectId == project).ToList();
                for (int i = 0; i < employeesWorkedOnSameProject.Count - 1; i++)
                {
                    DateTime firstPersonFromDate = employeesWorkedOnSameProject[i].FromDate;
                    DateTime firstPersonToDate = employeesWorkedOnSameProject[i].ToDate;
                    for (int j = i + 1; j < employeesWorkedOnSameProject.Count; j++)
                    {
                        DateTime secondPersonFromDate = employeesWorkedOnSameProject[j].FromDate;
                        DateTime secondPersonToDate = employeesWorkedOnSameProject[j].ToDate;

                        double days = OverlappingDays(firstPersonFromDate, firstPersonToDate, secondPersonFromDate, secondPersonToDate);

                        if (days>maxDays)
                        {
                            maxDays = days;
                            dto.DaysOnSameProject = (int)maxDays;
                            dto.FirstEmployeeId = employeesWorkedOnSameProject[i].EmployeeId;
                            dto.SecondEmployeeId = employeesWorkedOnSameProject[j].EmployeeId;
                        }
                    }
                }
                model.EmployeesInfo.Add(dto);
            }

            return View(model);
        }

        private static double OverlappingDays(DateTime firstStart, DateTime firstEnd, DateTime secondStart, DateTime secondEnd)
        {
            DateTime maxStart = firstStart > secondStart ? firstStart : secondStart;
            DateTime minEnd = firstEnd < secondEnd ? firstEnd : secondEnd;
            TimeSpan interval = minEnd - maxStart;
            double result = interval.TotalDays;
            return result;
        }

    }
}
