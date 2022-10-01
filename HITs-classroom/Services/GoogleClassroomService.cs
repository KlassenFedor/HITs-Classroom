using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.IAMCredentials.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using static Google.Apis.IAMCredentials.v1.ProjectsResource;
using System.Diagnostics;
using Google.Apis.IAMCredentials.v1.Data;

namespace HITs_classroom.Services
{
    public class GoogleClassroomService
    {
        public ClassroomService GetClassroomService(string relatedUser)
        {
            string accessToken = GetAccessToken(relatedUser);
            GoogleCredential credential = GoogleCredential.FromAccessToken(accessToken);
            ClassroomService classroomService = new ClassroomService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "HITs-Classroom"
            });

            return classroomService;
        }

        public string GetAccessToken(string relatedUser)
        {
            string[] scopes = {
                "https://www.googleapis.com/auth/cloud-platform"
            };

            string[] scopesGC = {
                ClassroomService.Scope.ClassroomCourses,
                ClassroomService.Scope.ClassroomRosters,
                ClassroomService.Scope.ClassroomProfileEmails
            };

            var dataRequest = new GenerateAccessTokenRequest();
            dataRequest.Scope = scopesGC;
            dataRequest.Lifetime = "3600s";
            UserCredential credential;
            using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    relatedUser,
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
            IClientService clientService = new IAMCredentialsService();

            ServiceAccountsResource.GenerateAccessTokenRequest request = new ServiceAccountsResource.GenerateAccessTokenRequest(
                clientService,
                dataRequest,
                "projects/-/serviceAccounts/sa-classroom-admin@hits-classroom-1661148456378.iam.gserviceaccount.com"
            );
            request.Credential = credential;

            try
            {
                var response = request.Execute();
                Debug.WriteLine(response.AccessToken);
                return response.AccessToken;
            }
            catch
            {
                throw new AccessViolationException();
            }
        }
    }
}
