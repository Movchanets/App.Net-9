using FluentValidation;

namespace Application.Commands.User.Profile.UploadProfilePicture;

public class UploadProfilePictureValidator : AbstractValidator<UploadProfilePictureCommand>
{
	private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
	private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
	private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

	public UploadProfilePictureValidator()
	{
		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("UserId is required");

		RuleFor(x => x.FileName)
			.NotEmpty()
			.WithMessage("FileName is required")
			.Must(HasValidExtension)
			.WithMessage($"File must have one of the following extensions: {string.Join(", ", AllowedExtensions)}");

		RuleFor(x => x.ContentType)
			.NotEmpty()
			.WithMessage("ContentType is required")
			.Must(IsValidContentType)
			.WithMessage($"ContentType must be one of: {string.Join(", ", AllowedContentTypes)}");

		RuleFor(x => x.FileStream)
			.NotNull()
			.WithMessage("File stream is required")
			.Must(stream => stream != null && stream.Length > 0)
			.WithMessage("File stream cannot be empty")
			.Must(stream => stream == null || stream.Length <= MaxFileSizeBytes)
			.WithMessage($"File size must not exceed {MaxFileSizeBytes / (1024 * 1024)} MB");
	}

	private bool HasValidExtension(string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName)) return false;
		var ext = Path.GetExtension(fileName).ToLowerInvariant();
		return AllowedExtensions.Contains(ext);
	}

	private bool IsValidContentType(string contentType)
	{
		if (string.IsNullOrWhiteSpace(contentType)) return false;
		return AllowedContentTypes.Contains(contentType.ToLowerInvariant());
	}
}
