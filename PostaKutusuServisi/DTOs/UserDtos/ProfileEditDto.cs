namespace PostaKutusuServisi.DTOs.UserDtos
{
    public class ProfileEditDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
