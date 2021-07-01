using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;
using HoneydewCore.Processors;

namespace HoneydewCore.IO.Readers
{
    public class SolutionFileLoader : ISolutionLoader
    {
        private readonly IList<IFactExtractor> _extractors;

        private readonly ISolutionProvider _solutionProvider;
        private readonly ISolutionLoadingStrategy _solutionLoadingStrategy;

        public SolutionFileLoader(IList<IFactExtractor> extractors, ISolutionProvider solutionProvider,
            ISolutionLoadingStrategy solutionLoadingStrategy)
        {
            _extractors = extractors;
            _solutionProvider = solutionProvider;
            this._solutionLoadingStrategy = solutionLoadingStrategy;
        }

        public SolutionModel LoadSolution(string pathToFile)
        {
            var solution = _solutionProvider.GetSolution(pathToFile);

            var solutionModel = _solutionLoadingStrategy.Load(solution, _extractors);

            solutionModel = AddFullNameToDependencies(solutionModel);

            return solutionModel;
        }

        private static SolutionModel AddFullNameToDependencies(SolutionModel solutionModel)
        {
            var solutionModelProcessable = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(new FullNameModelProcessor())
                .Finish<SolutionModel>();

            return solutionModelProcessable.Value;
        }

        // private static SolutionModel AddReferencesForMethodModels(SolutionModel solutionModel)
        // {
        //     Dictionary<string, Dictionary<string, MethodModel>> methodModelsDictionary = new();
        //
        //     foreach (var classModel in solutionModel.GetEnumerable())
        //     {
        //         foreach (var methodModel in classModel.Methods)
        //         {
        //             methodModel.ContainingClassModel =
        //                 solutionModel.GetClassModelByFullName(methodModel.ContainingClassName);
        //
        //             foreach (var methodCallModel in methodModel.CalledMethods)
        //             {
        //                 if (methodModelsDictionary.TryGetValue(methodCallModel.ContainingClass,
        //                     out var methodDictionary))
        //                 {
        //                     if (methodDictionary.TryGetValue(methodCallModel.MethodName, out var methodModelReference))
        //                     {
        //                         methodModel.CalledMethodsModels.Add(methodModelReference);
        //                     }
        //                     else
        //                     {
        //                         var classModelByFullName =
        //                             solutionModel.GetClassModelByFullName(methodCallModel.ContainingClass);
        //                         if (classModelByFullName == null) continue;
        //
        //                         var model = classModelByFullName.Methods.First(
        //                             m => m.Name == methodCallModel.MethodName);
        //                         methodModel.CalledMethodsModels.Add(model);
        //
        //                         methodDictionary.Add(methodCallModel.MethodName, model);
        //                     }
        //                 }
        //                 else
        //                 {
        //                     var dictionary = new Dictionary<string, MethodModel>();
        //
        //                     var classModelByFullName =
        //                         solutionModel.GetClassModelByFullName(methodCallModel.ContainingClass);
        //                     methodModel.CalledMethodsModels = new List<MethodModel>();
        //                     if (classModelByFullName == null)
        //                     {
        //                         continue;
        //                     }
        //
        //                     var model = classModelByFullName.Methods.First(
        //                         m => m.Name == methodCallModel.MethodName);
        //                     methodModel.CalledMethodsModels.Add(model);
        //
        //                     dictionary.Add(methodCallModel.MethodName, model);
        //
        //                     methodModelsDictionary.Add(methodCallModel.ContainingClass, dictionary);
        //                 }
        //             }
        //         }
        //     }
        //
        //     return solutionModel;
        // }
    }
}