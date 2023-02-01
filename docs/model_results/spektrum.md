# Spektrum

## Description

[Spektrum](https://github.com/dxworks/spektrum) is a static, language-independent code analysis tool that generates test coverage metrics based on input generated from other external tools.

Examples of method names and ids:

Method Name: `Method1#string,int`

Method ID: `Class1.cs->Class1@Method1#double`

## Entities

### Repository

```typescript
interface Repository 
{
    solutions: Solution[];
    projects: Project[];
}
```

### Solution

```typescript
interface Solution 
{
    path: string;
    projects: string[];
}
```

### Project

```typescript
interface Project 
{
    name: string;
    path: string;
    files: File[];
    projectReferences: string[];
    externalReferences: string[];
}
```

### File

```typescript
interface File 
{
    name: string;
    path: string;
    namespaces: Namespace[]
}
```

### Namespace

```typescript
interface Namespace 
{
    name: string;
    classes: Class[]
}
```

### Class

```typescript
interface Class 
{
    name: string;
    type: string;
    usingStatements: string[];
    attributes: string[];
    usedClasses: stirng[];
    methods: Method[];
}
```

### Method

```typescript
interface Method 
{
    name: string;
    type: string;
    attributes: string[];
    modifiers: string[];
    callers: string[];
    calledMethods: string[]; 
}
```
