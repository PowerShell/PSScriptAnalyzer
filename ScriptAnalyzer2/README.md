# PSScriptAnalyzer 2.0

PSScriptAnalyzer 2.0 seeks to re-architect the core of the PSScriptAnalyzer engine
with the following goals:

- Performance: both startup and repeated use should be fast
- Hostability: it should be possible to embed PSScriptAnalyzer in other projects with a minimum of difficulty or overhead
- Code reuse: problems should be solved once and shared, solutions that are applicable to other projects should ideally go upstream
- Static reproducibility: analysis should work the same everywhere when possible and not depend on the analysis host's state
- Configurability: configuration should be preferred over opinion, and configuration mechanisms should be discoverable and self-validating

## Building

To build the project, run:

```powershell
./build.ps1
```

For now this will just produce the module.

## Support

PSScriptAnalyzer 2.0 supports PowerShell 5.1 and 7,
and also seeks to support PowerShell versions post-7.

There is no plan to support older versions of PowerShell to host PSScriptAnalyzer,
but analyzing scripts with PowerShell 3 or 4 as a target platform is a goal.

## Defining rules

A rule can currently be defined like this:

```csharp
[RuleDescription("<Description of rule>")]
[Rule("<RuleName>")]
public class MyRule : ScriptRule
{
    public MyRule(RuleInfo ruleInfo)
        : base(ruleInfo)
    {
    }

    public override IEnumerable<ScriptDiagnostic> AnalyzeScript(Ast ast, IReadOnlyList<Token> tokens, string fileName)
    {
        // Implementation
    }
}
```

To improve performance with a `TypeRuleProvider`, it's possible to add other hints about the lifetime of a rule:

- The `[IdempotentRule]` attribute indicates that a rule instance can be reused, rather than reinstantiated each time
- The `[ThreadsafeRule]` attribute indicates that a rule instance can be run in parallel with other rules in an analysis run
- Implementing the `IResettable` interface will mean that a rule's `Reset()` method is called before the same instance is reused for runs
- Implementing the `IDisposable` interface will mean that the rule will be disposed after an analysis run

## Architecture

The main entry point of PSScriptAnalyzer is the `ScriptAnalyzer` class.
This composes:

- An `IRuleExecutorFactory`, which produces `IRuleExecutor`s,
  which provide an execution strategy for rules (such as executing them in parallel).
- An `IRuleProvider`, which provides rule instances.
  Currently the main form of this is `TypeRuleProvider`,
  which wraps a rule type and creates a factory internally to instantiate rules of that type.
