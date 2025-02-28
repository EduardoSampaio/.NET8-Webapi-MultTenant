using Application.Wrappers;
using MediatR;

namespace Application.Features.Identity.Tokens.Queries
{
    public class GetRefreshTokenQuery: IRequest<IResponseWrapper>
    {
        public RefreshTokenRequest RefreshTokenRequest { get; set; }
    }

    public class GetRefreshTokenQueryHandler(ITokenService tokenService) : IRequestHandler<GetRefreshTokenQuery, IResponseWrapper>
    {
        private readonly ITokenService _tokenService = tokenService;

        public async Task<IResponseWrapper> Handle(GetRefreshTokenQuery request, CancellationToken cancellationToken)
        {
            var refreshToken = await _tokenService.RefreshTokenAsync(request.RefreshTokenRequest);

            return await ResponseWrapper<TokenResponse>.SuccessAsync(data: refreshToken);
        }
    }
}
