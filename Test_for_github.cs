using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Test_For_GitHub_Issue
{
    [TestFixture]
    public class Tests
    {
        //const string GitHubAPIUsername =  "***" Please enter your username;
        //const string GitHubAPIPass = "***" Please enter your password;

        [Test, Order(1)]
        public void Test_GitHubAPI_GetIssuesByRepo()

        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues");
            client.Timeout = 3000;
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(response.ContentType.StartsWith("application/json"));

            var issues = new JsonDeserializer().Deserialize<List<IssueResponse>>(response);
        }


        [Test, Order(2)]
        public void Test_GitHubAPI_GetIssueByNumber()
        {

            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/1");
            client.Timeout = 3000;
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(response.ContentType.StartsWith("application/json"));

            var issue = new JsonDeserializer().Deserialize<IssueResponse>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(issue.id > 0);
            Assert.IsTrue(issue.number > 0);
        }
        [Test, Order(3)]
        public void Test_GitHubAPI_GetInvalidIssue()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/#631631265");
            client.Timeout = 3000;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test, Order(4)]
        public void Test_GitHubAPI_CreateNewIssue()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues");
            client.Timeout = 3000;
            var request = new RestRequest(Method.POST);
            client.Authenticator = new HttpBasicAuthenticator(GitHubAPIUsername, GitHubAPIPass);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new
            {
                title = "some title",
                body = "some body"
            });

            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.IsTrue(response.ContentType.StartsWith("application/json"));

            var issue = new JsonDeserializer().Deserialize<IssueResponse>(response);

            Assert.IsTrue(issue.id > 0);
            Assert.IsTrue(issue.number > 0);
            Assert.IsTrue(!String.IsNullOrEmpty(issue.title));
            Assert.IsTrue(!String.IsNullOrEmpty(issue.body));

            //Save issue number to file
            string issuenum = issue.number.ToString();
            File.WriteAllText("issuenuml.txt", issuenum);

            //Edit created issue
            var clientEdit = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/" + issue.number);
            clientEdit.Timeout = 3000;
            var editrequest = new RestRequest(Method.PATCH);

            editrequest.AddHeader("Authorization", "Basic U3RveWFuLUl2OjRmNDY5YWQxODVhODQzZDVhMWNmMGVhNjY0NzczMWNmMDYzNWU5YTE=");
            editrequest.AddHeader("Content-Type", "application/json");
            editrequest.AddParameter("application/json", "{\r\n    \"body\": \"This is edited Issue\"\r\n    \r\n}", ParameterType.RequestBody); ;

            IRestResponse editresponse = clientEdit.Execute(editrequest);
            var editissue = new JsonDeserializer().Deserialize<IssueResponse>(editresponse);

            Assert.AreNotEqual(editissue.body, issue.body);
            Assert.AreEqual(HttpStatusCode.OK, editresponse.StatusCode);
            Assert.IsTrue(editissue.id > 0);
            Assert.IsTrue(editissue.number > 0);
        }

        [Test, Order(5)]
        public void Test_GitHubAPI_CloseCreatedIssueWithouAuth()
        {
            string issuenum = File.ReadAllText("issuenuml.txt");

            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/" + issuenum);
            client.Timeout = 3000;
            var request = new RestRequest(Method.PATCH);

            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n    \"state\": \"closed\"\r\n}", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }


        [Test, Order(6)]
        public void Test_GitHubAPI_CloseCreatedIssue()
        {
            string issuenum = File.ReadAllText("issuenuml.txt");
            var clientClose = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/" + issuenum);
            clientClose.Timeout = 3000;
            var closerequest = new RestRequest(Method.PATCH);

            closerequest.AddHeader("Authorization", "Basic U3RveWFuLUl2OjRmNDY5YWQxODVhODQzZDVhMWNmMGVhNjY0NzczMWNmMDYzNWU5YTE=");
            closerequest.AddHeader("Content-Type", "application/json");
            closerequest.AddParameter("application/json", "{\r\n    \"state\": \"closed\"\r\n}", ParameterType.RequestBody);

            IRestResponse closeresponse = clientClose.Execute(closerequest);
            var closeissue = new JsonDeserializer().Deserialize<IssueResponse>(closeresponse);
            Assert.AreEqual(closeissue.state, "closed");
        }
        [Test, Order(7)]
        public void Test_GitHubAPI_EditIssueByInvalidID()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/#631631265");
            client.Timeout = 3000;
            var request = new RestRequest(Method.PATCH);

            request.AddHeader("Authorization", "Basic U3RveWFuLUl2OjRmNDY5YWQxODVhODQzZDVhMWNmMGVhNjY0NzczMWNmMDYzNWU5YTE=");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n    \"body\": \"This is edited Issue\"\r\n    \r\n}", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        }
        [Test, Order(8)]
        public void Test_GitHubAPI_CreateNewIssue_Unauthorized()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues");
            client.Timeout = 3000;
            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new
            {
                title = "some title",
                body = "some body"
            });

            var response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test, Order(9)]
        public void Test_GitHubAPI_CreateNewIssue_WithoutBody()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues");
            client.Timeout = 3000;
            var request = new RestRequest(Method.POST);
            client.Authenticator = new HttpBasicAuthenticator(GitHubAPIUsername, GitHubAPIPass);
            request.AddHeader("Content-Type", "application/json");

            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }
        [Test, Order(10)]
        public void Test_GitHubAPI_CreateNewIssue_MissingTitle()
        {

            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues");
            client.Timeout = 3000;
            var request = new RestRequest(Method.POST);

            client.Authenticator = new HttpBasicAuthenticator(GitHubAPIUsername, GitHubAPIPass);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n    \"body\": \"Issue from Stoyan\"\r\n}", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Test, Order(11)]
        public void Test_GitHubAPI_RetrieveAllLabelsForIssue()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/6/labels");
            client.Timeout = 3000;
            var request = new RestRequest(Method.GET);

            request.AddHeader("Authorization", "Basic U3RveWFuLUl2OjRmNDY5YWQxODVhODQzZDVhMWNmMGVhNjY0NzczMWNmMDYzNWU5YTE=");
            request.AddParameter("text/plain", "", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
        [Test, Order(12)]
        public void Test_GitHubAPI_CreateComment()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/6/comments");
            client.Timeout = 3000;
            var request = new RestRequest(Method.POST);

            client.Authenticator = new HttpBasicAuthenticator(GitHubAPIUsername, GitHubAPIPass);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n    \"body\": \"This is a comment from Stoyan\"\r\n}", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newComment = new JsonDeserializer().Deserialize<CommentResponse>(response);

            //Delete Created comment
            var clientDel = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/comments/" + newComment.id);
            client.Timeout = 3000;
            var delrequest = new RestRequest(Method.DELETE);
            clientDel.Authenticator = new HttpBasicAuthenticator(GitHubAPIUsername, GitHubAPIPass);
            var delResponse = clientDel.Execute(delrequest);

            Assert.AreEqual(HttpStatusCode.NoContent, delResponse.StatusCode);
        }
    }
}