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
                            classModel.FullName = solutionModel.GetClassFullName(namespaceName, classModel.FullName);
                        }
                    }
                }

                foreach (var classModel in solutionModel.GetEnumerable())
                {
                    classModel.BaseClassFullName =
                        solutionModel.GetClassFullName(classModel.Namespace, classModel.BaseClassFullName);
                    for (var i = 0; i < classModel.BaseInterfaces.Count; i++)
                    {
                        classModel.BaseInterfaces[i] =
                            solutionModel.GetClassFullName(classModel.Namespace, classModel.BaseInterfaces[i]);
                    }

                    foreach (var methodModel in classModel.Methods)
                    {
                        methodModel.ReturnType =
                            solutionModel.GetClassFullName(classModel.Namespace, methodModel.ReturnType);
                        
                        SetContainingClassAndCalledMethodsFullName(methodModel, classModel);
                    }

                    foreach (var methodModel in classModel.Constructors)
                    {
                        SetContainingClassAndCalledMethodsFullName(methodModel, classModel);
                    }
                }

                void SetContainingClassAndCalledMethodsFullName(MethodModel methodModel, ClassModel classModel)
                {
                    methodModel.ContainingClassName =
                        solutionModel.GetClassFullName(classModel.Namespace, methodModel.ContainingClassName);

                    foreach (var methodModelCalledMethod in methodModel.CalledMethods)
                    {
                        methodModelCalledMethod.ContainingClassName = solutionModel
                            .GetClassFullName(classModel.Namespace, methodModelCalledMethod.ContainingClassName);

                        foreach (var parameterModel in methodModelCalledMethod.ParameterTypes)
                        {
                            parameterModel.Type =
                                solutionModel.GetClassFullName(classModel.Namespace, parameterModel.Type);
                        }
                    }
                }

                return solutionModelProcessable;
            };
        }
    }
}