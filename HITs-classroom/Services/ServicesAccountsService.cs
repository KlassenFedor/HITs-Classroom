using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.IAMCredentials.v1.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using System.Diagnostics;
using static Google.Apis.IAMCredentials.v1.ProjectsResource;
using System.Security.Claims;
using Google.Apis.IAMCredentials.v1;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;

namespace HITs_classroom.Services
{
    public interface IServicesAccountsService
    {
        void GetCourse();
        void ImpersonateSA(string relatedUser);
    }
    public class ServicesAccountsService: IServicesAccountsService
    {
        public void GetCourse()
        {
            string[] scopes = {
                ClassroomService.Scope.ClassroomCourses,
                ClassroomService.Scope.ClassroomRosters,
                ClassroomService.Scope.ClassroomProfileEmails
            };

            GoogleCredential credential = GoogleCredential
                .FromStream(new FileStream("hits-classroom-1661148456378-fb06e5c714d4.json", FileMode.Open, FileAccess.Read))
                .CreateScoped(scopes);

            ClassroomService classroomService = new ClassroomService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "HITs-Classroom"
            });

            var course = classroomService.Courses.Get("490235491409").Execute();
            Debug.WriteLine(course.Name);
        }

        public void ImpersonateSA(string relatedUser)
        {
            string[] scopes = {
                ClassroomService.Scope.ClassroomCourses,
                ClassroomService.Scope.ClassroomRosters,
                ClassroomService.Scope.ClassroomProfileEmails
            };

            //GoogleCredential credential = GoogleCredential
            //    .FromStream(new FileStream("hits-classroom-1661148456378-fb06e5c714d4.json", FileMode.Open, FileAccess.Read))
            //    .CreateScoped(scopes);

            //ClassroomService classroomService = new ClassroomService(new BaseClientService.Initializer
            //{
            //    HttpClientInitializer = credential,
            //    ApplicationName = "HITs-Classroom"
            //});

            var dataRequest = new GenerateAccessTokenRequest();
            dataRequest.Scope = scopes;
            var clientService = new ClassroomService();
            ServiceAccountsResource.GenerateAccessTokenRequest request = new ServiceAccountsResource.GenerateAccessTokenRequest(
                clientService,
                dataRequest,
                "projects/-/serviceAccounts/sa-classroom-admin@hits-classroom-1661148456378.iam.gserviceaccount.com"
            );
            try
            {
                var response = request.Execute();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            //var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
            //var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            //{
            //    ClientSecrets = GoogleClientSecrets.FromStream(stream).Secrets,
            //    Scopes = scopes,
            //    DataStore = new FileDataStore("Store")
            //});

            //var token = new TokenResponse
            //{
            //    AccessToken = response.AccessToken
            //};

            //var userCredential = new UserCredential(flow, Environment.UserName, token);
            //ClassroomService userClassroomService = new ClassroomService(new BaseClientService.Initializer
            //{
            //    HttpClientInitializer = userCredential,
            //    ApplicationName = "HITs-Classroom"
            //});

            //var course = userClassroomService.Courses.Get("490235491409").Execute();
            //Debug.WriteLine(course.Name);
        }
    }
}
