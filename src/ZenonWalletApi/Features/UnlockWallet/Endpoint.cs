﻿using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.UnlockWallet
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapUnlockWalletEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/unlock", UnlockWalletAsync)
                .WithName("UnlockWallet")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <remarks>
        /// Unlocks the wallet
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized</para>
        /// </remarks>
        public static async Task<IResult> UnlockWalletAsync(
            IWalletService wallet,
            [Validate] UnlockWalletRequest request)
        {
            await wallet.UnlockAsync(request.Password);

            return Results.Ok();
        }
    }
}
