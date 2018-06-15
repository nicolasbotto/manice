/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommLendingWeb.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.AspNetCore.Hosting;

namespace CommLendingWeb.Controllers
{
	public class AuthController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult End()
		{
			return View();
		}

	}
}