using System.Text.Json;

namespace Image_encryption.middleWare
{
        public class IdValidationMiddleware
        {

            private readonly RequestDelegate _next;
            public IdValidationMiddleware(RequestDelegate next)
            {
                _next = next;

            }


            public async Task InvokeAsync(HttpContext context)
            {
                if ((context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                     context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase)) &&
                    context.Request.Body != null)
                {
                    context.Request.EnableBuffering();
                    using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
                    {
                        var requestBody = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;

                        try
                        {
                            using (JsonDocument document = JsonDocument.Parse(requestBody))
                            {
                                if (document.RootElement.TryGetProperty("id", out JsonElement idElement) &&
                                    idElement.TryGetInt64(out long id))
                                {
                                    if (!IsValidIsraeliId(id))
                                    {
                                        context.Response.StatusCode = 400; // Bad Request
                                        await context.Response.WriteAsync("Invalid Israeli ID.");
                                        return;
                                    }
                                }
                                else
                                {
                                    context.Response.StatusCode = 400; // Bad Request
                                    await context.Response.WriteAsync("ID is missing or invalid.");
                                    return;
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            context.Response.StatusCode = 400; // Bad Request
                            await context.Response.WriteAsync($"Invalid JSON payload: {ex.Message}");
                            return;
                        }
                    }
                }
                //else
                //{
                //    if ((context.Request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) ||
                //         context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) &&
                //        context.Request.Body != null)
                //    {// Check query string for 'id'
                //        string idString = context.Request.Query["id"];

                //        if (string.IsNullOrEmpty(idString))
                //        {
                //            // Check route data for 'id'
                //            if (context.Request.RouteValues.TryGetValue("id", out var routeValue))
                //            {
                //                idString = routeValue?.ToString();
                //            }
                //        }

                //        if (long.TryParse(idString, out long id))
                //        {
                //            if (!IsValidIsraeliId(id))
                //            {
                //                context.Response.StatusCode = 400; // Bad Request
                //                await context.Response.WriteAsync("Invalid Israeli ID.");
                //                return;
                //            }
                //        }
                //        else
                //        {
                //            context.Response.StatusCode = 400; // Bad Request
                //            await context.Response.WriteAsync("ID is missing or invalid.");
                //            return;
                //        }
                //    }
                await _next(context);
            }



            //public async Task InvokeAsync(HttpContext context)
            // {
            //     // if (context.Request.Path.StartsWithSegments("/api/Customer"))
            //     //  {
            //     var id = context.Request.Query["id"];
            //     if (long.TryParse(id, out long Id))
            //     {
            //         if (!IsValidIsraeliId(Id))
            //         {
            //             context.Response.StatusCode = 400; // Bad Request
            //             await context.Response.WriteAsync("Invalid ID number.");
            //             return;
            //         }
            //     }
            //     //  }

            //     await _next(context);
            // }

            private bool IsValidIsraeliId(long id)
            {
                if (id < 100000000)
                {
                    return false;
                }

                long sum = 0;
                long temp = id;
                for (int i = 0; i < 9; i++)
                {
                    long digit = temp % 10;
                    temp /= 10;
                    if (i % 2 != 0)
                    {
                        digit *= 2;
                    }
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                    sum += digit;
                }

                return sum % 10 == 0;
            }







        }
    }

