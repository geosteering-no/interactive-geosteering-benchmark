using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ServerStateInterfaces;
using Newtonsoft.Json;
using ServerDataStructures;

namespace GameServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeoController : ControllerBase
    {
        private readonly ILogger<GeoController> _logger;
        private readonly IFullServerState<WellPoint, UserData> _state;

        public GeoController(ILogger<GeoController> logger, 
            IFullServerState<WellPoint, UserData> state)
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
        public UserData InitUser(string userName)
        {
            var sessionId = userName;
            var result = _state.AddUser(sessionId);
            if (!result)
            {
                throw new Exception("Username already taken");
            }
            WriteSession(sessionId);
            var userState = _state.GetUserState(sessionId);

            return userState;
        }


        private void WriteSession(string sessionId)
        {
            HttpContext.Session.SetString("sessionId", sessionId);
        }

        [Route("commit")]
        public void Commit(WellPoint pt)
        {
            var sessionId = GetSessionId();
            var res = _state.UpdateUser(sessionId, pt);
            if (!res)
            {
                throw new Exception("We cannot update using this point on this user");
            }
        }

        private string GetSessionId()
        {
            var sessionString = HttpContext.Session.GetString("session");
            if (sessionString != null)
            {
                return sessionString;
            }
            else
            {
                throw new Exception("bad user! you need to init!");
            }
        }

        //TODO remove
        [Route("userdata")]
        public UserData GetUserState()
        {
            var sessionId = GetSessionId();
            return _state.GetUserState(sessionId);
        }
    }

}
