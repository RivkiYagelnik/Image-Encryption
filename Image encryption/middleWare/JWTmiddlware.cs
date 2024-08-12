using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace Image_encryption.middleWare
{
    public class JWTmiddlware
    {
        private readonly RequestDelegate _next;
        public JWTmiddlware(RequestDelegate next)
        {
            _next = next;
       
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                AttachUserToContext(context, token);
            }

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("our-very-secure-and-long-key-of-32-characters-or-more");
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "sub").Value;
                var role = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value;
                context.Items["User"] = userId;
                context.Items["Role"] = role;
            }
            catch (Exception ex)
            {
                // אם האימות נכשל, אין להצמיד את המידע
                context.Response.StatusCode = 401;
                context.Response.WriteAsync($"Token validation failed: {ex.Message}");
            }
        }
       
    }
}
