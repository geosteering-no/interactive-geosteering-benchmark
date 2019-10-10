﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GameServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeoController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<GeoController> _logger;

        public GeoController(ILogger<GeoController> logger)
        {
            _logger = logger;
        }

        private List<Realization> createRealization()
        {
            Random r = new Random();

            List<Realization> rs = Enumerable.Range(0, 100).Select(i =>
            {
                var startY = 0.2 + r.NextDouble();
                var startX = 0.0;
                var points = new List<Tuple<Double, Double>>();
                for (double x = 0.0; x <= 1.0; x += 0.1)
                {
                    points.Add(new Tuple<Double, Double>(x, 0.3 + (r.NextDouble() - 0.5) / 10));
                }
                for (double x = 1.0; x >= 0.0; x -= 0.1)
                {
                    points.Add(new Tuple<Double, Double>(x, 0.6 + (r.NextDouble() - 0.5) / 10));
                }
                var realization = new Realization();
                realization.polygons.Add(points);
                return realization;
            }).ToList();

            return rs;
        }

        [Route("init")]
        public void Init()
        {
            var session = new SessionState();


            session.realizations.Add(createRealization());
            WriteSession(session);
        }

        private void WriteSession(SessionState session)
        {
            HttpContext.Session.SetString("session", session.toJson());
        }

        [Route("commit")]
        public void Commit(double angle)
        {
            var session = GetSession();
            session.angles.Add(angle);
            //System.Console.WriteLine("wrote angle: " + angle);
            System.Console.WriteLine("angles: " + session.angles.Count);
            WriteSession(session);

        }

        private SessionState GetSession()
        {
            var sessionString = HttpContext.Session.GetString("session");
            if (sessionString != null)
            {
                return SessionState.fromJson(sessionString);
            }
            else
            {
                throw new Exception("bad user! you need to init!");
            }
        }

        [Route("realization")]
        public List<Realization> GetRealizations()
        {

            var session = GetSession();
            return session.realizations.Last();

        }
    }

}
