using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerDataStructures;
using ServerStateInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        private string FriendGameId_ID = "referrer-game-id";
        private readonly ILogger<GeoController> _logger;

        private readonly
            IFullServerStateGeocontroller<WellPoint, UserData, UserEvaluation,
                LevelDescription<WellPoint, RealizationData, TrueModelState>> _stateServer;

        public GeoController(ILogger<GeoController> logger,
            IFullServerStateGeocontroller<WellPoint, UserData, UserEvaluation,
                LevelDescription<WellPoint, RealizationData, TrueModelState>> stateServer)
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
        public LevelDescription<WellPoint, RealizationData, TrueModelState> GetScores([FromBody] int index)
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
        public LevelDescription<WellPoint, RealizationData, TrueModelState> LoadScoresFromFile(
            [FromBody] string fileName)
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

        [Route("admin/nextreplaywname/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        [HttpPost]
        public UserData LoadNextUserDataFromFile(ObjectWithTextString userToLoad)
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": Replay requested");

            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                var res = _stateServer.GetNextUserStateFromFile(userToLoad: userToLoad.text);
                return res;
            }
            else
            {
                return null;
                //throw new Exception("You are not the admin");
            }
        }

        [Route("admin/nextreplay/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        [HttpPost]
        public UserData LoadNextUserDataFromFile(bool nextUser)
        {
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": Replay requested");

            var userId = GetUserId();
            if (userId == ADMIN_SECRET_USER_NAME)
            {
                var res = _stateServer.GetNextUserStateFromFile(nextUser);
                return res;
            }
            else
            {
                return null;
                //throw new Exception("You are not the admin");
            }
        }

        private void DumpPrintStatistics(string fileName, string sharer)
        {
            var dirShareStatistics = "sharingStatistics";
            if (!Directory.Exists(dirShareStatistics))
            {
                Directory.CreateDirectory(dirShareStatistics);
            }

            foreach (var ch in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(ch, '-');
            }

            if (fileName == "")
            {
                return;
            }

            if (fileName.Length > 20)
            {
                fileName = fileName.Substring(0, 20);
            }

            using (StreamWriter file =
                new StreamWriter(dirShareStatistics + "/" + fileName + ".txt", true))
            {
                file.WriteLine(sharer + " / " + DateTime.Now);
            }

        }

        private string ComposeHtml(string userId=null, string fgi=null, string platform=null)
        {
            var dynamicText = System.IO.File.ReadAllText("wwwroot/responces/dynamic.html2");
            var challenger = fgi;

            var challengeText = "Create a game and then challenge your friends to beat your score!";
            var instructionsText = "Here's how to score:";
            if (challenger != null)
            {
                challengeText = challenger + " has challenged you! See if you can beat their score?";
                //" of " + score.toString() + "!"
                instructionsText = "Here's how to beat their score:";
            }

            dynamicText = dynamicText.Replace("{{CHALLENGE_TEXT_HERE}}", challengeText);
            dynamicText = dynamicText.Replace("{{INSTRUCTIONS_CAPTION_HERE}}", instructionsText);

            if (userId != null)
            {
                var loginText = System.IO.File.ReadAllText("wwwroot/responces/login_text.html2");
                dynamicText = dynamicText.Replace("{{LOGIN_TEXT_HERE}}", loginText);
            }
            else
            {
                var loggedInText = System.IO.File.ReadAllText("wwwroot/responces/continue_text.html2");
                //TODO show the name
                dynamicText = dynamicText.Replace("{{LOGIN_TEXT_HERE}}", loggedInText);
            }

            return dynamicText;
            
        }

        [Route("redirect")]
        [HttpGet]
        public ContentResult GetMePlaces([FromQuery] string fgi=null, [FromQuery] string platform="other")
        {
            try
            {
                
                if (fgi != null)
                {
                    SetFriendGameId(fgi);
                    DumpPrintStatistics(platform, fgi);
                }
                var userId = GetUserId();
                var dynamicString = ComposeHtml(userId, fgi, platform);
                var myResult = new ContentResult()
                {
                    ContentType = "text/html",
                    Content = dynamicString
                };
                return myResult;
                //if (userId != null)
                //{
                //    Response.Redirect("/index.html");
                //}
                //else
                //{
                //    Response.Redirect("/login.html");
                //}
            }catch (Exception e)
            {
                Response.Redirect("/login.html");
                return new ContentResult();
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
                SetUserId(userName);
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
                    SetUserId(userName);
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
            var friendGameId = GetFriendGameId();
            var time = DateTime.Now;
            _logger.LogInformation(time.ToLongTimeString() + ": " + userId + " is stopping.");
            MyScore res = null;
            if (friendGameId != null)
            {
                res = _stateServer.StopUser(userId, friendGameId);
            }
            else
            {
                res = _stateServer.StopUser(userId);
            }

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

        //[Route("addbot/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA")]
        //[HttpPost]
        //public void AddBot()
        //{
        //    var time = DateTime.Now;
        //    _logger.LogInformation(time.ToLongTimeString() + ": " + " adding a bot.");
        //    _stateServer.AddBotUserDefault();
        //    _logger.LogInformation("Bot started in {1}ms", (DateTime.Now - time).TotalMilliseconds);
        //}

        private void SetUserId(string userId)
        {
            CookieOptions option = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(30),
                IsEssential = true
            };
            Response.Cookies.Append(UserId_ID, userId, option);
        }

        private string GetUserId()
        {
            //var userId = HttpContext.Session.GetString("userId");
            var userId = HttpContext.Request.Cookies[UserId_ID];
            return userId;
        }

        private void SetFriendGameId(string friendGameId)
        {
            CookieOptions option = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(30),
                IsEssential = true
            };
            Response.Cookies.Append(FriendGameId_ID, friendGameId, option);
        }

        private string GetFriendGameId()
        {
            //var userId = HttpContext.Session.GetString("userId");
            var friendGameId = HttpContext.Request.Cookies[FriendGameId_ID];
            return friendGameId;
        }

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
