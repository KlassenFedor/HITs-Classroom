using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace HITs_classroom.Services
{
    public class GoogleClassroomService
    {
        static string[] Scopes = { ClassroomService.Scope.ClassroomCoursesReadonly };

        public ClassroomService GetClassroomService()
        {
            UserCredential credential = MakeCredential();
            ClassroomService classroomService = new ClassroomService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "HITs-Classroom"
            });

            return classroomService;
        }

        private UserCredential MakeCredential()
        {
            UserCredential credential;
            using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            return credential;
        }
    }
}
