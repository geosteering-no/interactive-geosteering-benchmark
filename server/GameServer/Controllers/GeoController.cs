using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerDataStructures;
using ServerStateInterfaces;
using System;

namespace GameServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeoController : ControllerBase
    {
        private const string UserId_ID = "geobanana-user-id";
        private readonly ILogger<GeoController> _logger;
        private readonly IFullServerState<WellPoint, UserData, UserEvaluation, PopulationScoreData> _state;

        public GeoController(ILogger<GeoController> logger, 
            IFullServerState<WellPoint, UserData, UserEvaluation, PopulationScoreData> state)
        {
            //Note! this is magic
            _logger = logger;
            _state = state;
        }

        //TODO add a secret token here or as an argument
        [Route("admin/restart/secret_token")]
        public void Restart(int seed = 0)
        {
            _state.RestartServer(seed);
        }

        [Route("init")]
        [HttpPost]
        public void InitNewUser([FromForm] string userName)
        {
            WriteUserId(userName);
            _state.GetOrAddUserState(userName);
            Response.Redirect("/index.html");
        }

        [Route("checkUser")]
        public bool CheckUser(string userName)
        {
            var userId = GetUserId();
            return userId == userName;
        }

        private void WriteUserId(string userId)
        {
            CookieOptions option = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(1),
                IsEssential = true
            };
            
            Response.Cookies.Append(UserId_ID, userId, option);
        }

        [Route("commit")]
        public void Commit(WellPoint pt)
        {
            var sessionId = GetUserId();
            var res = _state.UpdateUser(sessionId, pt);
            if (!res)
            {
                throw new Exception("We cannot update using this point on this user");
            }
        }

        private string GetUserId()
        {
            //var userId = HttpContext.Session.GetString("userId");
            var userId = HttpContext.Request.Cookies[UserId_ID];
            return userId;
        }

        //TODO remove
        [Route("userdata")]
        public UserData GetUserState()
        {
            var userId = GetUserId();
            return _state.GetOrAddUserState(userId);
        }
    }

}
