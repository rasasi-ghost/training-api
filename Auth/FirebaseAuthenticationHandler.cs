using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using TrainingApi.Services;
using System.Linq;

namespace TrainingApi.Auth
{
    public class FirebaseAuthenticationHandler : JwtBearerHandler
    {
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly ILogger<FirebaseAuthenticationHandler> _logger;

        public FirebaseAuthenticationHandler(
            IOptionsMonitor<JwtBearerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IFirebaseAuthService firebaseAuthService)
            : base(options, logger, encoder, clock)
        {
            _firebaseAuthService = firebaseAuthService;
            _logger = logger.CreateLogger<FirebaseAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Extract the token from the Authorization header
                string token = Context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No token found in the request");
                    return AuthenticateResult.Fail("No token found in the request");
                }
                
                _logger.LogInformation($"Verifying token: {token.Substring(0, Math.Min(token.Length, 20))}...");
                
                try
                {
                    // Directly verify the token with Firebase
                    string uid = await _firebaseAuthService.VerifyIdTokenAsync(token);
                    
                    if (string.IsNullOrEmpty(uid))
                    {
                        _logger.LogWarning("Firebase returned empty UID for token");
                        return AuthenticateResult.Fail("Invalid token");
                    }
                    
                    _logger.LogInformation($"Token validated successfully for user {uid}");
                    
                    // Get user details including custom claims
                    var userRecord = await _firebaseAuthService.GetUserAsync(uid);
                    
                    // Create claims identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, uid),
                        // new Claim(ClaimTypes.Name, userRecord.DisplayName ?? userRecord.Email)
                    };
                    
                   
                    
                    // Add role claim if available
                    if (userRecord.CustomClaims != null && userRecord.CustomClaims.ContainsKey("role"))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRecord.CustomClaims["role"].ToString()));
                        _logger.LogInformation($"Added role claim: {userRecord.CustomClaims["role"]}");
                    }
                    
                    var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, JwtBearerDefaults.AuthenticationScheme);
                    
                    return AuthenticateResult.Success(ticket);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Firebase token verification failed: {ex.Message}");
                    return AuthenticateResult.Fail($"Firebase token verification failed: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Authentication error: {ex.Message}");
                return AuthenticateResult.Fail($"Authentication error: {ex.Message}");
            }
        }
    }
}
