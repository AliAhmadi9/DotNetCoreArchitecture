using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebFramework.Configuration
{
    public static class SessionManager
    {
        public enum SessionKeys
        {
            JwtToken,   
            CurrentWorkSpcae,
        }

        static Dictionary<SessionKeys, string> keys = new Dictionary<SessionKeys, string>()
        {
            { SessionKeys.JwtToken,"JWT_TOKEN"},
            { SessionKeys.CurrentWorkSpcae,"CurrentWorkSpcae"},
        };

        public static string Get(HttpContext context,SessionKeys key)
        {
            string sessionName = keys[key];           

            if (context.Session == null) return null;

            return context.Session.GetString(sessionName);
        }
        public static void Set(HttpContext context, SessionKeys key, string value)
        {
            string sessionName = keys[key];

            context.Session.SetString(sessionName, value);
        }

        public static void Remove(HttpContext context, SessionKeys key)
        {
            string sessionName = keys[key];

            context.Session.Remove(sessionName);
        }
    }
}
