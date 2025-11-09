using TaskPlanner.Application.Abstractions;
using TaskPlanner.Application.Common;
using TaskPlanner.Application.Dtos;
using TaskPlanner.Application.Requests;
using TaskPlanner.Domain.Entities;
using TaskPlanner.Domain.ValueObjects;

namespace TaskPlanner.Application.Services;

public class TeamMemberService
{
    private readonly ITeamMemberRepository _memberRepository;

    public TeamMemberService(ITeamMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<OperationResult<TeamMemberDto>> RegisterMemberAsync(RegisterMemberRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = Email.Create(request.Email);
            var existing = await _memberRepository.GetByEmailAsync(email.Value, cancellationToken);
            if (existing is not null)
            {
                return OperationResult<TeamMemberDto>.Fail($"Member with email '{email}' already exists.");
            }

            var member = TeamMember.Create(request.FullName, email);
            await _memberRepository.AddAsync(member, cancellationToken);
            return OperationResult<TeamMemberDto>.Ok(ToDto(member));
        }
        catch (Exception ex)
        {
            return OperationResult<TeamMemberDto>.Fail(ex.Message);
        }
    }

    public async Task<IReadOnlyCollection<TeamMemberDto>> GetMembersAsync(CancellationToken cancellationToken = default)
    {
        var members = await _memberRepository.GetAllAsync(cancellationToken);
        return members.Select(ToDto).ToList();
    }

    private static TeamMemberDto ToDto(TeamMember member)
    {
        return new TeamMemberDto(member.Id, member.FullName, member.Email.Value, member.JoinedAt);
    }
}
