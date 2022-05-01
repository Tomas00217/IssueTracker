﻿namespace IssueTracker.Models
{
    public class ProjectDetails
    {
        public Project? Project { get; set; }
        
        public IQueryable<int>? projectIds;

        public string? projectLead;

        public PaginatedList<Issue>? issues;

        public ProjectRole userRole;

        public PaginatedList<PersonProject>? personProjects;

        public List<Person>? allUsers;
    }
}
