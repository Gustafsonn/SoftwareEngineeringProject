# Branching Strategy

## Overview
Our branching strategy follows GitFlow with some modifications to support both rapid feature development and critical bug fixes. This strategy ensures code stability while allowing for parallel development.

## Main Branches

### main
- Production-ready code
- Protected branch (requires pull request and review)
- Only contains tested, stable code
- Tagged with version numbers

### develop
- Integration branch for features
- Contains latest development changes
- Used for staging and testing
- Merged into main for releases

## Supporting Branches

### Feature Branches
- Format: `feature/feature-name`
- Created from: develop
- Merged into: develop
- Example: `feature/sensor-config`
- Used for:
  - New features
  - Major enhancements
  - Long-term development

### Bugfix Branches
- Format: `bugfix/issue-number`
- Created from: develop
- Merged into: develop
- Used for:
  - Non-critical bug fixes
  - Minor improvements
  - Documentation updates

### Hotfix Branches
- Format: `hotfix/issue-number`
- Created from: main
- Merged into: main and develop
- Used for:
  - Critical bug fixes
  - Security patches
  - Production issues

## Workflow

### Feature Development
1. Create feature branch from develop
2. Develop and test locally
3. Create pull request to develop
4. Code review and testing
5. Merge into develop

### Bug Fixes
1. Create bugfix branch from develop
2. Fix and test locally
3. Create pull request to develop
4. Code review and testing
5. Merge into develop

### Hotfixes
1. Create hotfix branch from main
2. Fix and test locally
3. Create pull request to main
4. Code review and testing
5. Merge into main
6. Merge into develop

## Pull Request Process
1. Create pull request with description
2. Assign reviewers
3. Address review comments
4. Ensure all tests pass
5. Get approval from reviewers
6. Merge into target branch

## Benefits
- Clear separation of concerns
- Support for parallel development
- Easy tracking of features and fixes
- Maintainable codebase
- Quick response to critical issues

## Best Practices
- Keep branches up to date with target
- Regular commits with meaningful messages
- Clean up branches after merge
- Use pull requests for all changes
- Follow code review guidelines 