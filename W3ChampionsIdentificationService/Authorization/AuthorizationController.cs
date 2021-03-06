﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Twitch;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.Authorization
{

    [ApiController]
    [Route("api/oauth")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IBlizzardAuthenticationService _authenticationService;
        private readonly IW3CAuthenticationService _w3CAuthenticationService;

        private readonly ITwitchAuthenticationService _twitchAuthenticationService;

        public AuthorizationController(
            IBlizzardAuthenticationService authenticationService,
            IW3CAuthenticationService w3CAuthenticationService,
            ITwitchAuthenticationService twitchAuthenticationService)
        {
            _authenticationService = authenticationService;
            _w3CAuthenticationService = w3CAuthenticationService;
            _twitchAuthenticationService = twitchAuthenticationService;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetBlizzardToken([FromQuery] string code, [FromQuery] string redirectUri)
        {
            var token = await _authenticationService.GetToken(code, redirectUri);
            if (token == null)
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var userInfo = await _authenticationService.GetUser(token.access_token);
            if (userInfo == null)
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var w3User = await _w3CAuthenticationService.GetUserById(userInfo.battletag);

            if (w3User == null)
            {
                var w3CUserAuthentication = W3CUserAuthentication.Create(userInfo.battletag);
                await _w3CAuthenticationService.Save(w3CUserAuthentication);
                return Ok(w3CUserAuthentication);
            }

            return Ok(w3User);
        }

        [HttpDelete("token")]
        [InjectActingPlayerAuthCode]
        public async Task<IActionResult> Logout([FromQuery] string actingPlayer)
        {
            await _w3CAuthenticationService.Delete(actingPlayer);
            return Ok();
        }

        [HttpGet("battleTag")]
        public async Task<IActionResult> GetUserInfo([FromQuery] string bearer)
        {
            var userInfo = await _w3CAuthenticationService.GetUserByToken(bearer);

            return userInfo == null ? (IActionResult) Unauthorized("Sorry H4ckerb0i") : Ok(userInfo);
        }

        [HttpGet("twitch")]
        public async Task<IActionResult> GetTwitchToken()
        {
            var token = await _twitchAuthenticationService.GetToken();
            return Ok(token);
        }
    }
}