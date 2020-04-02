using System;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddAppError(this HttpResponse res, string message)
        {
            res.Headers.Add("Application-Error", message);
            res.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            res.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        public static int CalculateAge(this DateTime dateObj)
        {
            var age = DateTime.Today.Year - dateObj.Year;
            if(dateObj.AddYears(age) > DateTime.Today)
                age--;

            return age;
        }
    }
}