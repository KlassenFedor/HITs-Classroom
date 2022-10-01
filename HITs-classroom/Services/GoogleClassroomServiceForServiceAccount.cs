using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Services;

namespace HITs_classroom.Services
{
    public class GoogleClassroomServiceForServiceAccount
    {
        public ClassroomService GetClassroomService()
        {
            string[] scopes = {
                ClassroomService.Scope.ClassroomCourses,
                ClassroomService.Scope.ClassroomRosters,
                ClassroomService.Scope.ClassroomProfileEmails
            };

            GoogleCredential credential = GoogleCredential
                .FromStream(new FileStream("Keys/hits-classroom-1661148456378-4f7735c17e75.json", FileMode.Open, FileAccess.Read))
                .CreateScoped(scopes);

            ClassroomService classroomService = new ClassroomService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "HITs-Classroom"
            });

            return classroomService;
        }

    }
}
