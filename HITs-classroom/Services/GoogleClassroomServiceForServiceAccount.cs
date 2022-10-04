using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Services;
using System.Diagnostics;

namespace HITs_classroom.Services
{
    public class GoogleClassroomServiceForServiceAccount
    {
        public ClassroomService GetClassroomService()
        {
            try
            {
                string[] scopes = {
                    ClassroomService.Scope.ClassroomCourses,
                    ClassroomService.Scope.ClassroomRosters,
                    ClassroomService.Scope.ClassroomProfileEmails,
                    ClassroomService.Scope.ClassroomCourseworkMe,
                    ClassroomService.Scope.ClassroomCourseworkStudents
                };

                var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                var user = MyConfig.GetValue<string>("UserEmail");
                var key = MyConfig.GetValue<string>("ServiceAccountKeyPath");

                GoogleCredential credential = GoogleCredential
                    .FromStream(new FileStream(key, FileMode.Open, FileAccess.Read))
                    .CreateScoped(scopes)
                    .CreateWithUser(user);

                ClassroomService classroomService = new ClassroomService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "HITs-Classroom"
                });

                return classroomService;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new AggregateException();
            }
        }

    }
}
