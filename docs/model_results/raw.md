# Raw Model

## Description

This is the raw model that is exported by the [extract command](/commands/extract) and is based on the interfaces described in the [Model Section](/model/types)

## Entities

### Repository

```typescript
interface Repository
{
  Version: string;
  Solutions: SolutionModel[];
  Projects: ProjectModel[]
}
```

### Solution

```typescript
interface Solution
{
    FilePath: string;
    ProjectsPaths: string[];
    Metadata: Dictionary<string, Dictionary<string, string>>
}
```

### Project

```typescript
interface ProjectModel
{
    Language: string;
    Name: string;
    FilePath: string;
    ProjectReferences: string[];
    Namespaces: NamespaceModel[];
    CompilationUnits: ICompilationUnitType[];
    Metadata: Dictionary<string, Dictionary<string, string>>
}
```
Language can be either: `C#` or `Visual Basic`


### Namespace

```typescript
interface NamespaceModel 
{
    Name: string;
    ClassNames: string[];
}
```

### Compilation Unit

Up until now, the presented models were common to all programming languages.
Starting from the compilation unit type, all the interface types are implemented in a dedicated project for each programming language.
By doing so, the extractors can keep the same interface across the model, yet still be able to store the particular features of each programming language.

=== "C#"

    ```typescript
    interface CompilationUnitType
    {
        FilePath: string;
        ClassTypes: IClassType[];
        Imports: IImportType[];
        Loc: LinesOfCode;
        Metrics: MetricModel[];
    }
    ```
=== "Visual Basic"

    ```typescript
    interface CompilationUnitType
    {
        FilePath: string;
        ClassTypes: IClassType[];
        Imports: IImportType[];
        Loc: LinesOfCode;
        Metrics: MetricModel[];
    }
    ```

### Class

=== "C#"

    ```typescript
    interface ClassModel 
    {
        ClassType: string;
        Mame: string;
        GenericParameters: IGenericParameterType[];
        FilePath: string;
        AccessModifier: string;
        Modifier: string;
        ContainingNamespaceName: string;
        ContainingClassName: string;
        BaseTypes: IBaseType[];
        Imports: IImportType[];
        Fields: FieldType[];
        Properties: PropertyType[];
        Constructors: IConstructorType[];
        Methods: IMethodType[];
        Destructor?: IDestructorType;
        Attributes: IAttributeType[];
        Metrics: MetricModel[];
        Loc: LinesOfCode;
    }
    ```
=== "Visual Basic"

    ```typescript
    interface ClassModel 
    {
        ClassType: string;
        Name: string;
        GenericParameters: IGenericParameterType[];
        FilePath: string; 
        AccessModifier: string;
        Modifier: string;
        ContainingNamespaceName: string;
        ContainingModuleName: string;
        ContainingClassName: string;
        BaseTypes: IBaseType[];
        Imports: IImportType[]; 
        Fields: FieldType[];
        Properties: PropertyType[];
        Constructors: ConstructorType[];
        Methods: MethodType[]; 
        Destructor?: IDestructorType;
        Attributes: AttributeType[];
        Metrics: MetricModel[];
        Loc: LinesOfCode;
    }
    ```

ClassType can be either: `class`, `interface` or `struct`

### Enum

=== "C#"

    ```typescript
    interface EnumModel
    {
        ClassType: string;
        Mame: string;
        FilePath: string;
        Type: string;
        Labels: IEnumLabelType[];
        AccessModifier: string;
        Modifier: string;
        ContainingNamespaceName: string;
        ContainingClassName: string;
        BaseTypes: IBaseType[];
        Imports: IImportType[]; 
        Attributes: IAttributeType[];
        Metrics: MetricModel[];
        Loc: LinesOfCode;
    }
    ```
=== "Visual Basic"

    ```typescript
    interface EnumModel 
    {
        ClassType: string;
        Mame: string;
        FilePath: string;
        Type: string;
        Labels: IEnumLabelType[];
        AccessModifier: string;
        Modifier: string;
        ContainingNamespaceName: string;
        ContainingModuleName: string;
        ContainingClassName: string;
        BaseTypes: IBaseType[];
        Imports: IImportType[]; 
        Attributes: IAttributeType[];
        Metrics: MetricModel[];
        Loc: LinesOfCode;
    }
    ```

ClassType is `enum`

### Enum Label

=== "C#"

    ```typescript
    interface EnumLabel 
    {
        Name: string; 
        Attributes: IAttributeType[]; 
    }
    ```
=== "Visual Basic"

    ```typescript
    interface EnumLabel
    {
        Name: string; 
        Attributes: IAttributeType[]; 
    }
    ```

### Delegate


=== "C#"

    ```typescript
    interface DelegateModel 
    {
        ClassType: string;
        Mame: string;
        FilePath: string;
        GenericParameters: IGenericParameterType[];
        AccessModifier: string;
        Modifier: string;
        ContainingNamespaceName: string;
        ContainingClassName: string;
        BaseTypes: IBaseType[];
        ParameterTypes: IParameterType[];
        ReturnValue: IReturnValueType;
        Imports: IImportType[]; 
        Attributes: IAttributeType[];
        Metrics: MetricModel[];
        Loc: LinesOfCode;
    }
    ```
=== "Visual Basic"

    ```typescript
    interface DelegateModel 
    {
        ClassType: string;
        Mame: string;
        FilePath: string;
        GenericParameters: IGenericParameterType[];
        AccessModifier: string;
        Modifier: string;
        ContainingNamespaceName: string;
        ContainingModuleName: string;
        ContainingClassName: string;
        BaseTypes: IBaseType[];
        ParameterTypes: IParameterType[];
        ReturnValue: IReturnValueType;
        Imports: IImportType[]; 
        Attributes: IAttributeType[];
        Metrics: MetricModel[];
        Loc: LinesOfCode;
    }
    ```

ClassType is `delegate`

### Import

=== "C#"

    ```typescript
    interface Import 
    {
        Name: string; 
        IsStatic: bool;
        Alias: string;
        AliasType: string;
    }
    ```
=== "Visual Basic"

    ```typescript
    interface Import 
    {
        Name: string; 
        IsStatic: bool;
        Alias: string;
        AliasType: string;
    }
    ```

AliasType can have one of the following values:
- None
- Namespace
- Class
- NotDetermined

### Base Type

=== "C#"

    ```typescript
    interface BaseType 
    {
        Type: IEntityType; 
        Kind: string; 
    }
    ```
=== "Visual Basic"

    ```typescript
    interface BaseType 
    {
        Type: IEntityType; 
        Kind: string; 
    }
    ```

Kind can be either: `class` or `interface`

### Entity Type


=== "C#"

    ```typescript
    interface EntityType 
    {
        Name: string; 
        FullType: GenericType;
        IsExtern: bool;
    }
    ```
=== "Visual Basic"

    ```typescript
    interface EntityType
    {
        Name: string; 
        FullType: GenericType;
        IsExtern: bool; 
    }
    ```

### Generic Type


```typescript
interface GenericType
{
    Name: string;
    ContainedTypes: GenericType[];
    IsNullable: bool;
}
```

### Attribute

=== "C#"

    ```typescript
    interface Attribute 
    {
        Name: string; 
        Type: IEntityType;
        ParameterTypes: IParameterTypes[];
        Target: string;
    }
    ```
=== "Visual Basic"

    ```typescript
    interface Attribute 
    {
        Name: string; 
        Type: IEntityType;
        ParameterTypes: IParameterTypes[];
        Target: string;
    }
    ```

Target can be one of the following:
- method
- type
- field
- property
- param
- return

### Field

=== "C#"

    ```typescript
    interface Field 
    {
        Name: string; 
        Type: IEntityType;
        Modifier: string;
        AccessModifier: string;
        IsEvent: bool;
        IsNullable: bool;
        Attributes: IAttributeType[];
        Metrics: MetricModel[]
    }
    ```
=== "Visual Basic"

    ```typescript
    interface Field 
    {
        Name: string; 
        Type: IEntityType;
        Modifier: string;
        AccessModifier: string;
        IsNullable: bool;
        Attributes: IAttributeType[];
        Metrics: MetricModel[]
    }
    ```

### Property

=== "C#"

    ```typescript
    interface Property 
    {
        Name: string; 
        Type: IEntityType;
        Modifier: string;
        AccessModifier: string;
        IsEvent: bool;
        IsNullable: bool;
        CyclomaticComplexity: number;
        Accessors: IAccessorMethodType[];
        Loc: LinesOfCode;
        Attributes: IAttributeType[];
        Metrics: MetricModel[]
    }
    ```
=== "Visual Basic"

    ```typescript
    interface Property 
    {
        Name: string; 
        Type: IEntityType;
        Modifier: string;
        AccessModifier: string;
        IsNullable: bool;
        CyclomaticComplexity: number;
        Accessors: IAccessorMethodType[];
        Loc: LinesOfCode;
        Attributes: IAttributeType[];
        Metrics: MetricModel[]
    }
    ```

### Accessor Method

=== "C#"

    ```typescript
    interface AccessorMethod 
    {
        Name: string;
        ReturnValue: IReturnValueType;
        ParameterTypes: IParameterType[];
        AccessModifier: string;
        Modifier: string;
        Attributes: IAttributeType[];
        CalledMethods: IMethodCallType[];
        AccessedFields: AccessField[];
        CyclomaticComplexity: number;
        LocalVariableTypes: ILocalVariableType[];
        LocalFunctions: IMethodType[];
        Loc: LinesOfCode;
        Metrics: MetricModel[];
    }
    ```

=== "Visual Basic"

    ```typescript
    interface AccessorMethod 
    {
        Name: string;
        ReturnValue: IReturnValueType;
        ParameterTypes: IParameterType[];
        AccessModifier: string;
        Modifier: string;
        Attributes: IAttributeType[];
        CalledMethods: IMethodCallType[];
        AccessedFields: AccessField[];
        CyclomaticComplexity: number;
        LocalVariableTypes: ILocalVariableType[];
        LocalFunctions: IMethodType[];
        Loc: LinesOfCode;
        Metrics: MetricModel[];
    }
    ```

### Method

=== "C#"

    ```typescript
    interface Method 
    {
        Name: string;
        ReturnValue: IReturnValueType;
        ParameterTypes: IParameterType[];
        GenericParameters: IGenericParameterType[];
        AccessModifier: string;
        Modifier: string;
        Attributes: IAttributeType[];
        CalledMethods: IMethodCallType[];
        AccessedFields: AccessField[];
        CyclomaticComplexity: number;
        LocalVariableTypes: ILocalVariableType[];
        LocalFunctions: IMethodType[];
        Loc: LinesOfCode;
        Metrics: MetricModel[];
    }

    ```
=== "Visual Basic"

    ```typescript
    interface Method 
    {
        Name: string;
        ReturnValue: IReturnValueType;
        ParameterTypes: IParameterType[];
        GenericParameters: IGenericParameterType[];
        AccessModifier: string;
        Modifier: string;
        Attributes: IAttributeType[];
        CalledMethods: IMethodCallType[];
        AccessedFields: AccessField[];
        CyclomaticComplexity: number;
        LocalVariableTypes: ILocalVariableType[];
        LocalFunctions: IMethodType[];
        Loc: LinesOfCode;
        Metrics: MetricModel[];
    }
    ```

### Constructor

=== "C#"

    ```typescript
    interface Constructor 
    {
        Name: string;
        ParameterTypes: IParameterType[];
        AccessModifier: string;
        Modifier: string;
        Attributes: IAttributeType[];
        CalledMethods: IMethodCallType[];
        AccessedFields: AccessField[];
        CyclomaticComplexity: number;
        LocalVariableTypes: ILocalVariableType[];
        LocalFunctions: IMethodType[];
        Loc: LinesOfCode;
        Metrics: MetricModel[];
    }

    ```
=== "Visual Basic"

    ```typescript
    interface Constructor 
    {
        Name: string;
        ParameterTypes: IParameterType[];
        AccessModifier: string;
        Modifier: string;
        Attributes: IAttributeType[];
        CalledMethods: IMethodCallType[];
        AccessedFields: AccessField[];
        CyclomaticComplexity: number;
        LocalVariableTypes: ILocalVariableType[];
        LocalFunctions: IMethodType[];
        Loc: LinesOfCode;
        Metrics: MetricModel[];
    }
    ```

### Destructor

=== "C#"

    ```typescript
    interface Destructor 
    {
        Name: string;
        ParameterTypes: IParameterType[];
        AccessModifier: string;
        Modifier: string;
        Attributes: IAttributeType[];
        CalledMethods: IMethodCallType[];
        AccessedFields: AccessField[];
        CyclomaticComplexity: number;
        LocalVariableTypes: ILocalVariableType[];
        LocalFunctions: IMethodType[];
        Loc: LinesOfCode;
        Metrics: MetricModel[];
    }

    ```
=== "Visual Basic"

    ```typescript
    interface Destructor 
    {
        Name: string;
        ParameterTypes: IParameterType[];
        AccessModifier: string;
        Modifier: string;
        Attributes: IAttributeType[];
        CalledMethods: IMethodCallType[];
        AccessedFields: AccessField[];
        CyclomaticComplexity: number;
        LocalVariableTypes: ILocalVariableType[];
        LocalFunctions: IMethodType[];
        Loc: LinesOfCode;
        Metrics: MetricModel[];
    }
    ```

### Parameter

=== "C#"

    ```typescript
    interface Parameter 
    {
        Type: EntityType; 
        Modifier: string;
        DefaultValue?: string;
        IsNullable: bool;
        Attributes: IAttributeType[];
    }

    ```
=== "Visual Basic"

    ```typescript
    interface Parameter 
    {
        Type: EntityType; 
        Modifier: string;
        DefaultValue?: string;
        IsNullable: bool;
        Attributes: IAttributeType[]; 
    }
    ```

### Generic Parameter

=== "C#"

    ```typescript
    interface GenericParameter 
    {
        Name: string;
        Modifier: string;
        Constraints: EntityType[];
        Attributes: IAttributeType[];
    }
    ```
=== "Visual Basic"

    ```typescript
    interface GenericParameter 
    {
        Name: string;
        Modifier: string;
        Constraints: EntityType[];
        Attributes: IAttributeType[];
    }
    ```

### Return Value

=== "C#"

    ```typescript
    interface ReturnValue
    {
        Type: IEntityType;
        Modifier: string;
        IsNullable: bool;
        Attributes: IAttributeType[];
    }
    ```
=== "Visual Basic"

    ```typescript
    interface ReturnValue 
    {
        Type: IEntityType;
        Modifier: string;
        IsNullable: bool;
        Attributes: IAttributeType[];
    }
    ```

### Accessed Field

```typescript
interface AccessedField
{
    Name: string;
    DefinitionClassName: string;
    LocationClassName: string;
    Kind: number;
}
```

Kind can be either `0` (Getter) or `1` (Setter)

`DefinitionClassName` is used for the base class and `LocationClassName` is used for the derived class

### Called Method

=== "C#"

    ```typescript
    interface MethodCall
    {
        Name: string;
        DefinitionClassName: string;
        LocationClassName: string;
        MethodDefinitionNames: string[];
        GenericParameters: EntitTye[];
        ParameterTypes: IParameterType[];
    }
    ```
=== "Visual Basic"

    ```typescript
    interface MethodCall
    {
        Name: string;
        DefinitionClassName: string;
        LocationClassName: string;
        MethodDefinitionNames: string[];
        GenericParameters: EntitTye[];
        ParameterTypes: IParameterType[];
    }
    ```

### Local Variable

=== "C#"

    ```typescript
    interface LocalVariable
    {
        Name: string;
        Type: IEntityType;
        Modifier: string;
        IsNullable: bool;
    }
    ```
=== "Visual Basic"

    ```typescript
    interface LocalVariable
    {
        Name: string;
        Type: IEntityType;
        Modifier: string;
        IsNullable: bool;
    }
    ```

### Metric Model

```typescript
interface MetricModel
{
    Name: string;
    ExtractorName: string;
    ValueType: string;
    Value?: object;
}
```

### Lines of Code

```typescript
interface LinesOfCode
{
    SourceLines: number;
    CommentedLines: number;
    EmptyLines: number;
}
```
