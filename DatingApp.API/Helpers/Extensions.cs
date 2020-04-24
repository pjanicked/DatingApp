using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        public static void AddPaginationHeader(this HttpResponse response, int currentPage,
                int itemsPerPage, int totalPages, int totalItems)
        {
            var pageObject = new PaginationHeader(currentPage, itemsPerPage, totalPages, totalItems);
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(pageObject, camelCaseFormatter));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
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