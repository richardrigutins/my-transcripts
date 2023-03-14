using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Reflection;

namespace Rigutins.MyTranscripts.Server.Shared;

// Based on https://gist.github.com/SteveSandersonMS/090145d7511c5190f62a409752c60d00#file-fluentvalidator-cs
public class FluentValidator<TValidator> : ComponentBase
	where TValidator : IValidator
{
	private readonly static char[] Separators = new[] { '.', '[' };

	private TValidator? _validator;

	[CascadingParameter] private EditContext? EditContext { get; set; }

	[Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

	private TValidator Validator
	{
		get
		{
			_validator ??= ActivatorUtilities.CreateInstance<TValidator>(ServiceProvider);
			return _validator;
		}
	}

	protected override void OnInitialized()
	{
		if (EditContext == null)
		{
			throw new InvalidOperationException($"{nameof(FluentValidator<TValidator>)} requires a cascading " +
				$"parameter of type {nameof(EditContext)}. For example, you can use {nameof(FluentValidator<TValidator>)} " +
				$"inside an {nameof(EditForm)}.");
		}

		var messages = new ValidationMessageStore(EditContext);

		// Revalidate when any field changes, or if the entire form requests validation
		// (e.g., on submit)

		EditContext.OnFieldChanged += (sender, eventArgs) => ValidateModel(sender as EditContext, messages);

		EditContext.OnValidationRequested += (sender, eventArgs) => ValidateModel(sender as EditContext, messages);
	}

	private void ValidateModel(EditContext? editContext, ValidationMessageStore messages)
	{
		if (editContext is null)
		{
			return;
		}

		var context = new ValidationContext<object>(editContext.Model);
		var validationResult = Validator.Validate(context);
		messages.Clear();
		foreach (var error in validationResult.Errors)
		{
			var fieldIdentifier = ToFieldIdentifier(editContext, error.PropertyName);
			messages.Add(fieldIdentifier, error.ErrorMessage);
		}

		editContext.NotifyValidationStateChanged();
	}

	private static FieldIdentifier ToFieldIdentifier(EditContext editContext, string propertyPath)
	{
		// This method parses property paths like 'SomeProp.MyCollection[123].ChildProp'
		// and returns a FieldIdentifier which is an (instance, propName) pair. For example,
		// it would return the pair (SomeProp.MyCollection[123], "ChildProp"). It traverses
		// as far into the propertyPath as it can go until it finds any null instance.

		var obj = editContext.Model;

		while (true)
		{
			int nextTokenEnd = propertyPath.IndexOfAny(Separators);
			if (nextTokenEnd < 0)
			{
				return new FieldIdentifier(obj, propertyPath);
			}

			string nextToken = propertyPath[..nextTokenEnd];
			propertyPath = propertyPath[(nextTokenEnd + 1)..];

			object? newObj;
			if (nextToken.EndsWith("]"))
			{
				// It's an indexer
				// This code assumes C# conventions (one indexer named Item with one param)
				nextToken = nextToken[..(nextToken.Length - 1)];
				PropertyInfo? prop = obj.GetType().GetProperty("Item");
				if (prop == null)
				{
					throw new InvalidOperationException($"Could not find property named Item on object of type {obj.GetType().FullName}.");
				}

				Type indexerType = prop.GetIndexParameters()[0].ParameterType;
				var indexerValue = Convert.ChangeType(nextToken, indexerType);
				newObj = prop.GetValue(obj, new object[] { indexerValue });
			}
			else
			{
				// It's a regular property
				PropertyInfo? prop = obj.GetType().GetProperty(nextToken);
				if (prop == null)
				{
					throw new InvalidOperationException($"Could not find property named {nextToken} on object of type {obj.GetType().FullName}.");
				}

				newObj = prop.GetValue(obj);
			}

			if (newObj == null)
			{
				// This is as far as we can go
				return new FieldIdentifier(obj, nextToken);
			}

			obj = newObj;
		}
	}
}
