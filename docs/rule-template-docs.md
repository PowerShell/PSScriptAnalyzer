# PSScriptAnalyzer rule template guidance

The [rule-template.md](rule-template.md) file illustrates the structure of the PSScriptAnalyzer rule
articles. Replace all placeholder text, then remove optional sections that don't apply.

## Required content

Every rule article starts with these YAML frontmatter fields:

- `description`
- `ms.date`
- `ms.topic: reference`
- `title`

Place the H1, severity line, and default-state line immediately after the frontmatter. Follow them
with `## Description`, which explains the diagnostic, why it matters, and the preferred alternative.

Each rule must have an `## Example` section. Each example includes a `### Noncompliant` code block
followed by a `### Compliant` alternative. Give each scenario an H3 heading and use H4 headings for
its noncompliant and compliant code pair.

Every rule article includes `## Configure rule` after its example or examples. For a configurable
rule, include a `Rules` hashtable entry. For an always-enabled or otherwise nonconfigurable rule,
use the following text.

```markdown
This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of
  [Using PSScriptAnalyzer][02].
```

## Optional sections

Use an additional H2 explanatory section between `## Description` and the example when readers need
context before reviewing the code. Existing articles use this space for compatibility profile
information, reference tables, supported values, and remediation guidance. For examples, see the
following articles.

- [AvoidUsingConvertToSecureStringWithPlainText](Rules/AvoidUsingConvertToSecureStringWithPlainText.md)
- [UseConsistentParameterSetName](Rules/UseConsistentParameterSetName.md)
- [UseConstrainedLanguageMode](Rules/UseConstrainedLanguageMode.md)

When the rule is configurable, include `### Parameter` for each setting after `## Configure rule`.
Use an H3 heading for each setting and document what it controls, accepted values, and its default
value.

Include `## Suppression` after the `## Configure rule` section only when the rule needs specific
suppression syntax or examples. Otherwise, link readers to the general _Suppressing rules_ guidance
from **Configure rule**.

In the `## Further reading` section, provide links to additional resources that help readers
understand the rule, its context, or related topics.

## Final checks

- Include `## Configure rule` for every rule article.
- Include `### Parameters` only for configurable settings that need individual documentation.
- Pair noncompliant code with a practical compliant alternative.
- Remove all unused optional headings, placeholder text, and link definitions.
