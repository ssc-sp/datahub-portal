# Pull Request

## Description

<!-- Provide a high-level, general description of the PR that can be used in release notes if needed. -->

<!-- If applicable, provide screenshots or GIFs of the changes. -->

## Related Issues

<!-- List the user story, task, or bug in ADO that this PR addresses -->

## Changes

<!-- Describe in detail all changes included in this PR. -->

- Change 1
- Change 2

## Testing

### Unit Tests

<!-- Describe the unit tests changed or added with this PR. -->

- Unit Test 1
- Unit Test 2

### Manual Testing

<!-- Describe precisely what manual testing you have conducted on this PR. -->

- Manual Test 1
- Manual Test 2

## Checklist 

<!-- If you are not sure about any of the following items, mention it to your reviewer. -->

### General

- [ ] The PR provides a clear high-level description for a general audience, including photos or GIFs if applicable
- [ ] The PR includes a detailed change list
- [ ] The PR is associated with a bug, user story, or other work item on Azure DevOps
- [ ] Changes to existing functionality have been communicated to affected users (including other DataHub team members), or there is a plan to communicate to affected users, or not applicable

### Code Quality

- [ ] The code is easily understandable and maintainable
- [ ] The code is consistent with .NET standards and the project architecture
- [ ] The code is adequately documented (such as in-code comments or dev documentation)
- [ ] The code does not contain duplicate code that could be abstracted

### Localization

- [ ] New user-facing text has been added to `localization.json` and `localization.fr.json`, or no new user-facing text has been added
- [ ] One of the following is true:
  - [ ] New page routes are added to `url_paths.json`, `url_paths.fr.json`, and `LanguageToggle.razor`
  - [ ] New page routes convey the same meaning in English and French and are not translated.
  - [ ] No new page routes are added.
- [ ] Updated functionality has been tested in both English and French, if applicable

### Tests

- [ ] New code behaves as expected and achieves its intended purpose
- [ ] Existing functionality behaves as expected and has been tested
- [ ] The PR includes adequate unit tests to cover code changes
- [ ] The PR does not break existing tests

### Additional Requirements

- [ ] The code does not introduce any security vulnerabilities
- [ ] The PR follows accessibility requirements or makes no changes that impact accessibility
