using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerDataStructures;
using ServerStateInterfaces;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GameServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeoController : ControllerBase
    {
        private const string ADMIN_SECRET_USER_NAME =
            "REmarIdYWorYpiETerdReMnAriDaYEpOsViABLEbACRoNCeNERbAlTIveIDECoMErTiOcHonypoLosenTioClATeRIGENEGMAty";

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

        [Route("restart/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        [HttpPost]
        public void Restart()
        {
            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                _state.RestartServer();
            }
            else
            {
                throw new Exception("You are not the admin");
            }
        }

        [Route("init")]
        [HttpPost]
        public void InitNewUser([FromForm] string userName)
        {
            if (userName == ADMIN_SECRET_USER_NAME)
            {
                WriteUserIdToCookie(userName);
                Response.Redirect("/admin.html");
            }
            else
            {
                WriteUserIdToCookie(userName);
                _state.GetOrAddUserState(userName);
                Response.Redirect("/index.html");
            }
        }

        [Route("checkUser")]
        public bool CheckUser(string userName)
        {
            var userId = GetUserId();
            return userId == userName;
        }

        private void WriteUserIdToCookie(string userId)
        {
            CookieOptions option = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(1),
                IsEssential = true
            };

            Response.Cookies.Append(UserId_ID, userId, option);
        }

        [Route("commitstop")]
        [HttpPost]
        public UserData CommitStop()
        {
            var userId = GetUserId();
            var res = _state.StopUser(userId);
            return res;
        }


        [Route("commitpoint")]
        [HttpPost]
        public UserData Commit([FromBody] WellPoint pt)
        {
            //TODO perform testing
            var userId = GetUserId();
            var res = _state.UpdateUser(userId, pt);
            return res;
        }

        [Route("evaluate")]
        [HttpPost]
        public UserEvaluation GetEvaluationForTrajectory([FromBody] IList<WellPoint> trajectory)
        {
            //TODO perform testing
            var userId = GetUserId();
            var res = _state.GetUserEvaluationData(userId, trajectory);
            return res;
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
