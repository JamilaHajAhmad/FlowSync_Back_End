namespace WebApplicationFlowSync.DTOs
{
    public class UpdateProfilePictureDto
    {
        /// <summary>
        /// The new profile picture URL for the user account. Can be left empty.
        /// </summary>
        public string? PictureURL { get; set; }
    }
}
