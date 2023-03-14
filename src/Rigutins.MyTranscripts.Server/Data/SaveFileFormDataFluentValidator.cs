using FluentValidation;

namespace Rigutins.MyTranscripts.Server.Data;

public class SaveFileFormDataFluentValidator : AbstractValidator<SaveFileFormData>
{
	private const int NameMaxLength = 50;

	private static readonly List<char> InvalidCharacters = new()
	{
		'<', '>', ':', '"', '/', '\\', '|', '?', '*'
	};

	public SaveFileFormDataFluentValidator()
	{
		RuleFor(r => r.Name).NotEmpty();
		RuleFor(r => r.Name).MaximumLength(NameMaxLength);
		RuleFor(r => r.Name).Must(n => n.All(c => !InvalidCharacters.Contains(c))).WithMessage("Insert a valid name.");
	}
}
