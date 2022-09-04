namespace HITs_classroom.Models.Invitation
{
    public class InvitationInfoModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string CourseId { get; set; }
        public string Role { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
