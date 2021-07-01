using System;
using HoneydewCore.Models;

namespace HoneydewCore.Processors
{
    public class FullNameModelProcessor : IProcessorFunction<SolutionModel, SolutionModel>
    {
        public Func<Processable<SolutionModel>, Processable<SolutionModel>> GetFunction()
        {
            return solutionModelProcessable =>
            {
                var solutionModel = solutionModelProcessable.Value;

                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var (namespaceName, namespaceModel) in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            if (!classModel.FullName.Contains("."))
                            {
                                classModel.FullName = $"{namespaceName}.{classModel.FullName}";
                            }
                        }
                    }
                }

                foreach (var classModel in solutionModel.GetEnumerable())
                {
                    foreach (var methodModel in classModel.Methods)
                    {
                        var modelByFullName = solutionModel
                            .GetClassModelByFullName(methodModel.ContainingClassName);
                        if (modelByFullName != null)
                        {
                            methodModel.ContainingClassName = modelByFullName.FullName;
                        }

                        foreach (var methodModelCalledMethod in methodModel.CalledMethods)
                        {
                            var classModelByFullName = solutionModel
                                .GetClassModelByFullName(methodModelCalledMethod.ContainingClassName);
                            if (classModelByFullName != null)
                            {
                                methodModelCalledMethod.ContainingClassName = classModelByFullName.FullName;
                            }
                        }
                    }
                }

                return solutionModelProcessable;
            };
        }
    }
}