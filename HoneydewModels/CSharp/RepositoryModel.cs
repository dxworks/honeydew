﻿using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public class RepositoryModel : IRepositoryModel
    {
        public string Version { get; set; }

        public IList<SolutionModel> Solutions { get; set; } = new List<SolutionModel>();

        public IEnumerable<IClassType> GetEnumerable()
        {
            foreach (var solutionModel in Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            yield return classModel;
                        }
                    }
                }
            }
        }
    }
}
