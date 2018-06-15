using CommLendingWeb.Helpers;
using CommLendingWeb.Models;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CommLendingWeb.Controllers
{
	//api/[commlending]/document
	[Authorize]
	public class DocumentController : BaseController
    {
		private readonly string ProposalManagerId;
		private readonly string SiteId;
		private readonly string ListId;
		private readonly string SharedDocListId;
		public DocumentController(IConfiguration configuration, IGraphSdkHelper graphSdkHelper) :
			base(configuration, graphSdkHelper)
		{
			// Get from config
			ProposalManagerId = "d6132f2f-f4db-4a2b-9ca4-bff6a1cb557f,35f14f56-d9d3-42a1-af97-d1f04c0a076d";
			SiteId = "agilesightms.sharepoint.com";
			ListId = "opportunities";
			SharedDocListId = "55e997af-a31d-4188-aab5-6747a30508d1";
		}

		[HttpPost]
		public async Task UpdateTask(string opportunityId, string documentData)
		{
			var graphClient = GraphHelper.GetAuthenticatedClient();

			try
			{
				var additionalData = new Dictionary<string, object>() { { "OpportunityObject", documentData } };
				var opportunityItem = new Microsoft.Graph.ListItem()
				{
					Fields = new Microsoft.Graph.FieldValueSet() { AdditionalData = additionalData }
				};

				await graphClient.Sites[$"{SiteId},{ProposalManagerId}"].Lists[ListId].Items[opportunityId].Request().UpdateAsync(opportunityItem);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error updating Opportunity: {ex.Message}");
			}
		}

		[HttpGet]
		public async Task<string> GetFormalProposal(string id)
		{
			var graphClient = GraphHelper.GetAuthenticatedClient();

			try
			{
				var queryItem = graphClient.Sites[$"{SiteId},{ProposalManagerId}"].Lists[ListId].Items.Request().Expand("fields").Filter($"fields/DisplayName eq '{id}'");

				var items = await queryItem.GetAsync();

				var item = items.FirstOrDefault();

				if (item == null)
				{
					throw new Exception($"No document found with id '{id}'.");
				}

				return item.Fields.AdditionalData["OpportunityObject"].ToString();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[HttpGet]
		public async Task<string> GetOOXml(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentNullException(nameof(id));
			}

			var graphClient = GraphHelper.GetAuthenticatedClient();
			var stream = await graphClient.Sites[SiteId].Lists[ListId].Items[id].DriveItem.Content.Request().GetAsync();

			using (var wordDocument = WordprocessingDocument.Open(stream, false))
			{
				return wordDocument.ToFlatOpcString();
			}
		}

		[HttpGet]
		public async Task<IEnumerable<Document>> List()
		{
			// Initialize the GraphServiceClient.
			var graphClient = GraphHelper.GetAuthenticatedClient();
			var items = await graphClient.Sites[$"{SiteId},{ProposalManagerId}"].Lists[SharedDocListId].Items.Request().GetAsync();

			var result = new List<Document>();

			foreach (var item in items)
			{
				result.Add(
					new Document()
					{
						Id = item.Id,
						WebUrl = item.WebUrl,
						CreatedByUser = new User() { Id = item.CreatedBy.User.Id, DisplayName = item.CreatedBy.User.DisplayName },
						LastModifiedByUser = new User() { Id = item.LastModifiedBy.User.Id, DisplayName = item.LastModifiedBy.User.DisplayName },
						LastModifiedDateTime = item.LastModifiedDateTime,
						CreatedDateTime = item.CreatedDateTime,
						Type = item.WebUrl.Substring(item.WebUrl.LastIndexOf('.') + 1),
						Name = item.WebUrl.Substring(item.WebUrl.LastIndexOf('/') + 1)
					});
			}

			return result.OrderBy(x => x.Name);
		}
	}
}
