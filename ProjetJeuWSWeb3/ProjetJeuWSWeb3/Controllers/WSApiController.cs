using ProjetJeuWSWeb3.WS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ProjetJeuWSWeb3.Controllers
{
    public class WSApiController : ApiController
    {
        [Route("api/websocket/{username}")]
        public HttpResponseMessage Get(string username)
        {
            if (System.Web.HttpContext.Current.IsWebSocketRequest)
            {
                if (username == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                System.Web.HttpContext.Current.AcceptWebSocketRequest(new GestionnaireWebSockets(username).Receiver);

                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}