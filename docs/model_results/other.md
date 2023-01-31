# Other Results

## Statistics Result

Offers a high-level view of the repository structure

```typescript
interface Statistics 
{
    Version: string;
    Projects: number;
    Solutions: number;
    Files: number;
    Classes: number;
    Delegates: number;
    Interfaces: number;
    Enums: number;
    SourceCodeLines: number;
}
```

## Cyclomatic Complexity Results

```typescript
interface CycloResult
{
    file: File
}

interface File 
{
    concerns: Concern[]
}

interface Concern
{
    entity: string;
    tag: string;
    strength: number;
}
```
