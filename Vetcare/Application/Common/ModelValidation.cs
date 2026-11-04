using System.ComponentModel.DataAnnotations;

namespace Vetcare.Application.Common;

internal static class ModelValidation
{
    public static Dictionary<string, string[]> Validate<T>(T model) where T : class
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        return results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty),
                (result, member) => (member, result.ErrorMessage))
            .GroupBy(item => item.member)
            .ToDictionary(group => group.Key,
                group => group.Select(item => item.ErrorMessage ?? "El valor no es válido.").ToArray());
    }

    public static void AddError(Dictionary<string, string[]> errors, string field, string message) =>
        errors[field] = errors.TryGetValue(field, out var existing) ? [.. existing, message] : [message];
}
