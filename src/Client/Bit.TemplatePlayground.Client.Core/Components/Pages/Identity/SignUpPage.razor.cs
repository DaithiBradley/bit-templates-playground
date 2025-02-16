﻿using Bit.TemplatePlayground.Shared.Dtos.Identity;
using Bit.TemplatePlayground.Client.Core.Controllers.Identity;

namespace Bit.TemplatePlayground.Client.Core.Components.Pages.Identity;

public partial class SignUpPage
{
    private bool isWaiting;
    private string? message;
    private ElementReference messageRef = default!;
    private readonly SignUpRequestDto signUpModel = new() { UserName = Guid.NewGuid().ToString() };


    [AutoInject] private ILocalHttpServer localHttpServer = default!;
    [AutoInject] private IIdentityController identityController = default!;
    [AutoInject] private IExternalNavigationService externalNavigationService = default!;

    private async Task DoSignUp()
    {
        if (isWaiting) return;

        var googleRecaptchaResponse = await JSRuntime.GoogleRecaptchaGetResponse();
        if (string.IsNullOrWhiteSpace(googleRecaptchaResponse))
        {
            message = Localizer[nameof(AppStrings.InvalidGoogleRecaptchaChallenge)];
            await messageRef.ScrollIntoView();
            return;
        }

        signUpModel.GoogleRecaptchaResponse = googleRecaptchaResponse;

        isWaiting = true;
        message = null;
        StateHasChanged();

        try
        {
            await identityController.SignUp(signUpModel, CurrentCancellationToken);

            var queryParams = new Dictionary<string, object?>();
            if (string.IsNullOrEmpty(signUpModel.Email) is false)
            {
                queryParams.Add("email", signUpModel.Email);
            }
            if (string.IsNullOrEmpty(signUpModel.PhoneNumber) is false)
            {
                queryParams.Add("phoneNumber", signUpModel.PhoneNumber);
            }
            var confirmUrl = NavigationManager.GetUriWithQueryParameters("confirm", queryParams);
            NavigationManager.NavigateTo(confirmUrl);
        }
        catch (KnownException e)
        {
            message = e is ResourceValidationException re
                                    ? string.Join(" ", re.Payload.Details.SelectMany(d => d.Errors).Select(e => e.Message))
                                    : e.Message;
            await messageRef.ScrollIntoView();

            await JSRuntime.GoogleRecaptchaReset();
        }
        finally
        {
            isWaiting = false;
        }
    }

    private async Task GoogleSignUp()
    {
        await SocialSignUp("Google");
    }

    private async Task GitHubSignUp()
    {
        await SocialSignUp("GitHub");
    }

    private async Task SocialSignUp(string provider)
    {
        if (isWaiting) return;

        isWaiting = true;
        message = null;

        try
        {
            var port = await localHttpServer.Start();

            var redirectUrl = await identityController.GetSocialSignInUri(provider, localHttpPort: port is -1 ? null : port);

            await externalNavigationService.NavigateToAsync(redirectUrl);
        }
        catch (KnownException e)
        {
            isWaiting = false;

            message = e.Message;

            await messageRef.ScrollIntoView();
        }
        catch
        {
            isWaiting = false;

            throw;
        }
    }
}
