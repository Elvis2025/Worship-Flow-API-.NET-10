using FluentValidation;

namespace WorshipFlow.Application.Features.Availability;

public sealed class UpdateAvailabilityDtoValidator : AbstractValidator<UpdateAvailabilityDto>
{
    public UpdateAvailabilityDtoValidator()
    {
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).When(x => x.StartTime.HasValue && x.EndTime.HasValue);
        });
    }
}
