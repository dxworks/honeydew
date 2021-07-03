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
                    classModel.BaseClassFullName =
                        solutionModel.GetClassFullName(classModel.BaseClassFullName);
                    for (var i = 0; i < classModel.BaseInterfaces.Count; i++)
                    {
                        classModel.BaseInterfaces[i] =
                            solutionModel.GetClassFullName(classModel.BaseInterfaces[i]);
                    }
                    
                    foreach (var methodModel in classModel.Methods)
                    {
                        methodModel.ContainingClassName =
                            solutionModel.GetClassFullName(methodModel.ContainingClassName);

                        foreach (var methodModelCalledMethod in methodModel.CalledMethods)
                        {
                            methodModelCalledMethod.ContainingClassName = solutionModel
                                .GetClassFullName(methodModelCalledMethod.ContainingClassName);
                        }
                    }
                }

                return solutionModelProcessable;
            };
        }
    }
}