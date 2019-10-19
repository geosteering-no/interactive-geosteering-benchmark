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
    //TODO consider changing to Geocontroller<TWellPoint>
    {
        private const string ADMIN_SECRET_USER_NAME =
            "REmarIdYWorYpiETerdReMnAriDaYEpOsViABLEbACRoNCeNERbAlTIveIDECoMErTiOcHonypoLosenTioClATeRIGENEGMAty";

        private const string UserId_ID = "geobanana-user-id";
        private readonly ILogger<GeoController> _logger;
        private readonly IFullServerStateGeocontroller<WellPoint, UserData, UserEvaluation, PopulationScoreData<WellPoint>> _stateServer;

        public GeoController(ILogger<GeoController> logger,
            IFullServerStateGeocontroller<WellPoint, UserData, UserEvaluation, PopulationScoreData<WellPoint>> stateServer)
        {
            //Note! this is magic
            _logger = logger;
            _stateServer = stateServer;
        }

        [Route("restart/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        [HttpPost]
        public void Restart()
        {
            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                _stateServer.RestartServer();
            }
            else
            {
                throw new Exception("You are not the admin");
            }
        }

        [Route("admin/scores/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        public PopulationScoreData<WellPoint> GetScores()
        {
            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                return _stateServer.GetScoreboard();
                //TODO finsh implementation
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
                _stateServer.GetOrAddUserState(userName);
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
            var res = _stateServer.StopUser(userId);
            return res;
        }


        [Route("commitpoint")]
        [HttpPost]
        public UserData Commit([FromBody] WellPoint pt)
        {
            //TODO perform testing
            var userId = GetUserId();
            var res = _stateServer.UpdateUser(userId, pt);
            return res;
        }

        [Route("evaluate")]
        [HttpPost]
        public UserEvaluation GetEvaluationForTrajectory([FromBody] IList<WellPoint> trajectory)
        {
            //TODO perform testing
            var userId = GetUserId();
            var res = _stateServer.GetUserEvaluationData(userId, trajectory);
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
            return _stateServer.GetOrAddUserState(userId);
        }
    }

}
