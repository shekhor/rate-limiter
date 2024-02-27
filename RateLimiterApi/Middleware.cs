using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace RateLimiterApi
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class Middleware
    {
        private readonly RequestDelegate _next;
        private const int MAX_REQUESTS = 5; // Max requests allowed per minute
        private const long TIME_WINDOW = 60 * 1000; // Time window in milliseconds (1 minute)

        private Dictionary<String, Queue<long>> requestTimesPerIp = new Dictionary<string, Queue<long>>();

        public Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Console.WriteLine("Request is going through token bucket RateLimitMiddleware");

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            String ipAdress = context.Connection.LocalIpAddress.ToString();

            if (!requestTimesPerIp.ContainsKey(ipAdress))
            {
                requestTimesPerIp[ipAdress] = new Queue<long>();
            }

            // Remove old request times
            while (requestTimesPerIp[ipAdress].Any() && currentTime - requestTimesPerIp[ipAdress].Peek() > TIME_WINDOW)
            {
                requestTimesPerIp[ipAdress].Dequeue();
            }

            // Check if requests exceed the limit
            if (requestTimesPerIp[ipAdress].Count >= MAX_REQUESTS)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            // Add current request time
            requestTimesPerIp[ipAdress].Enqueue(currentTime);

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddlewareClassTemplate(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Middleware>();
        }
    }
}

