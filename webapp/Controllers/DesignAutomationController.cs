/////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved
// Written by Forge Partner Development
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////

using Autodesk.Forge;
using Autodesk.Forge.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace forgeSample.Controllers
{
    public class DesignAutomationController : ControllerBase
    {
        // Used to access the application folder (temp location for files & bundles)
        private IWebHostEnvironment _env;
        // used to access the SignalR Hub
        private IHubContext<DesignAutomationHub> _hubContext;
        public DesignAutomationController(IWebHostEnvironment env, IHubContext<DesignAutomationHub> hubContext)
        {
            _env = env;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("api/styles")]
        public async Task<IActionResult> GetProperties([FromBody]dynamic body)
        {
            Credentials credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
            if (credentials == null) { return null; }

            string itemId = body["itemId"];
            string versionId = body["versionId"];
            string projectId = itemId.Split("/").Reverse().ElementAt(2);
            string connectionId = body["connectionId"];

            CompoundStructLayer da4revit = new CompoundStructLayer();
            await da4revit.StartExtract(credentials, projectId, versionId, connectionId, _env.WebRootPath);

            return Ok();
        }

        [HttpPost]
        [Route("api/forge/callback/designautomation/compoundStructLayer/{connectionId}/{fileName}")]
        public IActionResult OnReadyExtractStyles(string connectionId, string fileName, [FromBody]dynamic body)
        {
            // catch any errors, we don't want to return 500
            try
            {
                // your webhook should return immediately!
                // so can start a second thread (not good) or use a queueing system (e.g. hangfire)

                new System.Threading.Tasks.Task(async () =>
                  {
                      JObject bodyJson = JObject.Parse((string)body.ToString());
                      await _hubContext.Clients.Client(connectionId).SendAsync("onComplete", bodyJson.ToString());

                      ObjectsApi objects = new ObjectsApi();
                      objects.Configuration.AccessToken = (await Credentials.Get2LeggedTokenAsync(new Scope[]{Scope.DataWrite, Scope.DataRead})).access_token;
                      dynamic signedUrl = await objects.CreateSignedResourceAsyncWithHttpInfo(Utils.BucketName, fileName, new PostBucketsSigned(10), "read");
                      await _hubContext.Clients.Client(connectionId).SendAsync("propsReady", (string)(signedUrl.Data.signedUrl));

                  }).Start();
            }
            catch { }

            // ALWAYS return ok (200)
            return Ok();
        }
    }

    /// <summary>
    /// Class uses for SignalR
    /// </summary>
    public class DesignAutomationHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public string GetConnectionId() { return Context.ConnectionId; }
    }
}