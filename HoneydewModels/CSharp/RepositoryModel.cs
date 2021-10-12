﻿using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public class RepositoryModel : IRepositoryModel
    {
        public string Version { get; set; }

        public IList<SolutionModel> Solutions { get; set; } = new List<SolutionModel>();

        public IList<ProjectModel> Projects { get; set; } = new List<ProjectModel>();

        public IEnumerable<IClassType> GetEnumerable()
        {
            foreach (var projectModel in Projects)
            {
                foreach (var compilationUnitType in projectModel.CompilationUnits)
                {
                    foreach (var classType in compilationUnitType.ClassTypes)
                    {
                        yield return classType;
                    }
                }
            }
        }
    }
}
