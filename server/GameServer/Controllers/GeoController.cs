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
        private readonly IFullServerStateGeocontroller<WellPoint, UserData, UserEvaluation, PopulationScoreData<WellPoint, RealizationData>> _stateServer;

        public GeoController(ILogger<GeoController> logger,
            IFullServerStateGeocontroller<WellPoint, UserData, UserEvaluation, PopulationScoreData<WellPoint, RealizationData>> stateServer)
        {
            //Note! this is magic
            _logger = logger;
            _stateServer = stateServer;
        }

        [Route("stopall/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        [HttpPost]
        public void StopAll()
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": Stopping all users");
            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                _stateServer.StopAllUsers();
                _logger.LogInformation("Stopping finished in {1}ms", (DateTime.Now - time).TotalMilliseconds);
            }
            else
            {
                //throw new Exception("You are not the admin");
            }
        }

        [Route("restart/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        [HttpPost]
        public void Restart()
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": Starting new game");
            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                _stateServer.RestartServer();
                _logger.LogInformation("Game restart finished in {1}ms", (DateTime.Now - time).TotalMilliseconds);
            }
            else
            {
                //throw new Exception("You are not the admin");
            }
        }

        [Route("resetallscores/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        [HttpPost]
        public void ResetAllScores()
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": Resetting scores");
            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                _stateServer.ResetServer();
                _logger.LogInformation("Resetting scores finished in {1}ms", (DateTime.Now - time).TotalMilliseconds);
            }
            else
            {
                //throw new Exception("You are not the admin");
            }
        }

        [Route("admin/scores/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        public PopulationScoreData<WellPoint, RealizationData> GetScores()
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": Scores requested");
            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                var res = _stateServer.GetScoreboard();
                _logger.LogInformation("Score preparation finished in {1}ms", (DateTime.Now - time).TotalMilliseconds);
                return res;
            }
            else
            {
                return null;
                //throw new Exception("You are not the admin");
            }
        }


        [Route("init")]
        [HttpPost]
        public void InitNewUser([FromForm] string userName)
        {
            if (userName.Length < 2)
            {
                Response.Redirect("/username-taken.html");
                return;
                //throw new Exception("User ID too short");
            }
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": Requested adding of a user : " + userName);
            if (userName == ADMIN_SECRET_USER_NAME)
            {
                WriteUserIdToCookie(userName);
                Response.Redirect("/admin.html");
            }
            else
            {
                if (_stateServer.UserExists(userName))
                {
                    Response.Redirect("/username-taken.html");
                }
                else
                {
                    WriteUserIdToCookie(userName);
                    _stateServer.GetUserData(userName);
                    Response.Redirect("/index.html");
                }
            }
        }

        [Route("checkUser")]
        public bool CheckUser(string userName)
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": Requested checking a user : " + userName);
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
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": " + userId + " is stopping.");
            var res = _stateServer.StopUser(userId);
            var lossyRes = _stateServer.LossyCompress(res);
            _logger.LogInformation("User {1} stopped in {2}ms", userId, (DateTime.Now - time).TotalMilliseconds);
            return lossyRes;
        }


        [Route("commitpoint")]
        [HttpPost]
        public UserData Commit([FromBody] WellPoint pt)
        {
            var userId = GetUserId();
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": " + userId + " is submitting " + pt.X + ", " + pt.Y);
            var res = _stateServer.UpdateUser(userId, pt);
            var lossyRes = _stateServer.LossyCompress(res);
            _logger.LogInformation("User {1} updated in {2}ms", userId, (DateTime.Now - time).TotalMilliseconds);
            return lossyRes;
        }

        [Route("evaluate")]
        [HttpPost]
        public UserEvaluation GetEvaluationForTrajectory([FromBody] IList<WellPoint> trajectory)
        {
            var userId = GetUserId();
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": " + userId + " requested evaluation.");
            var res = _stateServer.GetUserEvaluationData(userId, trajectory);
            _logger.LogInformation("User {1}, sending evaluation in {2}ms", userId, (DateTime.Now - time).TotalMilliseconds);
            return res;
        }

        [Route("addbot/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        [HttpPost]
        public void AddBot()
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": " + " adding a bot.");
            _stateServer.AddBotUserDefault();
            _logger.LogInformation("Bot started in {1}ms", (DateTime.Now - time).TotalMilliseconds);
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
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": " + userId + " requested userdata.");
            var res = _stateServer.GetUserData(userId);
            var lossyRes = _stateServer.LossyCompress(res);
            _logger.LogInformation("User {1}, sending userdata in {2}ms", userId, (DateTime.Now - time).TotalMilliseconds);
            return lossyRes;
        }
    }

}
