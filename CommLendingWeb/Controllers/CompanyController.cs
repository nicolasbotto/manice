using CommLendingWeb.Helpers;
using CommLendingWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CommLendingWeb.Controllers
{
	[Authorize]
	public class CompanyController : BaseController
	{
		public CompanyController(IConfiguration configuration, IGraphSdkHelper graphSdkHelper) :
			base(configuration, graphSdkHelper)
		{
		}

		[HttpGet]
		public IEnumerable<Company> List()
		{
			var result = new List<Company>();

			for (int i = 0; i < 100; i++)
			{
				result.Add(new Company() { Id = i, Name = $"MS Company {i}" });
			}

			return result;
		}

		[HttpGet]
		public string GetProposal()
		{
			return "";
		}
	}
}