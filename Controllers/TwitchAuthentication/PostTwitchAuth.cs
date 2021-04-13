using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using RestSharp;
using voddy.Data;
using voddy.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace voddy.Controllers {
    [ApiController]
    [Route("auth/twitchAuth")]
    public class PostTwitchAuthController : ControllerBase {
        private readonly ILogger<PostTwitchAuthController> _logger;
        

        public PostTwitchAuthController(ILogger<PostTwitchAuthController> logger) {
            _logger = logger;
        }

        [HttpPost]
        public string Post(TwitchAuthBuilder.UserAuth userAuth) {
            TwitchAuthBuilder twitchAuthBuilder = new TwitchAuthBuilder();
            return twitchAuthBuilder.BuildTwitchAuth(userAuth, HttpContext);
        }
    }
}