﻿using System;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.RepositoryLoading.Strategies
{
    public class BasicProjectLoadingStrategy : IProjectLoadingStrategy
    {
        private readonly ILogger _logger;
        private readonly IRepositoryClassSet _repositoryClassSet;

        public BasicProjectLoadingStrategy(ILogger logger, IRepositoryClassSet repositoryClassSet)
        {
            _logger = logger;
            _repositoryClassSet = repositoryClassSet;
        }

        public async Task<ProjectModel> Load(Project project, CSharpFactExtractor extractors)
        {
            var projectModel = new ProjectModel(project.Name)
            {
                FilePath = project.FilePath,
                ProjectReferences = project.AllProjectReferences
                    .Select(reference => ExtractPathFromProjectId(reference.ProjectId.ToString())).ToList()
            };

            var i = 1;
            var documentCount = project.Documents.Count();
            foreach (var document in project.Documents)
            {
                try
                {
                    _logger.Log($"Extracting facts from {document.FilePath} ({i}/{documentCount})...");

                    var fileContent = await document.GetTextAsync();
                    var classModels = extractors.Extract(fileContent.ToString());

                    _logger.Log($"Done extracting from {document.FilePath} ({i}/{documentCount})");

                    foreach (var classModel in classModels)
                    {
                        classModel.FilePath = document.FilePath;
                        projectModel.Add(classModel);
                        _repositoryClassSet.Add(project.FilePath, classModel.FullName);
                    }
                }
                catch (Exception e)
                {
                    _logger.Log($"Could not extract from {document.FilePath} ({i}/{documentCount}) because {e}",
                        LogLevels.Warning);
                }

                i++;
            }

            return projectModel;
        }

        private string ExtractPathFromProjectId(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            var parts = s.Split(" - ");

            if (parts.Length != 2)
            {
                return s;
            }

            return parts[1][..^1];
        }
    }
}
