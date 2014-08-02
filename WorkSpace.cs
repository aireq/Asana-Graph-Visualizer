using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace AsanaGraphVisualizer
{
    class WorkSpace
    {

        public Dictionary<long, Team> Teams { get; private set; }
        public Dictionary<long, Project> Projects { get; private set; }
        public Dictionary<long, Task> Tasks { get; private set; }
        public Dictionary<long, User> Users { get; private set; }


        public long Id { get; private set; }


        IList<string> _apiKeys;

        public WorkSpace(long workspaceId, IList<string> apiKeys)
        {
            Id = workspaceId;

            if (apiKeys == null) throw new ArgumentNullException("apiKeys");
            if (apiKeys.Count < 1) throw new ArgumentException("apiKeys.Count must be > 0");

            _apiKeys = apiKeys;
        }


        public void GenerateGraph()
        {
            //Initalize Collections

            Teams = new Dictionary<long, Team>();
            Projects = new Dictionary<long, Project>();
            Tasks = new Dictionary<long, Task>();
            Users = new Dictionary<long, User>();


            foreach (string apiKey in _apiKeys)
            {
                ReadData(apiKey);
            }

        }

        private dynamic RequestData(string apiKey, Uri dataUri)
        {
            using (WebClient web = new WebClient())
            {
                var req = WebRequest.Create(dataUri);

                var authInfo = apiKey + ":";
                var encodedAuthInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

                web.Headers.Add("Authorization", "Basic " + encodedAuthInfo);

                var jsonString = web.DownloadString(dataUri);

                return JsonConvert.DeserializeObject(jsonString);
            }
        }


        private DateTime ParseDateString(string dateString)
        {
            // 2014-03-19T00:44:01.340Z

            string pattern = @"(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})\.\d{3}Z";


            var match = System.Text.RegularExpressions.Regex.Match(dateString, pattern);

            if (match.Success)
            {

                int year = System.Convert.ToInt32(match.Groups[1].Value);
                int month = System.Convert.ToInt32(match.Groups[2].Value);
                int day = System.Convert.ToInt32(match.Groups[3].Value);

                int hour = System.Convert.ToInt32(match.Groups[4].Value);
                int minute = System.Convert.ToInt32(match.Groups[5].Value);
                int second = System.Convert.ToInt32(match.Groups[6].Value);


                DateTime date = new DateTime(year, month, day, hour, minute, second);

                return date;

            }
            else
            {
                throw new ArgumentException("dateString is not proper format");
            }

        }

        private void ReadData(string apiKey)
        {
            //First get all projects the user can see in the workspace
            //https://app.asana.com/api/1.0/workspaces/11044771237435/projects?opt_pretty&opt_expand=.
            //https://app.asana.com/api/1.0/workspaces/11044771237435/projects?opt_expand=.


            Uri projectListURI = new Uri("https://app.asana.com/api/1.0/workspaces/" + Id + "/projects?opt_expand=.");



            var projectsData = RequestData(apiKey, projectListURI);



            List<Project> newProjects = new List<Project>();



            foreach (var projectData in projectsData.data)
            {

                long projId = projectData.id;

                if (!this.Projects.ContainsKey(projId))
                {
                    Project project = new Project();

                    project.Id = projId;




                    project.CreatedUTC = projectData.created_at;
                    project.ModifiedUTC = projectData.modified_at;

                    project.Name = projectData.name;
                    project.Notes = projectData.Notes;


                    project.Archived = projectData.archived;

                    long teamId = projectData.team.id;


                    //see if team already exist
                    Team projTeam = null;
                    this.Teams.TryGetValue(teamId, out projTeam);


                    //Init team is it doesn't exist
                    if (projTeam == null)
                    {
                        projTeam = new Team();
                        projTeam.Id = teamId;
                        projTeam.Name = projectData.team.name;



                        //https://app.asana.com/api/1.0/teams/11094681378698/users
                        Uri teamUsersUri = new Uri("https://app.asana.com/api/1.0/teams/" + teamId + "/users?opt_expand=.");

                        var teamusersData = RequestData(apiKey, teamUsersUri);


                        foreach (var userData in teamusersData.data)
                        {

                            long userId = userData.id;

                            User user = null;

                            Users.TryGetValue(userId, out user);

                            if (user == null)
                            {
                                user = ParseUserData(userData);
                                this.Users.Add(userId, user);
                            }

                            projTeam.Users.Add(userId, user);
                        }

                        this.Teams.Add(teamId, projTeam);

                    }

                    //add the project to it's team
                    projTeam.Projects.Add(projId, project);

                    //Add the project to the list of projects
                    this.Projects.Add(projId, project);




                    //For each new project get the project tasks
                    //https://app.asana.com/api/1.0/projects/11044771450805/tasks?opt_pretty&opt_expand=.

                    Uri projectTasksDataUri = new Uri("https://app.asana.com/api/1.0/projects/" + projId + "/tasks?opt_expand=.");

                    var projTasksData = RequestData(apiKey, projectTasksDataUri);


                    foreach (var taskData in projTasksData.data)
                    {

                        long newTaskId = taskData.id;

                        Task task = null;

                        Tasks.TryGetValue(newTaskId, out task);

                        if (task == null)
                        {

                            Task newTask = new Task();

                            newTask.Id = newTaskId;
                            newTask.CreatedUTC = taskData.created_at;
                            newTask.ModifiedUTC = taskData.modified_at;
                            newTask.Name = taskData.name;
                            newTask.Notes = taskData.notes;
                            newTask.Completed = taskData.completed;

                            if (newTask.Completed)
                            {
                                newTask.CompletedUTC = taskData.completed_at;
                            }

                            newTask.DueOnUTC = taskData.due_on;


                            if (taskData.assignee != null)
                            {
                                long assigneeId = taskData.assignee.id;

                                User assignee = null;

                                Users.TryGetValue(assigneeId, out assignee);

                                if(assignee == null)
                                {
                                    assignee = new User();

                                    assignee.Id = assigneeId;
                                    assignee.Name = taskData.assignee.name;

                                    this.Users.Add(assigneeId, assignee);
                                }

                                newTask.Assignee = assignee;                           
                            }


                            foreach (var followerData in taskData.followers)
                            {
                                long followerId = followerData.id;


                                User follower = null;

                                Users.TryGetValue(followerId, out follower);

                                if (follower == null)
                                {
                                    follower = new User();

                                    follower.Id = followerId;
                                    follower.Name = followerData.name;

                                    this.Users.Add(followerId, follower);
                                }

                                newTask.Followers.Add(followerId,follower);
                            }


                            project.Tasks.Add(newTaskId, newTask);
                            this.Tasks.Add(newTaskId, newTask);

                        }
                    




                    }



                    
                    newProjects.Add(project);




                }
            }
         


            



            //Recursivly read the sub tasks of each top level project task
            //https://app.asana.com/api/1.0/tasks/11336810799211/subtasks?opt_pretty&opt_expand=.






        }

        private User ParseUserData(dynamic userData)
        {

            User user = new User();

            user.Id = userData.id;
            user.Name = userData.name;
            //user.Email = userData.email;

            
            //if (userData.photo != null)
            //{
            //    user.PhotoUri = new Uri(userData.photo.image_60x60.Value);
            //}


            return user;


        }

        private Project ParseProject(dynamic projectData)
        {
            throw new NotImplementedException();
        }









    }
}



