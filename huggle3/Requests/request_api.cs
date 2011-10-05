﻿//This is a source code or part of Huggle project
//
//This file contains code for
//last modified by Petrb

//Copyright (C) 2011 Huggle team

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.


using System;
using System.Collections.Generic;
using System.Text;

namespace huggle3
{
    public class ApiResult
    {
        public string ErrorCode;
        public string ResultText;
        public string Error_Data;

        public bool ResultInError
        {
            get {
                if (ErrorCode == "")
                {
                    return false;
                }
                    return (!(ErrorCode == null));
                }
        }

        public ApiResult()
        {

        }
        public ApiResult(string Text, string Error = null, string Descr = null)
        {
            this.ResultText = Text;
            this.Error_Data = Descr;
            this.ErrorCode = Error;
        }

    }
    public class request_api : request_core.Request
    {
        static string ApiQuery = "";

        public static LoginResult DoLogin()
        {
            Core.History("DoLogin()");
            try
            {
                //Get the result of the api login request
                ApiResult result = new ApiResult();
                result = ApiRequest("action=login", "lgname=" + System.Web.HttpUtility.UrlEncode(Config.Username), Config.Project);

                //If this returns as null then the login has failed
                if (result == null || result.ResultText == null)
                {
                    return LoginResult.Failed;
                }

                //If no token is found (doesnt match regex) then the login has failed
                if (System.Text.RegularExpressions.Regex.Match(result.ResultText, "token=\"[0-9A-Za-z]*\"").Success == false)
                {
                    return LoginResult.Failed;
                }

                //This means that there must be a token, So lets get this token
                login.Token = System.Text.RegularExpressions.Regex.Match(result.ResultText, "token=\"[0-9A-Za-z]*\"").Value;
                //And format it properly
                login.Token = login.Token.Replace("\"", "");
                login.Token = login.Token.Replace("token=", "");

                //Now we will do a request with our new token
                result = ApiRequest("action=login", "lgname=" + System.Web.HttpUtility.UrlEncode(Config.Username) + "&lgpassword=" + System.Web.HttpUtility.UrlEncode(Config.Password) + "&lgtoken=" + login.Token, Config.Project);

                //As this has returned as null the login has probably failed
                if (result.ResultText == null)
                {
                    return LoginResult.Failed;
                }

                //Now we will try and match all of the other possible values
                if (result.ResultText.Contains("result=\"Success\""))
                {
                        return LoginResult.Success;
                }
                if (result.ResultText.Contains("result=\"Illegal\""))
                {
                    return LoginResult.Illegal;
                }
                if (result.ResultText.Contains("result=\"NotExists\""))
                {
                    return LoginResult.NotExists;
                }
                if (result.ResultText.Contains("result=\"WrongPass\""))
                {
                        return LoginResult.WrongPass;
                }
            }
            catch (Exception x)
            {
                Core.ExceptionHandler(x);
            }

            //TODO: This result we have not yet accounted for (see list below)
            //NoName
            //EmptyPass
            //WrongPluginPass
            //CreateBlocked
            //Throttled
            //Blocked
            //mustbeposted
            //NeedToken
            return LoginResult.None;
        }
       
    }
}