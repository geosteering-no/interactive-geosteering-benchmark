using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerDataStructures;
using ServerStateInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.IO.Pipelines;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UserState;

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
        private readonly IFullServerStateGeocontroller<WellPoint, UserData, UserEvaluation, LevelDescription<WellPoint, RealizationData, TrueModelState>> _stateServer;

        public GeoController(ILogger<GeoController> logger,
            IFullServerStateGeocontroller<WellPoint, UserData, UserEvaluation, LevelDescription<WellPoint, RealizationData, TrueModelState>> stateServer)
        {
            //Note! this is magic
            _logger = logger;
            _stateServer = stateServer;
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
        [HttpPost]
        public LevelDescription<WellPoint, RealizationData, TrueModelState> GetScores([FromBody]  int index)
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": Scores requested");
            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                var res = _stateServer.GetScoreboard(index);
                _logger.LogInformation("Score preparation finished in {1}ms", (DateTime.Now - time).TotalMilliseconds);
                return res;
            }
            else
            {
                return null;
                //throw new Exception("You are not the admin");
            }
        }

        [Route("admin/load/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        [HttpPost]
        public LevelDescription<WellPoint, RealizationData, TrueModelState> LoadScoresFromFile([FromBody] string fileName)
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": Scores requested");
            foreach (var ch in Path.GetInvalidFileNameChars())
            {
                if (fileName.Contains(ch))
                {
                    return null;
                }
            }
            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                var fName = fileName;
                var res = _stateServer.GetScoreboardFromFile(fName);
                return res;
            }
            else
            {
                return null;
                //throw new Exception("You are not the admin");
            }
        }

        [Route("redirect")]
        public void GetMePlaces()
        {
            try
            {
                var userId = GetUserId();
                if (userId != null)
                {
                    Response.Redirect("/index.html");
                }
                else
                {
                    Response.Redirect("/login.html");
                }
            }catch (Exception e)
            {
                Response.Redirect("/login.html");
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

        [Route("newgame")]
        [HttpPost]
        public int StartNewGameForUser()
        {
            var userId = GetUserId();
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": " + userId + " wants to new game.");
            var res = _stateServer.MoveUserToNewGame(userId);
            _logger.LogInformation("User {1} moved to new game in {2}ms", userId, (DateTime.Now - time).TotalMilliseconds);
            return res;
        }

        [Route("commitstop")]
        [HttpPost]
        public MyScore CommitStop()
        {
            var userId = GetUserId();
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": " + userId + " is stopping.");
            var res = _stateServer.StopUser(userId);
            _logger.LogInformation("User {1} stopped in {2}ms", userId, (DateTime.Now - time).TotalMilliseconds);
            return res;
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

        [Route("userdatadefault")]
        public UserData GetDummyUserData()
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": someone requested some userdata.");
            var res = _stateServer.GetUserDataDefault();
            var lossyRes = _stateServer.LossyCompress(res);
            _logger.LogInformation("Sending some userdata in {1}ms", (DateTime.Now - time).TotalMilliseconds);
            return lossyRes;
        }

        //TODO make sure not to get too many requests
        [Route("screen")]
        public ManyWells<WellPoint> GetUserState([FromQuery] object obj)
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": someone requested screen.");
            ManyWells<WellPoint> res;
            res = _stateServer.GetScreenFull();
            _logger.LogInformation("Sending screen in {1}ms", (DateTime.Now - time).TotalMilliseconds);
            return res;
        }
    }

}
