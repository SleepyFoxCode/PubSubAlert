using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;



namespace RazorPagesTwitchPubSub{

    public class CurrentUser{

        public CurrentUser(HttpContext ctx, IConfiguration _configuration){
            if(!TwitchHelper.AccessTokenIsValid(ctx.Request.Cookies["access_token"], _configuration)){
                TwitchJsonHelper.JsonRefresh refreshObj;
                refreshObj = TwitchHelper.RefreshTokens(ctx.Request.Cookies["refresh_token"], _configuration);
                if(refreshObj == null) return;
                else {
                    ctx.Response.Cookies.Delete("access_token");
                    ctx.Response.Cookies.Delete("refresh_token");

                    var options = new CookieOptions
                    {
                        IsEssential = true
                    };
                    
                    ctx.Response.Cookies.Append("access_token", refreshObj.access_token, options);
                    ctx.Response.Cookies.Append("refresh_token", refreshObj.refresh_token, options);
                    information = TwitchHelper.LoadUserInformation(refreshObj.access_token, _configuration);
                    // We refresh the acces token on our websocket server
                    MyWebsocketHelper.UpdateUser(information.id, information.login, refreshObj.access_token);
                }
            }
            else {
                information = TwitchHelper.LoadUserInformation(ctx.Request.Cookies["access_token"], _configuration);
                //MyWebsocketHelper.UpdateUserOnWS(information.id, information.login, ctx.Request.Cookies["access_token"]);
            }
        }
        // Overload: CALL ONLY IF YOU ARE SURE ACCESS_TOKEN IS VALID
        public CurrentUser(String access_token, IConfiguration _configuration){
                information = TwitchHelper.LoadUserInformation(access_token, _configuration);
                MyWebsocketHelper.UpdateUser(information.id, information.login, access_token);
        }
        public TwitchHelper.UserInformation information;    // Can be null

    }
}