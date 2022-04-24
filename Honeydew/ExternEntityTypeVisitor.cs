using Honeydew.Logging;
using Honeydew.Models.Types;

namespace Honeydew;

public class ExternEntityTypeVisitor 
{
    private readonly HashSet<string> _classNames;
    private readonly ILogger _logger;

    public ExternEntityTypeVisitor(HashSet<string> classNames, ILogger logger)
    {
        _classNames = classNames;
        _logger = logger;
    }

    public void Visit(IClassType modelType)
    {
        if (modelType is IDelegateType delegateType)
        {
            CheckIfExternTypeIsPresentInModel(modelType, delegateType.ReturnValue.Type);

            foreach (var parameterType in delegateType.ParameterTypes)
            {
                CheckIfExternTypeIsPresentInModel(modelType, parameterType.Type);
            }

            return;
        }

        if (modelType is not IMembersClassType membersClassType)
        {
            return;
        }

        foreach (var fieldType in membersClassType.Fields)
        {
            CheckIfExternTypeIsPresentInModel(modelType, fieldType.Type);
        }

        foreach (var methodType in membersClassType.Methods)
        {
            CheckIfExternTypeIsPresentInModel(modelType, methodType.ReturnValue.Type);

            foreach (var parameterType in methodType.ParameterTypes)
            {
                CheckIfExternTypeIsPresentInModel(modelType, parameterType.Type);
            }

            foreach (var localVariableType in methodType.LocalVariableTypes)
            {
                CheckIfExternTypeIsPresentInModel(modelType, localVariableType.Type);
            }

            if (methodType is ITypeWithLocalFunctions typeWithLocalFunctions)
            {
                CheckExternTypesInLocalFunctions(typeWithLocalFunctions);
            }
        }

        foreach (var constructorType in membersClassType.Constructors)
        {
            foreach (var parameterType in constructorType.ParameterTypes)
            {
                CheckIfExternTypeIsPresentInModel(modelType, parameterType.Type);
            }

            foreach (var localVariableType in constructorType.LocalVariableTypes)
            {
                CheckIfExternTypeIsPresentInModel(modelType, localVariableType.Type);
            }

            if (constructorType is ITypeWithLocalFunctions typeWithLocalFunctions)
            {
                CheckExternTypesInLocalFunctions(typeWithLocalFunctions);
            }
        }

        if (modelType is not IPropertyMembersClassType propertyMembersClassType)
        {
            return;
        }

        foreach (var propertyType in propertyMembersClassType.Properties)
        {
            CheckIfExternTypeIsPresentInModel(modelType, propertyType.Type);

            foreach (var accessor in propertyType.Accessors)
            {
                foreach (var localVariableType in accessor.LocalVariableTypes)
                {
                    CheckIfExternTypeIsPresentInModel(modelType, localVariableType.Type);
                }

                if (accessor is ITypeWithLocalFunctions typeWithLocalFunctions)
                {
                    CheckExternTypesInLocalFunctions(typeWithLocalFunctions);
                }
            }
        }

        void CheckExternTypesInLocalFunctions(ITypeWithLocalFunctions typeWithLocalFunctions)
        {
            foreach (var localFunction in typeWithLocalFunctions.LocalFunctions)
            {
                CheckIfExternTypeIsPresentInModel(modelType, localFunction.ReturnValue.Type);
                foreach (var parameterType in localFunction.ParameterTypes)
                {
                    CheckIfExternTypeIsPresentInModel(modelType, parameterType.Type);
                }

                foreach (var localVariableType in localFunction.LocalVariableTypes)
                {
                    CheckIfExternTypeIsPresentInModel(modelType, localVariableType.Type);
                }

                foreach (var function in localFunction.LocalFunctions)
                {
                    CheckExternTypesInLocalFunctions(function);
                }
            }
        }
    }

    private void CheckIfExternTypeIsPresentInModel(IClassType classType, IEntityType entityType)
    {
        if (!entityType.IsExtern)
        {
            return;
        }

        if (_classNames.Contains(entityType.Name))
        {
            _logger.Log(
                $"Extern type {entityType.Name} is used in {classType.Name} from {classType.FilePath} but the type is also a found in the model");
        }
    }
}
