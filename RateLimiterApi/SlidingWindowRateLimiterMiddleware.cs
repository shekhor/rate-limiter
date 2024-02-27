using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace RateLimiterApi
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class SlidingWindowRateLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int windowSizeInSeconds = 60;
        private readonly int maxRequests = 5;
        private int head;
        private int tail;
        private int count;
        private DateTime[] requests;

        public SlidingWindowRateLimiterMiddleware(RequestDelegate next)
        {
            this._next = next;
            this.head = 0;
            this.tail = 0;
            this.count = 0;
            this.requests = new DateTime[maxRequests];

        }

        public async Task Invoke(HttpContext httpContext)
        {
            Console.WriteLine("Request is going through sliding window RateLimitMiddleware");

            while (count > 0 && (DateTime.Now - requests[head]).TotalSeconds > windowSizeInSeconds)
            {
                head = (head + 1) % maxRequests;
                count--;
            }

            if(count >= maxRequests)
            {
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await httpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            requests[tail] = DateTime.Now;
            count++;
            tail = (tail + 1) % maxRequests;
            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class SlidingWindowRateLimiterMiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddlewareClassTemplate(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SlidingWindowRateLimiterMiddleware>();
        }
    }
}

