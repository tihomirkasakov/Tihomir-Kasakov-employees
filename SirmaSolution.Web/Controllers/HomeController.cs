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
            List<EmployeeInfo> employeesInfo = new List<EmployeeInfo>();
            Dictionary<int, (int EmployeeId, int DaysWorked)> result = new Dictionary<int, (int EmployeeId, int DaysWorked)>();

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    row++;
                    string line = reader.ReadLine();
                    string toDateValue = string.Empty;

                    if (!line.ToLower().Contains("empid"))
                    {
                        string[] data = line.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                        int employeeId = 0;
                        int.TryParse(data[0], out employeeId);
                        int projectId = 0;
                        int.TryParse(data[1], out projectId);

                        toDateValue = ValidateToDate(data);

                        if (employeeId == 0 || projectId == 0 ||  string.IsNullOrWhiteSpace(toDateValue))
                        {
                            continue;
                        }

                        DateTime fromDate;
                        DateTime.TryParse(data[2], CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate);
                        DateTime toDate;
                        DateTime.TryParse(toDateValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate);

                        if (fromDate == default(DateTime) || toDate == default(DateTime))
                        {
                            continue;
                        }

                        EmployeeInfo employeeInfo = new EmployeeInfo()
                        {
                            EmployeeId = employeeId,
                            ProjectId = projectId,
                            FromDate = fromDate,
                            ToDate = toDate
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

                if (employeesWorkedOnSameProject.Count < 2)
                {
                    continue;
                }

                for (int i = 0; i < employeesWorkedOnSameProject.Count - 1; i++)
                {
                    int firstEmployeeId = employeesWorkedOnSameProject[i].EmployeeId;
                    DateTime firstPersonFromDate = employeesWorkedOnSameProject[i].FromDate;
                    DateTime firstPersonToDate = employeesWorkedOnSameProject[i].ToDate;

                    for (int j = i + 1; j < employeesWorkedOnSameProject.Count; j++)
                    {
                        int secondEmployeeId = employeesWorkedOnSameProject[j].EmployeeId;
                        if (firstEmployeeId == secondEmployeeId)
                        {
                            continue;
                        }

                        DateTime secondPersonFromDate = employeesWorkedOnSameProject[j].FromDate;
                        DateTime secondPersonToDate = employeesWorkedOnSameProject[j].ToDate;

                        double days = OverlappingDays(firstPersonFromDate, firstPersonToDate, secondPersonFromDate, secondPersonToDate);

                        if (days > maxDays)
                        {
                            maxDays = days;
                            dto.DaysOnSameProject = (int)maxDays;
                            dto.FirstEmployeeId = firstEmployeeId;
                            dto.SecondEmployeeId = secondEmployeeId;
                        }
                    }
                }
                model.EmployeesInfo.Add(dto);
            }

            return View("Result", model);
        }

        private static string ValidateToDate(string[] data)
        {
            if (data.Length < 4)
            {
                return "";
            }
            else if (data.Length > 4)
            {
                return string.Join(", ", data.Skip(3));
            }
            else
            {
                if (data[3].ToLower() == "null")
                {
                    return DateTime.Today.ToString();
                }
                else
                {
                    if (data[3].Contains('/'))
                    {
                        string[] dateDetails = data[3].Split('/');
                        if (dateDetails[0].Length==2)
                        {
                            return DateTime.ParseExact(data[3], "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString();
                        }
                        else
                        {
                            return data[3];
                        }
                    }
                    else
                    {
                        return data[3];
                    }
                }
            }

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
