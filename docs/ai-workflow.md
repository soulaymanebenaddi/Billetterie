## AI-assisted development

AI tools may be used as development assistants, but generated code must
always be understood, reviewed and tested before being committed.

The project follows a workflow inspired by Addy Osmani's AI-assisted
development practices:

1. Understand the problem
2. Define the implementation plan
3. Implement incrementally
4. Review generated code
5. Run tests and static analysis
6. Review the Git diff
7. Commit only validated changes

AI should primarily be used for:
- explaining concepts
- brainstorming solutions
- reviewing code
- generating tests
- debugging assistance

Core business logic should not be blindly generated or committed without
developer review.