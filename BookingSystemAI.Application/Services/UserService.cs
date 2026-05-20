using BookingSystemAI.Application.Abstractions;

using BookingSystemAI.Application.DTOs;

using BookingSystemAI.Application.Models;
using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Services;



public class UserService(IIdentityUserManager identityUserManager, IJwtTokenService jwtTokenService) : IUserService

{

    public async Task<RegisterResult> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)

    {

        var validationErrors = ValidateRegistration(request);

        if (validationErrors is not null)

            return RegisterResult.ValidationFailure(validationErrors);



        var createResult = await identityUserManager.CreateAsync(request.Email, request.Password, cancellationToken);

        if (!createResult.Succeeded)

            return RegisterResult.ValidationFailure(createResult.Errors);



        var roleResult = await identityUserManager.AssignRoleAsync(createResult.UserId!, request.Role, cancellationToken);

        if (!roleResult.Succeeded)

            return RegisterResult.ValidationFailure(roleResult.Errors);



        return RegisterResult.Success(new RegisterResponseDto(createResult.UserId!));

    }



    public async Task<LoginResult> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)

    {

        var validationErrors = ValidateCredentials(request.Email, request.Password);

        if (validationErrors is not null)

            return LoginResult.ValidationFailure(validationErrors);



        var user = await identityUserManager.FindByEmailAsync(request.Email, cancellationToken);

        if (user is null || !await identityUserManager.CheckPasswordAsync(user, request.Password, cancellationToken))

            return LoginResult.Unauthorized();



        var roles = await identityUserManager.GetRolesAsync(user.Id, cancellationToken);

        var userWithRoles = new User
        {
            Id = user.Id,
            Email = user.Email,
            Roles = roles
        };



        return LoginResult.Success(jwtTokenService.CreateToken(userWithRoles));

    }



    private static IReadOnlyDictionary<string, string[]>? ValidateRegistration(RegisterRequestDto request)

    {

        var credentialErrors = ValidateCredentials(request.Email, request.Password);

        if (credentialErrors is not null)

            return credentialErrors;



        if (string.IsNullOrWhiteSpace(request.Role) || !ApplicationRoles.All.Contains(request.Role))

        {

            return new Dictionary<string, string[]>

            {

                ["role"] = ["Role must be Host or Client."]

            };

        }



        return null;

    }



    private static IReadOnlyDictionary<string, string[]>? ValidateCredentials(string email, string password)

    {

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(email))

            errors["email"] = ["Email is required."];

        if (string.IsNullOrWhiteSpace(password))

            errors["password"] = ["Password is required."];



        return errors.Count == 0 ? null : errors;

    }

}

